using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public enum Buildings { CONVEYOR, DRILL, CORE, LAST_NO_USE }

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
        List<ArrayList> list = Database.SendQuery("SELECT health FROM Buildings");

        for (int i = 0; i < (int)Buildings.LAST_NO_USE; i++)
        {
            Debug.Log(int.Parse("" + list[i][0]));
            buildings_prefabs[i].GetComponent<BuildingLife>().life = int.Parse("" + list[i][0]);
            Debug.Log(buildings_prefabs[i].GetComponent<BuildingLife>().life);
        }
    }
}