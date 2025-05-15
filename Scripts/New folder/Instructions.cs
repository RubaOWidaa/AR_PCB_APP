using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Instructions : MonoBehaviour
{
    public GameObject instructionPopup;
    public TMPro.TextMeshProUGUI instructionText;
    public TMPro.TextMeshProUGUI hintText;
    public float displayTime = 2.5f;

    private int currentMode = 0;
    private int currentStepIndex = 0;
    private bool modeSelected = false;
    private bool allStepsCompleted = false;
    private bool waitingForTap = false;

    private string[][] modeInstructions = new string[][]
    {
        // Mode 0: Model / Explore
        new string[]
        {
            " Step 1 – Rotate the PCB to view it from different angles.",
            " Step 2 – Switch to non-AR mode to examine it more closely.",
            " Step 3 – Tap on components to learn their names and functions."
        },

        // Mode 1: Quick Inspection
        new string[]
        {

            " Step 1 – Place the multimeter probes on the trace ends to check for continuity",
            " Step 2 – Press the button to restart the test if needed."
        },

        // Mode 2: Assembly
        new string[]
        {
            " Step 1 – Pick the correct component as shown.",
            " Step 2 – Place the component accurately on the board.",
            " Step 3 – Solder the component carefully to ensure a solid connection.",
            " Step 4 – Tap 'Next' when you're ready to continue to the next step."
        },

        // Mode 3: Trace Mode
        new string[]
        {
            " Step 1 – Tap the first component you'd like to analyze.",
            " Step 2 – Tap the second component to complete the selection.",
            " Step 3 – The connection path between them will now be displayed."
        }
    };

    void Start()
    {
        instructionPopup.SetActive(false);
        hintText.gameObject.SetActive(false);

        if (hintText.GetComponent<CanvasGroup>() == null)
        {
            hintText.gameObject.AddComponent<CanvasGroup>().alpha = 0;
        }
    }

    void Update()
    {
        if (!modeSelected || allStepsCompleted || !waitingForTap)
            return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began ||
            Input.GetMouseButtonDown(0))
        {
            ShowNextStep();
        }
    }

    public void SetMode(int modeIndex)
    {
        currentMode = modeIndex;
        currentStepIndex = 0;
        modeSelected = true;
        allStepsCompleted = false;
        ShowNextStep(); // Start first step
    }

    private void ShowNextStep()
    {
        if (currentStepIndex >= modeInstructions[currentMode].Length)
        {
            allStepsCompleted = true;
            instructionPopup.SetActive(false);
            hintText.gameObject.SetActive(false);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowStepCoroutine());
    }

    private IEnumerator ShowStepCoroutine()
    {
        instructionPopup.SetActive(true);
        instructionText.text = modeInstructions[currentMode][currentStepIndex];

        bool isLastStep = currentStepIndex == modeInstructions[currentMode].Length - 1;

        if (isLastStep)
        {
            hintText.gameObject.SetActive(false);
            waitingForTap = false;
            yield return new WaitForSeconds(displayTime);
            instructionPopup.SetActive(false);
        }
        else
        {
            waitingForTap = true;
            hintText.text = "Tap anywhere to continue...";
            hintText.gameObject.SetActive(true);

            CanvasGroup canvasGroup = hintText.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
        }

        currentStepIndex++;
    }
}
