using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByleSlime : Boss
{
    private float phaseTimer = 0f; // Timer for swapping phases
    private float desperation = 1f; // How much quicker the boss will move / attack
    [SerializeField] private float phaseLength = 2.0f; // How long each phase lasts
    [SerializeField] private float phaseCooldown = 1.0f; // Cooldown between attack phases
    private float nextAttackTime; // Time when next attack can be made during an attack phase
    private int phase = 0; // Current attack phase
    private int lastAttackPhase = 0; // Last attack phase to prevent same phases from playing repeatedly
    [SerializeField] Transform projectileSpawn; // Projectile spawn point

    // Jump phase
    private int currentJumpPosition = 1; // Position that the slime was at last time
    private int jumpPosition = 1; // Position that slime is going to jump to
    private int phasesSinceJump = 3; // How many phases have passed since the slime jumped

    // Slime ball phase
    [SerializeField] private float ballAttackSpeed = 0.33f; // Time between each slime ball shot
    [SerializeField] List<Projectile> aimedSlimeBalls; // Every pooled aimed slime ball projectile
    [SerializeField] List<Projectile> randomSlimeBalls; // Every pooled random slime ball
    private bool shotAimedProjLast = false; // True if the last projectile shot was an aimed one

    // Bubble phase
    [SerializeField] private float bubbleAttackSpeed = 0.5f; // Time between each bubble shot
    [SerializeField] List<Projectile> bubbles; // Every pooled bubble projectile

    // Baby slime phase
    [SerializeField] private float babyAttackSpeed = 0.66f;
    [SerializeField] List<Projectile> babySlimes;

    // Animations
    [SerializeField] string walkAnimation;
    [SerializeField] string jumpAnimation;
    [SerializeField] string attackAnimation;

    // Sound effects
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip landSound;
    [SerializeField] AudioClip slimeBallSound;
    [SerializeField] AudioClip bubbleSound;
    [SerializeField] AudioClip babySlimeSound;

    protected override void Update()
    {
        if (GameManager.instance.gameState == GameState.Playing)
        {
            base.Update();
            if (isDead) return;

            // Desperation (1 to 1.5 based on health)
            desperation = 1.5f - (GetHealthRatio()*0.5f);

            // Timer
            phaseTimer += Time.deltaTime * desperation / phaseLength;

            // Switch phases
            if (phaseTimer >= 1f)
            {
                // Reset timer
                phaseTimer = 0f;

                // Check if slime should jump
                // Will jump if has been 3 phases without a jump or 1/4 chance to jump anyway
                if (phasesSinceJump >= 3 || Random.Range(0,4) == 0)
                {
                    phase = 0;
                    phasesSinceJump = 0;
                }
                else
                {
                    // Get next attack phase. Phases can not be the same as the last attack phase (excluding jumps, which can be chained)
                    do
                    {
                        phase = Random.Range(1, 4);
                    } while (phase == lastAttackPhase);

                    lastAttackPhase = phase; // Set last attack phase
                    phasesSinceJump += 1; // Increase/reset phases since the last jump phase
                    if (phasesSinceJump > 1) phaseTimer = -(phaseCooldown / phaseLength); // 1 second delay between multiple attack phases
                }
            }

            // Phases
            if (phaseTimer >= 0f)
            {
                switch (phase)
                {
                    case 0:
                        JumpPhase();
                        break;
                    case 1:
                        SlimeBallPhase();
                        break;
                    case 2:
                        BubblePhase();
                        break;
                    case 3:
                        BabySlimePhase();
                        break;
                }
            }
        }

        else if (GameManager.instance.gameState == GameState.Intro)
        {
            Intro();
        }
    }

    // The slime will jump to another part of the screen. Three positions total, will not jump to the same spot
    private void JumpPhase()
    {
        // Start of phase
        // Checks if jump positions are the same and if early enough in the phase for a jump to happen
        if (currentJumpPosition == jumpPosition && phaseTimer < 0.25f)
        {
            // Play sfx and animation
            animator.speed = desperation;
            animator.Play(jumpAnimation);
            audioSource[1].PlayOneShot(jumpSound);

            // Get new jump location
            while (currentJumpPosition == jumpPosition)
            {
                jumpPosition = Random.Range(0, 3); // Random number between 0 and 2, cannot be the same jump pos as the current one
            }
        }

        // Lerp slime to position
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Lerp(currentJumpPosition * -2.5f, jumpPosition * -2.5f, Mathf.Clamp01(phaseTimer * 2f)),
            0.0f);

        // On land
        if (phaseTimer >= 0.5f && currentJumpPosition != jumpPosition)
        {
            // Set current jump pos for next jump phase
            currentJumpPosition = jumpPosition;

            // Reset animator, play sfx and particles
            animator.speed = 1f;
            audioSource[1].PlayOneShot(landSound);
            particles.Play();
        }
    }

    // Fires projectiles that alternate between firing randomly and aiming towards a random player
    // Below 50% health the aimed projectiles will track the player with the lowest health
    private void SlimeBallPhase()
    {
        if (nextAttackTime <= Time.time)
        {
            Projectile projectile = GetNextProjectile(shotAimedProjLast ? randomSlimeBalls : aimedSlimeBalls);

            // Shoot aimed projectile
            if (!shotAimedProjLast)
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
            }

            // Shoot random projectile
            else
            {
                projectile.Activate(gameObject, projectileSpawn.position);
            }

            // Swap which projectile to fire
            shotAimedProjLast = !shotAimedProjLast;

            // Play sound effect
            audioSource[1].PlayOneShot(slimeBallSound);

            // Play attack animation
            animator.Play(attackAnimation);

            // Reset attack timer
            nextAttackTime = Time.time + (ballAttackSpeed / desperation);
        }
    }

    // Shoots bubbles that move up and down following a sine wave
    private void BubblePhase()
    {
        if (nextAttackTime <= Time.time)
        {
            // Spawn projectile
            Projectile projectile = GetNextProjectile(bubbles);
            projectile.Activate(gameObject, projectileSpawn.position);

            // Play sound effect
            audioSource[1].PlayOneShot(bubbleSound);

            // Play attack animation
            animator.Play(attackAnimation);

            // Reset attack timer
            nextAttackTime = Time.time + (bubbleAttackSpeed / desperation);
        }
    }

    // Summons an army of baby slimes that spawn behind and bounce up and down on the games boundary walls
    private void BabySlimePhase()
    {
        if (nextAttackTime <= Time.time)
        {
            // Spawn projectile
            Projectile projectile = GetNextProjectile(babySlimes);
            projectile.Activate(gameObject, new Vector3(10.625f, Random.Range(1.625f, -4.75f), 0f));

            // Play sound effect
            audioSource[1].PlayOneShot(babySlimeSound);

            // Play attack animation
            animator.Play(attackAnimation);

            // Reset attack timer
            nextAttackTime = Time.time + (babyAttackSpeed / desperation);
        }
    }

    protected override void Intro()
    {
        GameManager.instance.StartGame();
    }

    public override void Reset()
    {
        base.Reset();

        phaseTimer = 1f; // Reset phase timer

        // Back to default jump position
        currentJumpPosition = 1;
        jumpPosition = 1;

        phasesSinceJump = 3; // Force jump as first phase
        phase = 0;
}
}
