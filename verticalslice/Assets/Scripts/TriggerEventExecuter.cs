using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEventExecuter : MonoBehaviour
{
    public string target_tag;

    public bool execute_one_time;

    public UnityEvent enter_events;
    public UnityEvent exit_events;

    private bool can_execute = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!can_execute) return;

        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                enter_events.Invoke();
                if (execute_one_time)
                    can_execute = false;
            }
        }
        else
        {
            enter_events.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!can_execute) return;

        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                exit_events.Invoke();
            }
        }
        else
        {
            exit_events.Invoke();
        }

    }
}
