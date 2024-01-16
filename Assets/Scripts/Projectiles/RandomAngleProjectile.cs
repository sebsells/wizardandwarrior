using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAngleProjectile : Projectile
{
    [SerializeField] float angleRange = 0.33f;

    public override void Activate(GameObject a_shooter, Vector3 position)
    {
        base.Activate(a_shooter, position);

        // Random angle
        moveDirection = new Vector3(
            isFriendly ? 1f : -1f,
            Random.Range(-angleRange, angleRange),
            0f);
        moveDirection.Normalize();
    }
}
