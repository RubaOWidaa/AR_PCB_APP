using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupComponents : MonoBehaviour
{
    void Start()
    {
        //GroupPCBComponents();
    }

   public void GroupPCBComponents()
    {
       
        Transform pcbRoot = transform.Find("PCB3.pdf"); // Update if necessary

        if (pcbRoot == null)
        {
            Debug.LogError("PCB root not found!");
            return;
        }

        GameObject currentGroup = null;
        int componentIndex = 1;

        foreach (Transform child in pcbRoot)
        {
            string name = child.name;

            if (name.StartsWith("Pad"))
            {
                // A Pad indicates separation, so reset the group
                currentGroup = null;
                continue;
            }

            if (name.StartsWith("ComponentBody"))
            {
                if (currentGroup == null)
                {
                    // Create a new group for the next component
                    currentGroup = new GameObject("Component_" + componentIndex);
                    currentGroup.transform.SetParent(pcbRoot);
                    componentIndex++;
                }

                // Move the ComponentBody under the current group
                child.SetParent(currentGroup.transform);
            }
        }
    }

}
