using UnityEngine;
using UnityEngine.Audio;

namespace WayfarerKit.Systems.WAudio
{
    [CreateAssetMenu(fileName = "Sound Data", menuName = "Wayfarer/SoundData")]
    public class SoundData: ScriptableObject
    {
        public AudioClip clip;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;
    }
}
