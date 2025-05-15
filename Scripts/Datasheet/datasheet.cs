using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;

public class Datasheet : MonoBehaviour
{
    [Header("Mouser API Settings")]
    public string mouserApiKey = "785fc4d4-a0c0-466b-add6-40169cd5ca84";

    [Header("SerpAPI Settings")]
    public string serpApiKey = "06151000c75a2772b963dd3db1fe9fc15fad425e722b2c585941293c6b80f60f";

    private string mouserBaseUrl = "https://api.mouser.com/api/v1/search/partnumber?apiKey=";
    private string serpApiBaseUrl = "https://serpapi.com/search.json?q=";

    [Header("UI References")]
    public Button searchButton;
    public GameObject loadingIndicator;

    [Header("PDF Viewer Reference")]
    public PDFViewer pdfViewerScript;
    public BOMLookup BOMlookup;

    // Cache for already fetched PDFs (partNumber -> URL)
    private Dictionary<string, string> cachedPdfUrls = new Dictionary<string, string>();

    void Start()
    {
        if (BOMlookup == null)
            BOMlookup = FindAnyObjectByType<BOMLookup>();

        if (searchButton != null)
            searchButton.onClick.AddListener(OnSearchButtonPressed);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    public void OnSearchButtonPressed()
    {
        if (BOMlookup == null)
        {
            Debug.LogError("? BOMLookup reference is not set.");
            return;
        }

        string partNumber = BOMlookup.currentIntLibPart;

        if (string.IsNullOrEmpty(partNumber) || partNumber == "Not Found")
        {
            Debug.LogError("? Invalid or missing part number. Please select a component first.");
            return;
        }

        // Check if this part was already fetched
        if (cachedPdfUrls.TryGetValue(partNumber, out string cachedUrl) && !string.IsNullOrEmpty(cachedUrl))
        {
            Debug.Log("? Using cached PDF for part: " + partNumber);
            pdfViewerScript.OpenRemotePdf(cachedUrl);
            return;
        }

        StartCoroutine(GetPartDetailsAndOpen(partNumber));
    }

    IEnumerator GetPartDetailsAndOpen(string partNumber)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        string pdfUrl = ""; // Fresh lookup for this component only
        string url = mouserBaseUrl + mouserApiKey;
        string jsonPayload = "{\"SearchByPartRequest\":{\"mouserPartNumber\":\"" + partNumber + "\",\"partSearchOptions\":\"\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                pdfUrl = ExtractDataSheetUrl(responseText);

                if (!string.IsNullOrEmpty(pdfUrl))
                {
                    Debug.Log("? Mouser datasheet found.");
                    cachedPdfUrls[partNumber] = pdfUrl;
                    if (loadingIndicator != null) loadingIndicator.SetActive(false);
                    pdfViewerScript.OpenRemotePdf(pdfUrl);
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("?? Mouser request failed: " + request.error);
            }

            // Fallback to Google
            yield return StartCoroutine(FetchFromGoogleAndOpen(partNumber));
        }
    }

    IEnumerator FetchFromGoogleAndOpen(string partNumber)
    {
        string pdfUrl = "";
        string query = Uri.EscapeDataString($"{partNumber} datasheet filetype:pdf");
        string searchUrl = $"{serpApiBaseUrl}{query}&api_key={serpApiKey}";

        using (UnityWebRequest request = UnityWebRequest.Get(searchUrl))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                pdfUrl = ExtractFirstPdfLink(jsonResponse);

                if (!string.IsNullOrEmpty(pdfUrl))
                {
                    Debug.Log("? Google PDF found.");
                    cachedPdfUrls[partNumber] = pdfUrl;
                    if (loadingIndicator != null) loadingIndicator.SetActive(false);
                    pdfViewerScript.OpenRemotePdf(pdfUrl);
                }
                else
                {
                    string googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(partNumber + " datasheet filetype:pdf")}";
                    Debug.LogWarning("No PDF found. Opening Google Search.");
                    if (loadingIndicator != null) loadingIndicator.SetActive(false);
                    Application.OpenURL(googleSearchUrl);
                }
            }
            else
            {
                Debug.LogError("? SerpAPI request failed: " + request.error);
                if (loadingIndicator != null) loadingIndicator.SetActive(false);
            }
        }
    }

    private string ExtractFirstPdfLink(string jsonResponse)
    {
        try
        {
            JObject data = JObject.Parse(jsonResponse);
            JArray results = (JArray)data["organic_results"];

            if (results != null)
            {
                foreach (JObject result in results)
                {
                    string link = result["link"]?.ToString();
                    if (!string.IsNullOrEmpty(link) && link.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return link;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing SerpAPI response: " + ex.Message);
        }

        return "";
    }

    private string ExtractDataSheetUrl(string jsonResponse)
    {
        try
        {
            JObject data = JObject.Parse(jsonResponse);
            JArray partsArray = (JArray)data["SearchResults"]?["Parts"];

            if (partsArray != null && partsArray.Count > 0)
            {
                JObject part = (JObject)partsArray[0];
                string extractedUrl = part["DataSheetUrl"]?.ToString();

                if (!string.IsNullOrEmpty(extractedUrl))
                    return extractedUrl;

                JArray documents = (JArray)part["Documents"];
                if (documents != null)
                {
                    foreach (JObject doc in documents)
                    {
                        if (doc["Type"]?.ToString() == "Datasheet")
                        {
                            return doc["Url"]?.ToString();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing Mouser JSON: " + ex.Message);
        }

        return "";
    }
}
