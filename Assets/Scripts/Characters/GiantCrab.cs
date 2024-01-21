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
    private int lastAttackPhase = 0; // Last attack phase (excluding walk) to prevent same attacks from playing repeatedly
    [SerializeField] private Transform projectileSpawn; // Spawn points of projectiles on the crab

    // Sound effects
    [SerializeField] AudioClip clawAttackSound;
    [SerializeField] AudioClip waterAttackSound;
    [SerializeField] AudioClip sandAttackSound;
    [SerializeField] AudioClip stomp1;
    [SerializeField] AudioClip stomp2;
    [SerializeField] AudioClip stomp3;
    [SerializeField] AudioClip stomp4;

    // Animation names
    [SerializeField] string walkAnimation;
    [SerializeField] string attackAnimation;
    [SerializeField] string introAnimation;

    protected override void Update()
    {
        // Boss AI
        if (GameManager.instance.gameState == GameState.Playing)
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
                else
                {
                    do
                    {
                        attackPhase = Random.Range(1, 4);
                    } while (attackPhase == lastAttackPhase);

                    lastAttackPhase = attackPhase;
                }

                // Reset timer
                phaseTimer = 0f;
            }
        }
        else if (GameManager.instance.gameState == GameState.Intro)
        {
            Intro();
        }
    }

    private void WalkPhase()
    {
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(moveTimer) * 1.5f) - 1.5f, 0f);
        moveTimer += Time.deltaTime * moveSpeed * desperation;
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
                projectile.GetComponent<AimedProjectile>().AimedActivate(gameObject, projectileSpawn.position, target);

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
                projectile.Activate(gameObject, spawnPos);

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
                        projectile.Activate(gameObject, projectileSpawn.position);
                    }
                }
                // Under half = 1 projectile is replaced by claw projectile
                else
                {
                    // Spawn projectiles
                    List<Projectile> projectiles = GetNextProjectiles(sandProjectiles, sandBurstSize - 1);
                    foreach (Projectile projectile in projectiles)
                    {
                        projectile.Activate(gameObject, projectileSpawn.position);
                    }

                    Projectile clawProjectile = GetNextProjectile(clawProjectiles);
                    clawProjectile.GetComponent<AimedProjectile>().AimedActivate(gameObject, projectileSpawn.position, Random.Range(0, 2));
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

    protected override void Intro()
    {
        animator.Play(introAnimation);
        phaseTimer += Time.deltaTime;

        // Syncing sound effects to animation
        // There's probably a better way to do this LOL
        if (phaseTimer >= 6.75)
        {
            animator.Play(walkAnimation);
            GameManager.instance.StartGame();
            phaseTimer = 0f;
        }
        else if (phaseTimer >= 6.333f && audioSource[0].clip != clawAttackSound)
        {
            if (audioSource[0].clip != clawAttackSound)
            {
                audioSource[0].clip = clawAttackSound;
                audioSource[0].Play();
            }
        }
        else if (phaseTimer >= 6f)
        {
            if (audioSource[1].clip != clawAttackSound)
            {
                audioSource[1].clip = clawAttackSound;
                audioSource[1].Play();
            }
        }
        else if (phaseTimer >= 5.25f)
        {
            if (audioSource[1].clip != waterAttackSound)
            {
                audioSource[1].clip = waterAttackSound;
                audioSource[1].Play();
            }
        }

        else if (phaseTimer >= 3f)
        {
            if (audioSource[1].clip != stomp4)
            {
                audioSource[1].clip = stomp4;
                audioSource[1].Play();
            }
        }
        else if (phaseTimer >= 2f)
        {
            if (audioSource[1].clip != stomp3)
            {
                audioSource[1].clip = stomp3;
                audioSource[1].Play();
            }
        }
        else if (phaseTimer >= 1f)
        {
            if (audioSource[1].clip != stomp2)
            {
                audioSource[1].clip = stomp2;
                audioSource[1].Play();
            }
        }
        else if (phaseTimer >= 0.05f) // if this is too low the sound effect can somehow play early
        {
            if (audioSource[1].clip != stomp1)
            {
                audioSource[1].clip = stomp1;
                audioSource[1].Play();
            }
        }
    }

    public override void Reset()
    {
        base.Reset();

        phaseTimer = 0f; // Reset phase timer
        moveTimer = 0f;
        attackPhase = 0; // Start with movement phase
    }
}
