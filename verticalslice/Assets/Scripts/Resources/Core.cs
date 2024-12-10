using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour
{
    public static Core instance { get; private set; }

    [Header("Stats")]
    [SerializeField] private float core_radius;
    [SerializeField] private LayerMask conveyor_layer;

    public void StartCore()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("You can't create two core");
            Destroy(this.gameObject);
        }
        CheckSurroundings();
    }
    public void CheckSurroundings()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, core_radius, conveyor_layer);

        foreach (Collider coll in colls)
        {
            Debug.Log((int)conveyor_layer);
            Conveyor conv = coll.GetComponent<Conveyor>();
            if (!conv.connected_to_conveyor && conv.nearest_conveyor == null)
            {
                if (conv.conveyor_stored > 0)
                {
                    conv.Deposite(1);
                }

                conv.connected_to_conveyor = true;
            }
        }
    }
}
