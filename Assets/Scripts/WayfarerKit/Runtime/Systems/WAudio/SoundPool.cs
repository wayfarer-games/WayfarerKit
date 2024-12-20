using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using WayfarerKit.Patterns.Singletons;

namespace WayfarerKit.Systems.WAudio
{
    public class SoundPool : PersistentSingleton<SoundPool>
    {
        [SerializeField] private SoundEmitter emitterPrefabOrigin;
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private int maxSoundInstances = 20;
        private readonly List<SoundEmitter> _activeEmitters = new();
        private IObjectPool<SoundEmitter> _emitterPool;

        public readonly Dictionary<SoundData, int> Counts = new();
        protected override void Initialize() => InitializePool();

        private void InitializePool() =>
            _emitterPool = new ObjectPool<SoundEmitter>(
                CreateEmitter,
                OnTakeEmitterFromPool,
                OnReturnEmitterToPool,
                OnDestroyPooledEmitter,
                collectionCheck,
                defaultCapacity,
                maxPoolSize);

        public SoundBuilder CreateSound() => new(this);

        public bool CanPlay(SoundData data) => !Counts.TryGetValue(data, out var count) || count < maxSoundInstances;
        public SoundEmitter GetNext() => _emitterPool.Get();
        public void ReturnBack(SoundEmitter emitter)
        {
            emitter.ResetValues();
            _emitterPool.Release(emitter);
        }

#region IObjectPool
        private static void OnDestroyPooledEmitter(SoundEmitter emitter) => Destroy(emitter.gameObject);

        private void OnReturnEmitterToPool(SoundEmitter emitter)
        {
            if (Counts.TryGetValue(emitter.Data, out var count))
                Counts[emitter.Data] -= count > 0 ? 1 : 0;
            
            emitter.gameObject.SetActive(false);
            _activeEmitters.Remove(emitter);
        }

        private void OnTakeEmitterFromPool(SoundEmitter emitter)
        {
            emitter.gameObject.SetActive(true);
            _activeEmitters.Add(emitter);
        }

        private SoundEmitter CreateEmitter()
        {
            var emitterObj = Instantiate(emitterPrefabOrigin);
            emitterObj.gameObject.SetActive(false);

            return emitterObj;
        }
#endregion
    }
}