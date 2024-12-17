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
    [SerializeField] private float step_speed;


    private bool is_sprinting;

    [Range(0, 1)]
    [SerializeField] private float decrease_on_static;

    private Rigidbody rb;

    public float delta = 0;

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
        else if (input.y != 0)
        {
            delta += Time.deltaTime;

            if ((delta > 0.5f && delta <= 1) || (delta > 1.5f && delta <= 2))
            {
                Camera.main.transform.Translate(-Vector3.up * (step_speed * Time.deltaTime));
            }
            if (delta <= 0.5f)
            {
                Camera.main.transform.Translate((Vector3.up + Vector3.right) * (step_speed * Time.deltaTime));
            }
            else if (delta > 1 && delta <= 1.5f)
            {
                Camera.main.transform.Translate((Vector3.up + -Vector3.right) * (step_speed * Time.deltaTime));
            }
            else if (delta > 2.2f)
            {
                delta = 0;
            }
        }

        if (is_sprinting && current_speed < sprint_speed)
        {
            current_speed = Mathf.Lerp(current_speed, sprint_speed, Time.deltaTime * 4);
            current_fov = Mathf.Lerp(current_fov, sprinting_fov, Time.deltaTime * 4);
        }
        else if (!is_sprinting && current_speed > speed)
        {
            current_speed = Mathf.Lerp(current_speed, speed, Time.deltaTime * 4);
            current_fov = Mathf.Lerp(current_fov, normal_fov, Time.deltaTime * 4);
        }

        Camera.main.fieldOfView = current_fov;
    }

    void FixedUpdate()
    {
        if (input != Vector2.zero)
        {
            if (input.x != 0 && input.y <= 0)
            {
                current_speed = speed * 0.75f;
            }
            else if (input.y < 0)
            {
                current_speed = speed * 0.5f;
            }
            else if (input.y > 0)
            {
                current_speed = speed;
            }

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