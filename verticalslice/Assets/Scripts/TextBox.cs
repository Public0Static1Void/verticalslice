using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    [TextArea(3,10)]
    public string text_to_show;
    public TMP_Text ui_text;
    public UnityEngine.UI.Image ui_txt_bg;

    public float text_speed;

    private bool showing_text;

    public void SetText(string text)
    {
        text_to_show = text;
    }

    public void ShowText()
    {
        StartCoroutine(ShowTextCoroutine());
    }
    public void HideText()
    {
        ui_text.text = "";
        ui_txt_bg.gameObject.SetActive(false);
    }

    private IEnumerator ShowTextCoroutine()
    {
        showing_text = true;
        
        ui_text.color = new Color(ui_text.color.r, ui_text.color.g, ui_text.color.b, 0);
        ui_txt_bg.color = new Color(ui_txt_bg.color.r, ui_txt_bg.color.g, ui_txt_bg.color.b, 0);
        ui_txt_bg.gameObject.SetActive(true);

        ui_text.text = "";

        float timer = 0;
        int curr_char = 0;

        string curr_text = "";

        ui_text.text = text_to_show;
        ui_txt_bg.rectTransform.sizeDelta = new Vector2(ui_txt_bg.rectTransform.sizeDelta.x, ui_text.preferredHeight * 2.5f > 600 ? 600 : ui_text.preferredHeight * 1.5f);
        ui_text.text = "";

        while (ui_text.text.Length < text_to_show.Length && curr_char < text_to_show.Length)
        {
            if (text_to_show[curr_char] == '<')
            {
                int tag_end = text_to_show.IndexOf('>', curr_char);
                if (tag_end != -1)
                {
                    curr_text += text_to_show.Substring(curr_char, tag_end - curr_char + 1);
                    curr_char = tag_end + 1;
                }
            }


            timer += Time.deltaTime;
            if (timer >= text_speed && curr_char < text_to_show.Length)
            {
                curr_text += text_to_show[curr_char];
                ui_text.text = curr_text;
                curr_char++;
                timer = 0;
            }

            if (ui_text.color.a < 1)
            {
                Color col = ui_text.color;
                ui_text.color = new Color(col.r, col.g, col.b, ui_text.color.a + Time.deltaTime);
            }
            if (ui_text.color.a < 1)
            {
                Color col = ui_txt_bg.color;
                ui_txt_bg.color = new Color(col.r, col.g, col.b, ui_txt_bg.color.a + Time.deltaTime);
            }

            yield return null;
        }
    }
}