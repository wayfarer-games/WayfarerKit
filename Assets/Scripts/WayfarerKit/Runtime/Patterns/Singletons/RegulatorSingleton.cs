using Unity.Logging;
using UnityEngine;

namespace WayfarerKit.Patterns.Singletons
{
    public abstract class RegulatorSingleton<T> : BaseSingleton<T> where T : Component
    {
        private float InitializationTime { get; set; }

        protected override void Awake()
        {
            // We don't want to call base.Awake() here, as PersistentSingleton has another behavior while another instance is already exists
            // base.Awake();
            if (!Application.isPlaying) return;

            transform.SetParent(null);

            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);

            var oldInstances = FindObjectsByType(typeof(T), FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var old in oldInstances)
            {
                var go = old as MonoBehaviour;
                if (go.gameObject.GetComponent<RegulatorSingleton<T>>().InitializationTime >= InitializationTime)
                {
                    continue;
                }

                if (instance == old)
                {
                    instance = null;
                }
                
                Log.Warning($"RegulatorSingleton.Awake: {typeof(T).Name} already exists in the scene. Destroying this instance.");
                Destroy(go.gameObject);
            }

            if (instance == null)
            {
                instance = this as T;

                Initialize();
            }
        }
    }
}