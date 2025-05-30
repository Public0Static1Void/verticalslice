using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class BuildingEditor : MonoBehaviour
{
    [SerializeField] private bool canEdit = false;
    [SerializeField] private string builds_tag;
    [SerializeField] private float edit_range;

    [Header("References")]
    [SerializeField] private GameObject selected_building;
    [SerializeField] private Material line_material;
    [SerializeField] private Material line_deselect_material;
    [SerializeField] private Material outline_material;
    [SerializeField] private UnityEngine.UI.Image sliced_right_click;
    [SerializeField] private UnityEngine.UI.Image right_click_img;

    private LineRenderer line_player;
    private Vector3 pos_line_player_target;

    private bool right_click_pressed = false;
    private float delta = 0;

    GameObject line;
    LineRenderer line_r;

    private Dictionary<GameObject, Material[]> trackedObjectMaterials = new Dictionary<GameObject, Material[]>();


    private void Start()
    {
        line_player = gameObject.AddComponent<LineRenderer>();
        line_player.enabled = false;

        line_player.startWidth = 0.01f;
        line_player.endWidth = 0.01f;

        line_player.material = line_material;
    }
    public void ChangeEditorMode(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            canEdit = !canEdit;
            if (!canEdit)
            {
                DeselectBuilding();
                GameManager.gm.ShowText("Edit mode OFF", 5);
            }
            else
            {
                line_player.enabled = true;
                line_player.SetPositions(new Vector3[2]{ transform.position, transform.position});
                GameManager.gm.ShowText("Edit mode ON", 0);
            }
        }
    }

    private void DeselectBuilding()
    {
        GameManager.gm.ShowText("Edit mode OFF", 2);
        canEdit = false;
        
        selected_building = null;
        delta = 0;
        right_click_pressed = false;

        if (line_r != null)
            line_r.material = line_deselect_material;

        line_player.enabled = false;

        sliced_right_click.fillAmount = 0;
        right_click_img.gameObject.SetActive(false);
        sliced_right_click.gameObject.SetActive(false);

        trackedObjectMaterials.Clear();
    }

    public void SelectBuilding(InputAction.CallbackContext con)
    {
        if (con.performed && canEdit)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Camera.main.transform.forward, out hit, edit_range))
            {
                if (hit.collider.tag == builds_tag)
                {
                    BuildingLife bl = hit.collider.GetComponent<BuildingLife>();
                    if (selected_building == null)
                    {
                        selected_building = hit.collider.gameObject;
                        GameManager.gm.ShowText("Selected building: " + hit.collider.name, 0);

                        SetLineBetweenConveyors(selected_building.transform.position, hit.collider.GetComponent<Conveyor>());
                    }
                    else
                    {
                        switch (bl.building_type)
                        {
                            case BuildingManager.Buildings.CONVEYOR:
                                BuildingLife selected_bl = selected_building.GetComponent<BuildingLife>();
                                if (selected_bl.building_type == BuildingManager.Buildings.CONVEYOR) // Ha seleccionado antes al conveyor
                                {
                                    Conveyor m_conv = selected_building.GetComponent<Conveyor>();
                                    m_conv.nearest_conveyor = hit.collider.GetComponent<Conveyor>();
                                    m_conv.OrientateMineral();

                                    line_r.SetPosition(0, selected_building.transform.position);
                                    line_r.SetPosition(1, m_conv.nearest_conveyor.transform.position);
                                    line_r.enabled = true;

                                    m_conv.can_deposite = true;

                                    Destroy(hit.collider.GetComponent<LineRenderer>());
                                }
                                else if (selected_bl.building_type == BuildingManager.Buildings.DRILL) // Ha seleccionado antes al drill
                                {
                                    Conveyor m_conv = bl.GetComponent<Conveyor>();
                                    m_conv.ConnectToToDrill(selected_building.GetComponent<Drill>());
                                    m_conv.CreateLineOfConection(new Vector3[2] { m_conv.transform.position, selected_building.transform.position });
                                }
                                else if (selected_bl.building_type == BuildingManager.Buildings.CORE) // ha seleccionado antes al core
                                {
                                    Conveyor m_conv = bl.GetComponent<Conveyor>();
                                    selected_building.GetComponent<Core>().CheckSurroundings();
                                    m_conv.CreateLineOfConection(new Vector3[2] { m_conv.transform.position, selected_building.transform.position });
                                    m_conv.can_deposite = true;
                                }
                                break;
                            case BuildingManager.Buildings.CORE:
                                hit.collider.GetComponent<Core>().CheckSurroundings();
                                if (selected_building.TryGetComponent<Conveyor>(out Conveyor conv_m))
                                {
                                    conv_m.CreateLineOfConection(new Vector3[2] { conv_m.transform.position, bl.transform.position });
                                    conv_m.can_deposite = true;
                                }
                                break;
                            case BuildingManager.Buildings.DRILL:
                                if (selected_building.TryGetComponent<Conveyor>(out Conveyor conv))
                                {
                                    conv.nearest_drill = hit.collider.GetComponent<Drill>();
                                    if (!conv.nearest_drill.conveyor_connected)
                                    {
                                        conv.nearest_drill.conveyor_connected = true;
                                        conv.can_extract = true;
                                    }
                                    else
                                    {
                                        conv.can_extract = false;
                                        conv.nearest_drill = null;
                                    }
                                }
                                break;
                        }
                        selected_building = null;
                        pos_line_player_target = transform.position;
                    }
                    right_click_img.gameObject.SetActive(true);
                    sliced_right_click.gameObject.SetActive(true);
                }
            }
        }
    }

    private void SetLineBetweenConveyors(Vector3 pos1, Conveyor conveyor)
    {
        if (conveyor == null) return;
        if (line == null)
        {
            line = new GameObject("Line");
            line_r = line.AddComponent<LineRenderer>();
            line_r.material = line_material;
        }
        else
        {
            line_r.enabled = true;
        }

        Vector3[] pos = new Vector3[2];
        pos[0] = pos1;
        if (conveyor.nearest_conveyor != null)
        {
            Conveyor conv = conveyor.nearest_conveyor;
            pos[1] = conv.transform.position;
        }
        else
        {
            pos[1] = pos[0];
        }

        line_r.SetPositions(pos);
        line_r.startColor = Color.yellow;
        line_r.startColor = Color.gray;

        line_r.startWidth = 0.01f;
        line_r.endWidth = 0.01f;
    }

    private void Update()
    {
        if (canEdit) line_player.SetPositions(new Vector3[2] { transform.position, pos_line_player_target });

        if (selected_building == null) return;

        if (Vector3.Distance(transform.position, selected_building.transform.position) > edit_range * 1.2f)
        {
            DeselectBuilding();
        }

        if (right_click_pressed)
        {
            delta += Time.deltaTime;
            sliced_right_click.fillAmount = delta * 1.25f;
            if (delta > 0.75)
            {
                Destroy(selected_building);
                DeselectBuilding();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!canEdit)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Camera.main.transform.forward, out hit, edit_range))
        {
            if (hit.collider.tag == builds_tag)
            {
                pos_line_player_target = hit.point;
                StartCoroutine(ChangeMaterial(hit.transform.gameObject, hit));
            }
        }
        else
        {
            pos_line_player_target = transform.position;
        }
    }

    private IEnumerator ChangeMaterial(GameObject ob, RaycastHit hit)
    {
        if (!trackedObjectMaterials.ContainsKey(ob))
        {
            Material[] materials = hit.transform.GetComponent<MeshRenderer>().materials;

            trackedObjectMaterials.Add(ob, materials);

            Material[] m = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                m[i] = materials[i];
            }
            m[materials.Length] = outline_material;

            hit.transform.GetComponent<MeshRenderer>().materials = m;

            yield return new WaitForSeconds(10);
            if (trackedObjectMaterials.ContainsKey(ob))
            {
                hit.transform.GetComponent<MeshRenderer>().materials = trackedObjectMaterials[ob];
                trackedObjectMaterials.Remove(ob);
            }
        }

        yield return null;
    }

    public void CheckRightClick(InputAction.CallbackContext con)
    {
        if (selected_building == null) return;

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