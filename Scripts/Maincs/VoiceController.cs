using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextSpeech;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;

public class VoiceController : MonoBehaviour
{
    const string LANG_CODE = "en-US";

    [SerializeField] TMP_Text uiText;
    [SerializeField] Button nextButton;
    [SerializeField] Button previousButton;

    void Start()
    {
        Setup(LANG_CODE);

#if UNITY_ANDROID
        SpeechToText.Instance.onPartialResultsCallback = OnPartialSpeechResult;
#endif
        SpeechToText.Instance.onResultCallback = OnFinalSpeechResult;
        TextToSpeech.Instance.onStartCallBack = OnSpeakStart;
        TextToSpeech.Instance.onDoneCallback = OnSpeakStop;
        CheckPermission();

        Debug.Log("Starting Speech Recognition...");
       // StartListening();
    }

    void CheckPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    #region Text to Speech 
    public void StartSpeaking(string message)
    {
        TextToSpeech.Instance.StartSpeak(message);
    }

    public void StopSpeaking()
    {
        TextToSpeech.Instance.StopSpeak();
    }

    void OnSpeakStart()
    {
        Debug.Log("Talking started...");
    }

    void OnSpeakStop()
    {
        Debug.Log("Talking stopped...");
    }
    #endregion

    #region Speech to Text 
    void StartListening()
    {
        SpeechToText.Instance.StartRecording();
    }

    public void StopListening()
    {
        SpeechToText.Instance.StopRecording();
    }

    void OnFinalSpeechResult(string result)
    {
        Debug.Log("Speech Result: " + result);
        uiText.text = result;
        ProcessSpeechCommand(result);
    }

    void OnPartialSpeechResult(string result)
    {
        uiText.text = result;
        ProcessSpeechCommand(result);
    }
    #endregion

    void Setup(string code)
    {
        TextToSpeech.Instance.Setting(code, 1, 1);
        SpeechToText.Instance.Setting(code);
    }

    // Process voice commands
    void ProcessSpeechCommand(string command)
    {
        command = command.ToLower(); // Normalize text input

        if (command.Contains("next"))
        {
            Debug.Log("Next button clicked via voice command.");
            if (nextButton.interactable)
            {
                nextButton.onClick.Invoke();
               // StartListening();
            }
            else
            {
                Debug.LogError("Next button is not interactable!");
            }
        }
        else if (command.Contains("previous"))
        {
            Debug.Log("Previous button clicked via voice command.");
            previousButton.onClick.Invoke(); // Simulate previous button click
           // StartListening();
        }
    }

    // Detect screen taps to reactivate the mic
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect screen tap
        {
            Debug.Log("Screen tapped! Restarting speech recognition...");
            StartListening();
        }
    }
}
