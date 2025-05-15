using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject assemblyGuideUI;
    [SerializeField] private GameObject modelViewUI;
    [SerializeField] private GameObject testingUI;
    [SerializeField] private GameObject nonARModelButton;
    [SerializeField] private GameObject DesignUploadPanel;
    [SerializeField] private GameObject quickInspectionUI;
    // Start is called before the first frame update
    void Start()
    {
        ShowDesignUploadMenu();
    }

    private void ShowDesignUploadMenu()
    {
        MenuController MenuControllerScript = FindObjectOfType<MenuController>();

        DesignUploadPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        MenuControllerScript.DisableAllScripts();
        MenuControllerScript.DestroyWireObjects();
        MenuControllerScript.HideAllAssemblyObjects();

    }

}
