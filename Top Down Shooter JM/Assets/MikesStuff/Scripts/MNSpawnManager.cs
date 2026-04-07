using UnityEngine;

public class MNSpawnManager : MonoBehaviour
{
    public static MNSpawnManager Instance;

    [Header("Corporate Assets")]
    public GameObject alienInterceptorPrefab; // Drops from Stratosphere
    public GameObject planetaryDefensePrefab; // Launches from Surface
    public GameObject irsDronePrefab;         // Enters from sides
    public GameObject civilianTransportPrefab;// Enters from sides
    public GameObject rivalAgentPrefab;       // Enters from sides

    [Header("Spawn Settings")]
    public float baseSpawnInterval = 1.5f;
    private float spawnTimer;

    [Header("Spawning Margins")]
    public float verticalSpawnBuffer = 5f; // How far above/below camera to spawn
    public float horizontalSpawnBuffer = 5f;

    void Awake()
    {
        Instance = this;
        spawnTimer = baseSpawnInterval;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            DeployAsset();
            spawnTimer = baseSpawnInterval;
        }
    }

    void DeployAsset()
    {
        // 1. Determine WHAT to spawn
        GameObject prefabToSpawn = SelectAssetToDeploy();
        
        // 2. Determine WHERE to spawn it based on WHAT it is
        Vector2 spawnPos = GetTacticalSpawnPoint(prefabToSpawn);

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
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

        // Stratosphere (Aliens drop down from the top)
        if (asset == alienInterceptorPrefab) 
        {
            return new Vector2(Random.Range(camPos.x - camWidth, camPos.x + camWidth), MNPlayerMovement.Instance.maxWorldY + verticalSpawnBuffer);
        }
        
        // Surface (Static Defenders spawn on the ground)
        if (asset == planetaryDefensePrefab) 
        {
            // Pick left or right side of the screen off-camera so they scroll into view
            bool spawnRight = Random.value > 0.5f;
            float spawnX = spawnRight ? (camPos.x + camWidth + horizontalSpawnBuffer) : (camPos.x - camWidth - horizontalSpawnBuffer);
            
            // Lock them directly to the surface boundary
            return new Vector2(spawnX, MNPlayerMovement.Instance.minWorldY);
        }

        // Mid-air (IRS, Civilians, Rivals fly in from left or right)
        bool spawnRightMid = Random.value > 0.5f;
        float spawnXMid = spawnRightMid ? (camPos.x + camWidth + horizontalSpawnBuffer) : (camPos.x - camWidth - horizontalSpawnBuffer);
        
        // Ensure they don't spawn below the ground or above the stratosphere
        float minSpawnY = Mathf.Max(camPos.y - camHeight, MNPlayerMovement.Instance.minWorldY + 2f);
        float maxSpawnY = Mathf.Min(camPos.y + camHeight, MNPlayerMovement.Instance.maxWorldY - 2f);
        float spawnYMid = Random.Range(minSpawnY, maxSpawnY);
        
        return new Vector2(spawnXMid, spawnYMid);
    }
}