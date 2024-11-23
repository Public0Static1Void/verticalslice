using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public Resource current_resource;
    [Header("Stats")]
    public float conveyor_speed;
    public int conveyor_stored;
    public int conveyor_max_stored;
    public float conveyor_range;
    [Range(0, 2)]
    public float conveyor_extract_rate;
    public Vector3 offset;

    public bool connected_to_conveyor = false;

    public LayerMask drill_layer;
    public LayerMask conveyor_layer;

    private Drill nearest_drill;
    [SerializeField] private Conveyor nearest_conveyor;

    private bool can_extract = false;

    public List<GameObject> resources_in_conveyor;

    private float delta = 0;
    private Vector3 dir;
    public void StartConveyor()
    {
        resources_in_conveyor = new List<GameObject>();
        Collider[] colls = Physics.OverlapSphere(transform.position, conveyor_range, drill_layer);
        
        if (colls.Length > 0 && colls[0].transform.TryGetComponent<Drill>(out Drill dr)){
            nearest_drill = dr;
            if (!nearest_drill.conveyor_connected)
                nearest_drill.conveyor_connected = true;
            else
                nearest_drill = null;
        }
        if (nearest_drill == null)
        {
            Debug.Log("Couldn't find a drill or it already has a connection");
            Collider[] colliders = Physics.OverlapSphere(transform.position, conveyor_range, conveyor_layer);
            float dist = 0;
            if (colliders.Length > 0)
            {
                dist = Vector3.Distance(transform.position, colliders[0].transform.position);
                nearest_conveyor = colliders[0].transform.GetComponent<Conveyor>();

                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].transform != this.transform && Vector3.Distance(transform.position, colliders[i].transform.position) < dist)
                    {
                        dist = Vector3.Distance(transform.position, colliders[i].transform.position);
                        nearest_conveyor = colliders[i].transform.GetComponent<Conveyor>();
                    }
                }
                
                nearest_conveyor.nearest_conveyor = this;
                nearest_conveyor.connected_to_conveyor = true;
                if (nearest_conveyor.conveyor_stored > 0)
                    nearest_conveyor.OrientateMineral();
                
                nearest_conveyor = null;

            }
            
        }
        else
        {
            can_extract = true;
        }
    }

    void Update()
    {
        if (can_extract)
        {
            delta += Time.deltaTime;
            if (delta >= conveyor_extract_rate)
            {
                ExtractFromDrill();
                delta = 0;
            }
        }
        if (conveyor_stored > 0 && nearest_conveyor != null && nearest_conveyor.conveyor_stored < nearest_conveyor.conveyor_max_stored)
        {
            for (int i = 0; i < resources_in_conveyor.Count; i++)
            {
                resources_in_conveyor[i].transform.Translate(dir * conveyor_speed * Time.deltaTime, Space.World);

                if (Vector3.Distance(resources_in_conveyor[i].transform.position, nearest_conveyor.transform.position + nearest_conveyor.offset) < 0.1f)
                {
                    if (nearest_conveyor.conveyor_stored < nearest_conveyor.conveyor_max_stored)
                    {
                        nearest_conveyor.conveyor_stored++;
                        nearest_conveyor.resources_in_conveyor.Add(resources_in_conveyor[i]);
                        nearest_conveyor.OrientateMineral();
                    }
                    if (conveyor_stored > 0)
                        conveyor_stored--;
                    resources_in_conveyor.Remove(resources_in_conveyor[i]);
                }
            }
        }
    }

    public void ExtractFromDrill()
    {
        if (nearest_drill == null)
            return;

        if (nearest_drill.amount_stored > 0 && conveyor_stored < conveyor_max_stored)
        {
            if (nearest_conveyor != null)
            {
                if (nearest_conveyor.conveyor_stored >= nearest_conveyor.conveyor_max_stored && conveyor_stored >= conveyor_max_stored)
                    return;
            }
            conveyor_stored++;
            nearest_drill.amount_stored--;

            current_resource = nearest_drill.resource;

            resources_in_conveyor.Add(Instantiate(current_resource.r_model, transform.position + offset, Quaternion.identity));
            OrientateMineral();
        }
    }

    public void OrientateMineral()
    {
        if (nearest_conveyor == null || resources_in_conveyor.Count <= 0) return;

        Vector3 point = nearest_conveyor.transform.position - transform.position;
        dir = point.normalized;
        Quaternion rot = Quaternion.LookRotation(dir, transform.up);
        resources_in_conveyor[resources_in_conveyor.Count - 1].transform.rotation = rot;
    }

    public void DestroyConveyor()
    {
        nearest_drill.conveyor_connected = false;
        conveyor_stored = 0;
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (nearest_drill != null)
        {
            Gizmos.color = Color.blue;
            Debug.DrawLine(transform.position, nearest_drill.transform.position);
        }
        else if (nearest_conveyor != null)
        {
            Gizmos.color = Color.black;
            Debug.DrawLine(transform.position, nearest_conveyor.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, conveyor_range);
        Gizmos.DrawWireSphere(transform.position + offset, 0.5f);
    }
}