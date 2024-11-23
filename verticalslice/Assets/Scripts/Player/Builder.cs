using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Builder : MonoBehaviour
{
    public enum Buildings { CONVEYOR, DRILL, LAST_NO_USE }
    public List<GameObject> buildings;

    private bool open_menu = false;
    [SerializeField] private GameObject build_menu;
    void Start()
    {
        if (buildings.Count >= (int)Buildings.LAST_NO_USE)
        {
            Debug.LogWarning("There are more buildings in the list that in the enum");
        }
    }

    
    void Update()
    {
        
    }

    public void OpenMenu(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            open_menu = !open_menu;
            build_menu.SetActive(open_menu);
        }
    }
}