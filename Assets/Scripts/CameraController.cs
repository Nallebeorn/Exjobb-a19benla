using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 10.0f;
    public float fastSpeed = 30.0f;
    public float rotateSpeed = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float strafe = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");
        float elevate = Input.GetAxis("Elevate");
        float yaw = Input.GetAxis("Mouse X");
        float pitch = Input.GetAxis("Mouse Y");
        bool fast = Input.GetButton("Fire3");

        float delta = Time.deltaTime * (fast ? fastSpeed : speed);
        
        transform.position += transform.forward * forward * delta;
        transform.position += transform.right * strafe * delta;
        transform.position += transform.up * elevate * delta;
        
        transform.Rotate(Vector3.up, rotateSpeed * yaw, Space.World);
        transform.Rotate(Vector3.right, -rotateSpeed * pitch, Space.Self);
    }
}
