using Unity.Logging;
using UnityEngine;

namespace WayfarerKit.Systems.WAudio
{
    public class SoundBuilder
    {
        private readonly SoundPool _soundPool;
        private Vector3 _position = Vector3.zero;
        private bool _randomPitch;
        private float _shiftPitch;
        private SoundData _soundData;

        public SoundBuilder(SoundPool soundPool) => _soundPool = soundPool;

        public SoundBuilder WithSoundData(SoundData soundData)
        {
            _soundData = soundData;
            return this;
        }

        public SoundBuilder WithWorldPosition(Vector3 position)
        {
            _position = position;
            return this;
        }

        public SoundBuilder WithRandomPitch()
        {
            _randomPitch = true;
            return this;
        }
        public SoundBuilder WithShiftPitch(float shift)
        {
            _shiftPitch = shift;
            return this;
        }
        

        public void Play()
        {
            if (!_soundPool.CanPlay(_soundData))
            {
                Log.Warning($"Can't play sound {_soundData.clip.name} because the sound pool is full.");
                return;
            }

            var emitter = _soundPool.GetNext();
            emitter.Initialize(_soundData);
            emitter.transform.position = _position;
            emitter.transform.parent = _soundPool.transform;

            if (_randomPitch)
                emitter.WithRandomPitch();
            
            if (_shiftPitch != 0f)
                emitter.WithPitchShift(_shiftPitch);

            _soundPool.Counts[_soundData] =
                _soundPool.Counts.TryGetValue(_soundData, out var count) ? count + 1 : 1;
            
            emitter.Play();
        }
    }
}