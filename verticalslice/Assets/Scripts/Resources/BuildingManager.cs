using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public enum Buildings { CONVEYOR, DRILL, CORE, WALL, TURRET, LAST_NO_USE }

    public List<GameObject> buildings_prefabs;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    void Start()
    {
        if (buildings_prefabs.Count == 0)
        {
            Debug.Log("There aren't any prefabs listed");
            return;
        }
        List<ArrayList> list = Database.SendQuery("SELECT health, canRotate FROM Buildings");

        for (int i = 0; i < (int)Buildings.LAST_NO_USE; i++)
        {
            Debug.Log(int.Parse("" + list[i][0]));
            buildings_prefabs[i].GetComponent<BuildingLife>().life = int.Parse("" + list[i][0]);
            int canRot = int.Parse("" + list[i][1]);
            buildings_prefabs[i].GetComponent<BuildingLife>().canRotate = canRot == 0 ? false : true;
            Debug.Log(buildings_prefabs[i].GetComponent<BuildingLife>().life);
        }

        for (int i = 0; i < (int)Buildings.LAST_NO_USE; i++)
        {
            list = Database.SendQuery(string.Format("SELECT gold_cost, stone_cost, coal_cost, iron_cost FROM Builddings_resources WHERE building_name = \"{0}\"", buildings_prefabs[i].name));
            BuildingLife bl = buildings_prefabs[i].GetComponent<BuildingLife>();
            bl.gold_cost = int.Parse("" + list[0][0]);
            bl.stone_cost = int.Parse("" + list[0][1]);
            bl.coal_cost = int.Parse("" + list[0][2]);
            bl.iron_cost = int.Parse("" + list[0][3]);
        }
    }
}