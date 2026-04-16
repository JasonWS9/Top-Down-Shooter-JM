using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class MNSpawnManager : MonoBehaviour
{
    public static MNSpawnManager Instance;

    [Header("Corporate Assets")]
    public GameObject alienInterceptorPrefab; 
    public GameObject planetaryDefensePrefab; 
    public GameObject irsDronePrefab;         
    public GameObject civilianTransportPrefab;
    public GameObject rivalAgentPrefab;       

    [Header("Spawn Settings")]
    public float baseSpawnInterval = 1.5f;
    private float spawnTimer;

    // --- NEW INVASION QUOTA LOGIC ---
    [Header("Invasion Quota")]
    [Tooltip("How many aliens make up this wave before the spawner stops?")]
    public int totalInvasionForce = 100;
    private int aliensSpawned = 0;
    public int aliensRemaining;

    // The event your UI is listening for
    public static event Action<int> OnInvasionCountUpdated; 
    // --------------------------------

    [Header("Spawning Margins")]
    public float verticalSpawnBuffer = 5f; 
    public float horizontalSpawnBuffer = 5f;

    void Awake()
    {
        Instance = this;
        spawnTimer = baseSpawnInterval;
        aliensRemaining = totalInvasionForce; // Set the starting count
    }

    void Start()
    {
        // Broadcast the starting number to the UI the moment the game loads
        OnInvasionCountUpdated?.Invoke(aliensRemaining);
    }

    void Update()
    {
        // If we've spawned the entire alien army, stop the spawner!
        if (aliensSpawned >= totalInvasionForce) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            DeployAsset();
            spawnTimer = baseSpawnInterval;
        }
    }

    void DeployAsset()
    {
        GameObject prefabToSpawn = SelectAssetToDeploy();
        Vector2 spawnPos = GetTacticalSpawnPoint(prefabToSpawn);
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Only count ACTUAL aliens against the quota (ignore IRS drones, defenders, etc.)
        if (prefabToSpawn == alienInterceptorPrefab)
        {
            aliensSpawned++;
        }
    }

    // --- NEW METHOD TO DECREMENT THE COUNTER ---
    public void RecordAlienDeath()
    {
        aliensRemaining--;
        OnInvasionCountUpdated?.Invoke(aliensRemaining);

        if (aliensRemaining <= 0)
        {
            Debug.Log("INVASION DEFEATED! The planet is saved (and highly audited).");
            // You can trigger your Win Screen or Next Wave logic here!
        }
    }

    GameObject SelectAssetToDeploy()
    {
        float roll = Random.Range(0f, 100f);
        if (roll < 35f) return alienInterceptorPrefab;       
        if (roll < 70f) return planetaryDefensePrefab;       
        if (roll < 85f) return civilianTransportPrefab;      
        if (roll < 95f) return rivalAgentPrefab;             
        return irsDronePrefab;                               
    }

    Vector2 GetTacticalSpawnPoint(GameObject asset)
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector2 camPos = cam.transform.position;

        // How far the treadmill has shifted the world
        float treadmillOffset = MNPlayerMovement.Instance.currentTreadmillY;

        // Stratosphere
        if (asset == alienInterceptorPrefab) 
        {
            float trueStratosphereY = (MNPlayerMovement.Instance.maxWorldY + verticalSpawnBuffer) - treadmillOffset;
            return new Vector2(Random.Range(camPos.x - camWidth, camPos.x + camWidth), trueStratosphereY);
        }
        
        // Surface
        if (asset == planetaryDefensePrefab) 
        {
            bool spawnRight = Random.value > 0.5f;
            float spawnX = spawnRight ? (camPos.x + camWidth + horizontalSpawnBuffer) : (camPos.x - camWidth - horizontalSpawnBuffer);
            
            float trueSurfaceY = MNPlayerMovement.Instance.minWorldY - treadmillOffset;
            return new Vector2(spawnX, trueSurfaceY);
        }

        // Mid-air 
        bool spawnRightMid = Random.value > 0.5f;
        float spawnXMid = spawnRightMid ? (camPos.x + camWidth + horizontalSpawnBuffer) : (camPos.x - camWidth - horizontalSpawnBuffer);
        
        float trueMinY = MNPlayerMovement.Instance.minWorldY + 2f - treadmillOffset;
        float trueMaxY = MNPlayerMovement.Instance.maxWorldY - 2f - treadmillOffset;
        
        float minSpawnY = Mathf.Max(camPos.y - camHeight, trueMinY);
        float maxSpawnY = Mathf.Min(camPos.y + camHeight, trueMaxY);
        
        float spawnYMid = Random.Range(minSpawnY, maxSpawnY);
        
        return new Vector2(spawnXMid, spawnYMid);
    }
}