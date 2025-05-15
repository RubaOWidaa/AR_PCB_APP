using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Linq;  // For Zip
using TMPro;
using UnityEngine.UI;
using System.IO; // For file reading
using UnityEngine.Networking;


public class trace : MonoBehaviour
{
    public string xmlFilePath = "Assets/StreamingAssets/circuit/sort_output.xml"; // Path to the XML file
    public Transform pcbModelTransform;    // Reference to the PCB model in Unity
    public float layerDepth = 0.1f;          // Depth adjustment for PCB layers
    private string xmlContent; // To store XML data
    private GameObject currentWireObject;
    private string component_1;
    private string component_2;

    // Global list to store the final wire trace from DFS:
    private List<Vector3> wirePoints = new List<Vector3>();

    // Reference to the UserInputHandler (or another script that holds the component values)
    public UserInputHandler userInputHandler;

    void Start()
    {
        StartCoroutine(LoadXML());

        // RuntimeImageTarget RuntimeImageTargetSc = FindObjectOfType<RuntimeImageTarget>();
        //pcbModelTransform = RuntimeImageTargetSc.uploadedModelPrefab2.transform;





    }


    public void ProcessInputValues(string input1, string input2)
    {
        component_1 = input1;
        component_2 = input2;

        Debug.Log("Processing inputs: Component 1 = " + component_1 + ", Component 2 = " + component_2);

        // First try the “direct” trace approach.
        ShowTraceBetweenComponents(component_1, component_2);
        ExtractPCBDimensions(xmlContent);
    }

