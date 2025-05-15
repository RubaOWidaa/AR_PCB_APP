using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController2 : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject assemblyGuideUI;
    [SerializeField] private GameObject quickInspectionUI;
    [SerializeField] private GameObject modelViewUI;
    [SerializeField] private GameObject testingUI;
    [SerializeField] private GameObject nonARModelButton;
    [SerializeField] private GameObject DesignUploadPanel;//  button for Non-AR mode
                                                          // [SerializeField] private GameObject DataSheetViewerButton;  // button for datasheet mode

    [Header("Model Parent")]
    public GameObject modelParent;
    public GameObject inspectionModel;
    public GameObject traceModel;
    public string currentMode;

    public AssemblyGuide assemblyGuideScript;
    public SelectableComponent selectableComponentsScript;
    public trace tracesScript;
    public quickInspection quickInspectionScript;
    public MenuController MenuControllerScript;
    //public XMLSorter XMLSorterScript;

    //private RotateWithMouse RotateWithMouseScript;
    // private SelectableComponent SelectableComponentsc;
    public void Start()
    {
        // Avoid getting scripts from the obsolete modelParent
        if (traceModel != null)
            traceModel.SetActive(false);
        else
            Debug.LogWarning("traceModel is null in ShowMainMenu()");

        if (inspectionModel != null)
            inspectionModel.SetActive(false);
        else
            Debug.LogWarning("inspectionModel is null in ShowMainMenu()");


        DisableAllScripts();
        HideAllAssemblyObjects();
       // ShowMainMenu();
    }

    /*private void Awake()
    {
        if (modelParent == null)
        {
            modelParent = FindObjectOfType<RuntimeImageTarget>()?.uploadedModelPrefab2;

        }
      
    }*/

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Android Back Button
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        if (mainMenuPanel.activeSelf)
        {
            // ShowDesignUploadMenu();

        }
        else if (assemblyGuideUI.activeSelf || modelViewUI.activeSelf || testingUI.activeSelf || quickInspectionUI.activeSelf)
        {
            



            Debug.Log("Returning to Main Menu...");
            ShowMainMenu();

        }
    }
    private void ShowDesignUploadMenu()
    {
        DesignUploadPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        DisableAllScripts();
        DestroyWireObjects();
        HideAllAssemblyObjects();
    }
    public void ShowMainMenu()
    {
        DesignUploadPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);

        if (tracesScript != null)
            tracesScript.enabled = false;

        if (quickInspectionScript != null)
            quickInspectionScript.enabled = false;

        DisableAllScripts();
        DestroyWireObjects();
        HideAllAssemblyObjects();

        if (modelParent != null)
            modelParent.SetActive(false);
        else
            Debug.LogWarning("modelParent is null in ShowMainMenu");

        if (traceModel != null)
            traceModel.SetActive(false);
        else
            Debug.LogWarning("traceModel is null in ShowMainMenu");

        if (inspectionModel != null)
            inspectionModel.SetActive(false);
        else
            Debug.LogWarning("inspectionModel is null in ShowMainMenu");
    }

    private void DisableAllScripts()
    {
        if (assemblyGuideScript != null) assemblyGuideScript.enabled = false;
        // if (selectableComponentsScript != null) selectableComponentsScript.enabled = false;
        if (tracesScript != null) tracesScript.enabled = false;
        if (quickInspectionScript != null) quickInspectionScript.enabled = false;

    }

    private void DestroyWireObjects()
    {
        GameObject[] wireObjects = GameObject.FindGameObjectsWithTag("Trace");
        int count = 0;

        foreach (GameObject wire in wireObjects)
        {
            Destroy(wire);
            count++;
        }

        Debug.Log($"Destroyed {count} wire objects.");
    }

    public void HideAllAssemblyObjects()
    {
        GameObject[] assemblyObjects = GameObject.FindGameObjectsWithTag("Assembly");
        int count = 0;

        foreach (GameObject obj in assemblyObjects)
        {
            obj.SetActive(false); // Hide all assembly-related objects
            count++;
        }

        Debug.Log($"Hid {count} assembly objects.");
    }

    private void ShowAllModelObjects()
    {
        if (modelParent != null)
        {
            modelParent.SetActive(true);  // Ensure the model parent is active
        }

        GameObject[] allModelObjects = GameObject.FindGameObjectsWithTag("Model");
        int count = 0;

        foreach (GameObject obj in allModelObjects)
        {
            obj.SetActive(true); // Make sure the model is visible
            count++;
        }

        Debug.Log($"Enabled {count} model objects.");
    }



    public void OnQuickInspectionSelected()
    {
        Debug.Log("Assembly Guide Selected");

        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        quickInspectionUI.SetActive(true);
        testingUI.SetActive(false);
        // RotateWithMouseScript.SetActive(false);
        currentMode = "QuickInspection";

        if (modelParent != null)
        {
            modelParent.SetActive(true); // Ensure modelParent is active
        }

        if (quickInspectionScript != null)
        {
            quickInspectionScript.enabled = true; // Ensure script is enabled
            tracesScript.enabled = false;

        }

        DisableAllScriptsExcept(quickInspectionScript);
        HideAllAssemblyObjects();
        HideAllModelObjects();
    }

    public void OnTestingTracesSelected()
    {
        Debug.Log("Testing Traces Selected");

        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        testingUI.SetActive(true);
        quickInspectionUI.SetActive(false);
        currentMode = "Trace";

        if (tracesScript != null)
        {
            tracesScript.enabled = true;
            quickInspectionScript.enabled = false;
        }

        DisableAllScriptsExcept(tracesScript);
        HideAllAssemblyObjects();

        // ? Hide everything under traceModel except the LineRenderer
        if (traceModel != null)
        {
            foreach (Transform child in traceModel.transform)
            {
                if (child.GetComponent<LineRenderer>() == null)
                {
                    child.gameObject.SetActive(false);
                }
            }

            traceModel.SetActive(true);
        }

        if (inspectionModel != null)
            inspectionModel.SetActive(false);
    }


    private void ShowAllAssemblyObjects()
    {
        GameObject[] assemblyObjects = GameObject.FindGameObjectsWithTag("Assembly");
        int count = 0;

        foreach (GameObject obj in assemblyObjects)
        {
            obj.SetActive(true); // Show assembly-related objects when needed
            count++;
        }

        Debug.Log($"Enabled {count} assembly objects.");
    }
    private void HideAllModelObjects()
    {
        if (modelParent != null)
        {
            foreach (Transform child in modelParent.transform)
            {
                child.gameObject.SetActive(false);
            }

            Debug.Log("Hid all model objects under modelParent.");
        }
    }
    private void DisableAllScriptsExcept(MonoBehaviour activeScript)
    {

        if (tracesScript != null && tracesScript != activeScript) tracesScript.enabled = false;
        if (quickInspectionScript != null && quickInspectionScript != activeScript) quickInspectionScript.enabled = false;
        if (MenuControllerScript != null && MenuControllerScript != activeScript) MenuControllerScript.enabled = false;
    }

}

