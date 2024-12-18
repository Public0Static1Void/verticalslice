using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Furnace : MonoBehaviour
{
    [SerializeField] private Resource current_resource;
    [SerializeField] private LayerMask conv_mask;
    [SerializeField] private float range;

    private Conveyor conv;

    public Conveyor connected_conv;
    void Start()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, range, conv_mask);
        if (colls.Length > 0)
        {
            float dis = Vector3.Distance(transform.position, colls[0].transform.position);
            connected_conv = colls[0].GetComponent<Conveyor>();

            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i].name.Contains("Furnace")) continue;

                float new_dis = Vector3.Distance(transform.position, colls[i].transform.position);
                if (new_dis < dis)
                {
                    connected_conv = colls[i].GetComponent<Conveyor>();
                    dis = new_dis;
                }
            }

            this.AddComponent<Conveyor>();
            conv = GetComponent<Conveyor>();
            conv.conveyor_max_stored = 1;
            conv.resources_in_conveyor = new List<GameObject>();
            
            if (connected_conv != null)
            {
                conv.conveyor_speed = connected_conv.conveyor_speed;
                connected_conv.nearest_conveyor = conv;
            }
        }
    }

    GameObject UpgradeMineral(Resource res)
    {
        GameObject new_mineral = UnityEngine.Resources.Load<GameObject>(string.Format("Models/Minerals/{0} {1}", "Processed", res.r_name));
        if (new_mineral == null)
        {
            return res.gameObject;
        }
        return new_mineral;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mineral") && conv.conveyor_stored < conv.conveyor_max_stored)
        {
            GameObject processed_ore = Instantiate(UpgradeMineral(other.GetComponent<Resource>()), transform.position + transform.up * transform.localScale.y, Quaternion.identity);
            conv.resources_in_conveyor.Add(processed_ore);
            conv.conveyor_stored++;
            connected_conv.resources_in_conveyor.Remove(other.gameObject);
            connected_conv.conveyor_stored--;
            Destroy(other.gameObject);
        }
    }
}