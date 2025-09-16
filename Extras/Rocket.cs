/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

public class Rocket : MonoBehaviour
{
    private Rigidbody rb;        // Rigidbody of the rocket
    private bool active;         // Is the rocket running?
    private bool canStart; // Can this rocket start?
    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        InvokeRepeating("CheckForFuel", 3, 3);
    }

    private void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody>();
        if(rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.linearVelocity = Vector3.zero;
        active = false;
    }

    private void OnEnable()
    {
        BlockDispatcher.Subscribe("rocket-start", () => StartEngine());
    }

    private void OnDisable()
    {
        BlockDispatcher.Unsubscribe("rocket-start", () => StartEngine());
    }

    void FixedUpdate()
    {
        if (active) rb.AddForce(transform.up * 12f, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < 2) return;
        // When colliding into the second planet
        if (!LayerMask.LayerToName(collision.gameObject.layer).Equals("Entity"))
        {
            source.Stop();
            active = false;
            if (rb)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.linearVelocity = Vector3.zero;
            }

            BlockDispatcher.Trigger("planet2-arrival");
        }
    }

    public void StartEngine()
    {
        if (!rb) SetupRigidbody();
        active = true;
        source.Play();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void CheckForFuel()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position + transform.up * 2, 5f);
        foreach (Collider c in colls)
        {
            if (c.GetComponent<Fuel>() != null) // If it's fuel, consume it
            {
                c.gameObject.SetActive(false);
                canStart = true;
                BlockDispatcher.Trigger("clear");
                CancelInvoke("CheckForFuel");
                break;
            }
        }
    }

    public bool CanStart() => canStart;
}
