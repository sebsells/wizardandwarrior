using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public int playerId = 0; // Which player are we controlling
    public bool isActive = true; // If false player is in solo mode and this character is disabled

    [SerializeField] protected float hurtCooldown = 2.0f; // How long the player can go between being hit
    protected float nextHurtTime; // Time when the player will be made vulnerable again
    [SerializeField] bool invincible = false; // Makes player invincible. For testing only

    [SerializeField] protected float directionTime = 22.0f; // How fast player changes direction
    public bool isMoving { get; protected set; } = false; // True if player is holding WASD or equivalent

    [SerializeField] protected Transform projectileSpawn; // Spawn position of projectiles

    [SerializeField] protected List<Projectile> lightProjectilePool; // List containing all the projectiles fired from this character
    [SerializeField] protected float lightResource = 10.0f; // Amount of resource used by a light attack
    [SerializeField] protected float lightFireRate = 0.25f; // Cooldown between each light attack
    protected float nextLightAttackTime; // Time at which the player can next attack after doing a light
    protected bool isLightAttacking = false; // True if player is on light attack cooldown

    [SerializeField] protected List<Projectile> heavyProjectilePool; // List containing all the projectiles fired from this character
    [SerializeField] protected float heavyResource = 33.3f; // Amount of resource used by a heavy attack
    [SerializeField] protected float heavyFireRate = 0.75f; // Cooldown between each heavy attack
    protected float nextHeavyAttackTime; // Time at which the player can next attack after doing a heavy
    protected bool isHeavyAttacking = false; // true if player is on heavy attack cooldown

    [SerializeField] protected float exchangeSpeed = 10.0f; // How much resource is drained when exchanging
    [SerializeField] protected float exchangeRate = 1.5f; // How much more resource the other player will get
    public bool isExchanging { get; protected set; } = false; // True if player is currently exchanging resources with the other

    protected PlayerKeys keys { get; private set; } // Input keys
    protected Player otherPlayer { get; private set; } = null; // Reference to other player

    protected float resource = 100.0f; // Current amount of resource
    protected float maxResource = 100.0f; // Max amount of resource
    protected float resourceSpeed = 10.0f; // Passive amount of resource gained per second

    // Animation names
    [SerializeField] protected string animIdle;
    [SerializeField] protected string animWalk;
    [SerializeField] protected string animAttack;
    [SerializeField] protected string animExchange;

    // Sound effects
    [SerializeField] protected AudioClip lightAttackSound;
    [SerializeField] protected AudioClip heavyAttackSound;
    [SerializeField] protected AudioClip exchangeSound;

    protected override void Start()
    {
        // Get controls
        keys = playerId == 0 ? GameManager.player0Keys : GameManager.player1Keys;

        // Get other player
        if (GameManager.instance != null) otherPlayer = GameManager.instance.GetOtherPlayer(playerId);
        else otherPlayer = this; // this is just so the player doesnt freak out when in a scene w/out a gamemanager (the start and end screen)

        // Set resource
        resource = maxResource;

        // Base character start
        base.Start();
    }

    protected virtual void Update()
    {
        if (!isDead && isActive && (GameManager.instance == null || GameManager.instance.gameState == GameState.Playing))
        {
            Movement();
            Attack();
            Exchange();
            Animate();

            if (!isExchanging) resource = Mathf.Min(maxResource, resource + (resourceSpeed * Time.deltaTime)); // Passive resource gain
        }
        else if (GameManager.instance.gameState == GameState.Intro)
        {
            animator.Play(animIdle);
        }
        else if (!isActive)
        {
            transform.position = otherPlayer.transform.position; // Keep disabled player on top of active player so boss ai doesnt break
        }
    }

    void Movement()
    {
        // Get Vector2 movement direction
        Vector2 heldDirection = Vector2.zero;
        if (!isExchanging) // Player cannot move whilst exchanging
        {
            // Get move direction
            if (Input.GetKey(keys.up)) heldDirection += Vector2.up;
            if (Input.GetKey(keys.down)) heldDirection += Vector2.down;
            if (Input.GetKey(keys.left)) heldDirection += Vector2.left;
            if (Input.GetKey(keys.right)) heldDirection += Vector2.right;
            heldDirection.Normalize();

            // Move state
            if (heldDirection == Vector2.zero) isMoving = false;
            else isMoving = true;
        }
        else isMoving = false;

        // Get rigidbody and lerp new direction vector onto velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = Vector2.Lerp(rb.velocity, heldDirection * moveSpeed, directionTime * Time.deltaTime);
    }

    protected virtual void Attack()
    {
        if (!isExchanging)
        {
            // Light Attack - Check for key being pressed, enough resource, not on cooldown and not already heavy attacking
            if (Input.GetKey(keys.light) && resource >= lightResource && Time.time >= nextLightAttackTime && !isHeavyAttacking)
            {
                LightAttack();

                isLightAttacking = true;
                isHeavyAttacking = false;
            }

            // Heavy Attack - Check for key being pressed, enough resource, not on cooldown and not already light attacking
            else if (Input.GetKey(keys.heavy) && resource >= heavyResource && Time.time >= nextHeavyAttackTime && !isLightAttacking)
            {
                HeavyAttack();

                isLightAttacking = false;
                isHeavyAttacking = true;
            }

            // Not currently attacking and not on any attack cooldown
            else if (Time.time >= nextHeavyAttackTime && Time.time >= nextLightAttackTime)
            {
                isLightAttacking = false;
                isHeavyAttacking = false;
            }
        }
    }
    protected virtual void LightAttack() // Called when doing a light attack
    {
        Projectile projectile = GetNextProjectile(lightProjectilePool); // Get next available projectile from pool
        if (projectile != null)
        {
            projectile.Activate(gameObject, projectileSpawn.position); // Activate projectile
            resource -= lightResource; // Take away resource

            animator.Play(animAttack); // Play hit animation

            audioSource[1].clip = lightAttackSound; // Play sound effect
            audioSource[1].Play();

            nextLightAttackTime = Time.time + lightFireRate; // Start attack cooldown
        }
    }
    protected virtual void HeavyAttack()
    {
        Projectile projectile = GetNextProjectile(heavyProjectilePool); // Get next available projectile from pool
        if (projectile != null)
        {
            projectile.Activate(gameObject, projectileSpawn.position); // Activate projectile
            resource -= heavyResource; // Take away resource

            animator.Play(animAttack); // Play hit animation

            audioSource[1].clip = heavyAttackSound; // Play sound effect
            audioSource[1].Play();

            nextHeavyAttackTime = Time.time + heavyFireRate; // Start attack cooldown
        }
    }

    protected virtual void Exchange()
    {
        // Cannot exchange if: other player is exchanging, resource is 0, or the other player has max resource
        if (!otherPlayer.isExchanging && resource >= 0.0f && otherPlayer.GetResourceRatio() < 1.0f)
        {
            // Start exchanging if already exchanging and key is held down or if not already exchanging and key is just pressed
            // Effectively makes it so player must press the key again if exchanging is interrupted by something
            if ((Input.GetKey(keys.exchange) && isExchanging) || (Input.GetKeyDown(keys.exchange) && !isExchanging))
            {
                // Get resource amounts
                float resourceTaken = Time.deltaTime * exchangeSpeed;
                float resourceGiven = resourceTaken * exchangeRate;

                // Exchange resources
                resource -= resourceTaken;
                otherPlayer.GiveResource(resourceGiven);

                // Particles
                if (!particles.isPlaying) particles.Play(true);

                // Sfx
                if (!audioSource[1].isPlaying || audioSource[1].clip != exchangeSound || !isExchanging)
                {
                    audioSource[1].clip = exchangeSound;
                    audioSource[1].Play();
                }

                // Track stat
                GameManager.instance.AddExchangeStat(resourceGiven);

                isExchanging = true;
            }
            else
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                isExchanging = false;
            }
        }
        else
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            isExchanging = false;
        }
    }
    public void GiveResource(float amount) { resource = Mathf.Clamp(resource + amount, 0.0f, maxResource); }
    public float GetResourceRatio() { return resource / maxResource; }

    protected virtual void Animate()
    {
        // Sprite animations
        if (isMoving && !isLightAttacking && !isHeavyAttacking)
        {
            animator.Play(animWalk);
        }
        else if (isExchanging)
        {
            animator.Play(animExchange);
        }
        else if (!isLightAttacking && !isHeavyAttacking && !isExchanging && !isMoving)
        {
            animator.Play(animWalk);
        }

        // Damage flash
        if (Time.time < nextHurtTime)
        {
            // Flash 5 times a second
            spriteRenderer.color = (int)(Time.time * 10) % 2 == 0 ? Color.white : Color.clear;
        }
        else if (spriteRenderer.color != Color.white)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public override bool Damage(float amount)
    {
        // Ignore damage if on cooldown
        if (Time.time >= nextHurtTime)
        {
            if (invincible || !isActive) return false;
            else return base.Damage(amount);
        }
        else return false;
    }
    protected override void OnDamage(float amount)
    {
        if (health > 0) // Didn't die from hit
        {
            nextHurtTime = Time.time + hurtCooldown;
        }
    }
    protected override void Die()
    {
        base.Die(); // Die
        isDead = true;

        spriteRenderer.color = Color.white; // Force player to be visible

        animator.speed = 0f; // Stop moving
        particles.Pause(); // Pause particles
        rb.velocity = Vector3.zero;
    }

    public override void Reset()
    {
        base.Reset();
        resource = maxResource;
    }
}
