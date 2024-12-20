using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WayfarerKit.Extensions;
using WayfarerKit.Patterns.Locator.Helpers;
using ArgumentException = System.ArgumentException;

namespace WayfarerKit.Patterns.Locator
{
	/// <summary>
	///     Three layers of service locators are supported:
	///     - Global (one per project)
	///     - Scene (will be alive as long as the scene is loaded)
	///     - GameObject (will be alive as long as the object is alive)
	///     If the service for GameObject is not found, lookup will continue for Scene and after in Global register.
	///     Registration can be done in any place and order (ServiceLocator.Global.Register).
	/// </summary>
	public class ServiceLocator : MonoBehaviour
    {
        private const string GlobalServiceLocatorName = "ServiceLocator [Global]";
        private const string SceneServiceLocatorName = "ServiceLocator [Scene]";

        private static ServiceLocator _global;
        private static Dictionary<Scene, ServiceLocator> _sceneContainers;
        private static List<GameObject> _tmpSceneGameObjects;
        private static bool _isQuitting;

        private readonly ServiceStorage _services = new();
        

        public static ServiceLocator Global
        {
            get
            {
                if (_global != null) return _global;

                if (_isQuitting) return null;
                if (FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>() is {} found)
                {
                    found.BootstrapOnDemand();

                    return _global;
                }

                var container = new GameObject(GlobalServiceLocatorName, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();
                container.name = "ServiceLocator [Global] [Generated]";
                DontDestroyOnLoad(container);

                return _global;
            }
        }

        private void OnDestroy()
        {
            if (this == _global)
            {
                _isQuitting = true;
                _global = null;
            }
            else if (_sceneContainers != null && _sceneContainers.ContainsValue(this)) _sceneContainers.Remove(gameObject.scene);
        }

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (_global == this) Log.Warning("ServiceLocator.ConfigureAsGlobal: Already configured as global");
            else if (_global != null) Log.Warning("ServiceLocator.ConfigureAsGlobal: Another ServiceLocator is already configured as global");
            else
            {
                _global = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

                Log.Debug($"ServiceLocator.ConfigureAsGlobal: Configuring as global with object <color=yellow>{name}</color>");
            }
        }

        internal void ConfigureForScene()
        {
            var scene = gameObject.scene;

            if (_sceneContainers.ContainsKey(scene))
            {
                Log.Error($"ServiceLocator.ConfigureForScene: Another ServiceLocator is already configured for this scene {scene.name}");

                return;
            }

            _sceneContainers.Add(scene, this);
            Log.Debug($"ServiceLocator.ConfigureForScene: Configuring for scene <color=yellow>{scene.name}</color> with object <color=yellow>{name}</color>");
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour mb)
        {
            var scene = mb.gameObject.scene;

            if (_sceneContainers.TryGetValue(scene, out var container) && container != mb) return container;

            _tmpSceneGameObjects.Clear();
            scene.GetRootGameObjects(_tmpSceneGameObjects);

            foreach (var go in _tmpSceneGameObjects.Where(go => go.GetComponent<ServiceLocatorSceneBootstrapper>() != null))
            {
                if (go.TryGetComponent(out ServiceLocatorSceneBootstrapper bootstrapper) && bootstrapper.Container != mb)
                {
                    bootstrapper.BootstrapOnDemand();

                    return bootstrapper.Container;
                }
            }

            return Global;
        }

        public static ServiceLocator For(MonoBehaviour mb)
            => mb.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(mb) ?? Global;

        public ServiceLocator Register<T>(T service)
        {
            _services.Register(service);

            return this;
        }

        public ServiceLocator Register(Type type, object service)
        {
            _services.Register(type, service);

            return this;
        }

        public ServiceLocator Get<T>(out T service) where T : class
        {
            if (TryGetService(out service)) return this;

            if (!TryGetNextInHierarchy(out var container)) throw new ArgumentException($"ServiceLocator.Get: Service of type {typeof(T).FullName} not registered");

            container.Get(out service);

            return this;
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);

            if (TryGetService(type, out T service)) return service;
            if (TryGetNextInHierarchy(out var container)) return container.Get<T>();

            throw new ArgumentException($"Could not resolve type '{typeof(T).FullName}'.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            service = null;

            if (TryGetService(type, out service)) return true;

            return TryGetNextInHierarchy(out var container) && container.TryGet(out service);
        }

        private bool TryGetService<T>(out T service) where T : class => _services.TryGet(out service);
        private bool TryGetService<T>(Type type, out T service) where T : class => _services.TryGet(out service);

        private bool TryGetNextInHierarchy(out ServiceLocator container)
        {
            if (this == _global)
            {
                container = null;

                return false;
            }

            container = transform.parent.OrNull()?
                                 .GetComponentInParent<ServiceLocator>()
                                 .OrNull()
                ?? ForSceneOf(this);

            return container != null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _global = null;
            _sceneContainers = new();
            _tmpSceneGameObjects = new();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Service Locator/Add Global")]
        private static void AddGlobal()
        {
            // ReSharper disable once UnusedVariable
            var go = new GameObject(GlobalServiceLocatorName, typeof(ServiceLocatorGlobalBootstrapper));
        }

        [MenuItem("GameObject/Service Locator/Add Scene")]
        private static void AddScene()
        {
            // ReSharper disable once UnusedVariable
            var go = new GameObject(SceneServiceLocatorName, typeof(ServiceLocatorSceneBootstrapper));
        }
#endif
    }
}