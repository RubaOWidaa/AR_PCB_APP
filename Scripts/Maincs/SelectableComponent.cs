using UnityEngine;
using TMPro;
using Vuforia;

//[RequireComponent(typeof(Renderer))]
public class SelectableComponent: MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isSelected = false;
    private static TextMeshProUGUI selectedComponentText;

    private bool isTracking = true; // assume true in non-AR mode
    public BOMLookup bomLookup;

    void Awake()
    {

        if (bomLookup == null)
            bomLookup = FindObjectOfType<BOMLookup>();
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            originalColor = objectRenderer.material.color;

        if (selectedComponentText == null)
            selectedComponentText = GameObject.Find("SelectedComponentText")?.GetComponent<TextMeshProUGUI>();

        // If we're in AR mode and attached to an ImageTarget
        ObserverBehaviour observer = GetComponentInParent<ObserverBehaviour>();
        if (observer != null)
        {
            isTracking = false; // initially false until tracked
            observer.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED)
        {
            isTracking = true;
            Debug.Log("ImageTarget Tracked");
        }
        else
        {
            isTracking = false;
            Debug.Log(" ImageTarget Lost");
        }
    }

    void Update()
    {
        if (!isTracking)
            return;

#if UNITY_EDITOR
        // Support mouse click in editor
        if (Input.GetMouseButtonDown(0))
        {
            CheckSelection(Camera.main.ScreenPointToRay(Input.mousePosition));
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            CheckSelection(Camera.main.ScreenPointToRay(Input.GetTouch(0).position));
        }
#endif
    }

    private void CheckSelection(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            isSelected = !isSelected;
            objectRenderer.material.color = isSelected ? Color.green : originalColor;

            if (selectedComponentText != null)
            {
                selectedComponentText.text = isSelected ? $"Selected: {gameObject.name}" : "No component selected";
                bomLookup.SearchComponent(gameObject.name);

                Debug.Log($" Toggled: {gameObject.name}");
            }
        }
    }
}
