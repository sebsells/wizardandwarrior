using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool isActive = false; // True if projectile is moving and can damage characters
    public bool isFriendly; // True if projectile was fired from player
    protected GameObject shooter; // Game object that shot this projectile
    [SerializeField] protected bool destroyOnHit; // True if projectile is destroyed when hitting something

    [SerializeField] protected float speed; // Speed that the projectile moves
    [SerializeField] protected float damage; // How much damage the projectile will deal upon contact
    protected Vector3 moveDirection = Vector3.zero; // Direction that the projectile will move (defined in attack based off isFriendly)

    protected float startTime; // Time at which the projectile became active
    [SerializeField] protected float lifeTime = 2.0f; // How long projectile will be active for before it dies

    protected virtual void Start()
    {
        Deactivate(); // Start projectile out as deactivated
    }

    protected virtual void Update()
    {
        if (isActive)
        {
            // Move projectile forwards
            transform.position += moveDirection * speed * Time.deltaTime;

            // Child projectile code
            OnUpdate();

            // If it has been too long kill the projectile
            if (startTime + lifeTime <= Time.time) { Deactivate(); }
        }
    }
    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive)
        {
            // Check if projectile has hit it's intended target (player or boss)
            if (!isFriendly && collision.GetComponent<Player>() != null || isFriendly && collision.GetComponent<Boss>() != null)
            {
                bool killedTarget = collision.GetComponent<Character>().Damage(damage); // Damage character that was hit

                GameManager.instance.AddDamageStat(shooter, damage); // Track damage stat

                if (killedTarget && GameManager.instance.gameState == GameState.Playing)
                {
                    GameManager.instance.GameOver(collision.gameObject, gameObject, transform.parent); // Game over if killed player
                    moveDirection = Vector3.zero; // Stop projectile from moving
                    startTime += 2f; // Make sure projectile isn't deactivated during gameover sequence
                    if (GetComponent<ParticleSystem>() != null) GetComponent<ParticleSystem>().Pause(); // Pause any particles
                }
                else if (destroyOnHit)
                {
                    Deactivate(); // Deactivate projectile
                }
            }
        }
    }

    public virtual void Activate(GameObject a_shooter, Vector3 position)
    {
        gameObject.SetActive(true);

        transform.position = position;

        isActive = true;
        startTime = Time.time;

        moveDirection = isFriendly ? Vector3.right : Vector3.left;

        if (GetComponent<ParticleSystem>() != null) GetComponent<ParticleSystem>().Play(); // Play any particles

        shooter = a_shooter;
    }

    public virtual void Deactivate()
    {
        transform.position = Vector3.up * 1000;
        isActive = false;
        gameObject.SetActive(false);
    }
}
