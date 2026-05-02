using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Spawning Settings")]
    public EnemyData[] possibleEnemies; 
    
    public float spawnInterval = 3f;
    private float spawnTimer;

    [Header("Screen Spawning Margins")]
    public float edgeBuffer = 0.1f; 

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemyOnScreen();
            spawnTimer = spawnInterval;
        }
    }
    
    public void SpawnSpecificEnemyAtLocation(EnemyData data, int level, Vector3 position)
    {
        // Safety check to make sure the data actually has a prefab assigned
        if (data == null || data.basePrefab == null) return;

        // Instantiate the specific prefab tied to this enemy data
        GameObject newEnemy = Instantiate(data.basePrefab, position, Quaternion.identity);
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        
        if (controller != null)
        {
            controller.Initialize(data, level);
        }
    }
    
    void SpawnEnemyOnScreen()
    {
        if (possibleEnemies.Length == 0) return;
        if (PlayerMovement.Instance == null || GameManager.Instance == null) return;

        EnemyFaction chosenFaction = DetermineFactionWeight();

        List<EnemyData> factionSpecificEnemies = new List<EnemyData>();
        foreach (EnemyData data in possibleEnemies)
        {
            if (data.faction == chosenFaction)
            {
                factionSpecificEnemies.Add(data);
            }
        }

        if (factionSpecificEnemies.Count == 0) return;

        EnemyData randomData = factionSpecificEnemies[Random.Range(0, factionSpecificEnemies.Count)];
        
        // Safety check before spawning
        if (randomData.basePrefab == null)
        {
            Debug.LogWarning($"EnemyData '{randomData.enemyName}' is missing a basePrefab!");
            return;
        }

        int spawnLevel = (randomData.faction == EnemyFaction.Purple) ? GameManager.Instance.purpleLevel : GameManager.Instance.orangeLevel;

        float randomX = Random.Range(edgeBuffer, 1f - edgeBuffer);
        float randomY = Random.Range(edgeBuffer, 1f - edgeBuffer);
        
        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, Camera.main.nearClipPlane));
        spawnPos.z = 0f;
        
        GameObject newEnemy = Instantiate(randomData.basePrefab, spawnPos, Quaternion.Euler(0, 0, 180f));
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        
        if (controller != null)
        {
            controller.Initialize(randomData, spawnLevel);
        }
    }

    EnemyFaction DetermineFactionWeight()
    {
        float currentX = PlayerMovement.Instance.currentWorldX;
        float minX = PlayerMovement.Instance.minWorldX;
        float maxX = PlayerMovement.Instance.maxWorldX;

        float mapPositionPercentage = Mathf.InverseLerp(minX, maxX, currentX);

        float minOrangeChance = 1f / 21f; 
        float maxOrangeChance = 20f / 21f; 

        float orangeProbability = Mathf.Lerp(minOrangeChance, maxOrangeChance, mapPositionPercentage);
        float diceRoll = Random.Range(0f, 1f);
        
        if (diceRoll <= orangeProbability)
        {
            return EnemyFaction.Orange;
        }
        else
        {
            return EnemyFaction.Purple;
        }
    }
}