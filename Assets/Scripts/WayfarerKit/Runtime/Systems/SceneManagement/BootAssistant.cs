using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions;
using WayfarerKit.Helpers.Serialization;
using WayfarerKit.Patterns.EventBus;
using WayfarerKit.Patterns.Singletons;
using WayfarerKit.Systems.SceneManagement.Helpers;

namespace WayfarerKit.Systems.SceneManagement
{
    /// <summary>
    ///     Entry point for scene loading. It's responsible for loading scene groups and handling transitions between them.
    ///     Manager for bootstrap scene loading.
    ///     Can be used as a base class for custom boot assistant.
    ///     Support start from any scene in Editor for testing purposes.
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-999)]
    public abstract class BootAssistant<T, TEnumGroups> : PersistentSingleton<T> where T : BootAssistant<T, TEnumGroups> where TEnumGroups : Enum
    {
        public delegate Awaitable BeforeTransitionHandler();
        public delegate Awaitable ExtraOperationHandler(IProgress<float> reporter);
        public delegate void SpecificGroupLoadHandler();
        [SerializeField] private bool debugVerbatimLog;
        [SerializeField] private Optional<TEnumGroups> debugStartGroup;

        [SerializeField] private SceneGroup[] sceneGroups;

        private readonly Dictionary<TEnumGroups, SpecificGroupLoadHandler> _groupLoadHandlers = new();

        private bool _isFirstLoad = true;
        private bool _isLoading;

        /// <summary>
        ///     Default transition will be with scene group index 0 together with initialization in StartOperation
        /// </summary>
        private (TEnumGroups group, ExtraOperationHandler prepareOperation) WithDefault
            => (debugStartGroup.Enabled ? debugStartGroup : Enum.GetValues(typeof(TEnumGroups)).Cast<TEnumGroups>().First(), FirstPrepareOperation);

        public bool IsLoading => _isLoading || Manager.IsLoading;
        public bool CanLoad => !IsLoading;

        private SceneGroupManager Manager { get; } = new();

        protected TEnumGroups LastLoadedGroup
        {
            get;
            private set;
        }

        protected override void Awake()
        {
            base.Awake();

#if !UNITY_EDITOR
            debugVerbatimLog = false;
#endif

            Manager.DebugVerbatimLog = debugVerbatimLog;
        }

        private async void Start()
        {
            Assert.IsFalse(Manager.IsLoading, "Fatal error: Loading is already in progress.");
            Assert.IsTrue(sceneGroups.Length > 0, "No scene groups found.");

            foreach (TEnumGroups enumValue in Enum.GetValues(typeof(TEnumGroups)))
            {
                var enumIndex = (int)(object)enumValue;
                if (enumIndex < 0 || enumIndex >= sceneGroups.Length)
                {
                    Log.Error($"BootAssistant.Start: Invalid group enum index: {enumIndex} for {enumValue}. Please check that enum values correspond to scene groups.");

                    return;
                }
            }

            if (!BootstrapperRuntimeInitialize.IsSetupValid)
            {
                Log.Error("BootAssistant.Start: Project is not set up properly. Please check the logs for more information.");

                return;
            }

            await LoadSceneGroup(WithDefault.group, null, WithDefault.prepareOperation);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearGroupLoadHandlers();
        }

        protected override void Initialize() => InitializeGroupLoadHandlers();


        /// <summary>
        ///     Use IProgress in ExtraOperationHandler to report custom progress (from [0 to 1])
        ///     (optionally you can use AsyncOperationGroup or AsyncOperationHandleGroup to track multiple operations progress
        ///     inside custom Awaitable operation)
        /// </summary>
        public async Awaitable<bool> LoadSceneGroup(
            TEnumGroups group,
            BeforeTransitionHandler beforeStart = null,
            ExtraOperationHandler extraOperation = null,
            bool? reloadDuplicates = null
        )
        {
            var index = (int)(object)group;
            Assert.IsTrue(index >= 0 && index < sceneGroups.Length, $"Invalid index: {index}");

            if (IsLoading || Manager.IsLoading)
            {
                Log.Warning($"BootAssistant.LoadSceneGroup: Loading is already in progress. Skip for {sceneGroups[index].GroupName}. Please wait until it's finished...");

                return false;
            }

            Log.Debug($"BootAssistant.LoadSceneGroup: Loading group <color=yellow><b>{sceneGroups[index].GroupName}</b></color> with index <b>{index}</b>");

            // now we can show transition interface
            _isLoading = true;
            LastLoadedGroup = group;

            if (beforeStart != null) await beforeStart.Invoke();
            await StartingSceneTransition(_isFirstLoad);

            // some preparations
            var hasAdditionalOperation = extraOperation != null;
            var steps = new float[hasAdditionalOperation ? 2 : 1];
            var stepIndex = 0;

            // wait for additional operation
            if (hasAdditionalOperation)
            {
                var additionalProgressReport = new LoadingProgressReport();
                await ExecuteOperationWithProgress(
                    extraOperation(additionalProgressReport),
                    additionalProgressReport, stepIndex++, steps);
            }

            // wait for scene group loading
            var loadingProgressReport = new LoadingProgressReport();
            await ExecuteOperationWithProgress(
                Manager.LoadGroup(sceneGroups[index],
                    loadingProgressReport,
                    reloadDuplicates.HasValue && reloadDuplicates.Value || BootstrapperSettings.Instance.ReloadDuplicateScenes),
                loadingProgressReport, stepIndex, steps);

            InvokeGroupLoadedHandlers(group);

            // hide transition interface
            await Awaitable.WaitForSecondsAsync(DelayBeforeFinishingTransition);
            await FinishingSceneTransition();

            _isLoading = false;
            _isFirstLoad = false;

            Log.Debug($"BootAssistant.LoadSceneGroup: Group <color=yellow><b>{sceneGroups[index].GroupName}</b></color> successfully loaded.");

            return true;
        }

        private static async Awaitable ExecuteOperationWithProgress(Awaitable operation, LoadingProgressReport progressReport, int stepIndex, float[] data)
        {
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Length > 0);

            var value = 0f;

            // handler to calculate average progress and report it with EventBus
            Action<float> loadingHandler =
                target =>
                {
                    value = ClampProgressReportValue(target, value);
                    data[stepIndex] = value;

                    // find closes step that still not completed...
                    var averageProgress = data.Average();
                    var reportStep = Mathf.Clamp(Array.FindIndex(data, x => x >= 1) + 1, 0, data.Length - 1);

                    EventBus<BootLoadingProgressEvent>.Raise(BootLoadingProgressEvent.From(averageProgress, reportStep, data.Length));
                };

            progressReport.Progressed += loadingHandler;
            await operation;

            progressReport.Report(1); // ensure that progress for 100% was reported (due to Unity specifics)
            await Awaitable.NextFrameAsync();

            progressReport.Progressed -= loadingHandler;
        }

        private static float ClampProgressReportValue(float value, float currentValue) => Mathf.Clamp(Mathf.Max(value, currentValue), 0f, 1f);

