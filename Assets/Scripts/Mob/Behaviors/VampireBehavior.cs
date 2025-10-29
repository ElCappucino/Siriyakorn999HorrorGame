using UnityEngine;
using System.Collections;

namespace MobSystem
{
    /// <summary>
    /// Vampire behavior: Bat form that transforms to humanoid when close to player
    /// Creates jumpscare effect on transformation
    /// </summary>
    public class VampireBehavior : MobBehavior
    {
        [Header("Vampire Settings")]
        [SerializeField] private GameObject batModel;
        [SerializeField] private GameObject humanoidModel;
        [SerializeField] private float flyHeight = 2f;
        [SerializeField] private float transformEffectDuration = 0.5f;
        [SerializeField] private GameObject transformEffect;

        private bool isInBatForm = true;
        private bool isTransforming = false;
        private bool hasTransformed = false;

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            
            // Start in bat form
            if (batModel != null) batModel.SetActive(true);
            if (humanoidModel != null) humanoidModel.SetActive(false);
        }

        public override void OnSpawned()
        {
            // Play bat wing flapping sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("BatWings", gameObject);
            }
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Check if close enough to transform
            if (isInBatForm && !isTransforming && !hasTransformed)
            {
                if (distanceToPlayer <= mobData.transformationDistance)
                {
                    StartCoroutine(TransformToBat());
                }
            }

            // Flying behavior when in bat form
            if (isInBatForm)
            {
                // Maintain flying height
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, player.position.y + flyHeight, Time.deltaTime * 2f);
                transform.position = pos;
            }
        }

        private IEnumerator TransformToBat()
        {
            isTransforming = true;

            // Play transformation sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("VampireTransform", transform.position);
            }

            // Spawn transformation effect
            if (transformEffect != null)
            {
                GameObject effect = Instantiate(transformEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Brief pause (jumpscare moment)
            yield return new WaitForSeconds(transformEffectDuration);

            // Switch models
            if (batModel != null) batModel.SetActive(false);
            if (humanoidModel != null) humanoidModel.SetActive(true);
            isInBatForm = false;
            hasTransformed = true;

            // Trigger jumpscare if player is looking
            if (mobData.hasJumpscare)
            {
                TriggerJumpscare();
            }

            // Play vampire humanoid sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("VampireRoar", gameObject);
            }

            isTransforming = false;
        }

        private void TriggerJumpscare()
        {
            // TODO: Trigger jumpscare system
            Debug.Log("JUMPSCARE!");
        }

        public override bool CustomMovement()
        {
            // Use custom flying movement in bat form
            return isInBatForm;
        }

        public override float GetSpeedMultiplier()
        {
            // Faster in bat form, normal in humanoid form
            return isInBatForm ? 1.3f : 1f;
        }
    }
}

