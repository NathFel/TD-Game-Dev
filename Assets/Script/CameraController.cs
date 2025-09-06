using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private bool doMovement = true;

    [Header("Zoom Settings")]
    public float scrollSpeed = 50f;
    public float minY = 10f;
    public float maxY = 80f;

    [Header("Pan Settings")]
    public float panSpeed = 30f;
    public float panBorderThickness = 10f;
    
    [Header("Movement Limits")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            doMovement = !doMovement;
        }

        if (!doMovement)
            return;

        Vector3 pos = transform.position;

        // --- WASD or Mouse-Edge Panning ---
        if (Keyboard.current.wKey.isPressed || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos += Vector3.forward * panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.sKey.isPressed || Input.mousePosition.y <= panBorderThickness)
        {
            pos += Vector3.back * panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.dKey.isPressed || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos += Vector3.right * panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.aKey.isPressed || Input.mousePosition.x <= panBorderThickness)
        {
            pos += Vector3.left * panSpeed * Time.deltaTime;
        }

        // --- Scroll Wheel Zoom ---
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            pos.y -= scroll * scrollSpeed * 0.01f;  
        }

        // --- Clamp movement ---
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}
