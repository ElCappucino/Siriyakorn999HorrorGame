using UnityEngine;
using System.Collections;

namespace MobSystem
{
    /// <summary>
    /// Vampire Long Hair behavior: Bat that transforms immediately to humanoid
    /// No need to get close to player - transforms right away after spawn
    /// Combines electricity and bat sounds
    /// </summary>
    public class VampireLongHairBehavior : MobBehavior
    {
        [Header("Vampire Long Hair Settings")]
        [SerializeField] private GameObject batModel;
        [SerializeField] private GameObject humanoidModel;
        [SerializeField] private float flyHeight = 2.5f;
        [SerializeField] private float transformDelay = 1.5f;
        [SerializeField] private GameObject transformEffect;
        [SerializeField] private GameObject electricityEffect;

        private bool isInBatForm = true;
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
            // Play bat sound with electricity
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("BatWings", gameObject);
                MobAudioManager.instance.PlayAudio3DAttached("ElectricityAmbient", gameObject);
            }

            // Transform immediately after a short delay
            StartCoroutine(TransformImmediately());
        }

        private IEnumerator TransformImmediately()
        {
            // Brief bat flight
            yield return new WaitForSeconds(transformDelay);

            // Transform without needing to be close to player
            yield return StartCoroutine(TransformToHumanoid());
        }

        private IEnumerator TransformToHumanoid()
        {
            // Play transformation sound with electricity
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("VampireTransform", transform.position);
                MobAudioManager.instance.PlayAudio3D("ElectricityZap", transform.position);
            }

            // Spawn transformation effect
            if (transformEffect != null)
            {
                GameObject effect = Instantiate(transformEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Spawn electricity effect
            if (electricityEffect != null)
            {
                GameObject elecEffect = Instantiate(electricityEffect, transform.position, Quaternion.identity, transform);
                Destroy(elecEffect, 2f);
            }

            // Brief pause
            yield return new WaitForSeconds(0.5f);

            // Switch models
            if (batModel != null) batModel.SetActive(false);
            if (humanoidModel != null) humanoidModel.SetActive(true);
            isInBatForm = false;
            hasTransformed = true;

            // Play vampire roar
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("VampireRoar", gameObject);
            }
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Flying behavior when in bat form (brief)
            if (isInBatForm)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, player.position.y + flyHeight, Time.deltaTime * 2f);
                transform.position = pos;
            }
        }

        public override void OnStartAttacking()
        {
            // Play electricity surge on attack
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("ElectricityZap", gameObject);
            }

            if (electricityEffect != null)
            {
                GameObject effect = Instantiate(electricityEffect, transform.position, Quaternion.identity, transform);
                Destroy(effect, 1f);
            }
        }

        public override bool CustomMovement()
        {
            // Use custom flying movement only in bat form
            return isInBatForm;
        }

        public override float GetSpeedMultiplier()
        {
            // Normal speed in humanoid form
            return isInBatForm ? 1.2f : 1f;
        }
    }
}

