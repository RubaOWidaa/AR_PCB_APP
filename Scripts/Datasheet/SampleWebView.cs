/*
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Android;
#if UNITY_2018_4_OR_NEWER
using UnityEngine.Networking;
#endif
using UnityEngine.UI;
using System.Threading.Tasks;
using System.IO;

public class SampleWebView : MonoBehaviour
{
    public string Url;
    public Text status;
    WebViewObject webViewObject;
    public Text filePathText;
    public AndroidJavaObject webView;
    private int currentSlide = 2;  // Add this field to track current slide
    private bool isMinimized = false;
    private Vector2 originalSize;
    private Vector2 originalPosition;
    private Canvas canvas;
    private RectTransform webViewRect;
    private string documentURL;

    void RequestStoragePermission()
    {
        #if UNITY_ANDROID
            Debug.Log("Checking Android permissions...");
            
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Debug.Log("Storage permission is already granted");
            }
            else
            {
                Debug.Log("Requesting storage permission...");
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }

            // For Android 11+ (API level 30+)
            if (new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT") >= 30)
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var environment = new AndroidJavaClass("android.os.Environment"))
                {
                    if (!environment.CallStatic<bool>("isExternalStorageManager"))
                    {
                        Debug.Log("Requesting MANAGE_EXTERNAL_STORAGE permission...");
                        using (var intent = new AndroidJavaObject("android.content.Intent", 
                            "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION"))
                        {
                            currentActivity.Call("startActivity", intent);
                        }
                    }
                }
            }
        #endif
    }

    IEnumerator Start()
    {
        RequestStoragePermission();
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.canvas = GameObject.Find("Canvas");
#endif
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log(string.Format("CallFromJS[{0}]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
            },
            err: (msg) =>
            {
                Debug.Log(string.Format("CallOnError[{0}]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
            },
            httpErr: (msg) =>
            {
                Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
            },
            started: (msg) =>
            {
                Debug.Log(string.Format("CallOnStarted[{0}]", msg));
            },
            hooked: (msg) =>
            {
                Debug.Log(string.Format("CallOnHooked[{0}]", msg));
            },
            cookies: (msg) =>
            {
                Debug.Log(string.Format("CallOnCookies[{0}]", msg));
            },
            ld: (msg) =>
            {
                Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
                // NOTE: the following js definition is required only for UIWebView; if
                // enabledWKWebView is true and runtime has WKWebView, Unity.call is defined
                // directly by the native plugin.
#if true
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                window.location = 'unity:' + msg;
                            }
                        };
                    }
                ";
#else
                // NOTE: depending on the situation, you might prefer this 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                var iframe = document.createElement('IFRAME');
                                iframe.setAttribute('src', 'unity:' + msg);
                                document.documentElement.appendChild(iframe);
                                iframe.parentNode.removeChild(iframe);
                                iframe = null;
                            }
                        };
                    }
                ";
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                var js = @"
                    window.Unity = {
                        call:function(msg) {
                            parent.unityWebView.sendMessage('WebViewObject', msg);
                        }
                    };
                ";
#else
                var js = "";
#endif
                webViewObject.EvaluateJS(js + @"Unity.call('ua=' + navigator.userAgent)");
            },
            
        androidForceDarkMode: 0,  // Add this if not present
        enableWKWebView: true    // Enable WKWebView for better PDF support
        );

        // cf. https://github.com/gree/unity-webview/issues/1094#issuecomment-2358718029
        while (!webViewObject.IsInitialized()) {
            yield return null;
        }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
        webViewObject.devicePixelRatio = 1;  // 1 or 2
#endif
        // cf. https://github.com/gree/unity-webview/pull/512
        // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
        //webViewObject.SetAlertDialogEnabled(false);

        // cf. https://github.com/gree/unity-webview/pull/728
        //webViewObject.SetCameraAccess(true);
        //webViewObject.SetMicrophoneAccess(true);

        // cf. https://github.com/gree/unity-webview/pull/550
        // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        // cf. https://github.com/gree/unity-webview/pull/570
        // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
        //webViewObject.SetBasicAuthInfo("id", "password");

        //webViewObject.SetScrollbarsVisibility(true);

        webViewObject.SetMargins(5, 100, 5, Screen.height / 29);
        webViewObject.SetTextZoom(90);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
        //webViewObject.SetMixedContentMode(2);  // android only. 0: MIXED_CONTENT_ALWAYS_ALLOW, 1: MIXED_CONTENT_NEVER_ALLOW, 2: MIXED_CONTENT_COMPATIBILITY_MODE
            // Add MIME type support for PDFs
    //webViewObject.SetMIMEType("application/pdf");
    webViewObject.SetVisibility(true);


            string presentationUrl = "https://docs.google.com/presentation/d/1FGPDVZrUeHGLQYcCoWQ9ZIFZtsLRNaDV/edit?usp=sharing&ouid=115760313580559039313&rtpof=true&sd=true";
            DisplayGoogleSlides(presentationUrl);
            
// #if !UNITY_WEBPLAYER && !UNITY_WEBGL
//         if (Url.StartsWith("http")) {
//             webViewObject.LoadURL(Url.Replace(" ", "%20"));
//         } else {
//             var exts = new string[]{
//                 ".jpg",
//                 ".js",
//                 ".html"  // should be last
//             };
//             foreach (var ext in exts) {
//                 var url = Url.Replace(".html", ext);
//                 var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
//                 var dst = System.IO.Path.Combine(Application.temporaryCachePath, url);
//                 byte[] result = null;
//                 if (src.Contains("://")) {  // for Android
// #if UNITY_2018_4_OR_NEWER
//                     // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
//                     var unityWebRequest = UnityWebRequest.Get(src);
//                     yield return unityWebRequest.SendWebRequest();
//                     result = unityWebRequest.downloadHandler.data;
// #else
//                     var www = new WWW(src);
//                     yield return www;
//                     result = www.bytes;
// #endif
//                 } else {
//                     result = System.IO.File.ReadAllBytes(src);
//                 }
//                 System.IO.File.WriteAllBytes(dst, result);
//                 if (ext == ".html") {
//                     webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
//                     break;
//                 }
//             }
//         }
// #else
//         if (Url.StartsWith("http")) {
//             webViewObject.LoadURL(Url.Replace(" ", "%20"));
//         } else {
//             webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
//         }
// #endif
//         yield break;
    }

IEnumerator AccessPDF(string filePath)
{
    Debug.Log("Starting AccessPDF with path: " + filePath);
    
    if (!System.IO.File.Exists(filePath))
    {
        Debug.LogError("File does not exist at path: " + filePath);
        yield break;
    }

    // Get file size to verify we can access it
    long fileSize = new System.IO.FileInfo(filePath).Length;
    Debug.Log($"PDF file size: {fileSize} bytes");

    // Try WebView first with a timeout
    bool loaded = false;
    float timeoutDuration = 10f; // 10 seconds timeout
    float elapsedTime = 0f;

    DisplayPDFInWebView(filePath);

    while (!loaded && elapsedTime < timeoutDuration)
    {
        elapsedTime += Time.deltaTime;
        
        // Check if the WebView has loaded
        if (webViewObject != null && webViewObject.Progress() >= 1.0f)
        {
            loaded = true;
            Debug.Log("PDF loaded successfully in WebView");
        }
        
        yield return null;
    }

    if (!loaded)
    {
        Debug.LogWarning("WebView PDF loading timed out, falling back to default viewer");
        OpenPDFWithDefaultViewer(filePath);
    }
}

void DisplayPDFInWebView(string filePath)
{
    if (webViewObject != null)
    {
        try
        {
            #if UNITY_ANDROID
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // Try using Google Docs PDF viewer as a fallback
                string googleDocsUrl = "https://docs.google.com/viewer?url=";
                string encodedPath = UnityWebRequest.EscapeURL("file://" + filePath);
                string viewerUrl = googleDocsUrl + encodedPath + "&embedded=true";
                
                Debug.Log("Attempting to load PDF using Google Docs viewer: " + viewerUrl);
                webViewObject.LoadURL(viewerUrl);
            }
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in DisplayPDFInWebView: {e.Message}\n{e.StackTrace}");
        }
    }
    else
    {
        Debug.LogError("WebViewObject is not initialized!");
    }
}

void OpenPDFWithDefaultViewer(string filePath)
{
    #if UNITY_ANDROID
    try
    {
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent"))
        using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_VIEW"));
            
            AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + filePath);
            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/pdf");
            intent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
            
            currentActivity.Call("startActivity", intent);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Error opening PDF with default viewer: {e.Message}");
    }
    #endif
}

void DisplayGoogleSlides(string presentationUrl)
{
    if (webViewObject != null)
    {
        try
        {
            // Convert edit URL to embedded presentation view
            string embedUrl = presentationUrl
                .Split('?')[0]  // Remove all parameters after ?
                .Replace("/edit", "/embed");
            
            // Add parameters for better control
            embedUrl += "?rm=minimal" +  // Remove UI elements
                       "&start=false" +  // Don't start automatically
                       "&loop=false" +   // Don't loop
                       "&delayms=3000";  // Delay between slides (if playing)
            
            Debug.Log("Loading Google Slides presentation at: " + embedUrl);
            webViewObject.LoadURL(embedUrl);
            currentSlide = 1;  // Reset slide counter
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading Google Slides: {e.Message}\n{e.StackTrace}");
        }
    }
    else
    {
        Debug.LogError("WebViewObject is not initialized!");
    }
}

void DisplayGoogleDrivePDF(string driveUrl)
{
    if (webViewObject != null)
    {
        try
        {
            // Convert the Google Drive sharing URL to an embedded viewer URL
            // Extract the file ID from the URL
            string fileId = driveUrl.Split('/')[5].Split('?')[0];
            string embedUrl = $"https://drive.google.com/file/d/{fileId}/preview";
            
            Debug.Log("Loading Google Drive PDF at: " + embedUrl);
            
            webViewObject.LoadURL(embedUrl);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading Google Drive PDF: {e.Message}\n{e.StackTrace}");
        }
    }
    else
    {
        Debug.LogError("WebViewObject is not initialized!");
    }
}

// Helper function to determine MIME type
private string GetMimeType(string fileName)
{
    string extension = Path.GetExtension(fileName).ToLower();
    switch (extension)
    {
        case ".pdf":
            return "application/pdf";
        case ".doc":
        case ".docx":
            return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        case ".ppt":
        case ".pptx":
            return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        // Add more types as needed
        default:
            return "application/octet-stream";
    }
}

    void OnGUI()
    {
        var x = 10;

        GUI.enabled = (webViewObject == null) ? false : webViewObject.CanGoBack();
        if (GUI.Button(new Rect(x, 10, 80, 80), "<")) {
            webViewObject?.GoBack();
        }
        GUI.enabled = true;
        x += 90;

        GUI.enabled = (webViewObject == null) ? false : webViewObject.CanGoForward();
        if (GUI.Button(new Rect(x, 10, 80, 80), ">")) {
            webViewObject?.GoForward();
        }
        GUI.enabled = true;
        x += 90;

        if (GUI.Button(new Rect(x, 10, 80, 80), "r")) {
            webViewObject?.Reload();
        }
        x += 90;

        GUI.TextField(new Rect(x, 10, 180, 80), "" + ((webViewObject == null) ? 0 : webViewObject.Progress()));
        x += 190;

        if (GUI.Button(new Rect(x, 10, 80, 80), "toggleZoom")) {
            if (webViewObject != null)
            {
                // Store the current margins to determine state
                //webViewObject.GetMargins(out left, out top, out right, out bottom);
                
                if (isMinimized) // Normal state (assuming these are your default margins)
                {
                    isMinimized = false;
                    // Calculate minimized size (20% of screen)
                    int newWidth = (int)(Screen.width * 0.2f);
                    int newHeight = (int)(Screen.height * 0.2f);
                    
                    // Position in upper right corner
                    int rightMargin = Screen.width - newWidth - 10;
                    int topMargin = 10;
                    
                    webViewObject.SetMargins(1555, 100, 10, Screen.height - newHeight - topMargin);
                }
                else // Minimized state
                {
                    isMinimized = true;
                    // Restore original margins
                    webViewObject.SetMargins(5, 100, 5, Screen.height / 29);
                }
            }
        }
        x += 90;

        if (GUI.Button(new Rect(x, 10, 80, 80), "openPDF")) {
            string pdfUrl = "https://drive.google.com/file/d/1PfHgOu2kLfbqLeHSWPjjZKhaVbJYbjKd/view?usp=sharing";
            DisplayGoogleDrivePDF(pdfUrl);
        }
        x += 90;

        if (GUI.Button(new Rect(x, 10, 80, 80), "prev Slide")) {
            if (currentSlide > 1)
            {
                currentSlide--;
                webViewObject?.GoBack();
            }
        }
        x += 90;

        if (GUI.Button(new Rect(x, 10, 80, 80), "next Slide")) {
            currentSlide++;
            webViewObject?.GoForward();
        }
        x += 90;

        if (GUI.Button(new Rect(x, 10, 80, 80), "E")) {
            string presentationUrl = "https://docs.google.com/presentation/d/1FGPDVZrUeHGLQYcCoWQ9ZIFZtsLRNaDV/edit?usp=sharing&ouid=115760313580559039313&rtpof=true&sd=true";
            DisplayGoogleSlides(presentationUrl);
            //webViewObject?.SetInteractionEnabled(true);
            //webViewObject?.EvaluateJS("window.scrollBy(0, 400);");
        }
        x += 90;
    }
    public void OpenWebView(string datasheetUrl)
    {
        if (!string.IsNullOrEmpty(datasheetUrl))
        {
            Url = datasheetUrl;
            webViewObject.LoadURL(Url);  // Load the datasheet URL
            webViewObject.SetVisibility(true);  // Make WebView visible
            Debug.Log("WebView opened with URL: " + Url);
        }
        else
        {
            Debug.LogError("Datasheet URL is empty.");
        }
    }
}
