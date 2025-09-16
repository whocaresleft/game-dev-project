/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using TMPro;
using UnityEngine;

public class RocketActivate : MonoBehaviour
{
    private Transform cam;    // Used to determine the direction the player is interacting
    public TextMeshProUGUI command_hint;
    public Rocket r;

    private void Start()
    {
        cam = transform.GetChild(1);
        Invoke("Later", 2);
        
    }

    private void Later()
    {
        r = UnityEngine.Object.FindAnyObjectByType<Rocket>();
    }

    private void Update()
    {
        if (r &&r.CanStart())
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit info, 5f))
            {
                Rocket r = info.transform.gameObject.GetComponent<Rocket>();
                if (info.transform.GetComponent<Rocket>() == r)
                {
                    command_hint.text = "Press F to start the rocket";
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        BlockDispatcher.Trigger("rocket-start");
                        GetComponent<CannonActivate>().enabled = true;
                        this.enabled = false;
                    }
                }
                else
                {
                    command_hint.text = "";
                }
            }
            else
            {
                command_hint.text = "";
            }
        }
    }
}
