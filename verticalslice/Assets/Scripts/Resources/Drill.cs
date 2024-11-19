using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    public Resource resource;

    [Header("Stats")]
    public float production;
    [Range(0, 1)]
    public float efficency;
    public float amount_stored;
    public float max_stored;
    
    public float recoil;
    private float delta;

    [SerializeField] private bool can_drill;


    void Start()
    {
        StartDrill();
    }
    void Update()
    {
        if (can_drill)
        {
            delta += Time.deltaTime;
            if (delta >= recoil)
            {
                Extract();
                delta = 0;
            }
        }
    }

    public void Extract()
    {
        if (amount_stored < max_stored)
            amount_stored += production * efficency;
    }

    public void StopDrill()
    {
        can_drill = false;
        delta = 0;
    }

    public void StartDrill()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, transform.localScale.x))
        {
            if (hit.transform.TryGetComponent<Resource>(out Resource res)) {
                resource = res;
                can_drill = true;
            }
            else
            {
                Debug.Log("Couldn't init the drill, no resource found");
                can_drill = false;
            }
        }
    }
}