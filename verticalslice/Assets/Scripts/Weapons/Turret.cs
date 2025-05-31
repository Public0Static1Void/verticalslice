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

    private bool turret_started = false;
    private Rigidbody cannon_rb;
    public void Start_Turret()
    {
        Destroy(transform.GetChild(0).gameObject);

        cannon = Instantiate(cannon);
        cannon.transform.position = transform.position + transform.forward + offset;
        cannon.transform.rotation = transform.rotation;
        cannon_rb = cannon.GetComponent<Rigidbody>();

        turret_started = true;
    }

    private void Update()
    {
        if (!turret_started) return;
        GetNearestEnemy();
        if (Vector3.Distance(cannon.transform.position, transform.position + transform.forward + offset) < 0.1f)
        {
            OrientateToEnemy();
            Attack();
        }
    }

    void GetNearestEnemy()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, range, enemy_mask);

        if (colls.Length <= 0)
        {
            current_target = null;
            delta = 0;
            return;
        }

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
        rot = new Quaternion(rot.x  + 90, rot.y, rot.z, rot.w);
        cannon.transform.position = transform.forward + transform.position + offset;
        cannon.transform.rotation = Quaternion.RotateTowards(cannon.transform.rotation, rot, 1.5f);
    }

    void Attack()
    {
        if (current_target == null) return;

        delta += Time.deltaTime;
        if (delta > recoil)
        {
            current_target.GetComponent<EnemyStats>().ChangeLife(-damage);
            current_target.GetComponent<Rigidbody>().AddForce(transform.forward * knock_back, ForceMode.Impulse);

            cannon_rb.AddForce(-cannon.transform.forward * knock_back, ForceMode.Impulse);

            delta = 0;
        }
    }

    private void FixedUpdate()
    {
        if (cannon_rb != null && cannon_rb.velocity.magnitude > 0.1f)
        {
            cannon_rb.velocity *= 0.8f;
        }
        else if (Vector3.Distance(cannon.transform.position, transform.position + transform.forward + offset) > 0.1f)
        {
            cannon.transform.position = Vector3.Lerp(cannon.transform.position, transform.position + transform.forward + offset, Time.fixedDeltaTime * 2);
        }
    }

    private void OnDestroy()
    {
        Destroy(cannon);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}