using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MobSystem;

public class ListSpawner : MonoBehaviour
{
    [Header("List Spawner Settings")]
    [Tooltip("Spawn point main object")]
    [SerializeField] private GameObject spawnMainObject;

    [Tooltip("The list of places to spawn")]
    [SerializeField] private List<Transform> spawnPoints;

    [Tooltip("The prefab to spawn")]
    [SerializeField] private GameObject mobPrefab;

    [Tooltip("The number of places to spawn")]
    [SerializeField] private int numPlaces = 1;

    [Tooltip("The time between spawns")]
    [SerializeField] private float spawnInterval = 1f;

    [Tooltip("The time before the first spawn")]
    [SerializeField] private float firstSpawnDelay = 1f;

    [Tooltip("The time for playing spawn animation")]
    [SerializeField] private float spawnAnimationTime = 1f;


    // Animation parameters
    private readonly int isSpawningHash = Animator.StringToHash("IsSpawning");
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");

    // Audio parameters
    [Tooltip("The MobAudioManager instance")]
    [SerializeField] private MobAudioManager mobAudioManager;

    [Tooltip("The name of the spawn sound")]
    [SerializeField] private List<string> spawnSoundNames = new List<string> { "MobSpawn" };

    private void Initialize()
    {
        spawnPoints = new List<Transform>();
        foreach (Transform child in spawnMainObject.transform)
        {
            spawnPoints.Add(child);
        }
    }

    private void Start()
    {
        Initialize();
        StartCoroutine(SpawnMobs());
    }

    private IEnumerator SpawnMobs()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        // Select random spawn points
        List<Transform> selectedSpawnPoints = new List<Transform>();
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < numPlaces && availablePoints.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedSpawnPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        // Spawn mobs at selected points with interval
        foreach (Transform spawnPoint in selectedSpawnPoints)
        {
            if (mobPrefab != null && spawnPoint != null)
            {
                GameObject mob = Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation); 
                SetSpawnAnimationPlaying(mob, true);
                SetMobAIPaused(mob, true);
                SetPlayingMobAudio(mob, spawnSoundNames);
                SetMobScale(mob, Vector3.zero);
                yield return new WaitForSeconds(spawnAnimationTime);
                SetSpawnAnimationPlaying(mob, false);
                SetMobAIPaused(mob, false);
                SetMobScale(mob, Vector3.one);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SetPlayingMobAudio(GameObject mob, List<string> soundNames)
    {
        string soundName = "";
        if (soundNames.Count > 0)
        {
            int randomIndex = Random.Range(0, soundNames.Count);
            soundName = soundNames[randomIndex];
        }
        
        if (mob != null)
        {
            if (mobAudioManager != null && !string.IsNullOrEmpty(soundName)) mobAudioManager.PlayAudio3D(soundName, mob.transform.position);
        }
    }

    private void SetMobAIPaused(GameObject mob, bool isPaused)
    {
        if (mob != null)
        {
            MobAI mobAI = mob.GetComponent<MobAI>();
            if (mobAI != null) mobAI.SetPaused(isPaused);
        }
    }

    private void SetSpawnAnimationPlaying(GameObject mob, bool isPlaying)
    {
        if (mob != null)
        {
            Animator animator = mob.GetComponent<Animator>();
            if (animator != null) animator.SetBool(isSpawningHash, isPlaying);
        }
    }

    private void SetMobScale(GameObject mob, Vector3 scale)
    {
        if (mob != null)
        {
            mob.transform.localScale = scale;   
        }
    }
}