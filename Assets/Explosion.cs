using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (startTime + 3.0f <= Time.time) Destroy(gameObject);
    }
}
