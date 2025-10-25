using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    public static class BGMAudioFade
    {
        public static IEnumerator CrossFade(AudioMixer audiomixer, float duration, AudioSource source, Sound[] bgm_sounds)
        {
            float currentTime = 0;
            audiomixer.GetFloat("bGMVolume", out float currentVol);
            currentVol = Mathf.Pow(10, currentVol / 20);
            float originVol = currentVol;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVol = Mathf.Lerp(currentVol, 0.0001f, currentTime / duration);
                audiomixer.SetFloat("bGMVolume", Mathf.Log10(newVol) * 20);
                yield return null;
            }
            foreach (Sound s in bgm_sounds)
            {
                s.source.Stop();
            }
            if (source != null)
            {
                source.Play();
            }
            currentTime = 0;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVol = Mathf.Lerp(currentVol, originVol, currentTime / duration);
                audiomixer.SetFloat("bGMVolume", Mathf.Log10(newVol) * 20);
                yield return null;
            }
            yield break;
        }
    }
}

