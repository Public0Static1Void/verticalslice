using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [Header("Speed")]
    [SerializeField] private float cameraSpeed;
    float x, y;

    Vector2 inp;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (inp.x > 1 || inp.x < -1)
        {
            x += Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime;
        }
        else
        {
            x += inp.x * cameraSpeed * Time.deltaTime;
        }
        if (inp.y > 1 || inp.y < -1)
        {
            y += Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime;
        }
        else
        {
            y += inp.y * cameraSpeed * Time.deltaTime;
        }

        y = Mathf.Clamp(y, -35, 60);


        player.rotation = Quaternion.Euler(0, x, 0);
        transform.rotation = Quaternion.Euler(-y, x, 0);
    }

    public void ChangeRotation(InputAction.CallbackContext con)
    {
        inp = con.ReadValue<Vector2>();
    }
}