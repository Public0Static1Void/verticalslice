using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemies;
    private int order = 0;
    public float delay;
    private float delta;
    private int wave = 0;

    bool can_start = true;

    private void Start()
    {
        StartCoroutine(WaitABit());
    }
    void Update()
    {
        if (!can_start) return;

        delta += Time.deltaTime;
        if (delta > delay - wave)
        {
            Instantiate(enemies[order], transform.position, Quaternion.identity);
            GameManager.gm.ShowText("Enemies spawning!! ---------------");

            if (wave < delay - 5)
                wave++;
            if (order < enemies.Count - 1)
            {
                order++;
            }
            else
            {
                order = 0;
            }
            delta = 0;
        }
    }
    IEnumerator WaitABit()
    {
        yield return new WaitForSeconds(1);
        GameManager.gm.ShowText("Enemies spawning!! -------------------");
        can_start = true;
    }
}