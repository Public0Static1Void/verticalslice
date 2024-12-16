using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLife : MonoBehaviour
{
    public int life = 1;
    public bool canRotate = false;
    public BuildingManager.Buildings building_type;

    public int gold_cost = 0;
    public int iron_cost = 0;
    public int stone_cost = 0;
    public int coal_cost = 0;
    
    public void ChangeLife(int amount)
    {
        life += amount;
        if (life <= 0)
        {
            DestroyBuilding();
        }
    }
    public void DestroyBuilding()
    {
        Destroy(gameObject);
    }
}
