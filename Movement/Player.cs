/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using UnityEngine;

// Script that handles the player's controls
public class Player : MovableEntity
{
    private float deltaX;                           // store horizontal input
    private float deltaZ;                           // store vertical input
    private float jumpForce = 9.0f;                  // instantaneous velocity applied at jump start meters per second (m/s)
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [SerializeField, ReadOnly] private bool healing;    // is the player healing?
    [SerializeField, ReadOnly] private float lastCheckTime;     // when were hp last checked
    [SerializeField, ReadOnly] private float lastCheckHp;       // hp value at the time of last check
    [SerializeField, ReadOnly] private float healingPerSecond;  // how many hp are restored each second
    [ReadOnly] public bool canGiveInput;  // if the player gives input, is it considered (true) or ignored (false)

    // ------------------------------------------------------------------------------------------------

    private new void Start()
    {
        base.Start();
        lastCheckHp = currentHealth;
        canGiveInput = true;
        healing = false;
    }
    private new void Update()
    {
        GetInput();                 // get input from keyboard
        base.Update();
        Jump();                     // jump if grounded

        PassiveHealingCheck();
    }

    // ------------------------------------------------------------------------------------------------

    /// <summary>
    /// Store input from keyboard horizontal and vertical movement.
    /// </summary>
    private void GetInput()
    {
        if (canGiveInput)
        {
            deltaX = Input.GetAxisRaw("Horizontal");
            deltaZ = Input.GetAxisRaw("Vertical");
        }
        else
        {
            deltaX = 0;
            deltaZ = 0;
        }
    }

    /// <summary>
    /// Calculate movement direction using input stored values.
    /// </summary>
    protected override void CalculateMovementDirection()
    {
        movementDirection = new Vector3(deltaX, 0, deltaZ);
        movementDirection = transform.TransformDirection(movementDirection);            // from local space to global
        movementDirection = movementDirection.normalized;                               // clamps diagonal movement
    }

    /// <summary>
    /// Apply an instantaneous velocity away from contact surface.
    /// </summary>
    private void Jump()
    {
        if (grounded && Input.GetKeyDown(jumpKey))
        {                           // if grounded and Jump button pressed
            Vector3 jumpDirection = this.transform.position - lastHit;          // calculate jump direction

            Rb.AddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);   // apply instantaneous force in the jump direction with a preset magnitude.
        }
    }

    private void PassiveHealingCheck()
    {

        if (currentHealth == maxHealth) return; 

        if (Time.time - lastCheckTime >= 5f)
        {
            float difference = Mathf.Abs(currentHealth - lastCheckHp);

            if (difference <= healingPerSecond)
            {
                if(!healing)
                    StartCoroutine(PassiveHealing());
            }
            else
            {
                healing = false;
                StopCoroutine(PassiveHealing());
            }
            lastCheckTime = Time.time;
            lastCheckHp = currentHealth;
        }
    }

    private IEnumerator PassiveHealing()
    {
        yield return null;
        healing = true;
        while (currentHealth != maxHealth && healing)
        {
            currentHealth = Mathf.Clamp(currentHealth + healingPerSecond, 0, maxHealth);
            yield return new WaitForSeconds(1f);
        }
        healing = false;
    }
}
