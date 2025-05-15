// MenuController3.cs - Controls Quick Inspection Mode
using UnityEngine;

public class MenuController3 : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject assemblyGuideUI;
    [SerializeField] private GameObject quickInspectionUI;
    [SerializeField] private GameObject modelViewUI;
    [SerializeField] private GameObject testingUI;
    [SerializeField] private GameObject DesignUploadPanel;

    [Header("Model References")]
    public GameObject inspectionModel;
    public quickInspection quickInspectionScript;
    public string currentMode = "QuickInspection";

    void Start()
    {
        if (inspectionModel != null)
        {
            quickInspectionScript = inspectionModel.GetComponent<quickInspection>();
            inspectionModel.SetActive(true);
            if (quickInspectionScript != null) quickInspectionScript.enabled = true;
        }

       // DisableAllUI();
        ShowMainMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) HandleBackButton();
    }

    private void HandleBackButton()
    {
        if (!mainMenuPanel.activeSelf)
        {
            Debug.Log("Returning to Main Menu...");
            ShowMainMenu();
        }
        else if (assemblyGuideUI.activeSelf || modelViewUI.activeSelf || testingUI.activeSelf || quickInspectionUI.activeSelf)
        {
            Debug.Log("Returning to Main Menu...");
            ShowMainMenu();

        }
    }

    public void ShowMainMenu()
    {
        currentMode = "None";

        DesignUploadPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        // DisableAllUI();
        DisableAllScripts();
        DestroyWireObjects();
        HideAllAssemblyObjects();

        if (inspectionModel != null) inspectionModel.SetActive(false);
    }

    public void ShowDesignUploadMenu()
    {
        DesignUploadPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
    
        //  DisableAllUI();
        DesignUploadPanel.SetActive(true);
        DisableAllScripts();
        DestroyWireObjects();
        HideAllAssemblyObjects();

        if (inspectionModel != null) inspectionModel.SetActive(false);
    }

    public void OnQuickInspectionSelected()
    {
        currentMode = "QuickInspection";
        DesignUploadPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(true);
       
        // DisableAllUI();
        quickInspectionUI.SetActive(true);

        if (inspectionModel != null)
        {
            inspectionModel.SetActive(true);
            quickInspectionScript = inspectionModel.GetComponent<quickInspection>();
            if (quickInspectionScript != null) quickInspectionScript.enabled = true;
        }

        HideAllAssemblyObjects();
        HideAllModelObjects(inspectionModel);
    }

   /* private void DisableAllUI()
    {
        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        DesignUploadPanel.SetActive(false);
    }*/

    private void DisableAllScripts()
    {
        if (quickInspectionScript != null) quickInspectionScript.enabled = false;
    }

    private void DestroyWireObjects()
    {
        foreach (GameObject wire in GameObject.FindGameObjectsWithTag("Trace"))
            Destroy(wire);
    }

    private void HideAllAssemblyObjects()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Assembly"))
            obj.SetActive(false);
    }

    private void HideAllModelObjects(GameObject model)
    {
        if (model != null)
        {
            foreach (Transform child in model.transform)
                child.gameObject.SetActive(false);
        }
    }
}
