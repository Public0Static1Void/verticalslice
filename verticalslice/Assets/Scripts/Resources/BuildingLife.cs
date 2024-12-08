using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLife : MonoBehaviour
{
    public int life = 1;
    public bool canRotate = false;
    public BuildingManager.Buildings building_type;
    
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
