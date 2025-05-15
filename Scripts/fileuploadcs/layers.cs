using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class Layers : MonoBehaviour
{
    private void Start()
    {
        CreateLayers();
    }
    public List<string> BottomLayer { get; private set; } = new List<string>();
    public List<string> TopLayer { get; private set; } = new List<string>();
    public void CreateLayers()
    {
        FileUploader fileUploadScript = FindObjectOfType<FileUploader>();
        if (fileUploadScript != null && !string.IsNullOrEmpty(fileUploadScript.xmlFilePath))
        {
            string xmlfile_1 = fileUploadScript.xmlFilePath;
            string xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(xmlfile_1));

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);  
            Debug.Log("XML Loaded Successfully");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581"); 

            XmlNodeList LayerFeatures = xmlDoc.SelectNodes("//ipc:LayerFeature", nsmgr);

            //Debug.Log("Total LayerFeature Nodes Found: " + LayerFeatures.Count);

            foreach (XmlNode LayerFeature in LayerFeatures)
            {
                string layerRef = LayerFeature.Attributes["layerRef"]?.Value;
               // Debug.Log("Layer Feature Found: " + layerRef);

                XmlNodeList Sets = LayerFeature.SelectNodes("ipc:Set", nsmgr); // Adjust for namespace

               // Debug.Log("Total Sets Found in " + layerRef + ": " + Sets.Count);

                foreach (XmlNode setNode in Sets)
                {
                    if (setNode.Attributes["net"] != null)
                    {
                        string net = setNode.Attributes["net"].Value;
                       // Debug.Log($"Layer: {layerRef}, Net: {net}");

                        // Store into the respective list based on layer type
                        if (layerRef == "Top Layer")
                        {
                            //TopLayer.Add(net);
                        }
                        else if (layerRef == "Bottom Layer")
                        {
                            BottomLayer.Add(net);
                        }
                    }
                    else
                    {
                      //  Debug.Log($"Layer: {layerRef}, No Net Attribute Found.");
                    }
                }
            }

            // Debugging to verify lists
            Debug.Log("Top Layer Nets: " + string.Join(", ", TopLayer));
            Debug.Log("Bottom Layer Nets: " + string.Join(", ", BottomLayer));
        


    }
    }
}









