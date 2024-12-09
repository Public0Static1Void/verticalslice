using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed;
    [SerializeField] private float stop_range;

    private NavMeshAgent nav;
    [SerializeField] private GameObject current_target;

    private bool onAttackRange;
    
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        
    }

    private void Update()
    {
        MoveToCore();
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
            if (Physics.Raycast(transform.position, Core.instance.transform.position, out RaycastHit hit))
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

        if (Vector3.Distance(transform.position, Core.instance.transform.position) > stop_range)
        {
            SetDestiny(current_target);
            onAttackRange = false;
        }
        else
        {
            onAttackRange = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, stop_range);
    }
}
