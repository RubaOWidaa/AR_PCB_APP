using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using TMPro;
using System;

public class BOMLookup : MonoBehaviour
{
    [Header("Text UI Fields")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI refDesText;
    public TextMeshProUGUI packageText;
    public TextMeshProUGUI internalPartNumberText;
    public TextMeshProUGUI layerText;
    public TextMeshProUGUI pinCountText;
    public string currentOemRef = "";
    public string currentIntLibPart = "";



    [Header("UI Panel Background for Theme Toggling")]
    public Image panelBackground;

    private bool darkMode = true;

    void Start() { }

    public class ComponentInfo
    {
        public string RefDes { get; set; }
        public string PackageRef { get; set; }
        public string LayerRef { get; set; }
        public string Description { get; set; }
        public string InternalPartNumber { get; set; }
        public string OemDesignNumberRef { get; set; }
        public int Quantity { get; set; }
        public int PinCount { get; set; }
        public string Category { get; set; }

        public ComponentInfo(string refDes, string packageRef, string layerRef, string description,
            string internalPartNumber, string oemDesignNumberRef, int quantity, int pinCount, string category)
        {
            RefDes = refDes;
            PackageRef = packageRef;
            LayerRef = layerRef;
            Description = description;
            InternalPartNumber = internalPartNumber;
            OemDesignNumberRef = oemDesignNumberRef;
            Quantity = quantity;
            PinCount = pinCount;
            Category = category;
        }
    }

    public void SearchComponent(string componentName)
    {
        string componentNameToFind = componentName.Trim();
        string xmlPath = "";
        FileUploader fileUploadSc = FindObjectOfType<FileUploader>();
        if (!string.IsNullOrEmpty(fileUploadSc.xmlFilePath))
        {
            xmlPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(fileUploadSc.xmlFilePath));
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
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("ipc", "http://webstds.ipc.org/2581");

                if (!string.IsNullOrEmpty(componentNameToFind))
                {
                    XmlNode componentNode = xmlDoc.SelectSingleNode("//ipc:BomItem[ipc:RefDes[@name='" + componentNameToFind + "']]", nsmgr);

                    if (componentNode != null)
                    {
                        XmlNode refDesNode = componentNode.SelectSingleNode("ipc:RefDes", nsmgr);
                        string name = refDesNode?.Attributes["name"]?.Value;
                        string packageRef = refDesNode?.Attributes["packageRef"]?.Value;
                        string layerRef = refDesNode?.Attributes["layerRef"]?.Value;
                        string description = componentNode.Attributes["description"]?.Value;
                        string internalPartNumber = componentNode.Attributes["internalPartNumber"]?.Value;
                        string oemDesignNumberRef = componentNode.Attributes["OEMDesignNumberRef"]?.Value;
                        currentOemRef = oemDesignNumberRef; //  Save it for Datasheet access

                        int quantity = int.TryParse(componentNode.Attributes["quantity"]?.Value, out int qty) ? qty : 0;
                        int pinCount = int.TryParse(componentNode.Attributes["pinCount"]?.Value, out int pins) ? pins : 0;
                        string category = componentNode.Attributes["category"]?.Value;

                        string intLibPart = ExtractIntLibPart(oemDesignNumberRef);
                        currentIntLibPart = intLibPart;  // Store it for Datasheet.cs
                        Debug.Log("IntLib Part: " + currentIntLibPart);

                        Debug.Log("IntLib Part: " + intLibPart);

                        ComponentInfo componentInfo = new ComponentInfo(name, packageRef, layerRef, description, internalPartNumber, oemDesignNumberRef, quantity, pinCount, category);
                        DisplayComponentInfo(componentInfo);
                    }
                    else
                    {
                        ClearDisplay();
                        Debug.Log("Component not found!");
                    }
                }
                else
                {
                    ClearDisplay();
                }
            }
            else
            {
                Debug.LogError("XML file does not exist at path: " + xmlPath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error finding component: " + ex.Message);
        }
    }

    public string ExtractIntLibPart(string oemRef)
    {
        if (!string.IsNullOrEmpty(oemRef) && oemRef.Contains(":"))
        {
            string[] parts = oemRef.Split(':');
            if (parts.Length > 1)
            {
                return parts[1];
            }
        }
        return "Not Found";
    }

    void DisplayComponentInfo(ComponentInfo component)
    {
        // RefDes – Cyan label
        refDesText.text = $"<b><color=#00CFFF>RefDes:</color></b> <color=#FFFFFF>{component.RefDes}</color>";

        // Description – Cyan label, white text
        descriptionText.text = $"<b><color=#00CFFF>Description:</color></b> <color=#FFFFFF>{component.Description}</color>";

        // Package – Cyan label + Yellow value
        packageText.text = $"<b><color=#00CFFF>Package:</color></b> <color=#FFD54F>{component.PackageRef}</color>";

        // Internal PN – Cyan label + white italic value
        internalPartNumberText.text = $"<b><color=#00CFFF>Internal PN:</color></b> <i><color=#FFFFFF>{component.InternalPartNumber}</color></i>";

        // Layer – Cyan label + white
        layerText.text = $"<b><color=#00CFFF>Layer:</color></b> <color=#FFFFFF>{component.LayerRef}</color>";

        // Pins – Cyan label + white
        pinCountText.text = $"<b><color=#00CFFF>Pins:</color></b> <color=#FFFFFF>{component.PinCount}</color>";
    }


    void ClearDisplay()
    {
        refDesText.text = "?? RefDes: Not Found";
        descriptionText.text = "?? Description: Not Found";
        packageText.text = "?? Package: Not Found";
        internalPartNumberText.text = "?? Internal PN: Not Found";
        layerText.text = "?? Layer: Not Found";
        pinCountText.text = "?? Pins: Not Found";
    }

    // === Theme Toggling Support ===
    public void ToggleTheme()
    {
        if (panelBackground == null)
        {
            Debug.LogWarning("Panel background image not assigned.");
            return;
        }

        if (darkMode)
        {
            panelBackground.color = new Color(1f, 1f, 1f, 0.9f); // light
            SetTextColor(Color.black);
        }
        else
        {
            panelBackground.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // dark
            SetTextColor(Color.white);
        }

        darkMode = !darkMode;
    }

    private void SetTextColor(Color color)
    {
        refDesText.color = color;
        descriptionText.color = color;
        packageText.color = color;
        internalPartNumberText.color = color;
        layerText.color = color;
        pinCountText.color = color;
    }
}