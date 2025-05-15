using UnityEngine;
using Vuforia;

public class ThirdModelAttacher : MonoBehaviour
{
    [Tooltip("The third model prefab to attach.")]
   // public GameObject thirdModelPrefab;

    public void Start()
    {

        FileUploader fileUploadSc = FindObjectOfType<FileUploader>();
        // Find your database image target in the scene
        ImageTargetBehaviour imageTarget = FindObjectOfType<ImageTargetBehaviour>();
        if (imageTarget != null)
        {
            // Instantiate the third model and parent it to the image target
            GameObject thirdInstance = Instantiate(fileUploadSc.model, imageTarget.transform);
            // Set local position, rotation, and scale as needed
            thirdInstance.transform.localPosition = new Vector3(0, 0, 0); // adjust to your needs
            thirdInstance.transform.localRotation = Quaternion.identity;
            thirdInstance.transform.localScale = Vector3.one;
            Debug.Log("Third model instance attached to " + imageTarget.TargetName);
        }
        else
        {
            Debug.LogError("ImageTargetBehaviour not found in the scene.");
        }
    }
}
