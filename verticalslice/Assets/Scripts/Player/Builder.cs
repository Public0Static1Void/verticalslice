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

    [Header("Stats")]
    [SerializeField] private float offset_from_ground;
    [SerializeField] private float distance_from_player;
    private float max_distance_from_player, max_offset_from_ground;

    public bool relocate = false;
    public bool right_click_pressed = false;
    private float delta = 0;
    public enum DistanceMode { UP, FORWARD }
    public DistanceMode distance_mode = DistanceMode.FORWARD;
    void Start()
    {
        if (buildings.Count > (int)Buildings.LAST_NO_USE)
        {
            Debug.LogWarning("There are more buildings in the list that in the enum");
        }

        CreateGrid(build_menu.transform.GetChild(0).gameObject);
        build_menu.SetActive(false);

        max_distance_from_player = distance_from_player;
        max_offset_from_ground = offset_from_ground;
    }

    void Update()
    {
        if (right_click_pressed)
        {
            delta += Time.deltaTime;
            if (delta > 1)
            {
                distance_mode = DistanceMode.UP;
            }
            else
            {
                distance_mode = DistanceMode.FORWARD;
            }
        }
    }

    void FixedUpdate()
    {
        if (relocate)
        {
            RaycastHit hit;
            if (Physics.Raycast(curr_build_ob.transform.position, -transform.up, out hit))
            {
                if (hit.distance != offset_from_ground)
                {
                    float new_y = Mathf.Lerp(curr_build_ob.transform.position.y, hit.point.y + offset_from_ground, Time.fixedDeltaTime * 3);
                    curr_build_ob.transform.position = new Vector3(curr_build_ob.transform.position.x, new_y, curr_build_ob.transform.position.z);
                }
            }
            curr_build_ob.transform.position = new Vector3(
                transform.position.x + Camera.main.transform.forward.x * distance_from_player, 
                curr_build_ob.transform.position.y, transform.position.z + Camera.main.transform.forward.z * distance_from_player);
        }
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
        curr_build_ob = Instantiate(buildings[(int)build], transform.position + (transform.forward * distance_from_player), Quaternion.identity);
        
        curr_build = build;
        relocate = true;
    }
    public void PlaceBuilding(InputAction.CallbackContext con)
    {
        if (con.performed && curr_build != Buildings.LAST_NO_USE)
        {
            relocate = false;

            switch (curr_build)
            {
                case Buildings.CONVEYOR:
                    curr_build_ob.GetComponent<Conveyor>().StartConveyor();
                    break;
                case Buildings.DRILL:
                    curr_build_ob.GetComponent<Drill>().StartDrill();
                    break;
            }

            curr_build_ob.transform.rotation = buildings[(int)curr_build].transform.rotation;
            curr_build_ob = null;
            curr_build = Buildings.LAST_NO_USE;
        }
    }

    public void ChangeLength(InputAction.CallbackContext con)
    {
        if (con.performed && curr_build != Buildings.LAST_NO_USE)
        {
            Vector2 input = con.ReadValue<Vector2>();
            Debug.Log(input);
            switch (distance_mode)
            {
                case DistanceMode.UP:
                    offset_from_ground += input.y / 100;
                    offset_from_ground = Mathf.Clamp(offset_from_ground, 0.5f, max_offset_from_ground);
                    break;
                case DistanceMode.FORWARD:
                    distance_from_player += input.y / 100;
                    distance_from_player = Mathf.Clamp(distance_from_player, 1, max_distance_from_player);
                    break;
            }
        }
    }

    public void ChangeModeOrDelete(InputAction.CallbackContext con)
    {
        if (curr_build == Buildings.LAST_NO_USE) return;

        if (con.performed)
        {
            right_click_pressed = true;
        }
        if (con.canceled)
        {
            delta = 0;
            right_click_pressed = false;
        }
    }
}