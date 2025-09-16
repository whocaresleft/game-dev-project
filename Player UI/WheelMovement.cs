/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

// This script handles the visual movement of the player's model's wheels
public class WheelMovement : MonoBehaviour
{
    [SerializeField] private Transform player;          // Transform of the player, has the wheels as children
    [SerializeField] private Player p;                  // Player script of the player
    [SerializeField] private Animator wheelAnimator;    // Animator for the wheels, handles the state transition

    void Start()
    {
        p = player.GetComponent<Player>();
        wheelAnimator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        wheelAnimator.SetBool("moving", p.movementDirection.magnitude > 0f);            // Player is moving/still based on velocity
        wheelAnimator.SetFloat("forward", player.InverseTransformDirection(p.sum).z);   // Player is going forward/backwards based on the movement direction (sum) 's local z (local forward), this sets the intensity, meaning faster or slower wheels as well
    }
}
