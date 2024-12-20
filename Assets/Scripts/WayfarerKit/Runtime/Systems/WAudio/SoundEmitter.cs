using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace WayfarerKit.Systems.WAudio
{
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private Coroutine _playingCoroutine;

        public SoundData Data { get; private set; }
        public AudioSource Source => audioSource;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            Assert.IsNotNull(audioSource, "audioSour ce != null");
        }

        public void Play()
        {
            if (_playingCoroutine != null)
                StopCoroutine(_playingCoroutine);

            audioSource.Play();
            _playingCoroutine = StartCoroutine(PlayRoutine());
        }

        public void Stop()
        {
            if (_playingCoroutine != null)
            {
                StopCoroutine(_playingCoroutine);
                _playingCoroutine = null;
            }

            audioSource.Stop();
            SoundPool.Instance.ReturnBack(this);
        }

        public void ResetValues() => audioSource.pitch = 1f;

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f) => audioSource.pitch += Random.Range(min, max);
        public void WithPitchShift(float shift = 0f) => audioSource.pitch = 1f + shift;

        private IEnumerator PlayRoutine()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            SoundPool.Instance.ReturnBack(this);
        }

        public void Initialize(SoundData data)
        {
            Data = data;

            audioSource.clip = data.clip;
            audioSource.outputAudioMixerGroup = data.mixerGroup;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;
        }
    }
}