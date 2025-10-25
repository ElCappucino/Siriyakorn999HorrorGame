using UnityEngine;
using AudioSystem;

namespace MobSystem
{
    /// <summary>
    /// Example weapon script that can damage mobs
    /// This is a basic raycast-based shooting system for reference
    /// </summary>
    public class ExampleWeapon : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [Tooltip("Damage dealt per shot")]
        [SerializeField] private float damage = 25f;
        
        [Tooltip("Maximum range of the weapon")]
        [SerializeField] private float range = 100f;
        
        [Tooltip("Fire rate (shots per second)")]
        [SerializeField] private float fireRate = 5f;
        
        [Tooltip("Layer mask for what can be hit")]
        [SerializeField] private LayerMask hitLayers = -1;

        [Header("References")]
        [Tooltip("Where bullets come from")]
        [SerializeField] private Transform firePoint;
        
        [Tooltip("Main camera (for aiming)")]
        [SerializeField] private Camera playerCamera;

        [Header("Effects")]
        [Tooltip("Muzzle flash effect")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        
        [Tooltip("Impact effect on hit")]
        [SerializeField] private GameObject impactEffectPrefab;
        
        [Tooltip("Blood effect when hitting enemy")]
        [SerializeField] private GameObject bloodEffectPrefab;

        [Header("Audio")]
        [Tooltip("Shoot sound name")]
        [SerializeField] private string shootSoundName = "WeaponShoot";
        
        [Tooltip("Randomize shoot pitch")]
        [SerializeField] private bool randomizePitch = true;
        
        [SerializeField] private float minPitch = 0.95f;
        [SerializeField] private float maxPitch = 1.05f;

        [Header("Debug")]
        [SerializeField] private bool showDebugRay = true;

        // Private variables
        private float nextFireTime = 0f;

        private void Start()
        {
            // Get camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            // Get fire point if not assigned
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            // Check for fire input
            bool fireInput = Input.GetButton("Fire1") || Input.GetMouseButton(0);

            if (fireInput && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }

        private void Fire()
        {
            // Play shoot sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(shootSoundName))
            {
                if (randomizePitch)
                {
                    AudioManager.instance.PlayRandomPitchAudio(shootSoundName, minPitch, maxPitch);
                }
                else
                {
                    AudioManager.instance.PlayAudio(shootSoundName);
                }
            }

            // Spawn muzzle flash
            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.1f);
            }

            // Raycast from camera center
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, hitLayers))
            {
                // Debug ray
                if (showDebugRay)
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
                }

                // Check if we hit a mob
                MobHealth mobHealth = hit.collider.GetComponent<MobHealth>();
                if (mobHealth != null)
                {
                    // Deal damage to the mob
                    mobHealth.TakeDamage(damage, firePoint.position);

                    // Spawn blood effect
                    if (bloodEffectPrefab != null)
                    {
                        GameObject blood = Instantiate(bloodEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(blood, 2f);
                    }
                }
                else
                {
                    // Spawn regular impact effect
                    if (impactEffectPrefab != null)
                    {
                        GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impact, 2f);
                    }
                }

                Debug.Log($"Hit: {hit.collider.name} at {hit.point}");
            }
            else if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * range, Color.yellow, 1f);
            }
        }

        /// <summary>
        /// Manually trigger a shot (useful for animations)
        /// </summary>
        public void Shoot()
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }

        /// <summary>
        /// Set weapon damage
        /// </summary>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Set fire rate
        /// </summary>
        public void SetFireRate(float newFireRate)
        {
            fireRate = newFireRate;
        }
    }
}

