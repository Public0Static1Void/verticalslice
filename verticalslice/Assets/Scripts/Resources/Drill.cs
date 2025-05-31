using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    public Resource resource;

    [Header("Stats")]
    public float production;
    [Range(0, 2f)]
    public float efficency;
    public float amount_stored;
    public float max_stored;

    public float drill_range;

    public float rotate_speed;
    public float float_speed;
    
    public float recoil;
    private float delta;

    [SerializeField] private bool can_drill;
    public bool conveyor_connected = false;

    RaycastHit hit;

    void Update()
    {
        transform.Rotate(new Vector3(0, rotate_speed * Time.deltaTime, 0));
        if (can_drill)
        {
            delta += Time.deltaTime;
            if (delta >= recoil)
            {
                Extract();
                delta = 0;
            }
        }
    }

    public void Extract()
    {
        if (amount_stored < max_stored)
            amount_stored += production * efficency;
    }

    public void StopDrill()
    {
        can_drill = false;
        delta = 0;
    }

    public void StartDrill()
    {
        efficency = 0;
        Vector3[] directions = {
                    new Vector3(transform.position.x - transform.localScale.x / 2, transform.position.y, transform.position.z),
                    new Vector3(transform.position.x + transform.localScale.x / 2, transform.position.y, transform.position.z),
                    new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z / 2),
                    new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z / 2) };

        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics.Raycast(directions[i], transform.up, out hit, transform.localScale.y * drill_range))
            {
                if (hit.transform.TryGetComponent<Resource>(out Resource r))
                {
                    if (r == resource)
                    {
                        efficency += 0.25f;
                    }
                    else if (resource == null)
                    {
                        resource = r;
                        efficency += 0.25f;
                    }
                }
            }
        }

        if (efficency > 0)
        {
            can_drill = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, -transform.up * transform.localScale.y);

        Vector3[] directions = {
            new Vector3(transform.position.x - transform.localScale.x / 2, transform.position.y, transform.position.z), 
            new Vector3(transform.position.x + transform.localScale.x / 2, transform.position.y, transform.position.z),
            new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z / 2),
            new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z / 2) };

        Gizmos.color = Color.yellow;
        for (int i = 0; i < directions.Length; i++)
        {
            Gizmos.DrawRay(directions[i], transform.up * transform.localScale.y * drill_range);
        }
    }
}