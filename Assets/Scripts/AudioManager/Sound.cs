using UnityEngine.Audio;
using UnityEngine;

namespace AudioSystem
{
    [System.Serializable]
    public class Sound
    {
        [Header("-----Sound Data Settings-----")]
        public string name;
        public AudioClip audioClip;
        [Range(0.0f, 1.0f)] public float volume = 1.0f;
        [Range(0.1f, 3.0f)] public float pitch = 1.0f;
        [Range(-1.0f, 1.0f)] public float stereoPan = 0.0f;
        public bool loop;

        [HideInInspector] public AudioSource source;
    }
}

