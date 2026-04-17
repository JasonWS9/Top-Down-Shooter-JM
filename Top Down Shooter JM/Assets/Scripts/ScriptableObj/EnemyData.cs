using UnityEngine;

public enum EnemyType { SingleShot, BurstShot, Rammer }
public enum EnemyFaction { Purple, Orange }

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TopDownShooter/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public EnemyType enemyType;
    public EnemyFaction faction;

    [Header("Base Stats")]
    public float baseHealth = 50f;
    public float baseSpeed = 3f;
    public float attackDamage = 20f;
    public float attackRange = 10f;

    [Header("Combat Settings")]
    [Tooltip("Leave empty if this is a Rammer")]
    public GameObject projectilePrefab; 
    public float projectileSpeed = 10f;
    public float fireRate = 2f; 
}