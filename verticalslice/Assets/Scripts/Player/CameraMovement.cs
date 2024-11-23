using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [Header("Speed")]
    [SerializeField] private float cameraSpeed;
    float x, y;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        x += Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime;
        y += Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime;

        y = Mathf.Clamp(y, -30, 60);

        
        player.rotation = Quaternion.Euler(0, x, 0);
        transform.rotation = Quaternion.Euler(-y, x, 0);
    }
}