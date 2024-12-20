using System;
using System.Linq;
using Unity.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WayfarerKit.Systems.SceneManagement
{
	/// <summary>
	///     Setup default behaviour in BootstrapperSettings scriptable object (which should be located in the root of Resources
	///     folder).
	///     This scene shouldn't have much content in it as it will persist throughout the game and never br unloaded.
	/// </summary>
	public static class BootstrapperRuntimeInitialize
    {
        private static GameObject _bootstrapperObject;
        public static bool IsSetupValid { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            //...
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Init()
        {
#if UNITY_EDITOR
            if (SceneManager.GetActiveScene().path.Contains("Plugins"))
            {
                Log.Warning($"Ignore BootstrapperRuntimeInitialize.Init for <color=red><b>{SceneManager.GetActiveScene().name}</b></color> scene as it's in Plugins folder.");

                return;
            }
#endif
            var bootstrapperSettings = BootstrapperSettings.Instance;

            if (bootstrapperSettings == null) throw new MissingReferenceException(" <color=red>BootstrapperRuntimeInitialize.Init -> BootstrapperSettings is not found</color>, use Assets menu to create new one.");

#if UNITY_EDITOR
            CheckBootstrapperHookExistence("", bootstrapperSettings.BootstrapperSceneName);
#endif

            IsSetupValid = true;

            Log.Debug("BootstrapperRuntimeInitialize.Init: Initializing <color=yellow><b>Wayfarer Scene Management System</b></color>");
            
            if ((!Application.isEditor || bootstrapperSettings.AlwaysStartWithBootstrapperScene) && SceneManager.GetActiveScene().name != bootstrapperSettings.BootstrapperSceneName)
            {
                //await Addressables.InitializeAsync().Task;
                await
                    SceneManager.LoadSceneAsync(bootstrapperSettings.BootstrapperSceneName, LoadSceneMode.Single);
            }

            InstantiateBootstrapper();
        }

        private static void InstantiateBootstrapper()
        {
            if (_bootstrapperObject != null) return;

            _bootstrapperObject = Object.Instantiate(BootstrapperSettings.Instance.BootstrapperPrefab);
            _bootstrapperObject.name = "Bootstrapper [runtime-initialize]";
        }

#if UNITY_EDITOR
        private static void CheckBootstrapperHookExistence(string message, string sceneName)
        {
            if (!IsSceneInBuildSettings(sceneName)) throw new MissingReferenceException($"<color=red>BootstrapperRuntimeInitialize.Init -> <b>{sceneName}</b> scene is not in the build settings</color>");

            if (BootstrapperSettings.Instance.BootstrapperPrefab == null) throw new MissingReferenceException("No bootstrapper prefab is set in BootstrapperSettings.");

            var components = BootstrapperSettings.Instance.BootstrapperPrefab.GetComponents<Component>();
            var found = components.Select(component => component.GetType())
                                  .Any(type => IsSubclassOfRawGeneric(typeof(BootAssistant<,>), type));

            if (!found) throw new MissingReferenceException("No BootAssistant<,> component is found in the bootstrapper prefab. Please add one...");
        }

        private static bool IsSceneInBuildSettings(string sceneName)
            => EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path.Contains($"/{sceneName}.unity"));

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (generic == cur) return true;
                toCheck = toCheck.BaseType;
            }

            return false;
        }
#endif
    }
}