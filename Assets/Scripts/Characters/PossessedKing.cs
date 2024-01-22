using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessedKing : Boss
{
    private float phaseTimer = 0f; // Timer for swapping phases
    private float desperation = 1f; // How much quicker the boss will move / attack
    [SerializeField] private float phaseCooldown = 1.0f; // Cooldown between attack phases
    private float nextPhaseTime; // Time when the next phase can start
    private float nextAttackTime; // Time when next attack can be made during an attack phase
    private int phase = 0; // Current attack phase
    private int lastAttackPhase = 0; // Last attack phase to prevent same phases from playing repeatedly
    [SerializeField] Transform projectileSpawn; // Projectile spawn point

    // Intro
    bool introStarted = false;

    // Timer used to spin the king in a circle
    private float moveTimer;

    // Crown boomerang phase
    [SerializeField] List<Projectile> crownProjectiles;
    [SerializeField] float crownAttackSpeed = 1f;
    [SerializeField] float crownPhaseLength = 2f; // Length of attack phase

    // False heavy phase
    [SerializeField] List<Projectile> wizardHeavies;
    [SerializeField] List<Projectile> warriorHeavies;
    [SerializeField] float falseAttackSpeed = 0.5f;
    [SerializeField] float falsePhaseLength = 3.0f;
    int nextFalseTarget = 1; // Which player to shoot at next

    // Coin phase
    [SerializeField] List<Projectile> coinProjectiles;
    [SerializeField] float coinAttackSpeed = 0.5f;
    [SerializeField] float coinPhaseLength = 3.0f;
    [SerializeField] int coinBurstSize = 4;

    // Animations
    [SerializeField] string floatAnimation;
    [SerializeField] string attackAnimation;
    [SerializeField] string introAnimation;

    // Sound effects
    [SerializeField] AudioClip crownAttackSound;
    [SerializeField] AudioClip falseWizardSound;
    [SerializeField] AudioClip falseWarriorSound;
    [SerializeField] AudioClip coinAttackSound;
    [SerializeField] AudioClip introFloatSound;
    [SerializeField] AudioClip introNoiseSound;

    protected override void Update()
    {
        if (GameManager.instance.gameState == GameState.Playing)
        {
            base.Update();
            if (isDead) return;

            // Desperation (1 to 1.5 based on health)
            desperation = 1.5f - (GetHealthRatio() * 0.5f);

            // Timer
            phaseTimer += Time.deltaTime * desperation;

            // Phase
            if (phaseTimer >= 0f)
            {
                switch (phase) {
                    case 0:
                        CrownPhase();
                        break;
                    case 1:
                        FalseHeavyPhase();
                        break;
                    case 2:
                        CoinPhase();
                        break;
                }
            }

            // Move boss
            moveTimer += Time.deltaTime * desperation * moveSpeed;
            transform.position = new Vector3(
                Mathf.Lerp(7.5f, 9.5f, (Mathf.Cos(moveTimer)+1f)*0.5f),
                Mathf.Lerp(-5f, 2f, (Mathf.Sin(moveTimer)+1f)*0.5f),
                0.0f);
        }
        else if (GameManager.instance.gameState == GameState.Intro)
        {
            if (!introStarted) StartCoroutine(Intro());
        }
    }

    // Shoots crowns that boomerang once between the king and a random point on the other side of the stage
    void CrownPhase()
    {
        if (phaseTimer >= crownPhaseLength)
        {
            PhaseReset();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            // Spawn projectile
            Projectile projectile = GetNextProjectile(crownProjectiles);
            projectile.Activate(gameObject, transform.position);

            // Play attack animation
            animator.Play(attackAnimation);

            // Play sound effect
            audioSource[1].PlayOneShot(crownAttackSound);

            // Reset attack timer
            nextAttackTime = Time.time + (crownAttackSpeed / desperation);
        }
    }

    // Shoots 2 large aimed projectiles at both players
    // If below 50% health then will always shoot at the lowest player
    void FalseHeavyPhase()
    {
        if (phaseTimer >= falsePhaseLength)
        {
            PhaseReset();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            // Get target
            if (GetHealthRatio() > 0.5f)
            {
                nextFalseTarget = nextFalseTarget == 0 ? 1 : 0;
            }
            else
            {
                if (GameManager.instance.GetPlayer(0).health > GameManager.instance.GetPlayer(1).health) nextFalseTarget = 1;
                else if (GameManager.instance.GetPlayer(0).health < GameManager.instance.GetPlayer(1).health) nextFalseTarget = 0;
                else nextFalseTarget = nextFalseTarget == 0 ? 1 : 0; // Swap target like normal if both players have equal health
            }            

            // Spawn projectile
            Projectile projectile = GetNextProjectile(nextFalseTarget == 0 ? wizardHeavies : warriorHeavies);
            projectile.GetComponent<AimedProjectile>().AimedActivate(gameObject, transform.position, nextFalseTarget);

            // Play attack animation
            animator.Play(attackAnimation);

            // Play sound effect
            audioSource[1].PlayOneShot(nextFalseTarget == 0 ? falseWizardSound : falseWarriorSound);

            // Reset attack timer
            nextAttackTime = Time.time + (falseAttackSpeed / desperation);
        }
    }

    // Shoots a burst of x amount of coins
    void CoinPhase()
    {
        if (phaseTimer >= coinPhaseLength)
        {
            PhaseReset();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            // Spawn projectiles
            List<Projectile> projectiles = GetNextProjectiles(coinProjectiles, coinBurstSize);
            foreach (Projectile projectile in projectiles)
            {
                projectile.Activate(gameObject, transform.position);
            }

            // Play attack animation
            animator.Play(attackAnimation);

            // Play sound effect
            audioSource[1].PlayOneShot(coinAttackSound);

            // Reset attack timer
            nextAttackTime = Time.time + (coinAttackSpeed / desperation);
        }
    }

    // Called to switch phases
    void PhaseReset()
    {
        // Reset timer with cooldown
        phaseTimer = -phaseCooldown;

        // Get next attack phase
        do
        {
            phase = Random.Range(0, 3);
        } while (phase == lastAttackPhase);
        lastAttackPhase = phase;

        // Idle animation
        animator.Play(floatAnimation);
    }

    IEnumerator Intro()
    {
        PhaseReset();
        introStarted = true;
        animator.Play(introAnimation);

        audioSource[1].PlayOneShot(introFloatSound);

        yield return new WaitForSeconds(1.5f);

        audioSource[1].PlayOneShot(introFloatSound);

        yield return new WaitForSeconds(2.5f);

        audioSource[1].PlayOneShot(introNoiseSound);

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 12; ++i)
        {
            audioSource[1].PlayOneShot(introNoiseSound);
            yield return new WaitForSeconds(1f / 6f); // laughs 6 times a second
        }

        yield return new WaitForSeconds(1f);

        audioSource[1].PlayOneShot(introFloatSound);

        yield return new WaitForSeconds(1f);

        audioSource[1].PlayOneShot(introFloatSound);

        yield return new WaitForSeconds(1f);

        introStarted = false;
        GameManager.instance.StartGame();
    }

    public override void Reset()
    {
        base.Reset();

        PhaseReset();
    }
}
