using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This projectile will move up and down following a sine wave
public class WaveProjectile : Projectile
{
    [SerializeField] float amplitude = 2.0f; // Amplitude of the wave (total height of the wave / 2)
    [SerializeField] float frequency = 4.0f; // Frequency of wave (distance between each peak horizontally)

    float startY;
    float wavePosition; // Starting point on wave

    public override void Activate(GameObject a_shooter, Vector3 position)
    {
        base.Activate(a_shooter, position); // Base activate code

        wavePosition = Random.Range(0f, 1f); // Start wave at random point
        startY = transform.position.y - (Mathf.Sin(wavePosition * frequency) * amplitude); // Get starting position
    }

    protected override void OnUpdate()
    {
        transform.position = new Vector3(transform.position.x, startY + (Mathf.Sin(wavePosition * frequency) * amplitude), 0f);
        wavePosition += Time.deltaTime;
    }
}
