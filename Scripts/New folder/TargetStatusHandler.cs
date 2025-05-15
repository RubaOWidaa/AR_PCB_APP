using UnityEngine;
using Vuforia;

public class TargetStatusHandler : MonoBehaviour
{
    private void Start()
    {
        // Get the observer event handler on the same object
        var observerHandler = GetComponent<DefaultObserverEventHandler>();

        if (observerHandler != null)
        {
            observerHandler.OnTargetFound.AddListener(OnTargetFound);
            observerHandler.OnTargetLost.AddListener(OnTargetLost);
        }
        else
        {
            Debug.LogWarning("DefaultObserverEventHandler not found!");
        }
    }

    public void OnTargetFound()
    {
        Debug.Log("?? Target FOUND");
        // Enable interaction, UI, or scripts here
    }

    public void OnTargetLost()
    {
        Debug.Log("? Target LOST");
        // Disable interaction, UI, or scripts here
    }
}
