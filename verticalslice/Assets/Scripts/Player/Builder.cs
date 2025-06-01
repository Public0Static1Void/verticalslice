using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
using System.ComponentModel;

public class Builder : MonoBehaviour
{
    [Header("References")]
    public List<GameObject> buildings;

    private bool open_menu = false;
    [SerializeField] private GameObject build_menu;
    [SerializeField] private UnityEngine.UI.Text iron_cost, coal_cost, gold_cost; /// Añadir un texto si se ponen más elementos
    [SerializeField] private Font textFont;

    public Material holographic_material;
    public Material line_material, line_connected_material;
    public Material construction_material;
    private Material[] object_original_materials;

    public BuildingManager.Buildings curr_build = BuildingManager.Buildings.LAST_NO_USE;
    GameObject curr_build_ob;

    public LayerMask layer_conveyor;
    public LayerMask layer_drill;
    public LayerMask layer_core;

    

    [Header("Stats")]
    [SerializeField] private float offset_from_ground;
    [SerializeField] private float distance_from_player, min_distance_from_player = 1;
    public float max_distance_from_player, max_offset_from_ground;

    [Header("Right click")]
    public Image right_click;
    public Image hold_right_click;
    public Image sliced_right_click;
    public bool relocate = false;
    public bool right_click_pressed = false;
    private float delta = 0;
    public enum DistanceMode { UP, FORWARD, LAST_NO_USE }
    public DistanceMode distance_mode = DistanceMode.FORWARD;
    private bool changed = false;

    private List<BuildingLife> buildings_life = new List<BuildingLife>();

    [Header("Events")]
    [Description("Estos eventos se borrarán una vez ejecutados")]
    public UnityEvent events_place;

    // Building rotation
    private float building_rot;

    private float current_offset_from_ground = 0;
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

        curr_build = BuildingManager.Buildings.LAST_NO_USE;

