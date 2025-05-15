using TMPro;
using UnityEngine;
using UnityEngine.UI; // Make sure to add this for Button references

public class UserInputHandler : MonoBehaviour
{
    public TMP_InputField component_1_InputField;
    public TMP_InputField component_2_InputField;
    public string component_1;
    public string component_2;
    public trace traceScript;  // Reference to the trace script
    public Button submitButton; // Reference to the button

    void Start()
    {
        if (component_1_InputField == null || component_2_InputField == null || submitButton == null || traceScript == null)
        {
            Debug.LogError("One or more UI components are not assigned in the Inspector!");
            return;
        }

        // Add listeners for the input fields and button
        component_1_InputField.onEndEdit.AddListener(OnComponent1Submit);
        component_2_InputField.onEndEdit.AddListener(OnComponent2Submit);

        submitButton.interactable = true;
        submitButton.onClick.AddListener(OnSubmitButtonClick);
    }


    public void OnComponent1Submit(string input)
    {
        component_1 = input;
        Debug.Log("Component 1 input: " + component_1);
    }

    public void OnComponent2Submit(string input)
    {
        component_2 = input;
        Debug.Log("Component 2 input: " + component_2);
    }


    public void OnSubmitButtonClick()
    {
        component_1 = component_1_InputField.text;
        component_2 = component_2_InputField.text;
        component_1_InputField.ForceLabelUpdate();
        component_2_InputField.ForceLabelUpdate();

        if (!string.IsNullOrEmpty(component_1) && !string.IsNullOrEmpty(component_2))
        {
            Debug.Log($"Processing inputs: Component 1 = {component_1}, Component 2 = {component_2}");
            traceScript.enabled = true;
            traceScript.ProcessInputValues(component_1, component_2);
        }
        else
        {
            Debug.LogWarning("One or both fields are empty.");
        }
    }

}