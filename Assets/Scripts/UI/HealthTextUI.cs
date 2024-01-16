using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthTextUI : MonoBehaviour
{
    [SerializeField] Character character;

    private void Update()
    {
        string maxHealth = character.maxHealth.ToString();
        string currentHealth = ((int)character.health).ToString("D"+maxHealth.Length.ToString());
        GetComponent<TextMeshProUGUI>().text = currentHealth + "/" + maxHealth;
    }
}
