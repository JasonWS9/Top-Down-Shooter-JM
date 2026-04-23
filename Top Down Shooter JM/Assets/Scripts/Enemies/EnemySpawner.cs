using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject baseEnemyPrefab;
    [Tooltip("Drag all your EnemyData ScriptableObjects here!")]
    public EnemyData[] possibleEnemies; 
    
    public float spawnInterval = 3f;
    private float spawnTimer;

    [Header("Screen Spawning Margins")]
    public float edgeBuffer = 0.1f; 

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemyOnScreen();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnEnemyOnScreen()
    {
        if (baseEnemyPrefab == null || possibleEnemies.Length == 0) return;
        if (PlayerMovement.Instance == null) return;

        // Determine the Faction based on the map position
        EnemyFaction chosenFaction = DetermineFactionWeight();

        // Filter the possible enemies to ONLY include the chosen faction
        List<EnemyData> factionSpecificEnemies = new List<EnemyData>();
        foreach (EnemyData data in possibleEnemies)
        {
            if (data.faction == chosenFaction)
            {
                factionSpecificEnemies.Add(data);
            }
        }

        // Fallback safety check in case the list is missing a faction
        if (factionSpecificEnemies.Count == 0)
        {
            Debug.LogWarning("No enemies of faction " + chosenFaction + " found in the possibleEnemies list!");
            return;
        }

        // Pick a random EnemyData asset from the filtered list
        EnemyData randomData = factionSpecificEnemies[Random.Range(0, factionSpecificEnemies.Count)];
        
        // Find out what level this faction currently is
        int spawnLevel = (randomData.faction == EnemyFaction.Purple) ? GameManager.Instance.purpleLevel : GameManager.Instance.orangeLevel;

        // Calculate a random position within the camera's viewport
        float randomX = Random.Range(edgeBuffer, 1f - edgeBuffer);
        float randomY = Random.Range(edgeBuffer, 1f - edgeBuffer);
        
        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, Camera.main.nearClipPlane));
        spawnPos.z = 0f;

        // Instantiate the generic enemy shell and inject the data
        GameObject newEnemy = Instantiate(baseEnemyPrefab, spawnPos, Quaternion.identity);
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        
        if (controller != null)
        {
            controller.Initialize(randomData, spawnLevel);
        }
    }

    EnemyFaction DetermineFactionWeight()
    {
        // Grab the map data from the PlayerMovement instance
        float currentX = PlayerMovement.Instance.currentWorldX;
        float minX = PlayerMovement.Instance.minWorldX;
        float maxX = PlayerMovement.Instance.maxWorldX;

        // Normalize the position between 0 and 1
        // (0 is the far left minX edge, 0.5 is center, 1 is the far right maxX edge)
        float mapPositionPercentage = Mathf.InverseLerp(minX, maxX, currentX);

        // Define our probabilities based on the requested ratios
        // 1:20 ratio = 1 Orange per 20 Purple (1 out of 21 chance)
        // 20:1 ratio = 20 Orange per 1 Purple (20 out of 21 chance)
        float minOrangeChance = 1f / 21f; 
        float maxOrangeChance = 20f / 21f; 

        // Interpolate the exact probability based on where the player is standing
        float orangeProbability = Mathf.Lerp(minOrangeChance, maxOrangeChance, mapPositionPercentage);

        // Roll a random number between 0.0 and 1.0 to decide the faction
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