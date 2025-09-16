/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

// This script handles the menu that pops out when pressing 'Esc' (pauses the game)
public class MenuHandler : MonoBehaviour
{
    public GameObject menuUI;           // Reference to the menu
    private bool paused;                // Is the game paused?
    public MouseHorizontalLook mouseX;  // Mouse x movement script
    public MouseVerticalLook mouseY;    // Mouse y movement script
    public Camera additionalCamera;     // When the player dies, we activate this camera to render the vast space
    public Slider mouseXSlider;         // Slider for horizontal mouse sensitivity
    public Slider mouseYSlider;         // Slider for vertical mouse sensitivity
    public TMP_Dropdown resolutionMenu; // Dropdown to choose the preferred resolution/framerate
    private List<Resolution> resolutions = new List<Resolution>(); // ^ Holds the options stated before 
    public Camera mainCamera;           // Camera for the player
    public Slider fov;                  // Slider do adjust FOV
    public Toggle fullscreen;           // Flag to determine if game is fullscreen or windowed (no borderless for now)
    public Volume v;                    // Volume used to enable/disable luminous textures (the sun, stars and weapon beam material glow)
    public TextMeshProUGUI hints;       // Textbox to show hints for the player
    public TextMeshProUGUI keybinds;    // Textbox to show keybinds for the player
    public Transform planetForRocket;
    public GameObject rocketP;

    private void Start()
    {
        BlockDispatcher.ClearAll();
        Player p = FindAnyObjectByType<Player>();
        p.OnDeath += DeathMenu;
        additionalCamera.gameObject.SetActive(false);
        mainCamera.fieldOfView = 110;
        mouseX.horizontalSensitivity = 5;
        mouseY.verticalSensitivity = 5;
        mouseXSlider.value = mouseX.horizontalSensitivity;
        mouseYSlider.value = mouseY.verticalSensitivity;
        Application.targetFrameRate = 60;
        fov.value = mainCamera.fieldOfView;

        resolutionMenu.ClearOptions();
        List<string> options = new List<string>();
        foreach (Resolution r in Screen.resolutions)
        {
            resolutions.Add(r);
            options.Add(r.width + "x" + r.height + "@" + r.refreshRateRatio + "Hz");
        }
        resolutionMenu.AddOptions(options);

        int current = 0;
        Resolution currentRes = Screen.currentResolution;
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].width == currentRes.width &&
                resolutions[i].height == currentRes.height)
            {
                current = i;
                break;
            }
        }
        resolutionMenu.value = current;
        resolutionMenu.RefreshShownValue();

        GameObject rocket = Instantiate(rocketP);
        rocket.SetActive(false);
        rocket.transform.SetParent(planetForRocket.transform);
        rocket.transform.localPosition = new Vector3(-4.20071125f, 1.65999997f, -8.65420628f);
        rocket.transform.localEulerAngles = new Vector3(270f, 24.1428928f, 0f);
        rocket.SetActive(true);
    }

    public void SetResolution()
    {
        Resolution res = resolutions[resolutionMenu.value];
        Screen.SetResolution(res.width, res.height, fullscreen.isOn);
    }

    public void SetFov()
    {
        mainCamera.fieldOfView = fov.value;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    public void ToggleGraphics()
    {
        v.enabled = !v.enabled;
    }

    private void Pause() 
    {
        menuUI.SetActive(true);
        Time.timeScale = 0f;
        paused = true;
        mouseY.enabled = false;
        mouseX.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Resume()
    {
        menuUI.SetActive(false);
        Time.timeScale = 1f;
        paused = false;
        mouseY.enabled = true;
        mouseX.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleHints()
    {
        hints.enabled = !hints.enabled;
        keybinds.enabled = hints.enabled;
    }

    public void UpdateSens()
    {
        mouseX.horizontalSensitivity = mouseXSlider.value;
        mouseY.verticalSensitivity = mouseYSlider.value;
    }

    private void DeathMenu(MovableEntity e)
    {
        additionalCamera.gameObject.SetActive(true);
        additionalCamera.enabled = true;
        Pause();
        menuUI.transform.GetChild(0).gameObject.SetActive(false);
    }
}
