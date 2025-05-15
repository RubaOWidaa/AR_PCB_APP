using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class FlashAndBrightnessManager : MonoBehaviour
{
    private bool isFlashOn = false;
    private float originalBrightness;
    private CameraDevice cameraDevice;

    private void Start()
    {
        originalBrightness = Screen.brightness;

        var observerHandler = GetComponent<DefaultObserverEventHandler>();
        if (observerHandler != null)
        {
            observerHandler.OnTargetFound.AddListener(OnTargetFound);
            observerHandler.OnTargetLost.AddListener(OnTargetLost);
        }

        VuforiaApplication.Instance.OnVuforiaStarted += () =>
        {
            cameraDevice = VuforiaBehaviour.Instance?.CameraDevice;
            if (cameraDevice != null)
            {
                cameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
                Debug.Log("? Autofocus enabled");
            }
        };
    }

    private void OnTargetFound()
    {
        Screen.brightness = originalBrightness;
        Debug.Log("?? Target Found");
    }

    private void OnTargetLost()
    {
        Screen.brightness = 1.0f;
        Debug.Log("?? Target Lost");
    }

    private void Update()
    {
        // Optionally override user brightness changes
        if (VuforiaBehaviour.Instance != null && !VuforiaBehaviour.Instance.DevicePoseBehaviour.enabled)
        {
            Screen.brightness = 1.0f;
        }
    }

    // ? Call this from a button to toggle flash
    public void ToggleFlash()
    {
        if (cameraDevice == null)
        {
            cameraDevice = VuforiaBehaviour.Instance?.CameraDevice;
            if (cameraDevice == null) return;
        }

        isFlashOn = !isFlashOn;
        cameraDevice.SetFlash(isFlashOn);
        Debug.Log("?? Flash toggled: " + isFlashOn);
    }
}
