using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gm { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject pause_menu;
    [SerializeField] private Text message_text;

    private float alpha = 0;
    private bool show_announce = false;
    private float txt_show_speed;
    // Pausa
    private bool pause = false;
    private Vector3 pause_original_position;
    private Vector2 pause_hide_position;
    private RectTransform pause_original_rect;
    private Image background;
    private float pause_alpha = 0;

    // Opciones
    [SerializeField] private GameObject settings_menu;
    [SerializeField] private Slider mouse_slider;
    [SerializeField] private TMP_Text mouse_slider_text;
    [SerializeField] private Slider volume_slider;
    [SerializeField] private TMP_Text volume_slider_text;
    [SerializeField] private AudioSource audio_source;

    private bool open_settings;
    private CameraMovement camera_m;


    void Awake()
    {
        if (gm == null)
            gm = this;
        else
            Destroy(this.gameObject);

        if (pause_menu == null) return;

        pause_original_rect = pause_menu.GetComponent<RectTransform>();

        pause_original_position = pause_original_rect.position;
        pause_hide_position = new Vector2(pause_original_position.x - Screen.currentResolution.width + pause_original_rect.rect.width, pause_original_position.y);
        pause_original_rect.position = pause_hide_position;
    }

    void Start()
    {
        if (pause_menu == null) return;

        background = pause_menu.transform.parent.GetComponent<Image>();
        background.gameObject.SetActive(false);
        camera_m = Camera.main.GetComponent<CameraMovement>();
    }

    void Update()
    {
        if (pause_menu == null) return;

        // Animación del menú
        if (pause)
        {
            if (pause_original_rect.position.x >= pause_original_position.x)
            {
                Time.timeScale = 0;
            }
            pause_original_rect.position = new Vector2(pause_original_rect.position.x + 10000 * Time.deltaTime, pause_original_rect.position.y);
            if (background.color.a < 1)
            {
                Color col = background.color;
                pause_alpha += Time.deltaTime * 5;
                background.color = new Color(col.r, col.g, col.b, pause_alpha);
            }
        }
        else if (pause_original_rect.position != pause_original_position)
        {
            if (pause_original_rect.position.x > pause_hide_position.x)
            {
                pause_original_rect.position = new Vector2(pause_original_rect.position.x - 10000 * Time.deltaTime, pause_original_rect.position.y);
            }
            if (background.color.a > 0)
            {
                Color col = background.color;
                pause_alpha -= Time.deltaTime * 5;
                background.color = new Color(col.r, col.g, col.b, pause_alpha);
            }
        }

        // Fade del texto
        if (show_announce)
        {
            alpha += Time.deltaTime * txt_show_speed;
            if (alpha < 1)
            {
                Color col = message_text.color;
                message_text.color = new Color(col.r, col.g, col.b, alpha);
            }
            else if (alpha / 3 > 2)
            {
                show_announce = false;
            }
        }
        else if (alpha > 0)
        {
            alpha -= Time.deltaTime * txt_show_speed;
            Color col = message_text.color;
            message_text.color = new Color(col.r, col.g, col.b, alpha);
        }
    }

    public void ShowText(string text, int show_speed = 3)
    {
        message_text.text = text;
        txt_show_speed = show_speed;
        show_announce = true;
    }

    public void PauseGame(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            pause = !pause;
            if (pause)
            {
                background.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                background.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1;
                CloseSettings();
            }
        }
    }

    public void OpenSettings()
    {
        open_settings = !open_settings;
        settings_menu.SetActive(open_settings);
        mouse_slider_text.text = camera_m.cameraSpeed.ToString("0.0");
        mouse_slider.value = camera_m.cameraSpeed;
        volume_slider_text.text = volume_slider.value.ToString("0.0");
    }
    public void CloseSettings()
    {
        open_settings = false;
        settings_menu.SetActive(false);
    }

    public void ChangeMouseSpeed()
    {
        camera_m.cameraSpeed = mouse_slider.value;
        mouse_slider_text.text = mouse_slider.value.ToString("0.0");
    }

    public void ChangeVolume()
    {
        audio_source.volume = volume_slider.value;
        volume_slider_text.text = volume_slider.value.ToString("0.0");
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}