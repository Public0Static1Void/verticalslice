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

    public bool onAttackRange;
    Rigidbody rb;
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        MoveToCore();
        if (rb.velocity.magnitude > 0.05f)
        {
            rb.velocity *= 0.99f;
        }
    }

    public void SetDestiny(GameObject target)
    {
        nav.SetDestination(target.transform.position);
    }

    private void MoveToCore()
    {
        if (Core.instance == null) return;
        
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
        else
        {
            nav.isStopped = true;
            onAttackRange = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, stop_range);
    }
    private void OnDrawGizmos()
    {
        if (Core.instance != null)
        {
            Gizmos.DrawRay(transform.position, Core.instance.transform.position);
        }
    }
}