using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This projectile gets the position of a random player on activation and will move to that point
public class AimedProjectile : Projectile
{
    public void AimedActivate(GameObject a_shooter, Vector3 position, int playerId)
    {
        Activate(a_shooter, position);
        Vector3 playerPosition = GameManager.instance.GetPlayer(playerId).transform.position;
        moveDirection = (playerPosition - transform.position).normalized;
    }

    public override void Activate(GameObject a_shooter, Vector3 position)
    {
        base.Activate(a_shooter, position);
        Vector3 playerPosition = GameManager.instance.GetPlayer(Random.Range(0, 2)).transform.position;
        moveDirection = (playerPosition - transform.position).normalized;
    }
}
