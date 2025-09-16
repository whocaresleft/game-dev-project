/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Rigidbody Rb { get; private set; }
    public Vector3 gravity { get; private set; }                         // gravity vector (0 force by default)
    public List<Planet> planets = new List<Planet>();            // planets orbited by the entity
    public float groundDrag = 3.0f;                                 // friction value while grounded or thouching surfaces

    // ------------------------------------------------------------------------------------------------

    protected void Start()
    {
        gravity = Vector3.zero;

        this.gameObject.layer = LayerMask.NameToLayer("Entity");    // set layer mask to "Entity"

        if (this.GetComponent<Rigidbody>() == null)                  // add Rigidbody component if missing
            this.AddComponent<Rigidbody>();

        // configure Rigidbody settings
        Rb = this.GetComponent<Rigidbody>();
        Rb.useGravity = false;
    }

    protected void Update()
    {
        CalculateGravity();     // calculate final gravity vector from all orbited planets
    }

    private void FixedUpdate()
    {
        Rb.AddForce(gravity, ForceMode.Acceleration);       // apply gravity to the entity
    }

    // ------------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a planet to the planets list.
    /// </summary>
    /// <param name="planet"></param>
    public void AddPlanet(Planet planet)
    {
        int index = planets.FindIndex(0, p => p.GetInstanceID() == planet.GetInstanceID());    // check if planet is already in list

        if (index == -1)                 // if planet not in list add it
            planets.Add(planet);

    }

    /// <summary>
    /// Remove a planet from the planets list
    /// </summary>
    /// <param name="planet"></param>
    public void RemovePlanet(Planet planet)
    {
        planets.Remove(planet);
    }

    /// <summary>
    /// Calculate net gravity vector from all planet forces.
    /// </summary>
    protected void CalculateGravity()
    {
        planets.RemoveAll(planet => planet == null);

        Vector3 entityPosition = this.transform.position;

        this.gravity = Vector3.zero;

        foreach (var planet in planets)
        {
            float distance = Vector3.Distance(planet.ClosestGravityPointTo(entityPosition), entityPosition);

            if (distance != 0)
                this.gravity += (planet.ClosestGravityPointTo(entityPosition) - entityPosition).normalized * planet.GravityAcceleration
                                / Mathf.Pow(distance, 0.3f);
        }
    }

    // ------------------------------------------------------------------------------------------------

    protected void OnCollisionStay(Collision collision)
    {
        Rb.linearDamping = groundDrag;  // enable friction while touching a surface
    }

    protected void OnCollisionExit(Collision collision)
    {
        Rb.linearDamping = 0;           // disable friction while airborne
    }
}
