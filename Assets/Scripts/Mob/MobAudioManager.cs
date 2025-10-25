using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using AudioSystem;

namespace MobSystem
{
    public class MobAudioManager : MonoBehaviour
    {
        public static MobAudioManager instance;

        [Header("-----Object References-----")]
        [SerializeField] private GameObject BGMHolder;
        [SerializeField] private GameObject SFXHolder;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup BGMAudioMixer;
        [SerializeField] private AudioMixerGroup SFXAudioMixer;

        [Header("-----BGM Settings-----")]
        [HideInInspector] public int[] bGMIndex;
        [SerializeField] private bool PlayOnStart = true;

        [Header("-----Sound Banks-----")]
        public AudioSystem.Sound[] BGMSounds;
        public AudioSystem.Sound[] SFXSounds;

        [Header("-----3D Audio Settings-----")]
        [Tooltip("Default minimum distance for 3D sounds")]
        [SerializeField] private float default3DMinDistance = 1f;
        
        [Tooltip("Default maximum distance for 3D sounds")]
        [SerializeField] private float default3DMaxDistance = 15f;
        
        [Tooltip("Spatial blend (0=2D, 1=3D)")]
        [SerializeField][Range(0f, 1f)] private float spatialBlend = 1f;

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

            foreach (AudioSystem.Sound s in BGMSounds)
            {
                s.source = BGMHolder.AddComponent<AudioSource>();
                s.source.clip = s.audioClip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.panStereo = s.stereoPan;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = BGMAudioMixer;
                s.source.playOnAwake = false;
            }

            foreach (AudioSystem.Sound s in SFXSounds)
            {
                s.source = SFXHolder.AddComponent<AudioSource>();
                s.source.clip = s.audioClip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.panStereo = s.stereoPan;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = SFXAudioMixer;
                s.source.playOnAwake = false;
            }
        }

        public void SwitchBGM(string name)
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.SwitchBGM("name");
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(BGMSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            if (s.source.isPlaying)
            {
                return;
            }

            StartCoroutine(AudioSystem.BGMAudioFade.CrossFade(audioMixer, 1.5f, s.source, BGMSounds));
        }

        public void SwitchBGM(int index)
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.SwitchBGM(index);
            // ------------------------------------------------
            if (index == 0)
            {
                StopBGM();
                return;
            }

            AudioSystem.Sound s = BGMSounds[index - 1];
            if (s == null)
            {
                Debug.LogWarning("Sound: " + (index - 1) + " not found!");
                return;
            }
            if (s.source.isPlaying)
            {
                return;
            }

