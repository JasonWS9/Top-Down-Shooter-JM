using UnityEngine;
using System.Collections.Generic;

public enum EnemyType { SingleShot, BurstShot, Rammer }
public enum EnemyFaction { Purple, Orange }
public enum SpecialAbility { None, CannonAtLowHealth, SpawnMinionsOnDeath }

[System.Serializable]
public class EvolutionTier
{
    [Tooltip("The level required to unlock this tier")]
    public int requiredLevel = 2;

    [Header("Visuals")]
    [Tooltip("The sprite to use for this evolution. Leave blank to keep the base sprite.")]
    public Sprite evolutionSprite;
    
    [Tooltip("Optional: The animator to use if this form moves differently.")]
    public RuntimeAnimatorController evolutionAnimator;

    [Header("Stat Multipliers (1.0 = base stats)")]
    public float healthMultiplier = 1.5f;
    public float speedMultiplier = 1.2f;
    public float damageMultiplier = 1.5f;

    [Header("Special Behavior")]
    public SpecialAbility unlockedAbility = SpecialAbility.None;
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TopDownShooter/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public EnemyType enemyType;
    public EnemyFaction faction;

    [Header("Base Stats (Level 1)")]
    public float baseHealth = 50f;
    public float baseSpeed = 3f;
    public float attackDamage = 20f;
    public float attackRange = 10f;

    [Header("Combat Settings")]
    public GameObject projectilePrefab; 
    public float projectileSpeed = 10f;
    public float fireRate = 2f; 

    [Header("Evolutions")]
    public List<EvolutionTier> evolutionTiers;
    
    [Header("Minion Settings")]
    public GameObject minionPrefab;
}