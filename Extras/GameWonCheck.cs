/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// This script is just for progressing in the level, when all planet enemies are killed, the game is won
public class GameWonCheck : MonoBehaviour
{
    public MovableEntity checkpoint;            // Reference to an enemy, needed to be killed in order to progress
    public GameObject gameWonMenu;              // Menu that will be displayed after winning
    public MenuHandler menuHandler;             // Script that handles the 'Esc' menu
    public Toggle increaseDiff;                 // Flag to determine if next run will be harder that this one
    [SerializeField] private TMPro.TextMeshProUGUI hint;    // Text field containing a hint for the player
    public MouseHorizontalLook mouseX;          // Reference to the player's horizontal mouse movement (will be disabled while on the menu)
    public MouseVerticalLook mouseY;            // Reference to the player's vertical mouse movement (will be disabled while on the menu)

    public void Start()
    {
        InvokeRepeating("Check", 3f, 3f); // Check if any enemies are alive
    }

    private void Check()
    {
        Object o = FindAnyObjectByType(typeof(GravAttack));
        if (o)
        {
            CancelInvoke("Check");
            checkpoint = o.GetComponent<Enemy>();
            if (checkpoint) checkpoint.OnDeath += (MovableEntity e) => { StartCoroutine(ChechAgain()); };
        }
        else checkpoint = null;
    }

    private IEnumerator ChechAgain()
    {
        yield return null;
        if (checkpoint) checkpoint.OnDeath -= (MovableEntity e) => { StartCoroutine(ChechAgain()); };
        Check();
        yield return new WaitForSeconds(3.5f);
        if (checkpoint == null)
            GameWon();
    }

    private void GameWon()
    {
        BlockDispatcher.Trigger("game-won");
        gameWonMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        Cursor.visible = true;
        menuHandler.enabled = false;
        mouseX.enabled = false;
        mouseY.enabled = false;
    }

    public void Restart()
    {
        if (increaseDiff.isOn)
        {
            string diff = PlayerPrefs.GetString("Difficulty", "Easy");
            string toSet = diff;
            switch (diff)
            {
                case "Easy":
                    toSet = "Normal";
                    break;

                case "Normal":
                    toSet = "Hard";
                    break;
            }
            PlayerPrefs.DeleteKey("Difficulty");
            PlayerPrefs.SetString("Difficulty", toSet);
        }
        mouseX.enabled = true;
        mouseY.enabled = true;
        menuHandler.enabled = true;
        menuHandler.RestartLevel();
    }
}