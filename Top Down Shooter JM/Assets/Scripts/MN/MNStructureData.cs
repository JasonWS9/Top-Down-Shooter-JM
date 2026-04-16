using UnityEngine;

[CreateAssetMenu(fileName = "NewStructureData", menuName = "VoidAudit/Structure Data")]
public class MNStructureData : ScriptableObject
{
    [Header("Identification")]
    public string structureName;
    
    [Tooltip("How much 'Destruction Quota' this building adds when fully destroyed.")]
    public int auditValue = 10; 

    [Header("Health Limits")]
    public int maxHealth = 100;
    public int damagedThreshold = 50; // The health number where it switches to the 'Damaged' phase

    [Header("Phase 1: Full Health")]
    public Sprite fullSprite;
    public RuntimeAnimatorController fullAnimator;

    [Header("Phase 2: Damaged")]
    public Sprite damagedSprite;
    public RuntimeAnimatorController damagedAnimator;

    [Header("Phase 3: Destroyed")]
    public Sprite destroyedSprite;
    public RuntimeAnimatorController destroyedAnimator;
}