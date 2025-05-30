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

    [Header("References")]
    public LayerMask drill_layer;
    public LayerMask conveyor_layer;

    public Drill nearest_drill;
    public Conveyor nearest_conveyor;

    [SerializeField] private Material line_material;

    public bool can_extract = false;

    public List<GameObject> resources_in_conveyor;
    public bool can_deposite = true;

    private float delta = 0;
    private Vector3 dir;
    public void StartConveyor()
    {
        resources_in_conveyor = new List<GameObject>();
        Collider[] colls = Physics.OverlapSphere(transform.position, conveyor_range, drill_layer);
        
        if (colls.Length > 0 && colls[0].transform.TryGetComponent<Drill>(out Drill dr)){
            float dist = Vector3.Distance(transform.position, colls[0].transform.position);
            for (int i = 0; i < colls.Length; i++)
            {
                if (Vector3.Distance(transform.position, colls[i].transform.position) < dist)
                {
                    dist = Vector3.Distance(transform.position, colls[i].transform.position);
                    dr = colls[i].GetComponent<Drill>();
                }
            }
            ConnectToToDrill(dr);
        }
        if (nearest_drill == null) // No hay un taladro cerca
        {
            can_extract = false;
            Debug.Log("Couldn't find a drill or it already has a connection");
            
            // Busca el conveyor m�s cercano
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
                //CreateLineOfConection(new Vector3[2] { transform.position, nearest_conveyor.transform.position });
            }
        }
    }
    public void CreateLineOfConection(Vector3[] positions)
    {
        /// Set the Linerenderer between conveyors
        GameObject conv_lr = new GameObject();
        conv_lr.transform.SetParent(transform);
        LineRenderer lr = conv_lr.AddComponent<LineRenderer>();
        Vector3[] pos = new Vector3[2];
        pos[0] = positions[0];
        pos[1] = positions[1];
        lr.SetPositions(pos);
        lr.startColor = Color.red;
        lr.endColor = Color.green;

        lr.material = line_material;
        lr.startWidth = 0.013f;
        lr.endWidth = 0.013f;

        nearest_conveyor = null;
    }
    public void ConnectToToDrill(Drill dr)
    {
        nearest_drill = dr;
        if (!nearest_drill.conveyor_connected)
        {
            nearest_drill.conveyor_connected = true;
            can_extract = true;
        }
        else
            nearest_drill = null;
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
                if (resources_in_conveyor[i] != null)
                {
                    resources_in_conveyor[i].transform.Translate(dir * conveyor_speed * Time.deltaTime, Space.World);
                    OrientateMineral();

                    if (Vector3.Distance(resources_in_conveyor[i].transform.position, nearest_conveyor.transform.position + nearest_conveyor.offset) < 0.1f)
                    {
                        if (nearest_conveyor.conveyor_stored < nearest_conveyor.conveyor_max_stored)
                        {
                            nearest_conveyor.conveyor_stored++;
                            nearest_conveyor.resources_in_conveyor.Add(resources_in_conveyor[i]);
                            nearest_conveyor.current_resource = resources_in_conveyor[i].GetComponent<Resource>();
                            nearest_conveyor.OrientateMineral();
                        }
                        if (conveyor_stored > 0)
                            conveyor_stored--;
                        resources_in_conveyor.Remove(resources_in_conveyor[i]);
                    }
                }
                else
                {
                    resources_in_conveyor.RemoveAt(i);
                    conveyor_stored = resources_in_conveyor.Count;
                }
            }
        }
        else if (conveyor_stored > 0 && nearest_conveyor == null && connected_to_conveyor && Core.instance != null)
        {
            Deposite(1);
        }
    }

    public void Deposite(int amount)
    {
        for (int i = 0; i < resources_in_conveyor.Count && i < amount; i++)
        {
            conveyor_stored--;
            int amount_to_sum = 1;
            if (resources_in_conveyor[i].name.Contains("Processed"))
                amount_to_sum *= 5;
            Resource resource_to_add = resources_in_conveyor[i].GetComponent<Resource>();
            if (resource_to_add == null)
            {
                GameObject ob = resources_in_conveyor[i];
                resources_in_conveyor.RemoveAt(i);
                Destroy(ob);
                return;
            }
            ResourceManager.instance.AddResource(resource_to_add.resource_type, amount_to_sum);
            Destroy(resources_in_conveyor[i]);
            resources_in_conveyor.Remove(resources_in_conveyor[i]);
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < resources_in_conveyor.Count; i++)
        {
            Destroy(resources_in_conveyor[i]);
        }
    }

    public void ExtractFromDrill()
    {
        if (nearest_drill == null) return;

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
            if (!current_resource.r_model.TryGetComponent<Resource>(out Resource res))
            {
                Resource resource = current_resource.r_model.AddComponent<Resource>();
                resource.r_name = current_resource.r_name;
                resource.id = current_resource.id;
                resource.resource_type = current_resource.resource_type;
                resource.GetComponent<Rigidbody>().useGravity = false;
            }
            OrientateMineral();
        }
    }

   

    public void OrientateMineral()
    {
        if (nearest_conveyor == null || resources_in_conveyor.Count <= 0 || resources_in_conveyor[resources_in_conveyor.Count - 1] == null) return;

        Vector3 point = (nearest_conveyor.transform.position + nearest_conveyor.offset) - resources_in_conveyor[resources_in_conveyor.Count - 1].transform.position;
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