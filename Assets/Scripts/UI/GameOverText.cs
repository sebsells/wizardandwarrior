using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void WinText()
    {
        text.text = "---- YOU WIN ----\nPRESS SPACE TO CONTINUE\n\nENTER: STATS - ESCAPE: QUIT";
    }

    public void FinalWinText()
    {
        text.text = "---- YOU BEAT THE GAME ----\nPRESS SPACE TO RETURN TO THE MAIN MENU\n\nENTER: STATS - ESCAPE: QUIT";
    }
}
