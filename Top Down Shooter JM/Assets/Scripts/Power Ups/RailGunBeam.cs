using UnityEngine;
using System.Collections.Generic;

public class RailGunBeam : MonoBehaviour
{
    public float baseDamage = 10f; // Set this in Inspector
    
    // We use Dictionaries to remember how long EACH individual enemy has been inside the beam
    private Dictionary<IDamageable, float> timeExposed = new Dictionary<IDamageable, float>();
    private Dictionary<IDamageable, int> damageTicks = new Dictionary<IDamageable, int>();

    void OnEnable()
    {
        // Clear memory every time the laser turns on
        timeExposed.Clear();
        damageTicks.Clear();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            if (!timeExposed.ContainsKey(target))
            {
                timeExposed[target] = 0f;
                damageTicks[target] = 0;
            }

            // Add time while they stay in the beam
            timeExposed[target] += Time.deltaTime;
            
            // Check how many half-seconds they have been burning
            int currentTick = Mathf.FloorToInt(timeExposed[target] / 0.5f);

            // If they crossed a new half-second threshold, blast them!
            if (currentTick > damageTicks[target])
            {
                damageTicks[target] = currentTick;
                
                // Get player's current level-up multiplier
                float pMult = PlayerMovement.Instance != null ? PlayerMovement.Instance.GetDamageMultiplier() : 1f;
                float actualBase = baseDamage * pMult;

                // THE EXPONENTIAL MATH (BaseDamage ^ ticks)
                // Ex: Tick 1 = 10^1 (10). Tick 2 = 10^2 (100). Tick 3 = 10^3 (1000). 
                int finalDmg = Mathf.RoundToInt(Mathf.Pow(actualBase, currentTick));
                
                target.TakeDamage(finalDmg, true);
                Debug.Log($"RailGun Tick {currentTick}! Dealt {finalDmg} exponential damage!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // If they manage to escape the beam, reset their timer
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null && timeExposed.ContainsKey(target))
        {
            timeExposed.Remove(target);
            damageTicks.Remove(target);
        }
    }
}