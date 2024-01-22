using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Character
{
    protected virtual void Update()
    {
        // Fade red colour away
        float lerpColour = Mathf.Lerp(spriteRenderer.color.g, 1, Time.deltaTime * 7.5f);
        spriteRenderer.color = new Color(1, lerpColour, lerpColour);
    }

    protected override void OnDamage(float amount)
    {
        if (health > 0)
        {
            // Turn red
            spriteRenderer.color = Color.red;
        }
    }
    protected override void Die()
    {
        base.Die();
        isDead = true;

        spriteRenderer.color = Color.white; // Force player to be visible

        animator.speed = 0f; // Stop moving
        rb.velocity = Vector3.zero;
    }
}
