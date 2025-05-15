using UnityEngine;
using Vuforia;

public class ARModeToggle : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject arPanel;
    public GameObject nonArPanel;

    [Header("AR Settings")]
    public GameObject arCamera;

    private bool inARMode = false;

    void Start()
    {
        // Initialize to non-AR mode by default
        SetMode(inARMode);
    }

    void Update()
    {
        // Optional: Android back button to toggle mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMode();
        }
    }

    public void ToggleMode()
    {
        inARMode = !inARMode;
        SetMode(inARMode);
    }

    private void SetMode(bool isAR)
    {
        // Toggle UI Panels
        arPanel.SetActive(isAR);
        nonArPanel.SetActive(!isAR);

        // Toggle AR Camera
        arCamera.SetActive(isAR);

        // Toggle Vuforia Tracking
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = isAR;
        }

        Debug.Log("Switched to " + (isAR ? "AR Mode" : "Non-AR Mode"));
    }
}
