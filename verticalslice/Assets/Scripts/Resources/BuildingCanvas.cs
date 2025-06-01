using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildingCanvas : MonoBehaviour
{
    public Canvas canvas;
    private UnityEngine.UI.Image ui_bg;
    private TMP_Text txt_description;

    private Transform player, parent;


    public void Init()
    {
        canvas = GetComponentInChildren<Canvas>();
        ui_bg = canvas.GetComponentInChildren<UnityEngine.UI.Image>();
        txt_description = canvas.GetComponentInChildren<TMP_Text>();

        // Consigue la descripción del edificio
        txt_description.text = ResourceManager.instance.resources_descriptions[(int)GetComponent<BuildingLife>().building_type];

        player = FindAnyObjectByType<PlayerMovement>().transform;
        parent = canvas.transform.parent;

        canvas.gameObject.SetActive(false);
    }

    public void SetDescription(string description)
    {
        txt_description.text = description;
    }

    public void ShowDescription()
    {
        Vector3 start_pos = canvas.transform.position;
        canvas.transform.SetParent(null, true);

        canvas.transform.position = start_pos;

        Vector3 dir = (player.position - ui_bg.transform.position).normalized;
        canvas.transform.rotation = Quaternion.LookRotation(-dir);


        canvas.gameObject.SetActive(true);
    }
    public void HideDescription()
    {
        canvas.gameObject.SetActive(false);

        canvas.transform.SetParent(parent);
    }
}