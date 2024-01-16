using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossBar : MonoBehaviour
{
    Boss boss;
    Slider slider;

    void Start()
    {
        boss = GameManager.instance.GetBoss();
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        slider.value = boss.GetHealthRatio();
    }
}
