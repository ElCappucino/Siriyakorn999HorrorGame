using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MobSystem;

public class ListSpawner : MonoBehaviour
{
    [Header("-----Spawner Identity-----")]
    [Tooltip("Unique identifier for this spawner")]
    [SerializeField] private string spawnerID = "ListSpawner_1";
    
    [Tooltip("Description of what this spawner is for")]
    [SerializeField] private string spawnerDescription = "Main entrance spawner";

    [Header("-----List Spawner Settings-----")]
    [Tooltip("Spawn point main object")]
    [SerializeField] private GameObject spawnMainObject;

    [Tooltip("The list of places to spawn")]
    [SerializeField] private List<Transform> spawnPoints;

    [Tooltip("The list of mob prefabs that can spawn here")]
    [SerializeField] private List<GameObject> mobPrefabs = new List<GameObject>();

    [Tooltip("The time for playing spawn animation")]
    [SerializeField] private float spawnAnimationTime = 1f;

    [Header("-----Audio Parameters-----")]
    [Tooltip("The MobAudioManager instance")]
    [SerializeField] private MobAudioManager mobAudioManager;

    [Tooltip("The names of the spawn sounds")]
    [SerializeField] private List<string> spawnSoundNames = new List<string> { "MobSpawn" };

    [Header("-----Animation Parameters-----")]
    private readonly int isSpawningHash = Animator.StringToHash("IsSpawning");
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");

    // Internal state
    private bool isInitialized = false;

    /// <summary>
    /// Initialize spawn points from main object
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        spawnPoints = new List<Transform>();
        
        if (spawnMainObject != null)
        {
            foreach (Transform child in spawnMainObject.transform)
            {
                spawnPoints.Add(child);
            }
        }
        
        isInitialized = true;
        
        Debug.Log($"ListSpawner '{spawnerID}': Initialized with {spawnPoints.Count} spawn points and {mobPrefabs.Count} mob types");
    }

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Get a random spawn point from this spawner
    /// </summary>
    public Transform GetRandomSpawnPoint()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"ListSpawner '{spawnerID}': No spawn points available!");
            return null;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    /// <summary>
    /// Spawn a specific mob prefab at a random spawn point with animation
    /// Called by MobSpawnManager
    /// </summary>
    public GameObject SpawnMob(GameObject mobPrefab)
    {
        if (mobPrefab == null)
        {
            Debug.LogWarning($"ListSpawner '{spawnerID}': Mob prefab is null!");
            return null;
        }

        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
        {
            return null;
        }

        return SpawnMobAt(mobPrefab, spawnPoint);
    }

    /// <summary>
    /// Spawn a mob at a specific spawn point with animation
    /// </summary>
    public GameObject SpawnMobAt(GameObject mobPrefab, Transform spawnPoint)
    {
        if (mobPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning($"ListSpawner '{spawnerID}': Invalid spawn parameters!");
            return null;
        }

        GameObject mob = Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation);
        StartCoroutine(PlaySpawnAnimation(mob));
        
        return mob;
    }

    /// <summary>
    /// Spawn animation coroutine
    /// </summary>
    private IEnumerator PlaySpawnAnimation(GameObject mob)
    {
        if (mob == null) yield break;

        // Store original scale
        Vector3 originalScale = mob.transform.localScale;

        // Start spawn animation
        SetSpawnAnimationPlaying(mob, true);
        SetMobAIPaused(mob, true);
        SetPlayingMobAudio(mob, spawnSoundNames);
        SetMobScale(mob, Vector3.zero);

        // Wait for animation
        yield return new WaitForSeconds(spawnAnimationTime);

        // End spawn animation
        SetSpawnAnimationPlaying(mob, false);
        SetMobAIPaused(mob, false);

        // Restore original prefab scale
        SetMobScale(mob, originalScale);
    }


    /// <summary>
    /// Check if a mob prefab can spawn from this spawner
    /// </summary>
    public bool CanSpawnMobType(GameObject mobPrefab)
    {
        if (mobPrefab == null) return false;
        if (mobPrefabs.Count == 0) return true; // If no filter, can spawn any
        
        return mobPrefabs.Contains(mobPrefab);
    }

    /// <summary>
    /// Get available mob prefabs for this spawner
    /// </summary>
    public List<GameObject> GetAvailableMobPrefabs()
    {
        return new List<GameObject>(mobPrefabs);
    }

    /// <summary>
    /// Get spawner ID
    /// </summary>
    public string GetSpawnerID()
    {
        return spawnerID;
    }

    /// <summary>
    /// Get spawn point count
    /// </summary>
    public int GetSpawnPointCount()
    {
        return spawnPoints.Count;
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

    private void OnDrawGizmosSelected()
    {
        if (!isInitialized && spawnMainObject != null)
        {
            // Show preview even when not initialized
            List<Transform> previewPoints = new List<Transform>();
            foreach (Transform child in spawnMainObject.transform)
            {
                previewPoints.Add(child);
            }
            DrawSpawnGizmos(previewPoints);
        }
        else if (spawnPoints != null && spawnPoints.Count > 0)
        {
            DrawSpawnGizmos(spawnPoints);
        }
    }

    private void DrawSpawnGizmos(List<Transform> points)
    {
        // Draw spawn points
        Gizmos.color = Color.cyan;
        foreach (var point in points)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
                Gizmos.DrawLine(transform.position, point.position);
            }
        }

        // Draw spawner position
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}