using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 10.0f;
    public float fastSpeed = 30.0f;
    public float rotateSpeed = 1.0f;

    public Transform light;

    private bool pressedEscape = false;

    private Rigidbody rbody;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Tex arrays? " + SystemInfo.supports2DArrayTextures);

        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pressedEscape = true;
            if (!Application.isEditor)
            {
                Application.Quit();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            pressedEscape = false;
        }

        if (!Application.isFocused || pressedEscape) return;

        float strafe = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");
        float elevate = Input.GetAxis("Elevate");
        float yaw = Input.GetAxis("Mouse X");
        float pitch = Input.GetAxis("Mouse Y");
        float lightx = Input.GetAxis("Light X");
        float lighty = Input.GetAxis("Light Y");
        bool fast = Input.GetButton("Fire3");

        float effectiveSpeed = fast ? fastSpeed : speed;
        float delta = Time.deltaTime * effectiveSpeed;
        
        Vector3 velocity = Vector3.zero;
        velocity += transform.forward * forward * effectiveSpeed;
        velocity += transform.right * strafe * effectiveSpeed;
        velocity += transform.up * elevate * effectiveSpeed;

        rbody.velocity = velocity;
        
        transform.Rotate(Vector3.up, rotateSpeed * yaw, Space.World);
        transform.Rotate(Vector3.right, -rotateSpeed * pitch, Space.Self);

        if (light)
        {
            light.Rotate(Vector3.up, rotateSpeed * lightx, Space.World);
            light.Rotate(Vector3.right, -rotateSpeed * lighty, Space.Self);
        }
        
        ScreenshotSaving();
    }

    private void ScreenshotSaving()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string filename = $"{Application.dataPath}/../Screenshots/Screenshot-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png";
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log("Saved screenshot to: " + filename);
        }
#endif
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            pressedEscape = false;
        }
    }
}