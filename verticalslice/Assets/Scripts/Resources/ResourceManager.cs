using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }
    public enum Resources { STONE, GOLD, COAL, LAST_NO_USE }
    public List<Resource> resources;
    public List<int> resources_amounts;
    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        resources = new List<Resource>();

        Resource res = new Resource();
        List<ArrayList> resources_from_db = Database.SendQuery("SELECT * FROM Resources");

        for (int i = 0; i <  resources_from_db.Count; i++)
        {
            res.id = int.Parse("" + resources_from_db[i][0]);
            Debug.Log(resources_from_db[i][1].ToString());
            res.r_name = resources_from_db[i][1].ToString();
            resources.Add(res);
        }

        resources_amounts = new List<int>();
        for (int i = 0; i < (int)Resources.LAST_NO_USE; i++)
        {
            resources_amounts.Add(0);
        }
    }

    public void AddResource(Resources res, int amount)
    {
        resources_amounts[(int)res] += amount;
    }
}