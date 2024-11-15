using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float speed;

    private Rigidbody rb;

    Vector2 input;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(input.x * speed * Time.deltaTime, rb.velocity.y, input.y * speed * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext con)
    {
        input = con.ReadValue<Vector2>();
    }
}