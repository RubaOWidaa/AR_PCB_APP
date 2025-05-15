using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject assemblyGuideUI;
    [SerializeField] private GameObject quickInspectionUI;
    [SerializeField] private GameObject modelViewUI;
    [SerializeField] private GameObject testingUI;
    [SerializeField] private GameObject nonARModelButton;
    [SerializeField] private GameObject DesignUploadPanel;//  button for Non-AR mode
    [SerializeField] private GameObject nonArModel;
    // [SerializeField] private GameObject DataSheetViewerButton;  // button for datasheet mode
   /* [Header("3D Models")]
    [SerializeField] private GameObject modelParent;   // AR-tracked PCB model
    [SerializeField] private GameObject nonArModel;    // Standalone PCB model for Non-AR
   */
    [Header("Cameras")]
    [SerializeField] private GameObject arCamera;      // Vuforia ARCamera
    [SerializeField] private GameObject nonArCamera;   // Unity Main Camera

    [Header("Model Parent")]
    public GameObject modelParent;
    public GameObject NONARmodelParent;


    public AssemblyGuide assemblyGuideScript;
    public SelectableComponent selectableComponentsScript;
    public SetupComponent SetupComponentScript;
    // public trace tracesScript;
    // public quickInspection quickInspectionScript;
    public MenuController2 MenuController2Script;
    public RotateWithMouse RotateWithMouseScript;
   // public SelectableComponent SelectableComponentsc;
    public void Start()
   {
        assemblyGuideScript = modelParent.GetComponent<AssemblyGuide>();
        selectableComponentsScript = modelParent.GetComponent<SelectableComponent>();
        SetupComponentScript = modelParent.GetComponent<SetupComponent>();
        RotateWithMouseScript = modelParent.GetComponent<RotateWithMouse>();


        // tracesScript = modelParent.GetComponent<trace>();
        // quickInspectionScript = modelParent.GetComponent<quickInspection>();
        DisableAllScripts();
        HideAllAssemblyObjects();
        ShowMainMenu();


    }
    private void Awake()
    {
        if (modelParent == null)
        {
            modelParent = FindObjectOfType<RuntimeImageTarget>()?.uploadedModelPrefab1;

        }
    }

      public void Update()
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
       else if (nonArModel.activeSelf)
            On3DModelViewSelected();
        }
    private void ShowDesignUploadMenu() {
        DesignUploadPanel.SetActive(true);
        mainMenuPanel.SetActive(false) ;
        assemblyGuideUI.SetActive(false);
        modelViewUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        nonArModel.SetActive(false);

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
        nonArModel.SetActive(false);

        // RotateWithMouseScript.SetActive(false);

        DisableAllScripts();
        DestroyWireObjects();
        HideAllAssemblyObjects();

        // Hide the 3D model when returning to the menu
        if (modelParent != null)
        {
            modelParent.SetActive(false);
        }
    }

    public void DisableAllScripts()
    {
        if (assemblyGuideScript != null) assemblyGuideScript.enabled = false;
        if (selectableComponentsScript != null) selectableComponentsScript.enabled = false;
        if (SetupComponentScript != null) SetupComponentScript.enabled = false;
        if (RotateWithMouseScript != null) RotateWithMouseScript.enabled = false;


        // if (tracesScript != null) tracesScript.enabled = false;
    }

    public void DestroyWireObjects()
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

    /*private void ShowAllModelObjects()
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
    }*/
    private void ShowAllModelObjects()
    {
        if (modelParent != null)
        {
            modelParent.SetActive(true);

            foreach (Transform child in modelParent.transform)
            {
                child.gameObject.SetActive(true);
            }

            Debug.Log("Restored all child components under modelParent.");
        }
    }



    public void On3DModelViewSelected()
    {
        Debug.Log("3D Model View Selected");

        mainMenuPanel.SetActive(false);
        modelViewUI.SetActive(true);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);

        RotateWithMouseScript.enabled=false;
        SetupComponentScript.enabled = false;
        selectableComponentsScript.enabled = false;

        // Ensure model parent is active
        if (modelParent != null)
        {
            modelParent.SetActive(true);
        }

        ShowAllModelObjects();
        nonArModel.SetActive(false);
        arCamera?.SetActive(true);
        nonArCamera?.SetActive(false);


        /*   ShowAllModelObjects();

           if (selectableComponentsScript != null)
           {
               selectableComponentsScript.enabled = true;

           }

           if (RotateWithMouseScript != null)
           {
               RotateWithMouseScript.enabled = true;
           }*/

        // DisableAllScriptsExcept(selectableComponentsScript);
    }
   /* public void OnViewIn3DNonAR()
    {
        Debug.Log("Switching to Non-AR Model Viewer...");
        //  SceneManager.LoadScene("NonAR"); // Load the Non-AR Scene
        SceneManager.LoadScene("NonAR", LoadSceneMode.Additive);
        nonArModel.SetActive(false);


    }
    /*public void OnDataSheetViewer()
    {
        Debug.Log("Switching to DataSheetViewer...");
        SceneManager.LoadScene("Sample"); // Load the Non-AR Scene
    }*/


    public void OnAssemblyGuideSelected()
    {
        Debug.Log("Assembly Guide Selected");

        mainMenuPanel.SetActive(false);
        modelViewUI.SetActive(false);
        assemblyGuideUI.SetActive(true);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        // RotateWithMouseScript.SetActive(false);
        /* RotateWithMouseScript.enabled = false;
         SetupComponentScript.enabled = false;
         selectableComponentsScript.enabled = false;*/
        nonArModel.SetActive(false);



        if (modelParent != null)
        {
            modelParent.SetActive(true); // Ensure modelParent is active
        }

        if (assemblyGuideScript != null)
        {
            assemblyGuideScript.enabled = true; // Ensure script is enabled
        }

        DisableAllScriptsExcept(assemblyGuideScript);
        ShowAllAssemblyObjects();
        HideAllModelObjects(); // Add this line to clean up first

    }

    public void OnNonArSelected()
    {
        Debug.Log("Non-AR Mode Selected");

        mainMenuPanel.SetActive(false);
        assemblyGuideUI.SetActive(false);
        testingUI.SetActive(false);
        quickInspectionUI.SetActive(false);
        modelViewUI.SetActive(false);
        nonArModel.SetActive(true);

        // Disable AR camera and enable Non-AR camera
        arCamera?.SetActive(false);
        nonArCamera?.SetActive(true);

        // Set the camera background to black
        Camera cam = nonArCamera.GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // Set up the model in view
        if (NONARmodelParent != null)
        {
            NONARmodelParent.SetActive(true);

            // Position in front of the camera
            Vector3 cameraPosition = nonArCamera.transform.position;
            Vector3 cameraForward = nonArCamera.transform.forward;
           // NONARmodelParent.transform.position = cameraPosition + cameraForward * 5f;

            // Face the camera
            NONARmodelParent.transform.rotation = Quaternion.LookRotation(cameraForward) * Quaternion.Euler(0, 180, 0);
            Vector3 upward = new Vector3(0, 0.5f, 0); // 1.5 units up
            NONARmodelParent.transform.position = cameraPosition + cameraForward * 5f + upward;


            // Scale down the model
            NONARmodelParent.transform.localScale = Vector3.one * 0.35f;
        }
    }


    //DisableAllScriptsExcept(assemblyGuideScript);
    //ShowAllAssemblyObjects();
    //HideAllModelObjects(); // Add this line to clean up first


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


    /* public void OnQuickInspectionSelected()
     {
         Debug.Log("Assembly Guide Selected");

         mainMenuPanel.SetActive(false);
         assemblyGuideUI.SetActive(false);
         quickInspectionUI.SetActive(true);
         testingUI.SetActive(false);
         // RotateWithMouseScript.SetActive(false);


         if (modelParent != null)
         {
             modelParent.SetActive(true); // Ensure modelParent is active
         }

        /* if (quickInspectionScript != null)
         {
             quickInspectionScript.enabled = true; // Ensure script is enabled
         }*/

    //  DisableAllScriptsExcept(quickInspectionScript);
    //   HideAllAssemblyObjects();
    //}

    /*  public void OnTestingTracesSelected()
      {
          Debug.Log("Testing Traces Selected");

          mainMenuPanel.SetActive(false);
          assemblyGuideUI.SetActive(false);
          testingUI.SetActive(true);
          quickInspectionUI.SetActive(false);
          // RotateWithMouseScript.SetActive(false);


          /*if (tracesScript != null)
          {
              tracesScript.enabled = true;
          }*/

    //DisableAllScriptsExcept(tracesScript);
    //HideAllAssemblyObjects();
    //}

    public void ShowAllAssemblyObjects()
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

   public void DisableAllScriptsExcept(MonoBehaviour activeScript) 
    { 
        if (assemblyGuideScript != null && assemblyGuideScript != activeScript) assemblyGuideScript.enabled = false;
        if (SetupComponentScript != null && SetupComponentScript != activeScript) SetupComponentScript.enabled = false;
       // if (tracesScript != null && tracesScript != activeScript) tracesScript.enabled = false;
       // if (quickInspectionScript != null && quickInspectionScript != activeScript) quickInspectionScript.enabled = false;
        if (MenuController2Script != null && MenuController2Script != activeScript) MenuController2Script.enabled = false;
    }

}
