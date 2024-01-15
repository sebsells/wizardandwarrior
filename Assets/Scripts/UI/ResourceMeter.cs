using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMeter : MonoBehaviour
{
    [SerializeField] int playerId;
    Player linkedPlayer;
    Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        linkedPlayer = GameManager.instance.GetPlayer(playerId);
    }

    private void Update()
    {
        slider.value = linkedPlayer.GetResourceRatio();
    }
}
