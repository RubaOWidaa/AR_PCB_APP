using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupComponent : MonoBehaviour
{
    public GameObject prefabWithScript;

    void Start()
    {
        AddCollidersAndScriptsToDirectChildren(transform);
    }

    private void AddCollidersAndScriptsToDirectChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Add MeshCollider if not present
            if (child.GetComponent<MeshCollider>() == null)
            {
                MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = false;
                Debug.Log($"MeshCollider added to {child.gameObject.name}");
            }
            if (child.GetComponent<Collider>() == null)
            {
                child.gameObject.AddComponent<BoxCollider>();
                Debug.Log($"BoxCollider added to {child.name}");
            }
            // Add SelectableComponent if not present
            if (prefabWithScript != null && child.GetComponent<SelectableComponent>() == null)
            {
                child.gameObject.AddComponent<SelectableComponent>();
                Debug.Log($"SelectableComponent added to {child.gameObject.name}");
            }

            //  Don't recurse: Only process direct children
            // AddCollidersAndScriptsToDirectChildren(child); // <--- remove or comment this line
        }
    }
}







