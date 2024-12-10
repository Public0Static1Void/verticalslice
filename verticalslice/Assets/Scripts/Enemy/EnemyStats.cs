using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    public string enemy_name;
    public float hp;
    public float max_hp;
    public float enemy_damage, enemy_recoil;

    public void ChangeLife(int amount)
    {
        hp += amount;
        if (hp > max_hp) hp = max_hp;
        else if (hp <= 0) Destroy(gameObject);
    }
}