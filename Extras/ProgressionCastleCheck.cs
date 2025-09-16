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

// This script is just for progressing in the level, when all ghosts are killed, the cannon in the planet is activated
public class ProgressionCastleCheck : MonoBehaviour
{
    public MovableEntity checkpoint;        // Reference to an enemy, needed to be killed in order to progress
    public Cannon cannon;                   // Reference to the cannon, blocked by the enemy's existence
    [SerializeField] private TMPro.TextMeshProUGUI hint;    // String text that tells the player what to do

    public void Start()
    {
        InvokeRepeating("Check", 3f, 3f); // Check if any enemies are alive
    }

    private void Check()
    {
        Object o = FindAnyObjectByType(typeof(ThrowFire));
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
            ActivateThirdCannon();
    }

    private void ActivateThirdCannon()
    {
        BlockDispatcher.Trigger("cannon-3");
        cannon.CanShoot(true);
        Destroy(this);
    }
}