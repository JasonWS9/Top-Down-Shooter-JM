using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator), typeof(Collider2D))]
public class MNStructureController : MonoBehaviour, IDamageable
{
    public MNStructureData data;
    private int currentHealth;

    public enum StructureState { Full, Damaged, Destroyed }
    public StructureState currentState = StructureState.Full;

    private SpriteRenderer sr;
    private Animator anim;

    // THE MAGIC EVENT: This broadcasts to your Game Manager every time a building takes phase damage
    public static event Action<int> OnAuditValueChanged; 

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        
        currentHealth = data.maxHealth;
        UpdateVisuals();
    }

    // Implementing the IDamageable interface so bullets can hurt it
    public void TakeDamage(int damage, bool fromPlayer = false)
    {
        if (currentState == StructureState.Destroyed) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            ChangeState(StructureState.Destroyed);
        }
        else if (currentHealth <= data.damagedThreshold && currentState == StructureState.Full)
        {
            ChangeState(StructureState.Damaged);
        }
    }

    void ChangeState(StructureState newState)
    {
        currentState = newState;
        UpdateVisuals();

        // Send the liability data to the Audit Manager!
        if (newState == StructureState.Damaged)
        {
            // Give 50% of the audit value for damaging it
            OnAuditValueChanged?.Invoke(data.auditValue / 2); 
        }
        else if (newState == StructureState.Destroyed)
        {
            // Give the remaining 50% for destroying it
            OnAuditValueChanged?.Invoke(data.auditValue / 2); 
            
            // Turn off the collider so bullets pass through the ruins and don't get blocked
            GetComponent<Collider2D>().enabled = false; 
        }
    }

    void UpdateVisuals()
    {
        switch (currentState)
        {
            case StructureState.Full:
                if (data.fullSprite != null) sr.sprite = data.fullSprite;
                if (data.fullAnimator != null) anim.runtimeAnimatorController = data.fullAnimator;
                break;
            case StructureState.Damaged:
                if (data.damagedSprite != null) sr.sprite = data.damagedSprite;
                if (data.damagedAnimator != null) anim.runtimeAnimatorController = data.damagedAnimator;
                break;
            case StructureState.Destroyed:
                if (data.destroyedSprite != null) sr.sprite = data.destroyedSprite;
                if (data.destroyedAnimator != null) anim.runtimeAnimatorController = data.destroyedAnimator;
                break;
        }
    }
}