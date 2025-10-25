using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace AudioSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [Header("-----Object References-----")]
        [SerializeField] private GameObject BGMHolder;
        [SerializeField] private GameObject SFXHolder;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup BGMAudioMixer;
        [SerializeField] private AudioMixerGroup SFXAudioMixer;

        [Header("-----BGM Settings-----")]
        [HideInInspector] public int[] bGMIndex;

        [Header("-----Sound Banks-----")]
        public Sound[] BGMSounds;
        public Sound[] SFXSounds;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            foreach (Sound s in BGMSounds)
            {
                s.source = BGMHolder.AddComponent<AudioSource>();
                s.source.clip = s.audioClip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.panStereo = s.stereoPan;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = BGMAudioMixer;
            }

            foreach (Sound s in SFXSounds)
            {
                s.source = SFXHolder.AddComponent<AudioSource>();
                s.source.clip = s.audioClip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.panStereo = s.stereoPan;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = SFXAudioMixer;
            }
        }

        public void SwitchBGM(string name)
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.SwitchBGM("name");
            // ------------------------------------------------

            Sound s = Array.Find(BGMSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            if (s.source.isPlaying)
            {
                return;
            }

            StartCoroutine(BGMAudioFade.CrossFade(audioMixer, 1.5f, s.source, BGMSounds));
        }

        public void SwitchBGM(int index)
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.SwitchBGM(index);
            // ------------------------------------------------
            if (index == 0)
            {
                StopBGM();
                return;
            }

            Sound s = BGMSounds[index - 1];
            if (s == null)
            {
                Debug.LogWarning("Sound: " + (index - 1) + " not found!");
                return;
            }
            if (s.source.isPlaying)
            {
                return;
            }

            StartCoroutine(BGMAudioFade.CrossFade(audioMixer, 1.5f, s.source, BGMSounds));
        }

        public void StopBGM()
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.StopBGM();
            // ------------------------------------------------

            StartCoroutine(BGMAudioFade.CrossFade(audioMixer, 1.5f, null, BGMSounds));
        }

        public void StopBGM(string name)
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.StopBGM("name");
            // ------------------------------------------------

            Sound s = Array.Find(BGMSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.Stop();
        }

        public void PlayAudio(string name)
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.PlayAudio("name");
            // ------------------------------------------------

            Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.pitch = s.pitch;
            s.source.Play();
        }

        public void PlayRandomPitchAudio(string name, float min, float max)
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.PlayRandomPitchAudio("name", Min, Max);
            // ------------------------------------------------

            Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.pitch = UnityEngine.Random.Range(min, max);
            s.source.Play();
        }

        public void StopAudio(string name)
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.StopAudio("name");
            // ------------------------------------------------

            Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.Stop();
        }

        public void StopAllAudio()
        {
            // ---------------- Calling Method ----------------
            // AudioManager.instance.StopAllAudio();
            // ------------------------------------------------

            foreach (Sound s in SFXSounds)
            {
                s.source.Stop();
            }
        }

        public void ToggleBGM()
        {
            foreach (Sound s in BGMSounds)
            {
                s.source.mute = !s.source.mute;
            }
        }

        public void SetBGMVolume(float volume)
        {
            foreach (Sound s in BGMSounds)
            {
                s.source.volume = s.volume * volume;
            }
        }

        public void ToggleSFX()
        {
            foreach (Sound s in SFXSounds)
            {
                s.source.mute = !s.source.mute;
            }
        }

        public void SetSFXVolume(float volume)
        {
            foreach (Sound s in SFXSounds)
            {
                s.source.volume = s.volume * volume;
            }
        }

        public void HandleSceneBGM(int scene_Index)
        {
            SwitchBGM(bGMIndex[scene_Index]);
        }

        // Start is called before the first frame update
        void Start()
        {
            HandleSceneBGM(SceneManager.GetActiveScene().buildIndex);
        }

        // Update is called once per frame
        void Update() { }
    }
}

