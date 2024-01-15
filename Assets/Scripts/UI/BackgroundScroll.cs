using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [SerializeField] float speedOnZeroHealth; // Multiplier on background scroll speed when boss hits 0 hp
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.speed = Mathf.Lerp(speedOnZeroHealth, 1f, GameManager.instance.GetBoss().GetHealthRatio());
    }
}
