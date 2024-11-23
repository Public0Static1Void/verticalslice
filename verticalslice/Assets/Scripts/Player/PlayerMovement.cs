using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float speed;
    [SerializeField] private float sprint_speed;
    [SerializeField] private float current_speed;
    [Header("Camera")]
    [SerializeField] private float normal_fov;
    [SerializeField] private float sprinting_fov;
    [SerializeField] private float current_fov;

    private bool is_sprinting;

    [Range(0, 1)]
    [SerializeField] private float decrease_on_static;

    private Rigidbody rb;

    Vector2 input;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        current_speed = speed;
        current_fov = normal_fov;
    }

    void Update()
    {
        if (input == Vector2.zero)
        {
            current_speed = speed;
            if (current_fov > normal_fov)
            {
                current_fov = Mathf.Lerp(current_fov, normal_fov, Time.deltaTime);
                Camera.main.fieldOfView = current_fov;
            }
            return;
        }

        if (is_sprinting && current_speed < sprint_speed)
        {
            current_speed = Mathf.Lerp(current_speed, sprint_speed, Time.deltaTime * 2);
            current_fov = Mathf.Lerp(current_fov, sprinting_fov, Time.deltaTime * 2);
        }
        else if (!is_sprinting && current_speed > speed)
        {
            current_speed = Mathf.Lerp(current_speed, speed, Time.deltaTime * 2);
            current_fov = Mathf.Lerp(current_fov, normal_fov, Time.deltaTime * 2);
        }

        Camera.main.fieldOfView = current_fov;
    }

    void FixedUpdate()
    {
        if (input != Vector2.zero)
        {
            Vector3 dir = new Vector3(((transform.forward.x * input.y) + (transform.right.x * input.x)) * current_speed * Time.deltaTime, rb.velocity.y,
                                        ((transform.forward.z * input.y) + (transform.right.z * input.x)) * current_speed * Time.deltaTime);
            rb.velocity = dir;
        }
        else if (rb.velocity.magnitude > 0.3f)
        {
            rb.velocity *= decrease_on_static;
        }
        else if (rb.velocity.x > 0 && rb.velocity.x < 0.3f && rb.velocity.z > 0 && rb.velocity.z < 0.3f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    public void Move(InputAction.CallbackContext con)
    {
        input = con.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext con)
    {
        if (con.performed && input != Vector2.zero)
        {
            is_sprinting = true;
        }
        if (con.canceled)
        {
            is_sprinting = false;
        }
    }
}