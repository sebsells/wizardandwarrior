using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerateProjectile : Projectile
{
    [SerializeField] float accelerationAmount;
    float originalSpeed;

    protected override void Start()
    {
        originalSpeed = speed; // Get starting speed
        base.Start();
    }

    protected override void OnUpdate()
    {
        speed += accelerationAmount * Time.deltaTime; // Accelerate
    }

    public override void Deactivate()
    {
        speed = originalSpeed; // Reset speed
        base.Deactivate();
    }
}
