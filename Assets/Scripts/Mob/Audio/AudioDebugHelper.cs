using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Helper script to debug audio issues with mob spawner
    /// Attach this to your spawner to see what's happening
    /// </summary>
    public class AudioDebugHelper : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Sound name to test")]
        public string testSoundName = "MobPreSpawn";
        
        [Tooltip("Press this key to test the sound")]
        public KeyCode testKey = KeyCode.T;

        private void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                TestSound();
            }
        }

        public void TestSound()
        {
            Debug.Log("=== TESTING SOUND ===");
            
            // Check if MobAudioManager exists
            if (MobAudioManager.instance == null)
            {
                Debug.LogError("❌ MobAudioManager.instance is NULL! MobAudioManager not in scene or not initialized!");
                return;
            }
            else
            {
                Debug.Log("✅ MobAudioManager found!");
            }

            // Check if sound exists in array
            bool soundFound = false;
            if (MobAudioManager.instance.SFXSounds != null)
            {
                Debug.Log($"Checking {MobAudioManager.instance.SFXSounds.Length} sounds in SFX array...");
                
                for (int i = 0; i < MobAudioManager.instance.SFXSounds.Length; i++)
                {
                    AudioSystem.Sound sound = MobAudioManager.instance.SFXSounds[i];
                    Debug.Log($"Sound {i}: Name='{sound.name}', Clip={(sound.audioClip != null ? sound.audioClip.name : "NULL")}");
                    
                    if (sound.name == testSoundName)
                    {
                        soundFound = true;
                        Debug.Log($"✅ FOUND '{testSoundName}' at index {i}!");
                        
                        if (sound.audioClip == null)
                        {
                            Debug.LogError($"❌ Sound '{testSoundName}' has NULL audio clip!");
                        }
                        else
                        {
                            Debug.Log($"✅ Audio clip assigned: {sound.audioClip.name}");
                        }
                    }
                }
                
                if (!soundFound)
                {
                    Debug.LogError($"❌ Sound '{testSoundName}' NOT FOUND in SFX array!");
                    Debug.LogError("Make sure the sound name matches EXACTLY (case-sensitive, no extra spaces)");
                }
            }
            else
            {
                Debug.LogError("❌ SFXSounds array is NULL!");
            }

            // Try to play the sound
            Debug.Log($"Attempting to play sound '{testSoundName}'...");
            MobAudioManager.instance.PlayAudio(testSoundName);
            Debug.Log("Play command sent. If you don't hear anything, check the errors above.");
        }

        private void OnDrawGizmos()
        {
            // Draw a label in scene view
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, 
                $"Press {testKey} to test\n'{testSoundName}' sound");
            #endif
        }
    }
}

