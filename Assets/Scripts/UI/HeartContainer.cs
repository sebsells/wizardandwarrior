using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartContainer : MonoBehaviour
{
    [SerializeField] int playerId;
    [SerializeField] List<GameObject> heartFills;
    Player player;

    private void Start()
    {
        foreach (GameObject heart in heartFills)
        {
            heart.SetActive(true);
        }
        player = GameManager.instance.GetPlayer(playerId);
    }

    private void Update()
    {
        UpdateHealth((int)player.health);
    }

    public void UpdateHealth(int health)
    {
        // Hide every heart
        foreach (GameObject heart in heartFills)
        {
            heart.SetActive(false);
        }

        // Refill each heart based off health
        for (int i = 0; i < health; ++i)
        {
            heartFills[i].SetActive(true);
        }
    }
}
