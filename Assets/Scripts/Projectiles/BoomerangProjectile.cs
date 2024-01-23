using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : Projectile
{
    Vector3 flyto = Vector3.right * 9.5f;
    Vector3 startPos;
    float timer;
    bool returning = false;

    protected override void Start()
    {
        base.Start();
        if (!isFriendly) flyto = -flyto; // Go to negative position if enemy bullet
    }

    public override void Activate(GameObject a_shooter, Vector3 position)
    {
        base.Activate(a_shooter, position);
        startPos = transform.position; // Starting position
        flyto.y = Random.Range(-5.0f, 2.5f); // Y position to reach
        timer = 0f;
        returning = false;
    }

    protected override void Update()
    {
        if (isActive)
        {
            // Check if projectile has reached destination and return to sender
            if (returning || (transform.position - flyto).magnitude <= 0.1f)
            {
                returning = true;
                startPos = shooter.transform.position;
            }

            // Move projectile
            transform.position = new Vector3(
                Mathf.Lerp(flyto.x, startPos.x, (Mathf.Cos(timer) + 1f) * 0.5f),
                Mathf.Lerp(flyto.y, startPos.y, (Mathf.Cos(timer) + 1f) * 0.5f),
                0.0f);
            timer += Time.deltaTime * speed;

            // Deactivate when reaching the shooter or after lifetime
            if (returning && (transform.position - startPos).magnitude <= 0.1f || startTime + lifeTime <= Time.time) Deactivate();
        }
    }
}