#region Group Events
        private void InitializeGroupLoadHandlers()
        {
            Assert.IsTrue(_groupLoadHandlers.Keys.Count == 0, "Seems like group load handlers were already initialized.");
            foreach (TEnumGroups enumValue in Enum.GetValues(typeof(TEnumGroups))) _groupLoadHandlers[enumValue] = delegate {};
        }

        private void ClearGroupLoadHandlers() => _groupLoadHandlers.Clear();
        public void SubscribeWhenLoaded(TEnumGroups group, SpecificGroupLoadHandler handler)
        {
            if (sceneGroups[(int)(object)group] == Manager.ActiveSceneGroup && !IsLoading) handler();
            _groupLoadHandlers[group] += handler;
        }

        private void InvokeGroupLoadedHandlers(TEnumGroups group) => _groupLoadHandlers[group].Invoke();
        public void UnsubscribeWhenLoaded(TEnumGroups group, SpecificGroupLoadHandler handler)
        {
            if (_groupLoadHandlers.ContainsKey(group)) _groupLoadHandlers[group] -= handler;
        }
#endregion

#region Interface to Implement
        protected abstract Awaitable FirstPrepareOperation(IProgress<float> reporter);
        protected abstract Awaitable StartingSceneTransition(bool isFirstLoad);
        protected abstract Awaitable FinishingSceneTransition();
        protected abstract float DelayBeforeFinishingTransition { get; }
#endregion
    }
}