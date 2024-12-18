using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed;
    public float stop_range;

    private NavMeshAgent nav;
    [SerializeField] private GameObject current_target;
    [SerializeField] private LayerMask buildingLayer;

    public bool onAttackRange;
    Rigidbody rb;
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!MoveToCore())
        {
            MoveToNearestBuilding();
        }
        if (rb.velocity.magnitude > 0.05f)
        {
            rb.velocity *= 0.99f;
        }
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            if (hit.distance < stop_range)
                nav.isStopped = true;
        }
        else
        {
            nav.isStopped = false;
        }
    }

    public void SetDestiny(GameObject target)
    {
        nav.SetDestination(target.transform.position);
    }

    private void MoveToNearestBuilding()
    {
        if (!nav.hasPath)
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, 5000);
            List<Collider> correct_colls = new List<Collider>();
            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i].tag == "Building" || colls[i].tag == "Player")
                {
                    correct_colls.Add(colls[i]);
                }
            }
            if (correct_colls.Count > 0)
            {
                float dist = Vector3.Distance(transform.position, correct_colls[0].transform.position);
                GameObject nearest = correct_colls[0].gameObject;
                for (int i = 0; i < correct_colls.Count; i++)
                {
                    if (Vector3.Distance(transform.position, correct_colls[i].transform.position) > dist)
                    {
                        dist = Vector3.Distance(transform.position, correct_colls[i].transform.position);
                        nearest = correct_colls[i].gameObject;
                    }
                }

                current_target = nearest;
                SetDestiny(nearest);
            }
        }

        if (current_target != null && Vector3.Distance(transform.position, current_target.transform.position) > stop_range)
        {
            SetDestiny(current_target);
            onAttackRange = false;
            nav.isStopped = false;
        }
        else if (current_target != null)
        {
            nav.isStopped = true;
            onAttackRange = true;
            transform.LookAt(current_target.transform.position);
        }
    }
    private bool MoveToCore()
    {
        if (Core.instance == null) return false;
        
        if (!nav.hasPath)
        {
            if (Physics.Raycast(transform.position, Core.instance.transform.position - transform.position, out RaycastHit hit))
            {
                if (hit.transform.gameObject != Core.instance.transform.gameObject)
                {
                    current_target = hit.transform.gameObject;
                }
                else
                {
                    current_target = Core.instance.transform.gameObject;
                }
            }
        }

        if (current_target != null && Vector3.Distance(transform.position, current_target.transform.position) > stop_range)
        {
            SetDestiny(current_target);
            onAttackRange = false;
            nav.isStopped = false;
        }
        else if (current_target != null)
        {
            nav.isStopped = true;
            onAttackRange = true;
            transform.LookAt(current_target.transform.position);
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, stop_range);
    }
    private void OnDrawGizmos()
    {
        if (Core.instance != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Core.instance.transform.position);
        }
    }
}