        for (int i = 0; i < buildings.Count; i++)
        {
            buildings_life.Add(buildings[i].GetComponent<BuildingLife>());
        }
    }

    void Update()
    {
        if (right_click_pressed)
        {
            // Si el jugador mantiene el click derecho por + de 1 segundo, cancelará la construcción, sino cambiará el modo de distancia
            delta += Time.deltaTime;
            if (delta > 0.25f)
            {
                sliced_right_click.fillAmount = delta * 1.25f;
            }
            if (delta > 1)
            {
                CancelBuilding();
            }
            else if (!changed)
            {
                distance_mode = (DistanceMode)((int)distance_mode + 1);
                if (distance_mode == DistanceMode.LAST_NO_USE)
                {
                    distance_mode = DistanceMode.UP;
                    GameManager.gm.ShowText("Distance mode: UP", 5);
                }
                else
                {
                    GameManager.gm.ShowText("Distance mode: FORWARD", 5);
                }
                changed = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (relocate && curr_build_ob != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(curr_build_ob.transform.position + Vector3.up, -Vector3.up, out hit))
            {
                if (hit.distance != offset_from_ground + curr_build_ob.transform.localScale.y / 2)
                {
                    float new_y = Mathf.Lerp(curr_build_ob.transform.position.y, hit.point.y + offset_from_ground + curr_build_ob.transform.localScale.y / 2, Time.fixedDeltaTime * 3);
                    curr_build_ob.transform.position = new Vector3(curr_build_ob.transform.position.x, new_y, curr_build_ob.transform.position.z);
                    current_offset_from_ground = new_y;
                }
            }
            curr_build_ob.transform.position = new Vector3(
                transform.position.x + Camera.main.transform.forward.x * distance_from_player, 
                curr_build_ob.transform.position.y, transform.position.z + Camera.main.transform.forward.z * distance_from_player);

            CheckNearestConnection();
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
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            RectTransform rect = ob.GetComponent<RectTransform>();
            rect.anchoredPosition = parent.GetComponent<RectTransform>().anchoredPosition;

            ob.transform.SetParent(parent.transform);

            UnityEngine.UI.Button bt = ob.AddComponent<UnityEngine.UI.Button>();
            int aux = i;
            bt.onClick.AddListener(() =>
            {
                Debug.Log("Click");
                CreateBuilding((BuildingManager.Buildings)aux);
            }); // Añade el evento que saldrá al pulsar el botón
            
            EventTrigger trig = ob.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => {
                ChangeBuildingCostPanel(buildings[aux].GetComponent<BuildingLife>());
                });
            trig.triggers.Add(entry);
            /*entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) => OnPointerExit());
            trig.triggers.Add(entry);*/

            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.None;
            bt.navigation = nav;
        }
    }

    public void ChangeBuildingCostPanel(BuildingLife bl)
    {
        iron_cost.text = "" + bl.iron_cost;
        coal_cost.text = "" + bl.coal_cost;
        gold_cost.text = "" + bl.gold_cost;
    }
    void ClearCostTexts()
    {
        iron_cost.text = "0";
        coal_cost.text = "0";
        gold_cost.text = "0";
    }

    public void CreateBuilding(BuildingManager.Buildings build)
    {
        Debug.Log(build);
        if (curr_build_ob != null)
        {
            CancelBuilding();
        }
        build_menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        open_menu = false;

        offset_from_ground = 0;
        distance_from_player = max_distance_from_player / 2;

        Debug.Log("Building " + build);
        curr_build_ob = Instantiate(buildings[(int)build], transform.position + (transform.forward * distance_from_player), buildings[(int)build].transform.rotation);
        /// Guarda el material original del objeto y le pone uno holográfico
        object_original_materials = curr_build_ob.GetComponent<MeshRenderer>().materials;
        SetObjectMaterial(curr_build_ob, holographic_material);
        curr_build = build;

        curr_build_ob.AddComponent<LineRenderer>();

        Collider[] colls = curr_build_ob.GetComponents<Collider>();
        foreach (Collider coll in colls)
        {
            coll.enabled = false;
        }
        if (build == BuildingManager.Buildings.WALL)
        {
            curr_build_ob.GetComponent<NavMeshObstacle>().carving = false;
        }

        min_distance_from_player = curr_build_ob.transform.localScale.z / 2 + 1;

        building_rot = 0;

        if (!buildings_life[(int)curr_build].canRotate)
            right_click.gameObject.SetActive(true);
        hold_right_click.gameObject.SetActive(true);
        sliced_right_click.gameObject.SetActive(true);

        relocate = true;
    }

    private void SetObjectMaterial(GameObject ob, Material mat)
    {
        MeshRenderer mesh_r = ob.GetComponent<MeshRenderer>();
        if (mesh_r != null)
            mesh_r.SetMaterials(new List<Material> { mat });

        if (ob.transform.childCount > 0)
        {
            foreach (Transform child in ob.transform)
            {
                SetObjectMaterial(child.gameObject, mat);
            }
        }
    }

    private void CheckNearestConnection()
    {
        GameObject nearest = null;
        float dist = 100;
        switch (curr_build)
        {
            case BuildingManager.Buildings.CONVEYOR:
                AssignNearest(ref dist, ref nearest, layer_conveyor);
                AssignNearest(ref dist, ref nearest, layer_drill);
                AssignNearest(ref dist, ref nearest, layer_core);
                break;
            case BuildingManager.Buildings.DRILL:
                AssignNearest(ref dist, ref nearest, layer_conveyor);
                break;
            case BuildingManager.Buildings.CORE:
                AssignNearest(ref dist, ref nearest, layer_conveyor);
                break;
            case BuildingManager.Buildings.FURNACE:
                AssignNearest(ref dist, ref nearest, layer_conveyor);
                break;
        }

        // Crea la línea de donde se conectará el edificio
        if (nearest != null)
            CreateLineOfConection(curr_build_ob, nearest.transform.position);
        else
        {
            curr_build_ob.GetComponent<LineRenderer>().enabled = false;
        }
    }

    private void AssignNearest(ref float dist, ref GameObject nearest, LayerMask layer)
    {
        Collider[] colls = Physics.OverlapSphere(curr_build_ob.transform.position, 8.5f, layer);

        for (int i = 0; i < colls.Length; i++)
        {
            float newDist = Vector3.Distance(curr_build_ob.transform.position, colls[i].transform.position);
            if (newDist < dist)
            {
                dist = newDist;
                nearest = colls[i].gameObject;
            }
        }
    }

    public void CreateLineOfConection(GameObject building, Vector3 dest)
    {
        // Dibuja una línea entre las posiciones
        LineRenderer lr = building.GetComponent<LineRenderer>();
        lr.enabled = true;

        Vector3[] pos = new Vector3[2];
        pos[0] = building.transform.position;
        pos[1] = dest;
        lr.SetPositions(pos);
        lr.startColor = Color.red;
        lr.endColor = Color.green;

        lr.material = line_material;
        lr.startWidth = 0.013f;
        lr.endWidth = 0.013f;
    }

    public void PlaceBuilding(InputAction.CallbackContext con)
    {
        if (curr_build_ob == null || !ResourceManager.instance.CheckIfAffordable(curr_build)) return;

        if (Physics.Raycast(curr_build_ob.transform.position + Vector3.up, -transform.up, out RaycastHit hit))
        {
            if (curr_build_ob.transform.position.y < hit.point.y + curr_build_ob.transform.localScale.y / 2)
            {
                curr_build_ob.transform.position = new Vector3(curr_build_ob.transform.position.x, hit.point.y + curr_build_ob.transform.localScale.y / 2, curr_build_ob.transform.position.z);
            }
        }

        if (con.performed && curr_build != BuildingManager.Buildings.LAST_NO_USE)
        {
            relocate = false;

            StartCoroutine(TransitionBuildingMaterial(curr_build, curr_build_ob));
        }
    }

    private IEnumerator TransitionBuildingMaterial(BuildingManager.Buildings building_type, GameObject ob_building)
    {
        curr_build_ob = null;
        curr_build = BuildingManager.Buildings.LAST_NO_USE;

        GameObject ob = ob_building;
        float transition = 0;
        MeshRenderer mesh_r = ob.GetComponent<MeshRenderer>();

        Material[] original_materials = object_original_materials;

        // Inicia el comportamiento del edificio
        switch (building_type)
        {
            case BuildingManager.Buildings.CONVEYOR:
            case BuildingManager.Buildings.FURNACE:
                ob.GetComponent<Conveyor>().StartConveyor();
                break;
            case BuildingManager.Buildings.DRILL:
                ob.GetComponent<Drill>().StartDrill();
                break;
            case BuildingManager.Buildings.CORE:
                ob.GetComponent<Core>().StartCore();
                break;
            case BuildingManager.Buildings.WALL:
                ob.GetComponent<NavMeshObstacle>().carving = true;
                break;
            case BuildingManager.Buildings.TURRET:
                ob.GetComponent<Turret>().Start_Turret();
                break;
        }

        // Ejecuta los eventos y los borra
        events_place.Invoke();
        events_place.RemoveAllListeners();


        right_click.gameObject.SetActive(false);
        hold_right_click.gameObject.SetActive(false);
        sliced_right_click.gameObject.SetActive(false);

        // Hace el efecto de construcción

        Texture main_texture = original_materials[0].GetTexture("_MainTex");

        mesh_r.material = construction_material;
        mesh_r.material.SetTexture("_Target_Texture", main_texture);
        mesh_r.material.SetColor("_MainColor", original_materials[0].GetColor("_Color"));

        mesh_r.material.SetFloat("_Transition", 0);
        while (transition < 1)
        {
            transition += Time.deltaTime;
            mesh_r.material.SetFloat("_Transition", transition);
            yield return null;
        }

        /// Pone los materiales originales del edificio
        ob.GetComponent<MeshRenderer>().materials = original_materials;
        Collider[] colls = ob.GetComponents<Collider>();
        foreach (Collider coll in colls)
        {
            coll.enabled = true;
        }

        ob.GetComponent<LineRenderer>().material = line_connected_material;
    }

    void CancelBuilding()
    {
        Destroy(curr_build_ob);
        sliced_right_click.fillAmount = 0;
        right_click.gameObject.SetActive(false);
        hold_right_click.gameObject.SetActive(false);
        sliced_right_click.gameObject.SetActive(false);
        relocate = false;
        curr_build_ob = null;
        curr_build = BuildingManager.Buildings.LAST_NO_USE;
        right_click_pressed = false;
        delta = 0;

        ClearCostTexts();
    }

    public void ChangeLength(InputAction.CallbackContext con)
    {
        if (con.performed && curr_build != BuildingManager.Buildings.LAST_NO_USE)
        {
            Vector2 input = con.ReadValue<Vector2>();
            Debug.Log(input);
            if (!buildings_life[(int)curr_build].canRotate)
            {
                switch (distance_mode)
                {
                    case DistanceMode.UP:
                        offset_from_ground += input.y / 100;
                        offset_from_ground = Mathf.Clamp(offset_from_ground, 0, max_offset_from_ground);
                        break;
                    case DistanceMode.FORWARD:
                        distance_from_player += input.y / 100;
                        distance_from_player = Mathf.Clamp(distance_from_player, min_distance_from_player, max_distance_from_player);
                        break;
                }
            }
            else
            {
                building_rot += input.y / 75;
                curr_build_ob.transform.rotation = Quaternion.Euler(new Vector3(0, building_rot));
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
            sliced_right_click.fillAmount = 0;
            right_click_pressed = false;
            changed = false;
        }
    }
}