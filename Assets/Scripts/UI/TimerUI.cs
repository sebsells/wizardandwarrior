using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    void Update()
    {
        if (GameManager.instance.gameState != GameState.Playing) return;
        GetComponent<TextMeshProUGUI>().text = "TIME:\n" + ((int)(Time.time - GameManager.instance.startTime)).ToString("D3");
    }
}
