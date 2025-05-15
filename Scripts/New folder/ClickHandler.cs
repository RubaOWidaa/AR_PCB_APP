using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent (typeof(Collider))]
public class ClickHandler : MonoBehaviour
{
    public UnityEvent upEvent;
    public UnityEvent downEvent;

    private void OnMouseDown()
    {
        Debug.Log("Down");
        downEvent?.Invoke();
    }
    private void OnMouseUp()
    {
        Debug.Log("Up");
        upEvent?.Invoke();

    }
   /* public void TriggerDownEvent()
    {
        Debug.Log("Down event trigger via voice command");
        downEvent?.Invoke();
    }
    public void TriggerUpEvent()
    {
        Debug.Log("Up event trigger via voice command");
        downEvent?.Invoke();
    }

    */









}
