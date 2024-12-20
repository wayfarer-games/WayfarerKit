using UnityEngine;

namespace WayfarerKit.Patterns.Singletons
{
    public abstract class PersistentSingleton<T> : BaseSingleton<T> where T : Component
    {
        protected override void Awake()
        {
            // we don't want to call base.Awake() here, as PersistentSingleton has another behavior while another instance is already exists
            // base.Awake();
            if (!Application.isPlaying) return;

            transform.SetParent(null);

            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);

                Initialize();
            }
            else
            {
                if (instance == this) return;

                //Log.Warning($"PersistentSingleton.Awake: [{typeof(T).Name}] already exists in the scene [{gameObject.scene.name}]. Destroying this instance.");
                Destroy(gameObject);
            }
        }
    }
}