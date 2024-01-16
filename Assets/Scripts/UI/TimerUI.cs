using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "TIME:\n" + ((int)(Time.time - GameManager.instance.startTime)).ToString("D3");
    }
}
