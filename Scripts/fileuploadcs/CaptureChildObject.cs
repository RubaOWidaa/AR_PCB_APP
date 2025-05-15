using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CaptureChildObject : MonoBehaviour
{
    public Camera renderCamera;
    public GameObject groupedObject1; // BottomLayer
    public GameObject groupedObject2; // TopLayer
    private GameObject originalParent;
    public string filePath_target;
    public string bottomLayerImagePath;
    public string topLayerImagePath;



    public float contrastFactor = 2f; // Adjust for stronger contrast
    private Layers LayersScript;

    public void Start()
    {
        FileUploader fileUploadSc = FindObjectOfType<FileUploader>();
        originalParent = fileUploadSc.model;
        CreateImageTarget();

        // Capture Bottom Layer while disabling Top Layer
        CaptureAndSave(groupedObject1, groupedObject2, "BottomLayer.png");

        // Capture Top Layer while disabling Bottom Layer
        CaptureAndSave(groupedObject2, groupedObject1, "TopLayer.png");

        // Disable all models after capturing
        DisableAllModels();
    }

    void DisableAllModels()
    {
        if (groupedObject1 != null) groupedObject1.SetActive(false);
        if (groupedObject2 != null) groupedObject2.SetActive(false);
        if (originalParent != null) originalParent.SetActive(false);
    }

    void CreateImageTarget()
    {
        // Find the Layers component in the scene
        LayersScript = FindObjectOfType<Layers>();

        if (LayersScript != null)
        {
            // Get lists from LayersScript
            List<string> topLayerNets = new List<string>(LayersScript.TopLayer);
            List<string> bottomLayerNets = new List<string>(LayersScript.BottomLayer);

            // Get all objects in the scene
            Transform[] allObjects = FindObjectsOfType<Transform>();

            // Add "Core" to both lists
            if (!topLayerNets.Contains("Core")) topLayerNets.Add("Core");
            if (!bottomLayerNets.Contains("Core")) bottomLayerNets.Add("Core");
            if (!bottomLayerNets.Contains("Pad")) bottomLayerNets.Add("Pad");
            if (!topLayerNets.Contains("Pad")) topLayerNets.Add("Pad");


            // Add all objects that start with "Pad" to both lists
            foreach (Transform obj in allObjects)
            {
                if (obj.name.StartsWith("Pad") && !topLayerNets.Contains(obj.name))
                {
                    topLayerNets.Add(obj.name);
                    bottomLayerNets.Add(obj.name);

                }

              /*  if (obj.name.StartsWith("Component") && !topLayerNets.Contains(obj.name))
                {
                    topLayerNets.Add(obj.name);
                    bottomLayerNets.Add(obj.name);
                }*/
            }
            // Automatically add all "Pad*" objects to both top and bottom lists
            foreach (Transform obj in allObjects)
            {
                if (obj.name.StartsWith("Pad"))
                {
                    if (!topLayerNets.Contains(obj.name))
                        topLayerNets.Add(obj.name);

                    if (!bottomLayerNets.Contains(obj.name))
                        bottomLayerNets.Add(obj.name);
                }
            }


            // Create parent object for the BottomLayer
            groupedObject1 = new GameObject("BottomLayer");
            CreateLayerObjects(bottomLayerNets, allObjects, groupedObject1);
           // groupedObject1.transform.Rotate(0, 0, 0);


            // Create parent object for the TopLayer
            groupedObject2 = new GameObject("TopLayer");
            CreateLayerObjects(topLayerNets, allObjects, groupedObject2);
          //  groupedObject2.transform.Rotate(0, 180, 0);
            // Rotate Top Layer by 180 degrees (Flipping for correct positioning)

        }
        else
        {
            Debug.LogError("LayersScript not found in the scene.");
        }
    }

    void CreateLayerObjects(List<string> layerNets, Transform[] allObjects, GameObject parentGroup)
    {
        List<GameObject> childrenList = new List<GameObject>();

        foreach (string netName in layerNets)
        {
            foreach (Transform obj in allObjects)
            {
                if (obj.name == netName)
                {
                    GameObject duplicate = Instantiate(obj.gameObject);
                    duplicate.transform.SetParent(parentGroup.transform, true);
                    duplicate.transform.position = obj.position;
                    duplicate.transform.rotation = obj.rotation;
                    childrenList.Add(duplicate);
                }
            }
        }

        if (childrenList.Count == 0)
        {
            Debug.LogWarning($"No matching children found for {parentGroup.name}!");
        }
    }

    void PositionCamera(GameObject groupedObject)
    {
        if (groupedObject == null) return;

        Bounds bounds = CalculateBounds(groupedObject);
        Vector3 camPosition = bounds.center + new Vector3(0, 0, -bounds.size.magnitude);
        renderCamera.transform.position = camPosition;
        renderCamera.transform.LookAt(bounds.center);

        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.white; // Ensures no transparency or black areas
    }

    Bounds CalculateBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    public void CaptureAndSave(GameObject groupedObject, GameObject otherObject, string fileName)
    {
        if (groupedObject == null) return;

        // Disable the other object to avoid interference
        if (otherObject != null) otherObject.SetActive(false);

        if (originalParent != null)
            originalParent.SetActive(false);

        PositionCamera(groupedObject);

        int width = 2048, height = 2048; // Increase resolution for better cropping
        RenderTexture rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
        renderCamera.targetTexture = rt;
        renderCamera.Render();

        // Capture image
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        // Auto-crop image to remove empty space
        Texture2D croppedImage = AutoCrop(image);

        // Increase contrast
        Texture2D finalImage = IncreaseContrast(croppedImage, contrastFactor);

        // Save as 24-bit PNG
        byte[] bytes = finalImage.EncodeToPNG();
        filePath_target = Application.persistentDataPath + "/" + fileName;
        File.WriteAllBytes(filePath_target, bytes);
        Debug.Log($"Image saved at: {filePath_target}");

        if (fileName.Contains("BottomLayer"))
        {
            bottomLayerImagePath = filePath_target;
        }
        else if (fileName.Contains("TopLayer"))
        {
            topLayerImagePath = filePath_target;
        }
        // Cleanup and Re-enable the other object
        if (otherObject != null) otherObject.SetActive(true);
        if (originalParent != null) originalParent.SetActive(true);

        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
    }


    Texture2D AutoCrop(Texture2D source)
        {
            int minX = source.width, minY = source.height, maxX = 0, maxY = 0;
            Color32[] pixels = source.GetPixels32();

            // Find board bounding box
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    Color32 pixel = pixels[y * source.width + x];
                    if (pixel.r != 255 || pixel.g != 255 || pixel.b != 255) // Not a white background
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            int croppedWidth = maxX - minX + 1;
            int croppedHeight = maxY - minY + 1;
            Texture2D croppedTexture = new Texture2D(croppedWidth, croppedHeight, TextureFormat.RGB24, false);
            croppedTexture.SetPixels(source.GetPixels(minX, minY, croppedWidth, croppedHeight));
            croppedTexture.Apply();

            return croppedTexture;
        }

        Texture2D IncreaseContrast(Texture2D source, float contrast)
        {
            Color[] pixels = source.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float r = pixels[i].r * contrast;
                float g = pixels[i].g * contrast;
                float b = pixels[i].b * contrast;
                pixels[i] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
            }
            source.SetPixels(pixels);
            source.Apply();
            return source;
        }
    
}

