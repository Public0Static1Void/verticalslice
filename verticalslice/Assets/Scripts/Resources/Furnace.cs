using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : MonoBehaviour
{
    [SerializeField] private Resource current_resource;
    [SerializeField] private LayerMask conv_mask;
    [SerializeField] private float range;

    [SerializeField] private Conveyor conv;
    void Start()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, range, conv_mask);
        if (colls.Length > 0)
        {
            float dis = Vector3.Distance(transform.position, colls[0].transform.position);
            conv = colls[0].GetComponent<Conveyor>();

            for (int i = 0; i < colls.Length; i++)
            {
                float new_dis = Vector3.Distance(transform.position, colls[i].transform.position);
                if (new_dis < dis)
                {
                    conv = colls[i].GetComponent<Conveyor>();
                    dis = new_dis;
                }
            }
        }
    }
}