using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrillPlace : MonoBehaviour
{
    public ResourceManager.Resources resource;
    
    void Start()
    {
        Resource res = transform.AddComponent<Resource>();

        res.r_name = ResourceManager.instance.resources[(int)resource].r_name;
        res.resource_type = ResourceManager.instance.resources[(int)resource].resource_type;
        res.r_model = ResourceManager.instance.resources[(int)resource].r_model;
        res.id = ResourceManager.instance.resources[(int)resource].id;
    }
}