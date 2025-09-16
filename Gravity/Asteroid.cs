/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using Unity.VisualScripting;
using UnityEngine;

public class Asteroid : Entity
{
    [SerializeField, ReadOnly] private float initialSpeed = 1.0f;
    [SerializeField] private bool showTray;

    private new void Start()
    {
        base.Start();

        Vector3 direction = (this.transform.GetChild(0).position - this.transform.position).normalized;

        Rb.AddForce(direction * initialSpeed, ForceMode.VelocityChange);

        if (GetComponent<TrailRenderer>() == null)
            this.AddComponent<TrailRenderer>();

        GetComponent<TrailRenderer>().enabled = true;
    }

    new void Update()
    {
        base.Update();
        GetComponent<TrailRenderer>().enabled = showTray;                           
    }
}