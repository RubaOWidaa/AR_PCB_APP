using System;
using System.Collections.Generic;
using UnityEngine;

public class PCBPadMapper : MonoBehaviour
{
    public void Start()
    {
        // Retrieve the uploaded model prefab from the FileUploader.
        FileUploader fileUploadSc = FindObjectOfType<FileUploader>();
        if (fileUploadSc == null)
        {
            Debug.LogError("FileUploader script not found.");
            return;
        }

        GameObject uploadedModelPrefab = fileUploadSc.model;
        if (uploadedModelPrefab == null)
        {
            Debug.LogError("Uploaded model prefab is missing.");
            return;
        }

        // Get the list of pads from the FileUploader class (ensure it has a public list of pads).
        List<FileUploader.PadInfo> pads = fileUploadSc.GetPads();  // Assumes you have a method in FileUploader

        if (pads == null || pads.Count == 0)
        {
            Debug.LogError("No pad information found in XML.");
            return;
        }

        // Assume grouped PCB components are now children of the uploaded model prefab.
        Transform pcbRoot = uploadedModelPrefab.transform;

        foreach (Transform group in pcbRoot)
        {
            // Only process groups whose names start with "Pad"
            if (!group.name.StartsWith("Pad"))
                continue;

            // Skip if the group has no children (no meshes).
            if (group.childCount == 0)
                continue;

            Vector3 centroid = CalculateCentroid2D(group);
            Debug.Log($"Group '{group.name}' centroid: {centroid}");

            // Find the closest pad from the XML
            FileUploader.PadInfo closestPad = FindClosestPad(centroid, pads);
            if (closestPad != null)
            {
                Debug.Log($"Assigned pad '{closestPad.name}' to group '{group.name}' at position ({closestPad.position.x}, {closestPad.position.y})");
            }
            else
            {
                Debug.LogWarning($"No matching pad found for group '{group.name}'.");
            }
        }
    }

    // Calculate the 2D centroid (ignoring y-coordinate) of a group.
    public Vector3 CalculateCentroid2D(Transform group)
    {
        Renderer[] renderers = group.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return group.position; // Fallback

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return new Vector3(bounds.center.x, 0, bounds.center.z); // Only use X and Z coordinates
    }

    // Find the closest pad from the list based on the centroid.
    private FileUploader.PadInfo FindClosestPad(Vector3 centroid, List<FileUploader.PadInfo> pads)
    {
        FileUploader.PadInfo closestPad = null;
        float minDistance = float.MaxValue;

        foreach (var pad in pads)
        {
            float distance = Vector2.Distance(new Vector2(centroid.x, centroid.z), pad.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPad = pad;
            }
        }
        return closestPad;
    }
}

