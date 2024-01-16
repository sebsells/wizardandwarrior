using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthTextUI : MonoBehaviour
{
    Boss boss;

    private void Start()
    {
        boss = GameManager.instance.GetBoss();
    }

    private void Update()
    {
        string maxHealth = boss.maxHealth.ToString();
        string currentHealth = ((int)boss.health).ToString("D"+maxHealth.Length.ToString());
        GetComponent<TextMeshProUGUI>().text = currentHealth + "/" + maxHealth;
    }
}
