using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] bool isMusic;
    void Start()
    {
        foreach (AudioSource audioSource in GetComponents<AudioSource>())
        {
            audioSource.volume = PlayerPrefs.GetFloat(isMusic ? "musicVolume" : "sfxVolume");
        }
    }
}