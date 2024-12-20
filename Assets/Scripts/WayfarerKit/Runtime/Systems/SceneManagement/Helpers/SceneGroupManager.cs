using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eflatun.SceneReference;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using WayfarerKit.Extensions;
using WayfarerKit.Helpers;
using WayfarerKit.Patterns.EventBus;
using WayfarerKit.Systems.SceneManagement.Helpers.BusEvents;

namespace WayfarerKit.Systems.SceneManagement.Helpers
{
    public enum SceneType
    {
        ActiveScene,
        UI,
        HUD,
        Environment,
        Tooling
    }

    public sealed class SceneGroupManager
    {
        private readonly AsyncOperationHandleGroup<SceneInstance> _assetLoadedScenesHandleGroup = new(10);

        public SceneGroup ActiveSceneGroup { get; private set; }

        public bool IsLoading { get; private set; }
        public bool DebugVerbatimLog { get; set; }

        public async Awaitable LoadGroup(SceneGroup group, IProgress<float> progress, bool reloadDuplicateScenes = true)
        {
            Assert.IsNotNull(group);

            if (IsLoading)
            {
                Log.Error($"SceneGroupManager.LoadGroup: Loading is already in progress for {ActiveSceneGroup.GroupName}. Skip for {group.GroupName} Please wait until it's finished...");

                return;
            }

            IsLoading = true;
            ActiveSceneGroup = group;

            var unloadWatch = Stopwatch.StartNew();

            await FindOrLoadBootstrapperScene();
            await UnloadScenes(ActiveSceneGroup, reloadDuplicateScenes);

            unloadWatch.Stop();
            EventBus<BootDumpPointEvent>.Raise(BootDumpPointEvent.With(group.ToString()));
            var loadWatch = Stopwatch.StartNew();

            var sceneDataToLoad = CollectSceneDataToLoadForGroup(ActiveSceneGroup);
            var operationGroup = new AsyncOperationGroup(sceneDataToLoad.Count);
            foreach (var sceneData in sceneDataToLoad)
            {
                switch (sceneData.Reference.State)
                {
                    case SceneReferenceState.Regular:
                    {
                        var operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                        operationGroup.AddOperation(operation);

                        break;
                    }
                    case SceneReferenceState.Addressable:
                    {
                        var sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                        _assetLoadedScenesHandleGroup.AddHandle(sceneHandle);

                        break;
                    }
                    case SceneReferenceState.Unsafe:
                    default:
                        continue;
                }

                if (DebugVerbatimLog) Log.Info($"<color=yellow>DEBUG:</color> SceneGroupManager.LoadGroup: Loading {sceneDataToLoad.Count} scenes for {sceneData.Name}");

#if UNITY_EDITOR
                await Awaitable.WaitForSecondsAsync(BootstrapperSettings.Instance.SceneSwitchDelay); // for testing purposes to see the loading screen
#endif
            }

            // report load progress
            while (!operationGroup.IsDone || !_assetLoadedScenesHandleGroup.IsDone)
            {
                progress?.Report((operationGroup.Progress + _assetLoadedScenesHandleGroup.Progress) / 2f);
                await Awaitable.EndOfFrameAsync();
            }

            // set the active scene
            var activeScene = SceneManager.GetSceneByName(ActiveSceneGroup.FindSceneNameByType(SceneType.ActiveScene));
            if (activeScene.IsValid()) SceneManager.SetActiveScene(activeScene);

            IsLoading = false;

            loadWatch.Stop();
            Log.Debug($"SceneGroupManager.LoadGroup: Unload scenes in {unloadWatch.ElapsedMilliseconds} ms. Load scenes in {loadWatch.ElapsedMilliseconds} ms.");
        }

        private static async Awaitable FindOrLoadBootstrapperScene()
        {
            var bootstrapperScene = SceneManager.GetSceneByName(BootstrapperSettings.Instance.BootstrapperSceneName);
            if (!bootstrapperScene.IsValid())
            {
                Log.Warning("SceneGroupManager.FindOrLoadBootstrapperScene: Bootstrapper scene is not loaded. Loading it now...");

                var operation = SceneManager.LoadSceneAsync(BootstrapperSettings.Instance.BootstrapperSceneReference.Path, LoadSceneMode.Additive);

                if (operation == null) return;

                while (!operation.isDone) await Awaitable.EndOfFrameAsync();
            }
        }

