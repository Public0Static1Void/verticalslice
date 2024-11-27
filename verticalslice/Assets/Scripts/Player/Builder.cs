using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    [Header("References")]
    public List<GameObject> buildings;

    private bool open_menu = false;
    [SerializeField] private GameObject build_menu;
    [SerializeField] private Font textFont;

    public Material holographic_material;
    private Material[] object_original_materials;

    public BuildingManager.Buildings curr_build = BuildingManager.Buildings.LAST_NO_USE;
    GameObject curr_build_ob;
    

    [Header("Stats")]
    [SerializeField] private float offset_from_ground;
    [SerializeField] private float distance_from_player, min_distance_from_player = 1;
    private float max_distance_from_player, max_offset_from_ground;

    public bool relocate = false;
    public bool right_click_pressed = false;
    private float delta = 0;
    public enum DistanceMode { UP, FORWARD, LAST_NO_USE }
    public DistanceMode distance_mode = DistanceMode.FORWARD;
    void Start()
    {
        buildings = BuildingManager.Instance.buildings_prefabs;
        if (buildings.Count > (int)BuildingManager.Buildings.LAST_NO_USE)
        {
            Debug.LogWarning("There are more buildings in the list that in the enum");
        }

        CreateGrid(build_menu.transform.GetChild(0).gameObject);
        build_menu.SetActive(false);

        max_distance_from_player = distance_from_player * 1.5f;
        max_offset_from_ground = offset_from_ground * 1.5f;

        curr_build = BuildingManager.Buildings.LAST_NO_USE;
    }

    void Update()
    {
        if (right_click_pressed)
        {
            // Si el jugador mantiene el click derecho por + de 1 segundo, cancelará la construcción, sino cambiará el modo de distancia
            delta += Time.deltaTime;
            if (delta > 1)
            {
                CancelBuilding();
            }
            else
            {
                distance_mode = (DistanceMode)((int)distance_mode + 1);
                if (distance_mode == DistanceMode.LAST_NO_USE)
                    distance_mode = DistanceMode.UP;
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
                if (hit.distance != offset_from_ground + curr_build_ob.transform.localScale.y / 2)
                {
                    float new_y = Mathf.Lerp(curr_build_ob.transform.position.y, hit.point.y + offset_from_ground + curr_build_ob.transform.localScale.y / 2, Time.fixedDeltaTime * 3);
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
        for (int i = 0; i < (int)BuildingManager.Buildings.LAST_NO_USE; i++)
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
            bt.onClick.AddListener(() => CreateBuilding((BuildingManager.Buildings)aux)); // Añade el evento que saldrá al pulsar el botón

            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.None;
            bt.navigation = nav;
        }
    }

    void CreateBuilding(BuildingManager.Buildings build)
    {
        build_menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        open_menu = false;

        Debug.Log("Building " + build);
        curr_build_ob = Instantiate(buildings[(int)build], transform.position + (transform.forward * distance_from_player), Quaternion.identity);
        /// Guarda el material original del objeto y le pone uno holográfico
        object_original_materials = curr_build_ob.GetComponent<MeshRenderer>().materials;
        curr_build_ob.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { holographic_material });
        curr_build = build;

        min_distance_from_player = curr_build_ob.transform.localScale.z / 2 + 0.5f;

        relocate = true;
    }
    public void PlaceBuilding(InputAction.CallbackContext con)
    {
        if (con.performed && curr_build != BuildingManager.Buildings.LAST_NO_USE)
        {
            relocate = false;

            switch (curr_build)
            {
                case BuildingManager.Buildings.CONVEYOR:
                    curr_build_ob.GetComponent<Conveyor>().StartConveyor();
                    break;
                case BuildingManager.Buildings.DRILL:
                    curr_build_ob.GetComponent<Drill>().StartDrill();
                    break;
                case BuildingManager.Buildings.CORE:
                    curr_build_ob.GetComponent<Core>().StartCore();
                    break;
            }

            curr_build_ob.transform.rotation = buildings[(int)curr_build].transform.rotation;
            curr_build_ob.GetComponent<MeshRenderer>().materials = object_original_materials;
            curr_build_ob = null;
            curr_build = BuildingManager.Buildings.LAST_NO_USE;
        }
    }

    void CancelBuilding()
    {
        Destroy(curr_build_ob);
        relocate = false;
        curr_build_ob = null;
        curr_build = BuildingManager.Buildings.LAST_NO_USE;
        right_click_pressed = false;
        delta = 0;
    }

    public void ChangeLength(InputAction.CallbackContext con)
    {
        if (con.performed && curr_build != BuildingManager.Buildings.LAST_NO_USE)
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
                    distance_from_player = Mathf.Clamp(distance_from_player, min_distance_from_player, max_distance_from_player);
                    break;
            }
        }
    }

    public void ChangeModeOrDelete(InputAction.CallbackContext con)
    {
        if (curr_build == BuildingManager.Buildings.LAST_NO_USE) return;

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