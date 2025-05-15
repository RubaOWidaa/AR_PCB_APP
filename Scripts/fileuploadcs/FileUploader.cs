using UnityEngine;
using System.IO;
using UnityEngine.Android;
using static NativeFilePicker;  // Correct namespace for NativeFilePicker
using Dummiesman;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Linq;

public class FileUploader : MonoBehaviour
{
    private string objFilePath;
    private string mtlFilePath;
    public string xmlFilePath;
    public GameObject model;
    public float PCBwidth;
    public float PCBHeight;
    public string outputFilePath = "Assets/StreamingAssets/circuit/sort_output.xml";  // Output file path

    public class PadInfo
    {
        public string name;
        public Vector2 position; // XML x and y values.
    }

   public  void Start()
    {
        RequestPermissions();
       
          
    }
    

    // Request storage permissions for Android
    private void RequestPermissions()
    {
        NativeFilePicker.Permission permission = NativeFilePicker.CheckPermission();
        if (permission == NativeFilePicker.Permission.Denied)
        {
            NativeFilePicker.RequestPermission();
        }

    }

    // Method to select an OBJ file
    public void PickOBJFile()
    {
        if (IsFilePickerBusy())
        {
            Debug.Log("File picker is busy...");
            return;
        }

        PickFile((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                objFilePath = path;
                Debug.Log("Selected OBJ: " + objFilePath);
                SaveFileLocally(objFilePath);
            }
            else
            {
                Debug.LogWarning("No OBJ file selected.");
            }
        }, new string[] { "model/obj", "*/*" });
    }

    // Method to select an MTL file
    public void PickMTLFile()
    {
        if (IsFilePickerBusy())
        {
            Debug.Log("File picker is busy...");
            return;
        }

        PickFile((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                mtlFilePath = path;
                Debug.Log("Selected MTL: " + mtlFilePath);
                SaveFileLocally(mtlFilePath);
            }
            else
            {
                Debug.LogWarning("No MTL file selected.");
            }
        }, new string[] { "model/mtl", "*/*" });
    }

    // Method to select an XML file
    public void PickXMLFile()
    {
        if (IsFilePickerBusy())
        {
            Debug.Log("File picker is busy...");
            return;
        }

        PickFile((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                xmlFilePath = path;
                Debug.Log("Selected XML: " + xmlFilePath);
                SaveFileLocally(xmlFilePath);
                SortXMLByStartX();
            }
            else
            {
                Debug.LogWarning("No XML file selected.");
            }
        }, new string[] { "text/xml", "application/xml", "*/*" });
    }

    // Save file to Unity's persistent data path
    private void SaveFileLocally(string sourcePath)
    {
        if (string.IsNullOrEmpty(sourcePath)) return;

        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.Copy(sourcePath, destinationPath, true);
            Debug.Log("File saved at: " + destinationPath);
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to copy file: " + e.Message);
        }


    }

    // Load an OBJ model from persistent storage
    public void LoadOBJModel()
    {
        List<PadInfo> parsedPads = GetPads();
        string objPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(objFilePath));
        string mtlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(mtlFilePath));
        if (File.Exists(objPath) && File.Exists(mtlPath))
        {
            Debug.Log("OBJ and MTL files successfully read!");
        }
        else
        {
            Debug.LogError("OBJ file not found!");
        }
        if (System.IO.File.Exists(objPath))
        {
            Debug.Log("OBJ file found: " + objPath);

            // Load the model
            OBJLoader objLoader = new OBJLoader();
            model = objLoader.Load(objPath, mtlPath);
            
            //model.transform.Rotate(-90f, 180f, 0f);

            if (model != null)
            {
                Debug.Log("Model successfully loaded!");

                // Attach it to the AR scene
                model.transform.position = new Vector3(0, 0, 0);
                // model.transform.localScale = Vector3.one * 0.1f; // Adjust size
                model.AddComponent<MeshCollider>(); // Add collider for selection
                                                    //    MatchPadsUsingXYRangeLogic(parsedPads)
                                                    // Make it selectable in your AR scene
                /* model.AddComponent<SelectableComponent>();
                 model.AddComponent<AssemblyGuide>();*/
//model.AddComponent<trace>();
                 //model.AddComponent<SetupComponent>();*/
                MatchPadsUsingXYRangeLogic(parsedPads);
              
                // Debug.Log($"[X] Pad {pad.1} centroid: {padCentroid}, xToMatch: {xToMatch}");
            }
            else
            {
                Debug.LogError("Model failed to load.");
            }
        }
        else
        {
            Debug.LogError("OBJ file not found: " + objPath);
        }
        Transform pcbRoot = model.transform;
        Debug.Log($"PCB Root: {pcbRoot.name}, Child Count: {pcbRoot.childCount}");
        foreach (Transform group in pcbRoot)
        {
            Debug.Log($"Checking group: {group.name}");

            if (!group.name.StartsWith("Pad"))
            {
                Debug.Log($"Skipping {group.name}, does not start with 'Pad'");
                continue;
            }

    
            MeshRenderer meshRenderer = group.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = group.GetComponent<MeshFilter>();

            if (meshRenderer == null || meshFilter == null)
            {
                Debug.Log($"Skipping {group.name}, no MeshRenderer or MeshFilter found.");
                continue;
            }
        }

    }



    public float delay = 2f; // Delay time in seconds

    public void LoadPCBAR()
    {
        Debug.Log("Switching to AR Mode...");
        StartCoroutine(LoadSceneAfterDelay("ARR", delay));

    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
    public void CreateLayers()
    {
        FileUploader fileUploadScript = FindObjectOfType<FileUploader>();
        if (fileUploadScript != null && !string.IsNullOrEmpty(fileUploadScript.xmlFilePath))
        {
            // string xmlfile_1 = fileUploadScript.xmlFilePath;
            string xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(xmlFilePath));

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);  // Corrected from LoadXml()

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

            Debug.Log("XML Loaded Successfully");

            List<string> BottomLayer = new List<string>();
            List<string> TopLayer = new List<string>();

            XmlNodeList LayerFeatures = xmlDoc.SelectNodes("//ipc:LayerFeature", nsmgr);
            foreach (XmlNode LayerFeature in LayerFeatures)
            {
                string layerref = LayerFeature.Attributes["layerRef"]?.Value;
                string netref = LayerFeature.Attributes["net"]?.Value;

                if (!string.IsNullOrEmpty(layerref) && !string.IsNullOrEmpty(netref))
                {
                    if (layerref == "Bottom Layer")
                        BottomLayer.Add(netref);
                    else if (layerref == "Top Layer")
                        TopLayer.Add(netref);
                }
            }

            Debug.Log("Top Layer Nets: " + string.Join(", ", TopLayer));
            Debug.Log("Bottom Layer Nets: " + string.Join(", ", BottomLayer));
        }
        else
        {
            Debug.LogError("FileUploader script not found or XML file path is empty.");
        }

    }
    public List<PadInfo> GetPads()
    {
        List<PadInfo> pads = new List<PadInfo>();

        // Ensure the XML file path is valid
        if (string.IsNullOrEmpty(xmlFilePath))
        {
            Debug.LogError("XML file path is empty or not set.");
            return pads;
        }

        // Construct the full path for the XML file.
        string xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(xmlFilePath));

        if (!File.Exists(xmlPath))
        {
            Debug.LogError("XML file does not exist at path: " + xmlPath);
            return pads;
        }

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            // Define XML namespace manager.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

            Debug.Log("XML Loaded Successfully");

            // Select Hole nodes within the specified LayerFeature using namespace.
            XmlNodeList holeNodes = xmlDoc.SelectNodes("//ipc:LayerFeature[@layerRef='Drill Guide (Top Layer - Bottom Layer)']/ipc:Set/ipc:Hole", nsmgr);

            // If no nodes found, fall back to no-namespace query.
            if (holeNodes == null || holeNodes.Count == 0)
            {
                Debug.LogWarning("No holes found with namespace. Trying without namespace...");
                holeNodes = xmlDoc.SelectNodes("//LayerFeature[@layerRef='Drill Guide (Top Layer - Bottom Layer)']/Set/Hole");
            }

            if (holeNodes == null || holeNodes.Count == 0)
            {
                Debug.LogWarning("No hole elements found in the XML.");
                return pads;
            }

            foreach (XmlNode holeNode in holeNodes)
            {
                if (holeNode.Attributes != null)
                {
                    string padName = holeNode.Attributes["name"]?.Value;
                    if (string.IsNullOrEmpty(padName))
                    {
                        Debug.LogWarning("Hole node missing name attribute, skipping...");
                        continue;
                    }

                    // Parse the x and y attributes safely.
                    if (float.TryParse(holeNode.Attributes["x"]?.Value, out float x) &&
                        float.TryParse(holeNode.Attributes["y"]?.Value, out float y))
                    {
                        PadInfo pad = new PadInfo { name = padName, position = new Vector2(x, y) };
                        pads.Add(pad);

                        // Log each pad added
                        Debug.Log($"Parsed Pad: Name = {pad.name}, Position = ({pad.position.x}, {pad.position.y})");
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid x or y value in hole: {padName}");
                    }
                }
            }

            Debug.Log($"Total Pads Parsed: {pads.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing XML: " + e.Message);
        }

        return pads;
    }
    public void MatchPadsUsingXYRangeLogic(List<PadInfo> parsedPads)
    {
        if (model == null)
        {
            Debug.LogError("Model not loaded.");
            return;
        }

        float epsilon = 0.01f;

        string xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(xmlFilePath));
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

        Vector2 datumCoordinates = ExtractDatumCoordinates(xmlDoc, nsmgr);
        Vector2 pcbDimensions = ExtractPCBWidthHeight(xmlDoc, nsmgr);

        // Step 1: Convert XML pads to Unity coordinates and store them
        List<(PadInfo, Vector2)> xmlUnityPads = new List<(PadInfo, Vector2)>();
        foreach (var pad in parsedPads)
        {
            Vector2 unityCoord = ConvertToUnityCoordinates(pad.position, datumCoordinates, pcbDimensions);
            xmlUnityPads.Add((pad, unityCoord));
            Debug.Log($"XML unity pad: {pad.name} at position {unityCoord}");
        }
        Dictionary<string, List<Vector2>> componentPads = new Dictionary<string, List<Vector2>>();

        foreach (var (pad, position) in xmlUnityPads)
        {
            string compName = pad.name.Split('-')[0];

            if (!componentPads.ContainsKey(compName))
                componentPads[compName] = new List<Vector2>();

            componentPads[compName].Add(position);
        }


        // Step 2: Build axis-aligned pairs (X or Y exactly match)
        List<(PadInfo, PadInfo, Vector2, Vector2)> alignedPairs = new List<(PadInfo, PadInfo, Vector2, Vector2)>();
        for (int i = 0; i < xmlUnityPads.Count; i++)
        {
            for (int j = i + 1; j < xmlUnityPads.Count; j++)
            {
                var (pad1, pos1) = xmlUnityPads[i];
                var (pad2, pos2) = xmlUnityPads[j];

                // Extract component names from pad names (assuming component names are separated by '_')
                string componentName1 = pad1.name.Split('-')[0];
                string componentName2 = pad2.name.Split('-')[0];

                // Check if the pads belong to the same component
                if (componentName1 != componentName2)
                {
                    continue;  // Skip pads that don't belong to the same component
                }

                bool xAligned = Mathf.Abs(pos1.x - pos2.x) < epsilon;
                bool yAligned = Mathf.Abs(pos1.y - pos2.y) < epsilon;

                if (xAligned || yAligned)
                {
                    alignedPairs.Add((pad1, pad2, pos1, pos2));
                    Debug.Log($"Aligned pair found: Pad {pad1.name} and Pad {pad2.name}, Positions: {pos1}, {pos2}");
                }
            }
        }

        // Step 3: Compare each Unity pad with the XML pairs
        foreach (Transform pad in model.transform)
        {
            if (!pad.name.StartsWith("Pad")) continue;

            Vector3 centroid3D = CalculateCentroidFromRenderers(pad);
            Vector2 padCentroid = new Vector2(centroid3D.x, centroid3D.y);
            Debug.Log(padCentroid);
            bool matched = false;

            foreach (var (pad1, pad2, pos1, pos2) in alignedPairs)
            {
                bool xAligned = Mathf.Abs(pos1.x - pos2.x) < epsilon;
                bool yAligned = Mathf.Abs(pos1.y - pos2.y) < epsilon;

                if (xAligned)
                {
                    float xToMatch = pos1.x;
                    if (Mathf.Abs(padCentroid.x - xToMatch) < epsilon)
                    {
                        Debug.Log($"[X] Pad {pad.name} centroid: {padCentroid}, xToMatch: {xToMatch}");

                        float minY = Mathf.Min(pos1.y, pos2.y);
                        float maxY = Mathf.Max(pos1.y, pos2.y);
                        if (padCentroid.y >= minY && padCentroid.y <= maxY)
                        {
                            string componentName = pad1.name.Split('-')[0];
                            pad.name = componentName + "_" + pad.name;
                            Debug.Log($"[X-match] Pad {pad.name} matched to component {componentName}");
                            matched = true;
                            break;
                        }
                    }
                }
                else if (yAligned)
                {
                    float yToMatch = pos1.y;
                    if (Mathf.Abs(padCentroid.y - yToMatch) < epsilon)
                    {
                        Debug.Log($"[Y] Pad {pad.name} centroid: {padCentroid}, yToMatch: {yToMatch}");

                        float minX = Mathf.Min(pos1.x, pos2.x);
                        float maxX = Mathf.Max(pos1.x, pos2.x);

                        if (padCentroid.x >= minX && padCentroid.x <= maxX)
                        {
                            string componentName = pad1.name.Split('-')[0];
                            pad.name = componentName + "_" + pad.name;
                            Debug.Log($"[Y-match] Pad {pad.name} matched to component {componentName}");
                            matched = true;
                            break;
                        }
                    }
                }
            }
            // Now check if the centroid is within the range of both the X and Y values
            if (!matched)
            {
                Vector2 centroid = padCentroid;

                foreach (var kvp in componentPads)
                {
                    string compName = kvp.Key;
                    List<Vector2> positions = kvp.Value;

                    // Skip if the component has less than 2 pads
                    if (positions.Count < 2) continue;

                    float minX = positions.Min(p => p.x);
                    float maxX = positions.Max(p => p.x);
                    float minY = positions.Min(p => p.y);
                    float maxY = positions.Max(p => p.y);

                    // Check if the 3D pad's centroid lies inside this bounding box
                    if (centroid.x >= minX && centroid.x <= maxX &&
                        centroid.y >= minY && centroid.y <= maxY)
                    {
                        pad.name = compName + "_" + pad.name;
                        Debug.Log($"[Bounding Box Match] Pad {pad.name} matched to component {compName}");
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    Debug.LogWarning($"Pad {pad.name} did not match any XML component.");
                }
            }
        }
    }
            

    public float padMatchThreshold = 0.01f;


    private Vector3 CalculateCentroidFromRenderers(Transform obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return obj.position;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        return bounds.center;
    }


    private Vector2 ConvertToUnityCoordinates(Vector2 xmlPos, Vector2 datumCoordinates, Vector2 pcbDimensions)
    {
        float scale = 0.1f;

        float offsetX = xmlPos.x - datumCoordinates.x - (pcbDimensions.x / 2f);
        float offsetY = xmlPos.y - datumCoordinates.y - (pcbDimensions.y / 2f);

        offsetX = -offsetX;

        return new Vector2(offsetX, offsetY) * scale;
    }

    public void ExtractPCBDimensions()
    {
        string xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(xmlFilePath));

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);  // Corrected from LoadXml()

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

        Vector2 datumCoordinates = ExtractDatumCoordinates(xmlDoc, nsmgr);
        Vector2 pcbDimensions = ExtractPCBWidthHeight(xmlDoc, nsmgr);

        Debug.Log("Datum Coordinates: " + datumCoordinates);
        Debug.Log("PCB Width: " + pcbDimensions.x + " units");
        Debug.Log("PCB Height: " + pcbDimensions.y + " units");
        PCBwidth = pcbDimensions.x;
        PCBHeight = pcbDimensions.y;


    }

    public Vector2 ExtractDatumCoordinates(XmlDocument xmlDoc, XmlNamespaceManager nsmgr)
    {
        XmlNode datumNode = xmlDoc.SelectSingleNode("//ipc:Step/ipc:Datum", nsmgr);
        if (datumNode != null)
        {
            float x = float.Parse(datumNode.Attributes["x"].Value);
            float y = float.Parse(datumNode.Attributes["y"].Value);
            return new Vector2(x, y);
        }
        else
        {
            Debug.LogWarning("Datum node not found in the XML file.");
            return Vector2.zero;
        }
    }

    public Vector2 ExtractPCBWidthHeight(XmlDocument xmlDoc, XmlNamespaceManager nsmgr)
    {
        XmlNode polygonNode = xmlDoc.SelectSingleNode("//ipc:Step/ipc:Profile/ipc:Polygon", nsmgr);
        XmlNodeList polyStepSegments = polygonNode.SelectNodes("ipc:PolyStepSegment", nsmgr);

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (XmlNode segment in polyStepSegments)
        {
            float x = float.Parse(segment.Attributes["x"].Value);
            float y = float.Parse(segment.Attributes["y"].Value);
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
        float width = maxX - minX;
        float height = maxY - minY;
        return new Vector2(width, height);
    }


    public void SortXMLByStartX()
    {
        string xmlPath = "";

        if (!string.IsNullOrEmpty(xmlFilePath))
        {
            xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(xmlFilePath));
        }
        else
        {
            Debug.LogError("XML file path is not provided.");
            return;
        }

        try
        {
            if (File.Exists(xmlPath))
            {
                XDocument xmlDoc = XDocument.Load(xmlPath);
                XNamespace ns = "http://webstds.ipc.org/2581";

                foreach (var setElement in xmlDoc.Descendants(ns + "Set"))
                {
                    var userSpecial = setElement.Descendants(ns + "UserSpecial").FirstOrDefault();
                    if (userSpecial != null)
                    {
                        var lines = userSpecial.Elements(ns + "Line")
                            .OrderBy(line => float.Parse(line.Attribute("startX")?.Value ?? "0"))
                            .ToList();

                        userSpecial.Elements(ns + "Line").Remove();
                        foreach (var line in lines)
                        {
                            userSpecial.Add(line);
                        }
                    }
                }

                // Ensure output directory exists
                string outputDirectory = Path.Combine(Application.persistentDataPath, "circuit");
                Directory.CreateDirectory(outputDirectory);

                string outputFilePath = Path.Combine(outputDirectory, "sort_output.xml");
                xmlDoc.Save(outputFilePath);

                Debug.Log("Sorting completed! Check: " + outputFilePath);
            }
            else
            {
                Debug.LogError("File not found: " + xmlPath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sorting XML: " + ex.Message);
        }
    }
   
}






