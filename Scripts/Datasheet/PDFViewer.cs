using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A script for displaying PDFs in a WebView using PDF.js.
/// - On Start(), it automatically copies "fileName" from StreamingAssets to persistentDataPath
///   and loads it in PDF.js.
/// - You can also call OpenRemotePdf(...) to download a remote PDF and load that instead.
/// 
/// NOTE: Requires the Unity WebView plugin (e.g., https://github.com/gree/unity-webview).
///       Also requires PDF.js files in Assets/StreamingAssets/pdfjs/web/ (viewer.html, pdf.js, etc.).
/// </summary>
public class PDFViewer : MonoBehaviour
{
    [Header("Local PDF to Load at Startup")]
    public string fileName = "sample.pdf"; // Name of a local PDF in StreamingAssets

    private WebViewObject webViewObject;
    private bool isViewerActive = false;

    void Start()
    {
        // If you don't want to load a local PDF on startup, you can remove or comment out this call.
        //StartCoroutine(LoadLocalPDFWithPDFJS());
    }

    void Update()
    {
        if (isViewerActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePdfViewer();
        }
    }

    public void ClosePdfViewer()
    {
        if (webViewObject != null)
        {
            Destroy(webViewObject.gameObject);
            webViewObject = null;
            isViewerActive = false;
            Debug.Log("PDF viewer closed and WebViewObject destroyed.");
        }
    }

    private IEnumerator LoadLocalPDFWithPDFJS()
    {
        string pdfSourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string localPDFPath = Path.Combine(Application.persistentDataPath, fileName);

        Debug.Log($"PDF Source Path: {pdfSourcePath}");
        Debug.Log($"Local PDF Path: {localPDFPath}");

        if (!File.Exists(localPDFPath))
        {
            Debug.Log("PDF not found in persistent path, copying...");
            if (Application.platform == RuntimePlatform.Android)
            {
                using (UnityWebRequest www = UnityWebRequest.Get(pdfSourcePath))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(localPDFPath, www.downloadHandler.data);
                        Debug.Log("PDF copied successfully to persistent path");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load PDF from StreamingAssets: {www.error}");
                        yield break;
                    }
                }
            }
            else
            {
                File.Copy(pdfSourcePath, localPDFPath);
                Debug.Log("PDF copied successfully to persistent path");
            }
        }

        webViewObject = new GameObject("WebViewObject_LocalPDF").AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) => {
                Debug.Log($"WebView Message: {msg}");
            },
            err: (msg) => {
                Debug.LogError($"WebView Error: {msg}");
            }
        );

        webViewObject.SetMargins(0, 0, 0, 0);
        webViewObject.SetVisibility(true);
        isViewerActive = true;

        string pdfViewerURL = Path.Combine(Application.streamingAssetsPath, "pdfjs/web/viewer.html");
        if (Application.platform == RuntimePlatform.Android)
        {
            pdfViewerURL = "file:///android_asset/pdfjs/web/viewer.html";
        }

        string localPDFURL = "file://" + localPDFPath;
        string finalURL = $"{pdfViewerURL}?file={UnityWebRequest.EscapeURL(localPDFURL)}";

        Debug.Log($"Loading PDF.js with URL: {finalURL}");
        webViewObject.LoadURL(finalURL);
    }

    public void OpenRemotePdf(string remotePdfUrl, string localFileName = "datasheet.pdf")
    {
        StartCoroutine(OpenRemotePdfCoroutine(remotePdfUrl, localFileName));
    }

    private IEnumerator OpenRemotePdfCoroutine(string remotePdfUrl, string localFileName)
    {
        string localPDFPath = Path.Combine(Application.persistentDataPath, localFileName);
        Debug.Log($"Downloading PDF from: {remotePdfUrl} to: {localPDFPath}");

        using (UnityWebRequest www = UnityWebRequest.Get(remotePdfUrl))
        {
            www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            www.SetRequestHeader("Referer", "https://www.mouser.com/");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(localPDFPath, www.downloadHandler.data);
                yield return new WaitForSeconds(0.1f);
                Debug.Log($"Remote PDF downloaded to: {localPDFPath}");
            }
            else
            {
                Debug.LogError($"Failed to download PDF from {remotePdfUrl}: {www.error}");
                yield break;
            }
        }

        if (webViewObject == null)
        {
            webViewObject = new GameObject("WebViewObject_RemotePDF").AddComponent<WebViewObject>();
            webViewObject.Init();
            webViewObject.SetMargins(0, 0, 0, 0);
            webViewObject.SetVisibility(true);
        }

        isViewerActive = true;

        string pdfViewerURL;
        if (Application.platform == RuntimePlatform.Android)
        {
            pdfViewerURL = "file:///android_asset/pdfjs/web/viewer.html";
        }
        else
        {
            pdfViewerURL = Path.Combine(Application.streamingAssetsPath, "pdfjs/web/viewer.html");
            pdfViewerURL = "file:///" + pdfViewerURL.Replace("\\", "/");
        }

        string localPDFURL = "file://" + localPDFPath;
        string finalURL = $"{pdfViewerURL}?file={UnityWebRequest.EscapeURL(localPDFURL)}";

        Debug.Log($"Loading PDF.js with URL: {finalURL}");
        webViewObject.LoadURL(finalURL);
    }
}