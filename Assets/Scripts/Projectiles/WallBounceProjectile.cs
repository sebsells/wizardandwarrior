using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBounceProjectile : Projectile
{
    [SerializeField] float verticalSpeed = 12.0f;
    Vector3 direction;

    public override void Activate(GameObject a_shooter, Vector3 position)
    {
        base.Activate(a_shooter, position);

        // Get start direction
        direction = Random.Range(0, 2) == 0 ? Vector3.up : Vector3.down;
    }

    protected override void OnUpdate()
    {
        transform.position += direction * verticalSpeed * Time.deltaTime;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive)
        {
            if (collision.name == "Boundary")
            {
                direction = -direction; // Inverse direction on hitting something that isn't a wall
            }
        }

        base.OnTriggerEnter2D(collision);
    }
}
