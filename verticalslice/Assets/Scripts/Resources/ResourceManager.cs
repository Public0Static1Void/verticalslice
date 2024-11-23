using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }
    public enum Resources { STONE, GOLD, COAL, LAST_NO_USE }
    public List<Resource> resources;
    public List<int> resources_amounts;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        resources = new List<Resource>();

        Resource res = new Resource();
        res.r_name = "Stone";
        res.id = 0;
        resources.Add(res);
        res.r_name = "Gold";
        res.id = 1;
        resources.Add(res);
        res.r_name = "Coal";
        res.id = 2;
        resources.Add(res);

        resources_amounts = new List<int>();
        for (int i = 0; i < (int)Resources.LAST_NO_USE; i++)
        {
            resources_amounts.Add(0);
        }
    }
}