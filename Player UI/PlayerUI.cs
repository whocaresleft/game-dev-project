/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using TMPro;
using UnityEngine;

// Script that handles the player's UI, rendering the crosshair, the player's hp and the hp of an enemy, if the player is looking at it
public class PlayerUI : MonoBehaviour
{

    [SerializeField] private int crosshairSize;                 // Size of the crosshair
    private Camera cam;                                         // Used to determine crosshair position 
    private Player p;                                           // Used to determine HP
    [SerializeField, ReadOnly] private TextMeshProUGUI hp;      // Text box that shows the HP of the player
    [SerializeField, ReadOnly] private TextMeshProUGUI enemyHP; // Text box that shows the HP of the enemy
    private RaycastHit info;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        enemyHP.color = Color.gray;

        cam = Camera.main;
        p = GetComponent<Player>();

        BlockDispatcher.Subscribe("game-won", () => this.enabled = false);
    }

    private void OnGUI()
    {
        float posX = cam.pixelWidth / 2 - crosshairSize / 4;
        float posY = cam.pixelHeight / 2 - crosshairSize / 2;
        GUIStyle s = new GUIStyle();
        s.normal.textColor = Color.green;
        s.fontStyle = FontStyle.Bold;
        s.fontSize = crosshairSize;
        GUI.Label(new Rect(posX, posY, crosshairSize, crosshairSize), " + ", s);
    }

    private void Update()
    {
        hp.text = "HP: " + Mathf.Round(Mathf.Clamp(p.currentHealth, 0, p.maxHealth) * 100f)/ 100f + "/" + p.maxHealth;
        hp.color = p.currentHealth > 70f ? Color.blue : (p.currentHealth > 30 ? Color.yellow : Color.red);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out info, 20f))
        {
            Enemy e = info.transform.gameObject.GetComponent<Enemy>();
            if (e)
            {
                enemyHP.enabled = true;
                enemyHP.text = "HP: " + Mathf.Round(Mathf.Clamp(e.currentHealth, 0f, e.maxHealth) * 100f)/100f + "/" + e.maxHealth;
            }
        }
        else
        {
            enemyHP.enabled = false;
        }
    }
}
