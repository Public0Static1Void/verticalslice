using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private float recoil;
    [SerializeField] private int damage;
    [SerializeField] private float knock_back;
    [SerializeField] private Vector3 offset;

    [Header("References")]
    [SerializeField] private LayerMask enemy_mask;

    public GameObject current_target;
    public GameObject cannon;

    public float delta = 0;
    void Start()
    {
        GetNearestEnemy();
    }

    private void Update()
    {
        GetNearestEnemy();
        OrientateToEnemy();
        Attack();
    }

    void GetNearestEnemy()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, range, enemy_mask);

        if (colls.Length <= 0) return;

        Transform nearest = colls[0].transform;
        for (int i = 0; i < colls.Length; i++)
        {
            if (Vector3.Distance(transform.position, colls[i].transform.position) < Vector3.Distance(nearest.position, transform.position))
            {
                nearest = colls[i].transform;
            }
        }
        current_target = nearest.gameObject;
    }

    void OrientateToEnemy()
    {
        if (current_target == null) return;
        
        Vector3 dif = current_target.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(dif);
        rot = new Quaternion(0, rot.y, 0, rot.w);
        transform.localRotation = Quaternion.RotateTowards(transform.rotation, rot, 5);

        // Cannon rotation
        dif = current_target.transform.position - cannon.transform.position;
        rot = Quaternion.LookRotation(dif);
        rot = new Quaternion(rot.x, rot.y, rot.z, rot.w);
        cannon.transform.position = transform.forward + transform.position + offset;
        cannon.transform.rotation = Quaternion.RotateTowards(cannon.transform.rotation, rot, 5);
    }

    void Attack()
    {
        if (current_target == null) return;

        delta += Time.deltaTime;
        if (delta > recoil)
        {
            current_target.GetComponent<EnemyStats>().ChangeLife(-damage);
            current_target.GetComponent<Rigidbody>().AddForce(transform.forward * knock_back, ForceMode.Impulse);
            delta = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}