        private async Awaitable UnloadScenes(SceneGroup requestGroup, bool reloadDuplicateScenes = true)
        {
            Assert.IsTrue(IsLoading, "SceneGroupManager.UnloadScenes -> Invalid state. No loading in progress...");

            var scenes = CollectScenesToUnload(requestGroup, _assetLoadedScenesHandleGroup, reloadDuplicateScenes);

            var operationGroup = new AsyncOperationGroup(scenes.Count);
            foreach (var scene in scenes)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                operationGroup.AddOperation(operation);
            }

            var unloadHandles = new List<AsyncOperationHandle>();
            foreach (var handle in _assetLoadedScenesHandleGroup.Handles)
            {
                if (!handle.IsValid()) continue;

                var unloadHandle = Addressables.UnloadSceneAsync(handle);
                unloadHandles.Add(unloadHandle);
            }

            // clear the asset loaded scenes handle group
            _assetLoadedScenesHandleGroup.Clear();

            if (DebugVerbatimLog) Log.Info($"<color=yellow>DEBUG:</color> SceneGroupManager.LoadGroup: Unloading {operationGroup.Operations.Count} scenes");
            await AwaitableExtensions.WaitUntil(() => operationGroup.IsDone && unloadHandles.All(x => x.IsDone));

            // optional, but still worth to call
            await Resources.UnloadUnusedAssets();
        }

        private static IReadOnlyList<SceneData> CollectSceneDataToLoadForGroup(SceneGroup group)
        {
            Assert.IsNotNull(group);

            var alreadyLoadedScenes = GetCurrentLoadedScenes(out var isBootstrapperSceneLoaded);
            Assert.IsTrue(isBootstrapperSceneLoaded, "Fata error: Bootstrapper scene is not loaded...");

            var scenes = new List<SceneData>(group.Scenes);

            var i = 0;
            while (i < scenes.Count)
            {
                var sceneData = scenes[i];
                if (alreadyLoadedScenes.Contains(sceneData.Name))
                {
                    scenes.RemoveAt(i);

                    continue;
                }

                ++i;
            }

            return scenes;
        }

        private static IReadOnlyList<string> CollectScenesToUnload(SceneGroup requestGroup, AsyncOperationHandleGroup<SceneInstance> sceneInstantiatedHandleGroup, bool reloadDuplicateScenes)
        {
            Assert.IsNotNull(requestGroup);
            Assert.IsNotNull(sceneInstantiatedHandleGroup);

            var scenes = new List<string>();
            var sceneCount = SceneManager.sceneCount;

            for (var i = sceneCount - 1; i >= 0; --i)
            {
                // skip the bootstrapper scene to be sure that at least one scene is always loaded
                var sceneAt = SceneManager.GetSceneAt(i);

                if (!sceneAt.isLoaded ||
                    sceneAt.name == BootstrapperSettings.Instance.BootstrapperSceneName ||
                    !reloadDuplicateScenes && requestGroup.HasSceneWithName(sceneAt.name)) continue;

                // skip scenes that are loaded with addressables, as they will be unloaded separately from in-built scenes
                var sceneName = sceneAt.name;

                if (sceneInstantiatedHandleGroup.Handles.Any(handle => handle.IsValid() && handle.Result.Scene.name == sceneName)) continue;

                scenes.Add(sceneName);
            } //end for

            return scenes;
        }

        private static IReadOnlyList<string> GetCurrentLoadedScenes(out bool isBootstrapperSceneLoaded)
        {
            isBootstrapperSceneLoaded = false;

            var sceneCount = SceneManager.sceneCount;
            var loadedScenes = new List<string>(sceneCount);

            for (var i = 0; i < sceneCount; ++i)
            {
                var sceneName = SceneManager.GetSceneAt(i).name;
                isBootstrapperSceneLoaded = sceneName == BootstrapperSettings.Instance.BootstrapperSceneName
                    || isBootstrapperSceneLoaded;

                loadedScenes.Add(sceneName);
            }

            return loadedScenes;
        }
    }
}