using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class quickInspection : MonoBehaviour
{
    private Transform[] nettraces;
    private Transform[] nontraces;

    // private RuntimeImageTarget runtimeImageTargetSc;
    public GameObject uploadedModel;
    public TextMeshProUGUI netText;

    public Button restartButton; // Assign this in the inspector if using a UI button

    public void Start()
    {
        /*if (uploadedModel != null)
        {
            InitializeNets();
            StartCoroutine(QuickInspectNets());
        }
        
        foreach (var net in nettraces)
        if (net != null) net.gameObject.SetActive(false);
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartInspection);
        }*/

    }

    private void InitializeNets()
    {
        var netList = new List<Transform>();
        var excludedList = new List<Transform>();

        foreach (Transform child in uploadedModel.transform)
        {
            if (child == null) continue;

            string lowerName = child.name.ToLower();
            Debug.Log("Found child: " + child.name);

            if (lowerName.StartsWith("net"))
            {
                netList.Add(child);
                child.gameObject.SetActive(false);
            }
            else
            {
                excludedList.Add(child);
                child.gameObject.SetActive(false);
            }
        }

        nettraces = netList.ToArray();
        nontraces = excludedList.ToArray();

        Debug.Log("Total nets: " + nettraces.Length);
       
    }
    public void InitializeAndRunInspection()
    {
        if (uploadedModel == null)
        {
            Debug.LogError("uploadedModel is still null!");
            return;
        }

        InitializeNets();
        StartCoroutine(QuickInspectNets());
    }

    private IEnumerator QuickInspectNets()
    {
        for (int i = 0; i < nettraces.Length; i++)
        {
            foreach (var net in nettraces)
                if (net != null) net.gameObject.SetActive(false);

            foreach (var obj in nontraces)
                if (obj != null) obj.gameObject.SetActive(false);

            if (nettraces[i] != null)
            {
                nettraces[i].gameObject.SetActive(true);
                string message = "Activating: " + nettraces[i].name;
                Debug.Log(message);
                netText.text = message; // Output to TextMeshProUGUI
            }
            else
            {
                Debug.LogWarning("nettraces[" + i + "] is null.");
            }

            yield return new WaitForSeconds(4f);
        }

        // Optional cleanup
        foreach (var net in nettraces)
            if (net != null) net.gameObject.SetActive(false);
    }

    public void RestartInspection()
    {
        StopAllCoroutines();
        StartCoroutine(QuickInspectNets());
    }
}

