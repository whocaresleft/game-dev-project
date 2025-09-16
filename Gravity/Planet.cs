/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] public float Radius = 500f;             // maximum distance where planet's gravity applies
    [SerializeField] public float GravityAcceleration = 25f;  // gravity acceleration in meter per second square (m/s^2)
    private static float gravityRefreshTime = 0f;                                     // how frequently gravity updates for orbiting entities
    private List<Collider> orbitalEntities = new List<Collider>();                    // entities within the planet's orbit
    private bool isRunning = false;                                                   // true if refresh gravity coroutine still running
    private List<Vector3> gravityPoints = new List<Vector3>();                        // positions in space that apply a gravity force on entities
    private List<Transform> colliders = new List<Transform>();                        // planet colliders used to extract gravitational points

    // ------------------------------------------------------------------------------------------------

    private void Update()
    {
        GetColliders();                             // update planet colliders

        if (!isRunning)
        {                            // prevent multiple instances of the coroutine from running
            isRunning = true;
            StartCoroutine(GravityRefresh());
        }
    }

    // ------------------------------------------------------------------------------------------------

    /// <summary>
    /// Add planet colliders with "GravityCollider" layer mask to the list colliders.
    /// </summary>
    private void GetColliders()
    {
        colliders.Clear();

        foreach (Transform collider in this.transform)
            if (collider.gameObject.layer == LayerMask.NameToLayer("GravityCollider"))
                colliders.Add(collider);
    }

    /// <summary>
    /// Calculates a gravity point for each collider based on the given position.
    /// </summary>
    /// <param name="position"></param>
    private void GetGravityPoints(Vector3 position)
    {
        gravityPoints.Clear();

        foreach (Transform collider in colliders)
        {
            Vector3 closestPoint = collider.GetComponent<Collider>().ClosestPoint(position);
            gravityPoints.Add(closestPoint);
        }
    }

    /// <summary>
    /// Returns the closest gravity point to the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 ClosestGravityPointTo(Vector3 position)
    {
        GetGravityPoints(position);

        if (gravityPoints.Count > 0)
        {
            float distance = Vector3.Distance(position, gravityPoints.First());
            Vector3 closestPoint = gravityPoints.First();

            foreach (Vector3 point in gravityPoints)
            {                                  // find the gravity point with the smallest distance to the given position
                if (Vector3.Distance(position, point) < distance)
                {                      // if this point is closer, update the reference
                    distance = Vector3.Distance(position, point);
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        return this.transform.position;                                                 // returns the planet's origin when no gravity points are available
    }

    /// <summary>
    /// Apply gravity to all entities within the planet's orbit.
    /// Also remove gravity to entities outside the planet's orbit.
    /// </summary>
    public void ApplyGravity()
    {
        Collider[] entities = Physics.OverlapSphere(transform.position, Radius, LayerMask.GetMask("Entity"));      // get all entity inside the orbit

        List<Collider> EjectedEntities = orbitalEntities.Except(entities.ToList()).ToList();                            // get entities that have left the orbit

        foreach (var entity in EjectedEntities)
        {                                                                       // update list for ejected entities
            if (entity != null) 
            {
                entity.GetComponent<Entity>().RemovePlanet(this);   // ejected entities do not consider this planet gravitational force anymore              
                orbitalEntities.Remove(entity);
            }                                                                                                                     // update list of entities inside the orbit
        }

        List<Collider> EnteredEntities = entities.ToList().Except(orbitalEntities).ToList();                            // newly entered entities in the orbit

        orbitalEntities.AddRange(EnteredEntities);                                                                      // appends entities that just entered the orbit

        foreach (Collider entity in EnteredEntities)                                                                    // update list for newly entered entities in the orbit
            entity.GetComponent<Entity>().AddPlanet(this);                                                              // newly entered entities now consider this planet gravitational force
    }

    /// <summary>
    /// Set how fast gravity is applyed
    /// </summary>
    private IEnumerator GravityRefresh()
    {
        ApplyGravity();
        yield return new WaitForSeconds(gravityRefreshTime);
        isRunning = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0f, 1f, 0.5f); // Red with custom alpha
        Gizmos.DrawSphere(transform.position, Radius);
    }
#endif
}