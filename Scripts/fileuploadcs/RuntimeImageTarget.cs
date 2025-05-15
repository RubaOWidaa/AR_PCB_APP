using UnityEngine;
using UnityEngine.Networking;
using Vuforia;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class RuntimeImageTarget : MonoBehaviour
{
    public enum Mode
    {
        QuickInspection,
        Trace
    }

    public Mode currentMode = Mode.QuickInspection;
    [Header("Image Settings")]
    public string imagePath1;
    public string imagePath2;
    public float physicalWidth = 0.1f;

    [Header("Model Settings")]
    public GameObject modelPrefab1;
    public GameObject modelPrefab2;

    private Texture2D runtimeTexture1;
    private Texture2D runtimeTexture2;
    private GameObject modelInstance1;
    private GameObject modelInstance2;

    [Header("Inspector Preconfigured Image Targets")]
    public ImageTargetBehaviour inspectorTarget3;
    public ImageTargetBehaviour inspectorTarget4;
    public GameObject modelInstance3;
    public GameObject modelInstance4;
      public GameObject inspectionModel;
    public GameObject traceModel;

    public GameObject uploadedModelPrefab1;
    public GameObject uploadedModelPrefab;

    public GameObject uploadedModelPrefab2;
    public GameObject modelInstanceTop;
    public GameObject modelInstanceBottom;


    public void Start()
    {
        FileUploader fileUploadSc = FindObjectOfType<FileUploader>();
        CaptureChildObject captureScript = FindObjectOfType<CaptureChildObject>();
        // MenuController MenuControllerScript = FindObjectOfType<MenuController>();
        trace traceScript = FindObjectOfType<trace>();
        //XMLSorter XMLSorterScript = FindObjectOfType<XMLSorter>(); 
        quickInspection quickInspectionScript = FindObjectOfType<quickInspection>();
        //RuntimeImageTargetHandler RuntimeImageTargetHandlerSc = FindObjectOfType<RuntimeImageTargetHandler>();


        modelPrefab2 = Instantiate(fileUploadSc.model);
        //modelPrefab2.transform.Rotate(180f, 0, 90f);

        modelPrefab1 = Instantiate(fileUploadSc.model);
       // modelPrefab1.transform.Rotate(180f, 0, 0);

        physicalWidth = (fileUploadSc.PCBwidth) * 0.1f;
       // uploadedModelPrefab1 = fileUploadSc.model;
    
        uploadedModelPrefab = fileUploadSc.model;

        if (modelInstanceTop == null && modelInstanceBottom == null && uploadedModelPrefab != null)
        {
            // Create Top Model
           /* modelInstanceTop = Instantiate(uploadedModelPrefab);
            modelInstanceTop.name = "TopLayerModel";
            modelInstanceTop.SetActive(true); */

            // Create Bottom Model
            modelInstanceBottom = Instantiate(uploadedModelPrefab);
            modelInstanceBottom.name = "BottomLayerModel";
           // modelInstanceBottom.transform.Rotate(0f, 180f, 0f); // Flip Y-axis
            modelInstanceBottom.SetActive(false); // Only show when tracked
        }
        uploadedModelPrefab1 = modelInstanceTop;
        uploadedModelPrefab2 = modelInstanceBottom;
        if (captureScript != null)
        {
            imagePath1 = captureScript.bottomLayerImagePath;
            imagePath2 = captureScript.topLayerImagePath;
        }

        if (uploadedModelPrefab1 != null)
        {
            GroupPCBComponents(uploadedModelPrefab1.transform);
        }
        if (uploadedModelPrefab2 != null)
        {
            GroupPCBComponents(uploadedModelPrefab2.transform);
        }

        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
       // AttachModelToInspectorTarget(inspectorTarget3, modelInstance4);
        AttachModelToInspectorTarget(inspectorTarget4, modelInstance3);
       // traceScript.pcbModelTransform = uploadedModelPrefab1.transform;
      //  traceScript.pcbModelTransform = uploadedModelPrefab2.transform;


       //  quickInspectionScript.uploadedModel = uploadedModelPrefab2;
        //quickInspectionScript.uploadedModel = uploadedModelPrefab1;


    }

    public void CopyAllComponents(GameObject source, GameObject destination)
    {
        // Get all components of the source GameObject
        Component[] components = source.GetComponents<Component>();


        // Loop through each component and copy it to the new GameObject
        foreach (Component component in components)
        {
            // Add the same type of component to the new GameObject
            Component newComponent = destination.AddComponent(component.GetType());
            
            // Copy values from the original component to the new one
            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                // Ensure the field is not static and is not read-only
                if (!field.IsStatic)
                {
                    try
                    {
                        // Copy the value from the original component to the new component
                        field.SetValue(newComponent, field.GetValue(component));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to copy field {field.Name}: {ex.Message}");
                    }
                }

            }

        }
    }
   

    void OnVuforiaStarted()
    {
        StartCoroutine(LoadBothImagesSequentially());
    }

    IEnumerator LoadBothImagesSequentially()
    {
        yield return StartCoroutine(LoadImageAndCreateTarget(imagePath2, 2));
        yield return StartCoroutine(LoadImageAndCreateTarget(imagePath1, 1));
    }

    void AttachModelToInspectorTarget(ImageTargetBehaviour target, GameObject modelInstance)
    {
        if (target == inspectorTarget3)
        {
            if (inspectorTarget3 != null && modelInstanceTop != null)
            {
                modelInstance4 = modelInstanceTop;  //  Reuse existing top layer
                modelInstance4.transform.SetParent(inspectorTarget3.transform, false);

                // If needed, you can reset local transforms (optional if already set correctly)
                modelInstance4.transform.localPosition = Vector3.zero;
                modelInstance4.transform.localRotation = Quaternion.identity;
                // modelInstance4.transform.localScale = Vector3.one; // optional


                MenuController MenuControllerScript = FindObjectOfType<MenuController>();

                if (MenuControllerScript == null)
                {
                    Debug.LogError("MenuController not found in the scene!");
                    return;
                }

                MenuControllerScript.modelParent = modelInstance4;

                GameObject New = GameObject.Find("New");
                if (New == null)
                {
                    Debug.LogError("GameObject 'New' not found!");
                    return;
                }

                Debug.Log("Copying components from 'New' to modelInstance3.");
                // CopyAllComponents(New, MenuControllerScript.modelParent);
                /*  CopyComponent<AssemblyGuide>(New, MenuControllerScript.modelParent);
                  CopyComponent<MenuController>(New, MenuControllerScript.modelParent);
                  CopyComponent<SelectableComponent>(New, MenuControllerScript.modelParent);
                  CopyComponent<SetupComponent>(New, MenuControllerScript.modelParent);
                  CopyComponent<RotateWithMouse>(New, MenuControllerScript.modelParent);*/


                // traceScript.pcbModelTransform = modelInstance3.transform;
                //  MenuControllerScript.assemblyGuideScript = MenuControllerScript.modelParent.GetComponent<AssemblyGuide>();
                //  MenuControllerScript.selectableComponentsScript = MenuControllerScript.modelParent.GetComponent<SelectableComponent>();
                //  MenuControllerScript.SetupComponentScript = MenuControllerScript.modelParent.GetComponent<SetupComponent>();
                // MenuControllerScript.RotateWithMouseScript = MenuControllerScript.modelParent.GetComponent<RotateWithMouse>();


                //SetupComponent SetupComponentScript = FindObjectOfType<SetupComponent>();
                // SetupComponentScript.prefabWithScript = modelInstance3;


                float modelWidth = GetModelWidth(modelInstance4);
                if (modelWidth > 0)
                {
                    float scaleFactor = physicalWidth / modelWidth;
                    modelInstance4.transform.localScale = Vector3.one * scaleFactor;
                }

                modelInstance4.SetActive(false); // Ensure it is disabled initially

                inspectorTarget4.OnTargetStatusChanged += (behaviour, status) =>
                {
                    bool isTracking = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;
                    modelInstance4.SetActive(isTracking);
                    Debug.Log("ModelInstance3 tracking status: " + isTracking);
                };

                Debug.Log("ModelInstance3 successfully attached.");
            }
        }
        else if (target == inspectorTarget4)
        {
            if (inspectorTarget4 != null && uploadedModelPrefab2 != null)
            {
                // === Create Trace Model Instance ===
                GameObject traceModelInstance = Instantiate(uploadedModelPrefab2, inspectorTarget4.transform);
                traceModelInstance.name = "TraceModelInstance";


                traceModelInstance.transform.localPosition = Vector3.zero;
                traceModelInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Specific to Trace
                traceModelInstance.transform.localScale = Vector3.one;

                // === Create Quick Inspection Model Instance ===
                GameObject inspectionModelInstance = Instantiate(uploadedModelPrefab2, inspectorTarget4.transform);
                inspectionModelInstance.name = "InspectionModelInstance";
                inspectionModelInstance.transform.localPosition = Vector3.zero;
                inspectionModelInstance.transform.localRotation = Quaternion.Euler(270f, 0f, -180f); // Specific to Quick Inspection
                inspectionModelInstance.transform.localScale = Vector3.one;

                // === Optional: Apply material color to inspection model for visual difference ===
                Renderer[] renderers = inspectionModelInstance.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    foreach (Material mat in rend.materials)
                    {
                        mat.color = Color.green;
                    }
                }

                // === Assign to quickInspection ===
                quickInspection quickInspectionScript = FindAnyObjectByType<quickInspection>();
                quickInspectionScript.uploadedModel = inspectionModelInstance;
                quickInspectionScript.InitializeAndRunInspection();

                // === Assign trace script ===
                trace traceScript = FindObjectOfType<trace>();
                traceScript.pcbModelTransform = traceModelInstance.transform;

                // === Assign both to MenuController2 ===
                MenuController2 menuController = FindObjectOfType<MenuController2>();
                if (menuController == null)
                {
                    Debug.LogError("MenuController2 not found in the scene!");
                    return;
                }

                GameObject New = GameObject.Find("New");
                if (New == null)
                {
                    Debug.LogError("GameObject 'New' not found!");
                    return;
                }

                if (modelInstanceBottom != null)
                {
                    modelInstanceBottom.SetActive(false);
                }

                // === Copy scripts to both instances ===
                Debug.Log("Copying components to trace and inspection models...");
                CopyComponent<trace>(New, traceModelInstance);
                CopyComponent<quickInspection>(New, inspectionModelInstance);
                CopyComponent<MenuController2>(New, traceModelInstance);
                CopyComponent<MenuController2>(New, inspectionModelInstance);

                // === Assign references to menu controller ===
                menuController.traceModel = traceModelInstance;
                menuController.inspectionModel = inspectionModelInstance;
                menuController.tracesScript = traceModelInstance.GetComponent<trace>();
                menuController.quickInspectionScript = inspectionModelInstance.GetComponent<quickInspection>();
                menuController.ShowMainMenu();
                // === Initially disable both ===
                traceModelInstance.SetActive(false);
                inspectionModelInstance.SetActive(false);

                // === Track and show based on current mode ===
                inspectorTarget4.OnTargetStatusChanged += (behaviour, status) =>
                {
                    bool isTracking = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

                    // These booleans should be set based on user mode
                    bool isTraceModeEnabled = menuController.currentMode == "Trace";
                    bool isInspectionModeEnabled = menuController.currentMode == "QuickInspection";

                    traceModelInstance.SetActive(isTracking && isTraceModeEnabled);
                    inspectionModelInstance.SetActive(isTracking && isInspectionModeEnabled);

                    Debug.Log("Trace model tracking status: " + traceModelInstance.activeSelf);
                    Debug.Log("Inspection model tracking status: " + inspectionModelInstance.activeSelf);
                };

                Debug.Log("Both model instances successfully created and assigned.");
            }
        }

    }
    void GroupPCBComponents(Transform pcbRoot)
    {
        if (pcbRoot == null) return;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in pcbRoot)
        {
            children.Add(child);
        }

        List<Transform> componentGroup = new List<Transform>();
        string lastPadPrefix = null;

        foreach (Transform child in children)
        {
            string name = child.name;

            if (name.Contains("Pad"))
            {
                // Extract and store the prefix, e.g., "R1" from "R1_pad1"
                lastPadPrefix = name.Split('_')[0];

                if (componentGroup.Count > 0)
                {
                    CreateComponentGroup(pcbRoot, componentGroup, lastPadPrefix);
                    componentGroup.Clear();
                }
                continue;
            }

            if (name.StartsWith("ComponentBody"))
            {
                componentGroup.Add(child);
            }
        }

        // Final group
        if (componentGroup.Count > 0)
        {
            CreateComponentGroup(pcbRoot, componentGroup, lastPadPrefix ?? "Unknown");
        }

        foreach (Transform child in pcbRoot.transform)
        {
            // Add MeshCollider if not present
            if (child.GetComponent<MeshCollider>() == null)
            {
                MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = false;
                Debug.Log($"MeshCollider added to {child.gameObject.name}");
            }
        }

    }

    void CreateComponentGroup(Transform pcbRoot, List<Transform> components, string groupName)
    {
        GameObject newGroup = new GameObject(groupName);
        newGroup.transform.SetParent(pcbRoot);

        foreach (Transform component in components)
        {
            component.SetParent(newGroup.transform);
        }
    }


    public T CopyComponent<T>(GameObject source, GameObject destination) where T : Component
    {
        T original = source.GetComponent<T>();
        if (original == null)
        {
            Debug.LogWarning($"Component of type {typeof(T)} not found on source.");
            return null;
        }

        T copy = destination.AddComponent<T>();
        Type type = typeof(T);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        FieldInfo[] fields = type.GetFields(flags);
        foreach (FieldInfo field in fields)
        {
            if (!field.IsStatic)
            {
                try
                {
                    field.SetValue(copy, field.GetValue(original));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to copy field '{field.Name}' from {type.Name}: {e.Message}");
                }
            }
        }

        // Disable all copied MonoBehaviours by default
        if (copy is MonoBehaviour mono)
        {
            mono.enabled = typeof(T) == typeof(MenuController2) || typeof(T) == typeof(MenuController);

            // Enable ONLY if it's MenuController2
            // Enable ONLY if it's MenuController2
        }

        return copy;
    }
    IEnumerator LoadImageAndCreateTarget(string pathOrUrl, int targetIndex)
    {
        if (string.IsNullOrEmpty(pathOrUrl))
        {
            Debug.LogError($"Image path/URL {targetIndex} is empty.");
            yield break;
        }

        if (!pathOrUrl.StartsWith("http") && !pathOrUrl.StartsWith("file://"))
        {
            pathOrUrl = "file://" + pathOrUrl;
            Debug.Log($"Loading image for target {targetIndex} from: {pathOrUrl}");

        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(pathOrUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D loadedTexture = DownloadHandlerTexture.GetContent(uwr);
                if (loadedTexture != null)
                {
                    if (targetIndex == 2)
                    {
                        runtimeTexture1 = loadedTexture;
                        CreateRuntimeImageTarget(runtimeTexture1, modelPrefab1, "Target1");
                    }
                    else if (targetIndex == 1)
                    {
                        runtimeTexture2 = loadedTexture;
                        CreateRuntimeImageTarget(runtimeTexture2, modelPrefab2, "Target2");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to create Texture2D for target {targetIndex}.");
                }
            }
            else
            {
                Debug.LogError($"Failed to load image for target {targetIndex}. Error: {uwr.error}");
            }
        }
    }

    /// Creates a runtime image target using the loaded Texture2D.
    void CreateRuntimeImageTarget(Texture2D image, GameObject modelPrefab, string targetName)
    {
        if (VuforiaBehaviour.Instance == null)
        {
            Debug.LogError("Vuforia is not initialized.");
            return;
        }

        var observerFactory = VuforiaBehaviour.Instance.ObserverFactory;
        if (observerFactory == null)
        {
            Debug.LogError("ObserverFactory is not available.");
            return;
        }

        // Create Image Target using physicalWidth (which can be set from your XML dimensions)
        ImageTargetBehaviour imageTargetBehaviour = observerFactory.CreateImageTarget(
            image, physicalWidth, targetName);

        if (imageTargetBehaviour != null)
        {
            Debug.Log($"Runtime Image Target '{targetName}' created successfully!");

            imageTargetBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;

            // Position the targets distinctly in the scene
            if (targetName == "Target1")
                imageTargetBehaviour.transform.position = new Vector3(-0.5f, 0, 1f);
            else if (targetName == "Target2")
                imageTargetBehaviour.transform.position = new Vector3(0.5f, 0, 1f);

            imageTargetBehaviour.transform.eulerAngles = Vector3.zero;

            // Instead of using a quad, we now only attach the 3D model.
            if (modelPrefab != null)
            {
                GameObject modelInstance = Instantiate(modelPrefab, imageTargetBehaviour.transform);
                modelInstance.transform.localPosition = Vector3.zero;

                // Measure the model's current width and scale it to match the target's physical width
                float modelWidth = GetModelWidth(modelInstance);
                if (modelWidth > 0)
                {
                    float scaleFactor = physicalWidth / modelWidth;
                    modelInstance.transform.localScale = Vector3.one * scaleFactor;

                }
                else
                {
                    Debug.LogWarning("Unable to measure model width; using default scale.");
                    modelInstance.transform.localScale = Vector3.one * 0.1f;
                }

                modelInstance.SetActive(false);

                // Store the model instance for status tracking
                if (targetName == "Target1") modelInstance1 = modelInstance;
                else if (targetName == "Target2") modelInstance2 = modelInstance;
            }
        }
        else
        {
            Debug.LogError($"Failed to create runtime image target '{targetName}'.");
        }
    }
    /// Helper function to measure the width of a model (using its renderers).
    float GetModelWidth(GameObject model)
    {
        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.size.x;
    }
   

    /// Callback for tracking status changes, enabling or disabling the associated model.
    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        Debug.Log($"Target Status: {status.Status} | {status.StatusInfo}");
        if (behaviour.TargetName == "Target1" && modelInstance1 != null)
        {
            modelInstance1.SetActive(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }
        else if (behaviour.TargetName == "Target2" && modelInstance2 != null)
        {
            modelInstance2.SetActive(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }
       /* else if (behaviour.TargetName == "InspectorTarget3" && modelInstance3 != null)
        {
            modelInstance3.SetActive(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }*/
        else if (behaviour.TargetName == "InspectorTarget4" && modelInstance3 != null)
        {
            modelInstance3.SetActive(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }
    }
}