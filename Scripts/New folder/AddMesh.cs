using UnityEngine;

public class AddMeshColliders : MonoBehaviour
{
    void Awake()
    {
        // Add MeshCollider to the parent GameObject if it has a MeshFilter
        if (GetComponent<MeshFilter>() != null && GetComponent<MeshCollider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
            Debug.Log($"MeshCollider added to {gameObject.name}");
        }

        // Iterate through direct children and add MeshCollider if they have a MeshFilter
        foreach (Transform child in transform)
        {
            if (child.GetComponent<MeshFilter>() != null && child.GetComponent<MeshCollider>() == null)
            {
                child.gameObject.AddComponent<MeshCollider>();
                Debug.Log($"MeshCollider added to {child.gameObject.name}");
            }
        }
    }
}

