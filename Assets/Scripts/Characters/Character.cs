using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Components
    protected Animator animator = null;
    protected AudioSource[] audioSource = null; // Characters have multiple sources, one for hurt sounds and the other for attack sounds. Multiple sources prevents sounds overwriting each other
    protected Rigidbody2D rb = null;
    protected SpriteRenderer spriteRenderer = null;

    public float health { get; protected set; } // Current health
    public float maxHealth; // Starting/max health
    public bool isDead { get; protected set; } = false; // True if dead

    [SerializeField] protected float moveSpeed; // Movement speed

    Vector3 startingPosition; // Position that the character started at on scene load (used to reset after death)

    // Sound effects
    [SerializeField] protected AudioClip hurtSound;
    [SerializeField] protected AudioClip deathSound;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponents<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        health = maxHealth;

        startingPosition = transform.position;
    }

    protected Projectile GetNextProjectile(List<Projectile> attackProjectiles) // Returns the first free projectile for use in the pool
    {
        foreach (Projectile projectile in attackProjectiles)
        {
            if (!projectile.isActive) return projectile;
        }
        return null;
    }

    protected List<Projectile> GetNextProjectiles(List<Projectile> attackProjectiles, int amount) // Returns first free X projectiles for use in the pool
    {
        List<Projectile> inactiveProjectiles = new List<Projectile>();
        foreach (Projectile projectile in attackProjectiles)
        {
            if (!projectile.isActive) inactiveProjectiles.Add(projectile);
            if (inactiveProjectiles.Count >= amount) break;
        }
        return inactiveProjectiles;
    }

    // Returns true/false depending on if character died
    public virtual bool Damage(float amount)
    {
        if (amount <= 0.0f) return false; // Ignore damage equal to or less than 0 as it will do nothing

        health = Mathf.Max(0.0f, health - amount); // Reduce health

        OnDamage(amount);

        if (health <= 0.0f) // Check for death
        {
            Die(); 
            return true;
        }

        // Play sound effect
        audioSource[0].clip = hurtSound;
        audioSource[0].Play();

        return false;
    }
    protected virtual void OnDamage(float amount)
    {

    }
    protected virtual void Die()
    {
        // Play sound effect
        audioSource[0].clip = deathSound;
        audioSource[0].Play();
    }

    public float GetHealthRatio() { return health / maxHealth; }

    public virtual void Reset()
    {
        isDead = false; // No longer dead
        health = maxHealth; // Reset health
        transform.position = startingPosition; // Move back to starting position
        if (animator != null) animator.speed = 1f; // Reset animator speed in case any animations were paused
    }
}
