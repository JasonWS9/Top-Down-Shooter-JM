using UnityEngine;

public class PowerUpDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("Drag your 5 Power-Up prefabs in here!")]
    public GameObject[] powerUpPrefabs;
    
    [Tooltip("The base percentage chance to drop a power-up (e.g. 5 = 5%)")]
    public float baseDropChance = 5f; 
    
    [Tooltip("How much the chance increases per level. (+2.5% per level)")]
    public float bonusChancePerLevel = 2.5f;

    // Any script can call this right before destroying the GameObject
    public void TryDrop(int objectLevel = 1)
    {
        // Don't try to drop anything if the array is empty
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        // Calculate the final drop chance. 
        // Example: Level 1 = 5%. Level 5 = 15%.
        float finalDropChance = baseDropChance + (bonusChancePerLevel * (objectLevel - 1));

        // Roll a random number between 0 and 100
        float diceRoll = Random.Range(0f, 100f);

        // If the roll is lower than or equal to our chance, we drop loot!
        if (diceRoll <= finalDropChance)
        {
            // Pick a random power-up from the array
            GameObject randomPowerUp = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            
            // Spawn it at this object's exact location
            Instantiate(randomPowerUp, transform.position, Quaternion.identity);
            
            // Optional: Print a debug log to see the math in action
            Debug.Log($"[{gameObject.name}] dropped a Power-Up! (Rolled {diceRoll:F1} against a {finalDropChance:F1}% chance)");
        }
    }
}