    // -------------------- EXISTING XML EXTRACTION CODE --------------------
    void ShowOnly(GameObject parent, string childName)
    {
        foreach (Transform child in parent.transform)
        {
            bool isTargetChild = child.gameObject.name.ToLower() == childName.ToLower();
            child.gameObject.SetActive(isTargetChild);

            // Change material to red if this is the target child
            if (isTargetChild)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red; // Change color to red
                }
                else
                {
                    Debug.LogWarning("Renderer not found on " + childName);
                }
            }
        }
    }

    public IEnumerator LoadXML()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "circuit/sort_output.xml");

        if (filePath.Contains("://") || filePath.Contains(":///")) // Android case
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                xmlContent = www.downloadHandler.text;
                Debug.Log("XML Loaded: " + xmlContent);

                // Parse XML to get net, trace points, pad positions, and layer data.
                string netName;
                List<Vector3> tracePoints;
                List<Vector3> padPositionComponent1, padPositionComponent2;
                List<Vector3> padPositionComponent11, padPositionComponent22;
                string traceLayer;
                List<string> wireLayers;
                ParseXML(xmlContent, component_1, component_2,
                    out netName, out tracePoints,
                    out padPositionComponent1, out padPositionComponent2,
                    out padPositionComponent11, out padPositionComponent22,
                    out traceLayer, out wireLayers);
            }
            else
            {
                Debug.LogError("Error loading XML: " + www.error);
            }
        }
        else // PC/macOS/iOS case
        {
            xmlContent = File.ReadAllText(filePath);
            Debug.Log("XML Loaded: " + xmlContent);
            string netName;
            List<Vector3> tracePoints;
            List<Vector3> padPositionComponent1, padPositionComponent2;
            List<Vector3> padPositionComponent11, padPositionComponent22;
            string traceLayer;
            List<string> wireLayers;
            ParseXML(xmlContent, component_1, component_2,
                out netName, out tracePoints,
                out padPositionComponent1, out padPositionComponent2,
                out padPositionComponent11, out padPositionComponent22,
                out traceLayer, out wireLayers);
        }
    }

    public void ExtractPCBDimensions(string filePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(filePath);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

        Vector2 datumCoordinates = ExtractDatumCoordinates(xmlDoc, nsmgr);
        Vector2 pcbDimensions = ExtractPCBWidthHeight(xmlDoc, nsmgr);

        Debug.Log("Datum Coordinates: " + datumCoordinates);
        Debug.Log("PCB Width: " + pcbDimensions.x + " units");
        Debug.Log("PCB Height: " + pcbDimensions.y + " units");
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

    // -------------------- SHOW TRACE & PARSE XML --------------------
    public void ShowTraceBetweenComponents(string component_1, string component_2)
    {
        string netName;
        List<Vector3> tracePoints;
        List<Vector3> padPositionComponent1 = new List<Vector3>();
        List<Vector3> padPositionComponent2 = new List<Vector3>();
        List<Vector3> padPositionComponent11 = new List<Vector3>(); // Exact positions for component_1
        List<Vector3> padPositionComponent22 = new List<Vector3>(); // Exact positions for component_2
        string traceLayer = null;
        List<string> wireLayers = new List<string>();

        // Parse XML to get net, trace points, pad positions, and layer data.
        ParseXML(xmlContent, component_1, component_2,
            out netName, out tracePoints,
            out padPositionComponent1, out padPositionComponent2,
            out padPositionComponent11, out padPositionComponent22,
            out traceLayer, out wireLayers);

        if (tracePoints.Count > 0)
        {
            // Adjust the trace points to match the PCB model.
            for (int i = 0; i < tracePoints.Count; i++)
            {
                tracePoints[i] = AdjustCoordinates(tracePoints[i], pcbModelTransform);
            }
            // Render the direct trace using the layers recorded for each segment.
            RenderTrace(tracePoints, wireLayers);
        }
        else
        {
            // If no direct trace is found, try the DFS approach.
            if (padPositionComponent11.Count > 0 && padPositionComponent22.Count > 0)
            {
                Debug.Log("No direct trace found – starting DFS search...");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                // Use the first pad position for each component.
                float startX = padPositionComponent11[0].x;
                float startY = padPositionComponent11[0].y;
                float targetX = padPositionComponent22[0].x;
                float targetY = padPositionComponent22[0].y;

                StartTracing(xmlDoc, startX, startY, targetX, targetY);

                if (wirePoints.Count > 0)
                {
                    List<Vector3> adjustedPath = new List<Vector3>();
                    foreach (var point in wirePoints)
                    {
                        adjustedPath.Add(AdjustCoordinates(point, pcbModelTransform));
                    }

                    // Fill in missing layer entries with a default value.
                    int segments = adjustedPath.Count - 1;
                    if (wireLayers.Count < segments)
                    {
                        wireLayers.Clear();
                        for (int i = 0; i < segments; i++)
                        {
                            wireLayers.Add("Bottom Layer"); // default value, adjust as needed
                        }
                    }
                    RenderTrace(adjustedPath, wireLayers);
                }
                else
                {
                    Debug.Log("DFS search did not find a valid path.");
                }
            }
            else
            {
                Debug.LogWarning($"No trace found between components {component_1} and {component_2}.");
            }
        }
    }

    // -------------------- HELPER METHOD TO EXTRACT LAYER --------------------
    // Uses an XPath query to directly get the layer attribute from the ancestor LayerFeature node.
    string GetLayerFromLine(XmlNode line, XmlNamespaceManager nsmgr)
    {
        // The XML structure is assumed to be:
        // LayerFeature -> Set -> Features -> UserSpecial -> Line
        // So we query the ancestor LayerFeature node directly.
        XmlNode layerFeatureNode = line.SelectSingleNode("ancestor::ipc:LayerFeature", nsmgr);
        if (layerFeatureNode != null && layerFeatureNode.Attributes["layerRef"] != null)
        {
            return layerFeatureNode.Attributes["layerRef"].Value;
        }
        return null;
    }

    // -------------------- MODIFIED PARSEXML METHOD --------------------
    // This method extracts direct trace data, pad positions, and layer information.
    void ParseXML(string xmlContent, string component_1, string component_2,
        out string netName, out List<Vector3> wirePoints,
        out List<Vector3> padPositionComponent1, out List<Vector3> padPositionComponent2,
        out List<Vector3> padPositionComponent11, out List<Vector3> padPositionComponent22,
        out string traceLayer, out List<string> wireLayers)
    {
        netName = null;
        wirePoints = new List<Vector3>();
        padPositionComponent1 = new List<Vector3>();
        padPositionComponent2 = new List<Vector3>();
        padPositionComponent11 = new List<Vector3>();
        padPositionComponent22 = new List<Vector3>();
        traceLayer = null;
        wireLayers = new List<string>();

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

        Debug.Log("XML Loaded Successfully");

        // Build a dictionary of nets to components and extract pad positions.
        Dictionary<string, HashSet<string>> netToComponents = new Dictionary<string, HashSet<string>>();
        XmlNodeList padStacks = xmlDoc.SelectNodes("//ipc:PadStack", nsmgr);

        foreach (XmlNode padStack in padStacks)
        {
            foreach (XmlNode pinRef in padStack.SelectNodes(".//ipc:PinRef", nsmgr))
            {
                string compRef = pinRef.Attributes["componentRef"].Value;
                netName = padStack.Attributes["net"].Value;
                foreach (XmlNode location in padStack.SelectNodes(".//ipc:LayerPad[@layerRef='Bottom Layer']/ipc:Location", nsmgr))
                {
                    if (float.TryParse(location.Attributes["x"].Value, out float POSX) &&
                        float.TryParse(location.Attributes["y"].Value, out float POSY))
                    {
                        Vector3 position = new Vector3(POSX * 0.1f, POSY * 0.1f, 0);
                        if (compRef == component_1)
                        {
                            padPositionComponent1.Add(position);
                        }
                        else if (compRef == component_2)
                        {
                            padPositionComponent2.Add(position);
                        }
                    }
                }

                if (!netToComponents.ContainsKey(netName))
                    netToComponents[netName] = new HashSet<string>();
                netToComponents[netName].Add(compRef);
            }
        }

        // Find nets common to both components.
        List<string> commonNets = new List<string>();
        foreach (var kvp in netToComponents)
        {
            netName = kvp.Key;
            HashSet<string> associatedComponents = kvp.Value;
            if (associatedComponents.Contains(component_1) && associatedComponents.Contains(component_2))
            {
                commonNets.Add(netName);
                Debug.Log($"Net {netName} is common to both {component_1} and {component_2}");
            }
        }

        // Extract exact pad positions for the common net.
        if (commonNets.Count > 0)
        {
            netName = commonNets[0];
            Debug.Log($"First common net: {netName}");
            foreach (XmlNode padStack in padStacks)
            {
                string net = padStack.Attributes["net"]?.Value;
                if (net == netName)
                {
                    foreach (XmlNode pinRef in padStack.SelectNodes(".//ipc:PinRef", nsmgr))
                    {
                        string compRef = pinRef.Attributes["componentRef"]?.Value;
                        if (string.IsNullOrEmpty(compRef))
                            continue;

                        foreach (XmlNode location in padStack.SelectNodes(".//ipc:LayerPad[@layerRef='Bottom Layer']/ipc:Location", nsmgr))
                        {
                            if (float.TryParse(location.Attributes["x"]?.Value, out float posX) &&
                                float.TryParse(location.Attributes["y"]?.Value, out float posY))
                            {
                                Vector3 position = new Vector3(posX * 0.1f, posY * 0.1f, 0);
                                if (compRef == component_1)
                                {
                                    padPositionComponent11.Add(position);
                                    break;
                                }
                                else if (compRef == component_2)
                                {
                                    padPositionComponent22.Add(position);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No common nets found between the components.");
        }

        // Attempt to get a direct trace by looking for a line that connects a pad from component 1 to component 2.
        foreach (string commonNet in commonNets)
        {
            XmlNodeList lines = xmlDoc.SelectNodes(
                $"//ipc:LayerFeature[@layerRef='Bottom Layer' or @layerRef='Top Layer']/ipc:Set[@net='{commonNet}']/ipc:Features/ipc:UserSpecial/ipc:Line",
                nsmgr
            );
            foreach (XmlNode line in lines)
            {
                // Use the helper method to extract the layer attribute from the ancestor node.
                traceLayer = GetLayerFromLine(line, nsmgr);

                float startX = float.Parse(line.Attributes["startX"].Value) * 0.1f;
                float startY = float.Parse(line.Attributes["startY"].Value) * 0.1f;
                float endX = float.Parse(line.Attributes["endX"].Value) * 0.1f;
                float endY = float.Parse(line.Attributes["endY"].Value) * 0.1f;

                bool matchFoundComponent1 = padPositionComponent1.Any(p =>
                    (Mathf.Approximately(startX, p.x) && Mathf.Approximately(startY, p.y)) ||
                    (Mathf.Approximately(endX, p.x) && Mathf.Approximately(endY, p.y))
                );
                bool matchFoundComponent2 = padPositionComponent2.Any(p =>
                    (Mathf.Approximately(startX, p.x) && Mathf.Approximately(startY, p.y)) ||
                    (Mathf.Approximately(endX, p.x) && Mathf.Approximately(endY, p.y))
                );

                if (matchFoundComponent1 && matchFoundComponent2)
                {
                    // Add both endpoints as a segment.
                    wirePoints.Add(new Vector3(startX, startY, 0));
                    wirePoints.Add(new Vector3(endX, endY, 0));
                    wireLayers.Add(traceLayer); // Record the correct layer for this segment.

                    Debug.Log($"Direct trace found on layer: {traceLayer}");
                    break;
                }
            }
        }
    }

    // -------------------- DFS PATHFINDING METHODS --------------------
    // Build connections using an updated XPath to include the namespace.
    Dictionary<Vector2, List<Vector2>> BuildLineConnections(XmlDocument xmlDoc)
    {
        Dictionary<Vector2, List<Vector2>> lineConnections = new Dictionary<Vector2, List<Vector2>>();
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

        XmlNodeList lineNodes = xmlDoc.SelectNodes(
            "//ipc:LayerFeature[@layerRef='Bottom Layer' or @layerRef='Top Layer']/ipc:Set/ipc:Features/ipc:UserSpecial/ipc:Line",
            nsmgr
        );
        foreach (XmlNode lineNode in lineNodes)
        {
            float sX = float.Parse(lineNode.Attributes["startX"].Value) * 0.1f;
            float sY = float.Parse(lineNode.Attributes["startY"].Value) * 0.1f;
            float eX = float.Parse(lineNode.Attributes["endX"].Value) * 0.1f;
            float eY = float.Parse(lineNode.Attributes["endY"].Value) * 0.1f;

            Vector2 start = new Vector2(sX, sY);
            Vector2 end = new Vector2(eX, eY);

            if (!lineConnections.ContainsKey(start))
                lineConnections[start] = new List<Vector2>();
            lineConnections[start].Add(end);

            if (!lineConnections.ContainsKey(end))
                lineConnections[end] = new List<Vector2>();
            lineConnections[end].Add(start);
        }
        return lineConnections;
    }

    // Recursive DFS that “walks” from the current position to the target.
    void FindPath(Vector2 currentPos, Vector2 target, List<Vector3> path,
          HashSet<(float, float, float, float)> visitedLines,
          Dictionary<Vector2, List<Vector2>> lineConnections)
    {
        if (currentPos == target)
        {
            Debug.Log("Target reached via DFS!");
            wirePoints.Clear();
            wirePoints.AddRange(path);
            return;
        }

        if (!lineConnections.ContainsKey(currentPos))
            return;

        foreach (Vector2 nextPos in lineConnections[currentPos])
        {
            (float, float, float, float) lineKey = (currentPos.x, currentPos.y, nextPos.x, nextPos.y);
            (float, float, float, float) reverseLineKey = (nextPos.x, nextPos.y, currentPos.x, currentPos.y);

            if (visitedLines.Contains(lineKey) || visitedLines.Contains(reverseLineKey))
                continue;

            visitedLines.Add(lineKey);
            path.Add(new Vector3(nextPos.x, nextPos.y, 0));

            Debug.Log($"DFS moving from {currentPos} to {nextPos}");
            FindPath(nextPos, target, path, visitedLines, lineConnections);

            if (wirePoints.Count > 0)
                return;

            visitedLines.Remove(lineKey);
            path.RemoveAt(path.Count - 1);
        }
    }

    // StartTracing sets up the DFS search.
    void StartTracing(XmlDocument xmlDoc, float startX, float startY, float targetX, float targetY)
    {
        Debug.Log("Starting DFS sequence search...");
        Dictionary<Vector2, List<Vector2>> lineConnections = BuildLineConnections(xmlDoc);
        HashSet<(float, float, float, float)> visitedLines = new HashSet<(float, float, float, float)>();
        List<Vector3> path = new List<Vector3>();
        path.Add(new Vector3(startX, startY, 0));
        wirePoints.Clear();
        FindPath(new Vector2(startX, startY), new Vector2(targetX, targetY), path, visitedLines, lineConnections);

        if (wirePoints.Count > 0)
        {
            Debug.Log("DFS sequence successfully completed.");
        }
        else
        {
            Debug.Log("DFS sequence failed to find a valid path.");
        }
    }

    // -------------------- ADJUST & RENDER METHODS --------------------
    Vector3 AdjustCoordinates(Vector3 point, Transform modelTransform)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");
        Vector2 pcbDimensions = ExtractPCBWidthHeight(xmlDoc, nsmgr);
        Vector2 datumCoordinates = ExtractDatumCoordinates(xmlDoc, nsmgr);

        point.x -= (datumCoordinates.x * 0.1f) + (pcbDimensions.x * 0.1f) / 2;
        point.y -= (datumCoordinates.y * 0.1f) + (pcbDimensions.y * 0.1f) / 2;
        point.x = -point.x;
        point.z += -layerDepth;

        return modelTransform.TransformPoint(point);
    }

    void RenderTrace(List<Vector3> tracePoints, List<string> layers)
    {
        if (currentWireObject != null)
            Destroy(currentWireObject);

        currentWireObject = new GameObject("Wire");
        currentWireObject.tag = "Trace";
        LineRenderer lineRenderer = currentWireObject.AddComponent<LineRenderer>();

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("Shader not found.");
            return;
        }
        Material mat = new Material(shader);
        lineRenderer.material = mat;

        lineRenderer.startWidth = 0.06f;
        lineRenderer.endWidth = 0.06f;
        lineRenderer.useWorldSpace = true;

        // Set the positions for the trace.
        lineRenderer.positionCount = tracePoints.Count;
        lineRenderer.SetPositions(tracePoints.ToArray());

        // Determine the number of segments.
        int segments = tracePoints.Count - 1;

        // Ensure there is a layer entry for each segment.
        if (layers.Count < segments)
        {
            if (layers.Count == 0)
            {
                for (int i = 0; i < segments; i++)
                    layers.Add("Default");
            }
            else
            {
                string lastLayer = layers[layers.Count - 1];
                while (layers.Count < segments)
                {
                    layers.Add(lastLayer);
                }
            }
        }

        // Create a gradient for per-segment coloring.
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[segments + 1];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[segments + 1];

        // Helper function to map a layer string to a color.
        Color GetColorForLayer(string layerStr)
        {
            Debug.Log("Mapping layer to color: " + layerStr);
            if (layerStr == "Top Layer")
                return Color.red;
            else if (layerStr == "Bottom Layer")
                return Color.white;
            else
                return Color.green;
        }

        // Create color keys along the line, spacing them evenly.
        for (int i = 0; i < segments; i++)
        {
            Color segColor = GetColorForLayer(layers[i]);
            float timeKey = (float)i / segments;
            colorKeys[i] = new GradientColorKey(segColor, timeKey);
            alphaKeys[i] = new GradientAlphaKey(1.0f, timeKey);
        }
        // Use the last segment's color for the final key.
        Color lastColor = GetColorForLayer(layers[segments - 1]);
        colorKeys[segments] = new GradientColorKey(lastColor, 1.0f);
        alphaKeys[segments] = new GradientAlphaKey(1.0f, 1.0f);

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;

        Debug.Log("Rendering trace with multi-layer support using gradient colors.");
    }
}
