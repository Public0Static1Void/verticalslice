using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingEditor : MonoBehaviour
{
    [SerializeField] private bool canEdit = false;
    [SerializeField] private string builds_tag;
    [SerializeField] private float edit_range;

    [Header("References")]
    [SerializeField] private GameObject selected_building;
    [SerializeField] private TMP_Text selected_building_text;
    [SerializeField] private Material line_material;
    [SerializeField] private Material outline_material;

    private bool right_click_pressed = false;
    private float delta = 0;

    GameObject line;
    LineRenderer line_r;

    private Dictionary<GameObject, Material[]> trackedObjectMaterials = new Dictionary<GameObject, Material[]>();

    public void ChangeEditorMode(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            canEdit = !canEdit;
            if (!canEdit)
            {
                DeselectBuilding();
            }
            else
            {
                selected_building_text.text = "Edit mode ON";
            }
        }
    }

    private void DeselectBuilding()
    {
        selected_building = null;
        selected_building_text.text = "";
        delta = 0;
        right_click_pressed = false;

        if (line_r != null)
            line_r.enabled = false;

        trackedObjectMaterials.Clear();
    }

    public void SelectBuilding(InputAction.CallbackContext con)
    {
        if (con.performed && canEdit)
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 1.5f, Camera.main.transform.forward, out hit, edit_range))
            {
                if (hit.collider.tag == builds_tag)
                {
                    BuildingLife bl = hit.collider.GetComponent<BuildingLife>();
                    if (selected_building == null)
                    {
                        selected_building = hit.collider.gameObject;
                        selected_building_text.text = "Selected building: " + hit.collider.name;

                        SetLineBetweenConveyors(selected_building.transform.position, hit.collider.GetComponent<Conveyor>());
                    }
                    else
                    {
                        switch (bl.building_type)
                        {
                            case BuildingManager.Buildings.CONVEYOR:
                                if (selected_building.GetComponent<BuildingLife>().building_type == BuildingManager.Buildings.CONVEYOR)
                                {
                                    Conveyor m_conv = selected_building.GetComponent<Conveyor>();
                                    m_conv.nearest_conveyor = hit.collider.GetComponent<Conveyor>();
                                    m_conv.OrientateMineral();

                                    line_r.SetPosition(0, selected_building.transform.position);
                                    line_r.SetPosition(1, m_conv.nearest_conveyor.transform.position);
                                    line_r.enabled = true;
                                }
                                break;
                            case BuildingManager.Buildings.CORE:
                                hit.collider.GetComponent<Core>().CheckSurroundings();
                                break;
                            case BuildingManager.Buildings.DRILL:
                                if (selected_building.TryGetComponent<Conveyor>(out Conveyor conv))
                                {
                                    conv.nearest_drill = hit.collider.GetComponent<Drill>();
                                    if (!conv.nearest_drill.conveyor_connected)
                                        conv.nearest_drill.conveyor_connected = true;
                                    else
                                        conv.nearest_drill = null;
                                }
                                break;
                        }
                        selected_building = null;

                    }
                }
            }
        }
    }

    private void SetLineBetweenConveyors(Vector3 pos1, Conveyor conveyor)
    {
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
        if (selected_building == null) return;

        if (Vector3.Distance(transform.position, selected_building.transform.position) > edit_range * 1.2f)
        {
            DeselectBuilding();
        }

        if (right_click_pressed)
        {
            delta += Time.deltaTime;
            if (delta > 1)
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
        if (Physics.SphereCast(transform.position, 1.5f, Camera.main.transform.forward, out hit, edit_range))
        {
            if (hit.collider.tag == builds_tag)
            {
                StartCoroutine(ChangeMaterial(hit.transform.gameObject, hit));
            }
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

            hit.transform.GetComponent<MeshRenderer>().materials = trackedObjectMaterials[ob];
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