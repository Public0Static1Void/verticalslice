using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }
    public enum Resources { STONE, GOLD, COAL, IRON, LAST_NO_USE }
    public List<Resource> resources;
    public List<int> resources_amounts;
    public List<UnityEngine.UI.Text> resources_text;
    public List<string> resources_descriptions;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        resources = new List<Resource>();

        List<ArrayList> resources_from_db = Database.SendQuery("SELECT * FROM Resources");
        for (int i = 0; i <  resources_from_db.Count; i++)
        {
            Resource res = this.AddComponent<Resource>();

            res.id = int.Parse("" + resources_from_db[i][0]);
            Debug.Log(resources_from_db[i][1].ToString());
            res.r_name = resources_from_db[i][1].ToString();
            res.r_model = UnityEngine.Resources.Load<GameObject>(string.Format("Models/Minerals/{0} {1}", "Ore", res.r_name));

            res.resource_type = (Resources)i;
            resources.Add(res);

            Destroy(res);
        }

        resources_amounts = new List<int>();
        for (int i = 0; i < (int)Resources.LAST_NO_USE; i++)
        {
            resources_amounts.Add(0);
        }

        List<ArrayList> descriptions_from_db = Database.SendQuery("SELECT \"description\" FROM Buildings_descriptions");
        resources_descriptions = new List<string>();
        for (int i = 0; i < descriptions_from_db.Count; i++) 
        {
            resources_descriptions.Add(descriptions_from_db[i][0].ToString());
        }


        AddResource(Resources.GOLD, 1000);
        AddResource(Resources.IRON, 1000);
        AddResource(Resources.STONE, 1000);
        AddResource(Resources.COAL, 1000);
    }

    public void AddResource(Resources res, int amount)
    {
        resources_amounts[(int)res] += amount;
        if (resources_amounts[(int)res] > 0)
        {
            resources_text[(int)res].text = "" + resources_amounts[(int)res];
        }
    }

    public bool CheckIfAffordable(BuildingManager.Buildings build)
    {
        int gold, coal, stone, iron;
        BuildingLife bl = BuildingManager.Instance.buildings_prefabs[(int)build].GetComponent<BuildingLife>();
        gold = bl.gold_cost;
        coal = bl.coal_cost;
        stone = bl.stone_cost;
        iron = bl.iron_cost;

        if (gold <= resources_amounts[(int)Resources.GOLD]
            && coal <= resources_amounts[(int)Resources.COAL]
            && stone <= resources_amounts[(int)Resources.STONE]
            && iron <= resources_amounts[(int)Resources.IRON])
        {
            AddResource(Resources.GOLD, -gold);
            AddResource(Resources.COAL, -coal);
            AddResource(Resources.STONE, -stone);
            AddResource(Resources.IRON, -iron);

            return true;
        }

        string txt = "Get more resources to build that ";
        switch (build)
        {
            default: txt += "building!"; break;
            case BuildingManager.Buildings.CONVEYOR: txt += " Conveyor!"; break;
            case BuildingManager.Buildings.DRILL: txt += " Drill!"; break;
            case BuildingManager.Buildings.WALL: txt += " Wall!"; break;
            case BuildingManager.Buildings.CORE: txt += " Core!"; break;
            case BuildingManager.Buildings.TURRET: txt += " Turret!"; break;
        }
        GameManager.gm.ShowText(txt);

        return false;
    }
}