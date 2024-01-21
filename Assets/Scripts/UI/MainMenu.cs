using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject playMenu;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] Button continueButton;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] GameObject background;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider loadingBar;
    [SerializeField] AudioSource backgroundMusic;
    AudioSource sfxTest;
    bool sfxChanged = false;
    bool isRestarting = false; // Is true when player is going to reset game progress instead of continuing

    private void Start()
    {
        if (PlayerPrefs.GetInt("newPlayer") == 0)
        {
            PlayerPrefs.SetInt("newPlayer", 1);
            PlayerPrefs.SetFloat("sfxVolume", 0.5f);
            PlayerPrefs.SetFloat("musicVolume", 0.25f);
            PlayerPrefs.Save();
        }

        // Set values for sfx
        backgroundMusic.volume = PlayerPrefs.GetFloat("musicVolume");

        sfxTest = GetComponent<AudioSource>();
        sfxTest.volume = sfxSlider.value;

        // Enable continue button if possible
        continueButton.interactable = PlayerPrefs.GetInt("lastBoss") != 0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && sfxChanged)
        {
            sfxTest.Play();
            sfxChanged = false;
        }
    }

    public void OpenMain()
    {
        playMenu.SetActive(false);
        controlsMenu.SetActive(false);
        optionsMenu.SetActive(false);
        creditsMenu.SetActive(false);

        mainMenu.SetActive(true);
    }

    public void OpenStart() {
        isRestarting = true;
        OpenPlay();
    }

    public void OpenContinue()
    {
        isRestarting = false;
        OpenPlay();
    }

    public void OpenPlay()
    {
        mainMenu.SetActive(false);
        playMenu.SetActive(true);
    }

    public void OpenControls()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(true);
    }

    public void OpenOptions()
    {
        // Set slider values
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxTest.volume = 0f; // Make audio test 0 since the slider change above sets off the sound

        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void StartGame(int players)
    {
        PlayerPrefs.SetInt("players", players);
        PlayerPrefs.Save();
        if (isRestarting || PlayerPrefs.GetInt("lastBoss") == 0) SceneManager.LoadScene(1);
        else SceneManager.LoadScene(PlayerPrefs.GetInt("lastBoss"));
        //StartCoroutine(LoadGameAsync()); the game doesn't really need a loading screen, its not very big lol
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
            loadingBar.value = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }
    }
}
