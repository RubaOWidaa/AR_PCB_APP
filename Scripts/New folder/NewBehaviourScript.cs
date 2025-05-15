using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{   //Button DataSheetViewerButton;
    [SerializeField] private GameObject DataSheetViewerButton;  // button for datasheet mode
    [SerializeField] private GameObject backtoar;  // button for datasheet mode
   // DataSheetViewerButton.interactable= true;
    public void BackToAR()
    {

      //  SceneManager.LoadScene("PCBAPP", LoadSceneMode.Additive);
       SceneManager.LoadScene("PCBAPP"); // Switch back to AR Scene
    }
    public void OnDataSheetViewer()
    {
        Debug.Log("Switching to DataSheetViewer...");
        SceneManager.LoadScene("Sample"); // Load the Non-AR Scene
    }
}