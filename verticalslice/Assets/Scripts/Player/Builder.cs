using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    public enum Buildings { CONVEYOR, DRILL, LAST_NO_USE }
    public List<GameObject> buildings;

    private bool open_menu = false;
    [SerializeField] private GameObject build_menu;
    [SerializeField] private Font textFont;

    public Buildings curr_build = Buildings.LAST_NO_USE;
    GameObject curr_build_ob;
    void Start()
    {
        if (buildings.Count >= (int)Buildings.LAST_NO_USE)
        {
            Debug.LogWarning("There are more buildings in the list that in the enum");
        }

        CreateGrid(build_menu.transform.GetChild(0).gameObject);
        build_menu.SetActive(false);
    }

    public void OpenMenu(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            open_menu = !open_menu;
            build_menu.SetActive(open_menu);
            if (open_menu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void CreateGrid(GameObject parent)
    {
        for (int i = 0; i < (int)Buildings.LAST_NO_USE; i++)
        {
            GameObject ob = new GameObject();
            ob.name = buildings[i].name;

            UnityEngine.UI.Text text = ob.AddComponent<UnityEngine.UI.Text>();
            text.text = ob.name;
            text.font = textFont;
            text.fontSize = 32;

            RectTransform rect = ob.GetComponent<RectTransform>();
            rect.anchoredPosition = parent.GetComponent<RectTransform>().anchoredPosition;

            ob.transform.SetParent(parent.transform);

            UnityEngine.UI.Button bt = ob.AddComponent<UnityEngine.UI.Button>();
            int aux = i;
            bt.onClick.AddListener(() => CreateBuilding((Buildings)aux)); // Añade el evento que saldrá al pulsar el botón

            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.None;
            bt.navigation = nav;
        }
    }

    void CreateBuilding(Buildings build)
    {
        build_menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        open_menu = false;

        Debug.Log("Building " + build);
        curr_build_ob = Instantiate(buildings[(int)build], transform.position + (transform.forward * 5), Quaternion.identity);
        curr_build_ob.transform.SetParent(Camera.main.transform);
        curr_build = build;
    }
    public void PlaceBuilding(InputAction.CallbackContext con)
    {
        if (con.performed && curr_build != Buildings.LAST_NO_USE)
        {
            switch (curr_build)
            {
                case Buildings.CONVEYOR:
                    curr_build_ob.GetComponent<Conveyor>().StartConveyor();
                    break;
                case Buildings.DRILL:
                    curr_build_ob.GetComponent<Drill>().StartDrill();
                    break;
            }

            curr_build_ob.transform.SetParent(null);
            curr_build_ob.transform.rotation = buildings[(int)curr_build].transform.rotation;
            curr_build_ob = null;
            curr_build = Buildings.LAST_NO_USE;
        }
    }
}