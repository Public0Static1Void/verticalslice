using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int attack_damage;
    public float attack_recoil;

    private float delta = 0;

    private EnemyMovement em;

    [Header("References")]
    [SerializeField] private string player_tag, buildings_tag;
    // Start is called before the first frame update
    void Start()
    {
        em = GetComponent<EnemyMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (em.onAttackRange)
        {
            delta += Time.deltaTime;
            if (delta > attack_recoil)
            {
                Attack();
                delta = 0;
            }
        }
        else if (delta != 0)
        {
            delta = 0;
        }
    }

    public void Attack()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, em.stop_range))
        {
            if (hit.transform.tag == buildings_tag || hit.transform.tag == "Player")
            {
                hit.transform.GetComponent<BuildingLife>().ChangeLife(-attack_damage);
            }
        }
    }
}
