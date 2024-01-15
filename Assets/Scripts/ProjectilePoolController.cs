using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolController : MonoBehaviour
{
    // Deactivates every projectile
    public void DeactivateAll()
    {
        foreach (Transform parentTransform in GetComponentsInChildren<Transform>())
        {
            foreach(Projectile projectile in parentTransform.GetComponentsInChildren<Projectile>())
            {
                projectile.Deactivate();
            }
        }
    }
}
