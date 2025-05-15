using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssemblyGuide : MonoBehaviour
{
    public GameObject layerToggleUIPrefab;       // UI step indicator prefab
    [SerializeField] private Transform uiParent; // Parent for toggle indicators
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TMP_Text stepIndicator;

    public BOMLookup bomLookup;

    // === Preview-related fields ===
    public Camera previewCamera;
    public RenderTexture previewTexture;
    public RawImage previewImage;
    public Transform previewAnchor;  // Position for instantiating preview copy
    public Transform modelRoot;      // Root containing all original component models

    // === Component handling ===
    private Transform[] components;
    private Transform[] nonComponentObjects;
    private int currentStep = 0;

    private void Start()
    {
        InitializeComponents();
        UpdateUI();

        if (bomLookup == null)
            bomLookup = FindObjectOfType<BOMLookup>();
    }

    private void InitializeComponents()
    {
        var componentList = new System.Collections.Generic.List<Transform>();
        var excludedList = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in transform)
        {
            string lowerName = child.name.ToLower();
            if (lowerName.Contains("silk") || lowerName.Contains("board") || lowerName.Contains("no_net") || lowerName.Contains("nets"))
            {
                excludedList.Add(child);
                child.gameObject.SetActive(false);
            }
            else
            {
                componentList.Add(child);
                child.gameObject.SetActive(false);
                CreateStepIndicator(componentList.Count, child.name);
            }
        }

        components = componentList.ToArray();
        nonComponentObjects = excludedList.ToArray();

        if (components.Length > 0)
        {
            components[currentStep].gameObject.SetActive(true);
        }

        if (nextButton != null) nextButton.onClick.AddListener(GoToNextStep);
        if (previousButton != null) previousButton.onClick.AddListener(GoToPreviousStep);
    }

    private void CreateStepIndicator(int stepNumber, string componentName)
    {
        if (layerToggleUIPrefab == null || uiParent == null)
        {
            Debug.LogWarning("LayerToggleUIPrefab or UIParent not assigned.");
            return;
        }

        GameObject toggleGO = Instantiate(layerToggleUIPrefab, uiParent);
        TMP_Text label = toggleGO.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = $"{stepNumber}: {componentName}";

        Toggle toggle = toggleGO.GetComponent<Toggle>();
        if (toggle != null)
            toggle.interactable = false;
    }

    private void GoToNextStep()
    {
        if (currentStep < components.Length - 1)
        {
            components[currentStep].gameObject.SetActive(false);
            currentStep++;
            components[currentStep].gameObject.SetActive(true);
            UpdateUI();
        }
    }

    private void GoToPreviousStep()
    {
        if (currentStep > 0)
        {
            components[currentStep].gameObject.SetActive(false);
            currentStep--;
            components[currentStep].gameObject.SetActive(true);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (stepIndicator != null)
            stepIndicator.text = $"Step {currentStep + 1} of {components.Length}: {components[currentStep].name}";

        if (nextButton != null)
            nextButton.interactable = currentStep < components.Length - 1;

        if (previousButton != null)
            previousButton.interactable = currentStep > 0;

        if (bomLookup != null)
            bomLookup.SearchComponent(components[currentStep].name);

        //  Show preview of component by name
        ShowModelPreview(components[currentStep].name);
    }

    // === 3D Model Preview ===
    private void ShowModelPreview(string componentName)
    {
        if (modelRoot == null)
        {
            Debug.LogWarning("Model root not assigned.");
            return;
        }

        Transform source = modelRoot.Find(componentName);
        if (source == null)
        {
            Debug.LogWarning($"No matching model found in modelRoot for: {componentName}");
            return;
        }

        foreach (Transform child in previewAnchor)
        {
            Destroy(child.gameObject);
        }

        GameObject previewObj = Instantiate(source.gameObject, previewAnchor);
        SetLayerRecursively(previewObj.transform, LayerMask.NameToLayer("PreviewOnly"));
        previewObj.transform.localPosition = Vector3.zero;
        previewObj.transform.localRotation = Quaternion.Euler(20f, -30f, 0f);
        previewObj.transform.localScale = Vector3.one;

        if (previewCamera != null)
        {
            previewCamera.cullingMask = 1 << LayerMask.NameToLayer("PreviewOnly");
            previewCamera.targetTexture = previewTexture;
        }

        if (previewImage != null)
        {
            previewImage.texture = previewTexture;
        }
    }

    private void SetLayerRecursively(Transform obj, int newLayer)
    {
        obj.gameObject.layer = newLayer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, newLayer);
        }
    }
}
