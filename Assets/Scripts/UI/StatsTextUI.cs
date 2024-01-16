using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsTextUI : MonoBehaviour
{
    void OnEnable()
    {
        string resourceStat = ((int)GameManager.instance.playerExchange).ToString();
        if (GameManager.instance.playerExchange == 0f) resourceStat += " :(";

        GetComponent<TextMeshProUGUI>().text = "\n\n" +
            ((int)GameManager.instance.player0Damage).ToString() + "\n" +
            ((int)GameManager.instance.player1Damage).ToString() + "\n\n" +
            resourceStat + "\n\n\n" + 
            ((int)(GameManager.instance.endTime - GameManager.instance.startTime)).ToString();
    }
}