            StartCoroutine(AudioSystem.BGMAudioFade.CrossFade(audioMixer, 1.5f, s.source, BGMSounds));
        }

        public void StopBGM()
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.StopBGM();
            // ------------------------------------------------

            StartCoroutine(AudioSystem.BGMAudioFade.CrossFade(audioMixer, 1.5f, null, BGMSounds));
        }

        public void StopBGM(string name)
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.StopBGM("name");
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(BGMSounds, sound => sound.name == name);
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
            // MobAudioManager.instance.PlayAudio("name");
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.pitch = s.pitch;
            s.source.Play();
        }

        /// <summary>
        /// Play a 3D sound at a specific world position (creates temporary AudioSource)
        /// </summary>
        public void PlayAudio3D(string name, Vector3 position)
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.PlayAudio3D("name", transform.position);
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            
            if (s.audioClip == null)
            {
                Debug.LogWarning("Sound: " + name + " has no audio clip assigned!");
                return;
            }

            // Create temporary GameObject for 3D audio
            GameObject audioObj = new GameObject($"TempAudio_{name}");
            audioObj.transform.position = position;
            
            AudioSource tempSource = audioObj.AddComponent<AudioSource>();
            tempSource.clip = s.audioClip;
            tempSource.volume = s.volume;
            tempSource.pitch = s.pitch;
            tempSource.loop = false;
            tempSource.outputAudioMixerGroup = SFXAudioMixer;
            tempSource.spatialBlend = spatialBlend;
            tempSource.minDistance = default3DMinDistance;
            tempSource.maxDistance = default3DMaxDistance;
            tempSource.Play();
            
            // Destroy after clip finishes
            Destroy(audioObj, s.audioClip.length + 0.1f);
        }

        /// <summary>
        /// Play a 3D sound attached to a GameObject (enemy, player, etc.)
        /// The sound will follow the object as it moves
        /// </summary>
        public AudioSource PlayAudio3DAttached(string name, GameObject attachTo)
        {
            // ---------------- Calling Method ----------------
            // AudioSource source = MobAudioManager.instance.PlayAudio3DAttached("name", enemyGameObject);
            // Store the returned AudioSource if you need to control it later
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return null;
            }
            
            if (s.audioClip == null)
            {
                Debug.LogWarning("Sound: " + name + " has no audio clip assigned!");
                return null;
            }

            // Create child GameObject for audio
            GameObject audioObj = new GameObject($"Audio_{name}");
            audioObj.transform.SetParent(attachTo.transform);
            audioObj.transform.localPosition = Vector3.zero;
            
            AudioSource attachedSource = audioObj.AddComponent<AudioSource>();
            attachedSource.clip = s.audioClip;
            attachedSource.volume = s.volume;
            attachedSource.pitch = s.pitch;
            attachedSource.loop = s.loop;
            attachedSource.outputAudioMixerGroup = SFXAudioMixer;
            attachedSource.spatialBlend = spatialBlend;
            attachedSource.minDistance = default3DMinDistance;
            attachedSource.maxDistance = default3DMaxDistance;
            attachedSource.Play();
            
            // If not looping, destroy after clip finishes
            if (!s.loop)
            {
                Destroy(audioObj, s.audioClip.length + 0.1f);
            }
            
            return attachedSource;
        }

        /// <summary>
        /// Play a 3D sound at a specific position with custom distance settings
        /// </summary>
        public void PlayAudio3DCustom(string name, Vector3 position, float minDistance, float maxDistance)
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.PlayAudio3DCustom("name", transform.position, 2f, 20f);
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(SFXSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            
            if (s.audioClip == null)
            {
                Debug.LogWarning("Sound: " + name + " has no audio clip assigned!");
                return;
            }

            GameObject audioObj = new GameObject($"TempAudio_{name}");
            audioObj.transform.position = position;
            
            AudioSource tempSource = audioObj.AddComponent<AudioSource>();
            tempSource.clip = s.audioClip;
            tempSource.volume = s.volume;
            tempSource.pitch = s.pitch;
            tempSource.loop = false;
            tempSource.outputAudioMixerGroup = SFXAudioMixer;
            tempSource.spatialBlend = spatialBlend;
            tempSource.minDistance = minDistance;
            tempSource.maxDistance = maxDistance;
            tempSource.Play();
            
            Destroy(audioObj, s.audioClip.length + 0.1f);
        }

        public void PlayRandomPitchAudio(string name, float min, float max)
        {
            // ---------------- Calling Method ----------------
            // MobAudioManager.instance.PlayRandomPitchAudio("name", Min, Max);
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(SFXSounds, sound => sound.name == name);
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
            // MobAudioManager.instance.StopAudio("name");
            // ------------------------------------------------

            AudioSystem.Sound s = Array.Find(SFXSounds, sound => sound.name == name);
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
            // MobAudioManager.instance.StopAllAudio();
            // ------------------------------------------------

            foreach (AudioSystem.Sound s in SFXSounds)
            {
                s.source.Stop();
            }
        }

        public void ToggleBGM()
        {
            foreach (AudioSystem.Sound s in BGMSounds)
            {
                s.source.mute = !s.source.mute;
            }
        }

        public void SetBGMVolume(float volume)
        {
            foreach (AudioSystem.Sound s in BGMSounds)
            {
                s.source.volume = s.volume * volume;
            }
        }

        public void ToggleSFX()
        {
            foreach (AudioSystem.Sound s in SFXSounds)
            {
                s.source.mute = !s.source.mute;
            }
        }

        public void SetSFXVolume(float volume)
        {
            foreach (AudioSystem.Sound s in SFXSounds)
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
            if (PlayOnStart)
            {
                HandleSceneBGM(SceneManager.GetActiveScene().buildIndex);
            }
        }

        // Update is called once per frame
        void Update() { }
    }
}
