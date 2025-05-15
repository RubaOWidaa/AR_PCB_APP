using UnityEngine;
using static NativeFilePicker;

public class Permissions : MonoBehaviour
{
    void Start()
    {
        Permission permission = NativeFilePicker.CheckPermission();
        Debug.Log("Current permission: " + permission);

        if (permission != Permission.Granted)
        {
            NativeFilePicker.RequestPermission();
            Debug.Log("Requesting file access permission...");
        }
    }
}
