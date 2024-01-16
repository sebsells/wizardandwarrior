using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] GameObject background;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider loadingBar;
    [SerializeField] AudioSource backgroundMusic;
    AudioSource sfxTest;
    bool sfxChanged = false;

    private void Start()
    {
        if (PlayerPrefs.GetInt("newPlayer") == 0)
        {
            PlayerPrefs.SetInt("newPlayer", 1);
            PlayerPrefs.SetFloat("sfxVolume", 0.5f);
            PlayerPrefs.SetFloat("musicVolume", 0.25f);
            PlayerPrefs.Save();
        }

        // Set values
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        backgroundMusic.volume = musicSlider.value;

        sfxTest = GetComponent<AudioSource>();
        sfxTest.volume = sfxSlider.value;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && sfxChanged)
        {
            sfxTest.Play();
        }
    }

    public void OpenMain()
    {
        controlsMenu.SetActive(false);
        optionsMenu.SetActive(false);
        creditsMenu.SetActive(false);

        mainMenu.SetActive(true);
    }

    public void OpenControls()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(true);
    }

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void Play()
    {
        StartCoroutine(LoadGameAsync());
    }

    public void ChangeSFXVolume()
    {
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
        PlayerPrefs.Save();
        sfxTest.volume = sfxSlider.value;
        sfxChanged = true;
    }

    public void ChangeMusicVolume()
    {
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        PlayerPrefs.Save();
        backgroundMusic.volume = musicSlider.value;
    }

    IEnumerator LoadGameAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        loadingScreen.SetActive(true);
        mainMenu.SetActive(false);
        background.SetActive(false);

        while (!operation.isDone)
        {
            loadingBar.value = operation.progress;
            yield return null;
        }
    }
}
