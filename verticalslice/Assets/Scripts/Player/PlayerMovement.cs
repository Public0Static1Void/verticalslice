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

    private bool is_sprinting;

    [Range(0, 1)]
    [SerializeField] private float decrease_on_static;

    private Rigidbody rb;

    Vector2 input;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        current_speed = speed;
    }

    void Update()
    {
        if (input == Vector2.zero)
        {
            current_speed = speed;
            return;
        }

        if (is_sprinting && current_speed < sprint_speed)
        {
            current_speed = Mathf.Lerp(current_speed, sprint_speed, Time.deltaTime * 2);
        }
        else if (!is_sprinting && current_speed > speed)
        {
            current_speed = Mathf.Lerp(current_speed, speed, Time.deltaTime * 2);
        }
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
        else if (rb.velocity.magnitude > 0 && rb.velocity.magnitude < 3f)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        Debug.Log(rb.velocity.magnitude);
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