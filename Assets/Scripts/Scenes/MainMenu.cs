using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

// Credit to 3p0ch on Newgrounds for the PlayerPrefs code
// This changes how PlayerPrefs are saved so that updating the game on Newgrounds doesn't unintentionally wipe existing PlayerPrefs saves
// Alternatively I could use NGIO and make a save file but this works and is leagues easier
#if UNITY_EDITOR
#elif UNITY_WEBGL
public static class PlayerPrefs {
// **********************************
// ** PUT YOUR SAVE PATH NAME HERE **
// **********************************  
  static string savePathName = "5360002";
  static string fileName;
  static string[] fileContents;
  static Dictionary<string, string> saveData = new Dictionary<string, string>();
  
  // This is the static constructor for the class
  // When invoked, it looks for a savegame file
  // and reads the keys and values
  static PlayerPrefs() {
    fileName = "/idbfs/" + savePathName + "/NGsave.dat";
    
    // Open the savegame file and read all of the lines
    // into fileContents
    // First make sure the directory and save file exist,
    // and make them if they don't already
    // (If the file is created, the filestream needs to be
    // closed afterward so it can be saved to later)
    if (!Directory.Exists("/idbfs/" + savePathName)) {
      Directory.CreateDirectory("/idbfs/" + savePathName);
    }
    if (!File.Exists(fileName)) {
      FileStream fs = File.Create(fileName);
      fs.Close();
    } else {
      // Read the file if it already existed
      fileContents = File.ReadAllLines(fileName);
      
      // If you want to use encryption/decryption, add your
      // code for decrypting here
      //   ******* decryption algorithm ********
      
      // Put all of the values into saveData
      for (int i=0; i<fileContents.Length; i += 2) {
        saveData.Add(fileContents[i], fileContents[i+1]);
      }
    }
  }
  
  // This saves the saveData to the player's IndexedDB
  public static void Save() {
    // Put the saveData dictionary into the fileContents
    // array of strings
    Array.Resize(ref fileContents, 2 * saveData.Count);
    int i=0;
    foreach (string key in saveData.Keys) {
      fileContents[i++] = key;
      fileContents[i++] = saveData[key];
    }
    
    // If you want to use encryption/decryption, add your
    // code for encrypting here
    //   ******* encryption algorithm ********
    
    // Write fileContents to the save file
    File.WriteAllLines(fileName, fileContents);
  }
  
  // The following methods emulate what PlayerPrefs does
  public static void DeleteAll() {
    saveData.Clear();
    Save();
  }
  
  public static void DeleteKey(string key) {
    saveData.Remove(key);
    Save();
  }
  
  public static float GetFloat(string key) {
    return float.Parse(saveData[key]);
  }
  public static float GetFloat(string key, float defaultValue) {
    if (saveData.ContainsKey(key)) {
      return float.Parse(saveData[key]);
    } else {
      return defaultValue;
    }
  }
  
  public static int GetInt(string key) {
    return int.Parse(saveData[key]);
  }
  public static int GetInt(string key, int defaultValue) {
    if (saveData.ContainsKey(key)) {
      return int.Parse(saveData[key]);
    } else {
      return defaultValue;
    }
  }
  
  public static string GetString(string key) {
    return saveData[key];
  }
  public static string GetString(string key, string defaultValue) {
    if (saveData.ContainsKey(key)) {
      return saveData[key];
    } else {
      return defaultValue;
    }
  }
  
  public static bool HasKey(string key) {
    return saveData.ContainsKey(key);
  }
  
  public static void SetFloat(string key, float setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue.ToString();
    } else {
      saveData.Add(key, setValue.ToString());
    }
    Save();
  }
  
  public static void SetInt(string key, int setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue.ToString();
    } else {
      saveData.Add(key, setValue.ToString());
    }
    Save();
  }
  
  public static void SetString(string key, string setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue;
    } else {
      saveData.Add(key, setValue);
    }
    Save();
  }
}
#endif

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
        if (!PlayerPrefs.HasKey("sfxVolume")) PlayerPrefs.SetFloat("sfxVolume", 0.5f);
        if (!PlayerPrefs.HasKey("musicVolume")) PlayerPrefs.SetFloat("musicVolume", 0.25f);
        PlayerPrefs.Save();

        // Set values for sfx
        backgroundMusic.volume = PlayerPrefs.GetFloat("musicVolume");

        sfxTest = GetComponent<AudioSource>();
        sfxTest.volume = sfxSlider.value;

        // Enable continue button if possible
        continueButton.interactable = PlayerPrefs.HasKey("lastBoss");
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
        if (isRestarting || PlayerPrefs.GetInt("lastBoss") == 0) SceneManager.LoadScene("StartScreen");
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
