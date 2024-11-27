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
        x += inp.x * cameraSpeed * Time.deltaTime;
        y += inp.y * cameraSpeed * Time.deltaTime;

        y = Mathf.Clamp(y, -30, 60);


        player.rotation = Quaternion.Euler(0, x, 0);
        transform.rotation = Quaternion.Euler(-y, x, 0);
    }

    public void ChangeRotation(InputAction.CallbackContext con)
    {
        inp = con.ReadValue<Vector2>();
        inp = inp.normalized;
        Debug.Log(inp);
    }
}