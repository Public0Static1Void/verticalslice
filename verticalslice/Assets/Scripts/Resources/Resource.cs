using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Resource : MonoBehaviour
{
    public string r_name;
    public int id;
    public GameObject r_model;
    public ResourceManager.Resources resource_type;

    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();   
    }
    private void FixedUpdate()
    {
        if (rb.velocity.magnitude > 0)
        {
            rb.velocity *= 0.9f;
        }
    }
}