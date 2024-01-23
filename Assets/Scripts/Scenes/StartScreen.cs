using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreen : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> textObjects;
    [SerializeField] GameObject promptText;
    List<string> stringText;
    int currentIndex = 0;
    bool writingText = false;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        stringText = new List<string>();
        foreach(TextMeshProUGUI text in textObjects)
        {
            stringText.Add(text.text);
            text.text = "";
        }
        StartCoroutine(WriteText());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Skip animation and write all text
            if (writingText)
            {
                writingText = false;
                StopCoroutine(WriteText());
                textObjects[currentIndex].text = stringText[currentIndex];
            }

            // Move to next textbox
            else
            {
                currentIndex += 1;
                if (currentIndex == textObjects.Count) { SceneManager.LoadScene(1); } // All text done, load first boss
                else {
                    textObjects[currentIndex - 1].text = ""; // Remove last box
                    StartCoroutine(WriteText()); // Start writing next box
                }
            }
        }

        promptText.SetActive(!writingText);
    }

    IEnumerator WriteText()
    {
        writingText = true;
        // Loop through every character and add it to text
        foreach(char character in stringText[currentIndex])
        {
            if (writingText == false) break;

            textObjects[currentIndex].text += character;

            if (char.IsWhiteSpace(character)) continue; // Ignore spaces
            yield return new WaitForSeconds(1f / 30f);
            audioSource.Play();
        }
        writingText = false;
    }
}
