using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemymanager : MonoBehaviour
{
    public List<GameObject> enemies_prefabs;

    public enum Enemies { TROOPER, LAST_NO_USE }
    
    void Start()
    {
        List<ArrayList> result = Database.SendQuery("SELECT * FROM Enemies");
        for (int i = 0; i < (int)Enemies.LAST_NO_USE; i++)
        {
            EnemyStats stats = enemies_prefabs[i].GetComponent<EnemyStats>();
            stats.enemy_name = result[i][1].ToString();
            stats.max_hp = int.Parse(result[i][2].ToString());
            stats.enemy_damage = int.Parse(result[i][3].ToString());
            stats.enemy_recoil = float.Parse(result[i][4].ToString());
        }
    }
}