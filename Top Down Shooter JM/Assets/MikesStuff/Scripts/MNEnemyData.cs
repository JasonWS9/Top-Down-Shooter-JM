using UnityEngine;

public enum FactionType { Alien, Defender, Rival, Neutral }

public enum EnemyBehaviorType 
{ 
    BurstChaser, 
    MomentumChaser, 
    SlowShooter, 
    WandererErratic, 
    WandererSmooth,
    StaticTurret // For Planetary Defense Towers
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "VoidAudit/Enemy Data")]
public class MNEnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName;
    public int maxHealth = 10;
    public int damageToPlayer = 1;
    public float baseSpeed = 3f;

    [Header("Faction & Targeting")]
    public FactionType faction;
    public float radarRange = 15f; 

    [Header("Behavior Type")]
    public EnemyBehaviorType behaviorType;

    [Header("Burst Chaser Settings")]
    public float minBurstInterval = 2f;
    public float maxBurstInterval = 5f;
    public float burstForce = 15f;
    public float burstDrag = 2f;

    [Header("Momentum Chaser Settings")]
    public float turnSpeed = 2f;
    public float acceleration = 5f;
    public float momentumLossMultiplier = 0.5f;

    [Header("Shooter Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float minShootInterval = 1.5f;
    public float maxShootInterval = 4f;

    [Header("Wander Settings")]
    public float changeDirectionTime = 2f;
}