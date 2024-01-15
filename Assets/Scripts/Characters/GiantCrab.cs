using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantCrab : Boss
{
    protected float moveTimer = 0f; // Increases when boss is walking

    [SerializeField] private List<Projectile> clawProjectiles;
    private float clawShootSpeed = 0.33f;

    [SerializeField] private List<Projectile> waterProjectiles;
    private float waterShootSpeed = 0.2f;

    [SerializeField] private List<Projectile> sandProjectiles;
    [SerializeField] private int sandBurstSize = 5;
    private float sandAttackSpeed = 0.49f;
    private bool sandPhaseStarted = false;

    private float phaseTimer = 0f; // Timer for swapping phases
    private float desperation = 1f; // How much quicker the boss will move / attack
    [SerializeField] private float phaseLength = 2.0f; // How long each phase lasts
    private float nextAttackTime; // Time when next attack can be made during an attack phase
    private int attackPhase = 0; // Current attack phase
    [SerializeField] private Transform projectileSpawn; // Spawn points of projectiles on the crab

    // Sound effects
    [SerializeField] AudioClip clawAttackSound;
    [SerializeField] AudioClip waterAttackSound;
    [SerializeField] AudioClip sandAttackSound;

    // Animation names
    [SerializeField] string attackAnimation;

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Desperation
        desperation = 2f - GetHealthRatio();

        // Timer
        phaseTimer += Time.deltaTime * desperation / phaseLength;

        // Attack phases
        switch (attackPhase)
        {
            case 0:
                WalkPhase();
                break;
            case 1:
                ClawAttack();
                break;
            case 2:
                WaterAttack();
                break;
            case 3:
                SandAttack();
                break;
            default:
                attackPhase = 0;
                break;
        }

        // Switch phases
        if (phaseTimer >= 1f)
        {
            // Swap to walk phase
            if (attackPhase != 0) attackPhase = 0;

            // Swap to random attack phase
            else attackPhase = Random.Range(1, 4);

            // Reset timer
            phaseTimer = 0f;
        }
    }

    private void WalkPhase()
    {
        moveTimer += Time.deltaTime * moveSpeed * desperation;
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(moveTimer) * 1.5f) - 1.5f, 0f);
    }

    // Fires fast moving projectiles that lock on to the player on activation and travel towards that initial position
    private void ClawAttack()
    {
        if (nextAttackTime <= Time.time)
        {
            Projectile projectile = GetNextProjectile(clawProjectiles);
            if (projectile != null)
            {
                // Get target to aim at
                // If above 50% health its random
                // If below then target lowest player (or random again if both equal)
                int target;
                if (GetHealthRatio() > 0.5) target = Random.Range(0, 2);
                else
                {
                    if (GameManager.instance.GetPlayer(0).health > GameManager.instance.GetPlayer(1).health)
                    {
                        target = 1;
                    }
                    else if (GameManager.instance.GetPlayer(0).health < GameManager.instance.GetPlayer(1).health)
                    {
                        target = 0;
                    }
                    else target = Random.Range(0, 2);
                }

                // Activate projectile
                projectile.GetComponent<AimedProjectile>().AimedActivate(projectileSpawn.position, target);

                // Play audio
                audioSource[1].clip = clawAttackSound;
                audioSource[1].Play();

                // Attack animation
                animator.Play(attackAnimation);

                // Start cooldown
                nextAttackTime = Time.time + (clawShootSpeed / desperation);
            }
        }
    }

    // Fires projectiles that start slow and quickly accelerate in a straight line at random heights
    private void WaterAttack()
    {
        // Rapidly move up and down the screen
        moveTimer += Time.deltaTime * moveSpeed * 3.0f;
        WalkPhase();

        if (nextAttackTime <= Time.time)
        {
            Projectile projectile = GetNextProjectile(waterProjectiles);
            if (projectile != null)
            {
                Vector3 spawnPos = new Vector3(projectileSpawn.position.x, Random.Range(3f, -6f), 0);
                projectile.Activate(spawnPos);

                audioSource[1].clip = waterAttackSound;
                audioSource[1].Play();

                animator.Play(attackAnimation);

                nextAttackTime = Time.time + (waterShootSpeed / desperation);
            }
        }
    }

    // Move to top of screen then move down whilst firing bursts of randomly angled sand projectiles
    private void SandAttack()
    {
        // Stay in move phase until at the top of the screen
        if (transform.position.y <= -0.05f && !sandPhaseStarted)
        {
            WalkPhase();
            phaseTimer = 0.0f;
        }
        else
        {
            sandPhaseStarted = true;

            // Slowly move down screen
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(0f, -3.0f, phaseTimer), 0f);
            moveTimer = 1.5f * Mathf.PI;

            // Sand Attack
            if (nextAttackTime <= Time.time)
            {
                // Over half health = all sand projectiles
                if (GetHealthRatio() >= 0.5f)
                {
                    // Spawn projectiles
                    List<Projectile> projectiles = GetNextProjectiles(sandProjectiles, sandBurstSize);
                    foreach (Projectile projectile in projectiles)
                    {
                        projectile.Activate(projectileSpawn.position);
                    }
                }
                // Under half = 1 projectile is replaced by claw projectile
                else
                {
                    // Spawn projectiles
                    List<Projectile> projectiles = GetNextProjectiles(sandProjectiles, sandBurstSize - 1);
                    foreach (Projectile projectile in projectiles)
                    {
                        projectile.Activate(projectileSpawn.position);
                    }

                    Projectile clawProjectile = GetNextProjectile(clawProjectiles);
                    clawProjectile.GetComponent<AimedProjectile>().AimedActivate(projectileSpawn.position, Random.Range(0, 2));
                }

                // Play sound effect
                audioSource[1].clip = sandAttackSound;
                audioSource[1].Play();

                animator.Play(attackAnimation);

                // Reset attack timer
                nextAttackTime = Time.time + (sandAttackSpeed / desperation);
            }
        }

        if (phaseTimer >= 1f) sandPhaseStarted = false; // Reset phase started flag at the end of the phase
    }

    public override void Reset()
    {
        base.Reset();

        phaseTimer = 0f; // Reset phase timer
        moveTimer = 0f;
        attackPhase = 0; // Start with movement phase
    }
}
