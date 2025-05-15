using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithMouse : MonoBehaviour
{
    public float rotationSpeed = 0.6f;
    public float zoomSpeed = 0.01f;

    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private float lastTouchDistance;

    private float lastTapTime = 0f;
    private float doubleTapDelay = 0.3f;

    private Vector3 initialRotation;
    private Vector3 initialScale;

    void Start()
    {
        initialRotation = transform.eulerAngles;
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Double-tap to reset
                if (Time.time - lastTapTime < doubleTapDelay)
                {
                    ResetTransform();
                    lastTapTime = 0f;
                }
                else
                {
                    lastTapTime = Time.time;
                }

                lastTouchPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                float rotationX = delta.y * rotationSpeed * Time.deltaTime;
                float rotationY = -delta.x * rotationSpeed * Time.deltaTime;

                transform.Rotate(rotationX, rotationY, 0, Space.World);
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                lastTouchDistance = currentTouchDistance;
            }
            else
            {
                float distanceDelta = currentTouchDistance - lastTouchDistance;

                Vector3 newScale = transform.localScale + Vector3.one * distanceDelta * zoomSpeed;
                transform.localScale = newScale; // No clamp

                lastTouchDistance = currentTouchDistance;
            }
        }
    }

    private void ResetTransform()
    {
        transform.eulerAngles = initialRotation;
        transform.localScale = initialScale;
    }
}
