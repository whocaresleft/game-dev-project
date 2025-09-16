/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public abstract class MovableEntity : Entity
{
    public float groundAcceleration = 50.0f;        // movement acceleration while grounded
    public float airAcceleration = 30.0f;           // movement acceleration while airborne
    public Vector3 movementDirection { get; protected set; }            // normalized movement vector
    private float movementAcceleration;             // strength of the movement in meters per second squared (m/s^2)
    public float movementVelocityCap = 6.0f;        // maximum velocity achievable through movement alone (can be exceeded by external forces)
    public bool preventForceStacking = false;       // if true, combined force magnitude will be clamped to the strongest individual force (movement or gravity)
    public Vector3 sum { get; private set; }                        // processed vector combining movement and gravity
    protected bool grounded;                        // surface contact detection flag
    protected Vector3 lastHit;                      // most recent collision position
    public float rotationSpeed = 0.3f;              // controls how fast entity aligns its down axis with planetary gravity

    public float groundRotationSpeed = 15f;     
    public float airRotationSpeed = 0.3f;

    public bool alive;
    public float currentHealth;
    public float maxHealth = 5.0f;
    public float minimumImpactVelocity;
    public Vector3 lastKnownVelocity;

    public event Action<MovableEntity> OnDeath;
    public event Func<bool> OnBeforeDeath;

    // ------------------------------------------------------------------------------------------------

    protected new void Start() 
    {
        base.Start();
        Rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        grounded = false;                           // starts with no surface collision (ground check will update if needed)
        movementAcceleration = airAcceleration;     // default with airborne movement acceleration (ground check will update if needed)

        currentHealth = maxHealth;
        alive = true;
    }

    protected new void Update()
    {
        if (alive)
        {
            base.Update();
            CalculateMovementDirection();       // calculate desired movement direction
            CalculateTotalForce();              // calculate net force from movement and gravity vectors
            Rotate();                           // aligns entity's orientation with nearest planet center
            StartCoroutine(VelocityFromSecondsAgo(0.1f));
        }
    }

    protected void FixedUpdate()
    {
        if(alive)
            Rb.AddForce(sum, ForceMode.Acceleration);   // apply net force to entity
    }

    // ------------------------------------------------------------------------------------------------

    /// <summary>
    /// Calculates the normalized movement direction.
    /// </summary>
    protected abstract void CalculateMovementDirection();

    /// <summary>
    /// Calculate net force vector combining movement and gravity.
    /// </summary>
    private void CalculateTotalForce()
    {
        Vector3 movementForce = movementDirection * movementAcceleration;
        Vector3 horizontalVelocity = Rb.linearVelocity;
        horizontalVelocity.y = 0;

        if (horizontalVelocity.magnitude >= movementVelocityCap && movementDirection != Vector3.zero)
        {      // change movement vector direction and magnitude if the horizontal velocity is greater than the movement velocity cap
            movementForce = (movementForce - horizontalVelocity.normalized * movementAcceleration) * 0.5f;
        }

        sum = movementForce + gravity;

        if (preventForceStacking)
        {                                                                          // if true, combined force magnitude will be clamped to the strongest individual force (movement or gravity) 
            if (gravity.magnitude >= movementForce.magnitude)
            {                                              // if gravity force greater than movement force 
                if (Vector3.Angle(movementForce, movementForce + 2 * gravity) < 90)
                {                          // angle where combined forces would exceed gravity magnitude
                    sum = sum.normalized * gravity.magnitude;                                               // result force magnitude is clamped to the strongest force (gravity)
                }
            }
            if (movementForce.magnitude > gravity.magnitude)
            {                                               // if movement force greater than gravity force
                if (Vector3.Angle(gravity, gravity + 2 * movementForce) < 90)
                {                                // Angle where combined forces would exceed movement magnitude
                    sum = sum.normalized * movementForce.magnitude;                                         // result force magnitude is clamped to the strongest force (movement)
                }
            }
        }
    }

    /// <summary>
    /// Aligns entity's orientation with nearest planet center.
    /// </summary>
    private void Rotate()
    {
        Quaternion targetRotation;

        if (base.planets.Count > 0)
        {
            targetRotation = Quaternion.FromToRotation(transform.up, -(NearestPlanet().ClosestGravityPointTo(this.transform.position) - transform.position)) * transform.rotation;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Returns the closest planet's position to the entity.
    /// </summary>
    /// <returns> Nearest planet position </returns>
    private Planet NearestPlanet()
    {
        Planet nearestPlanet = base.planets.First();

        if (base.planets.Count > 0)
            foreach (var planet in base.planets)
                if ((nearestPlanet.transform.position - this.transform.position).magnitude > (planet.transform.position - this.transform.position).magnitude)
                    nearestPlanet = planet;

        return nearestPlanet;
    }

    // ------------------------------------------------------------------------------------------------
    protected void OnCollisionEnter(Collision collision)
    {
        float impactVelocity = Vector3.Dot(lastKnownVelocity, -collision.contacts[0].normal); // Use the velocity the entity had 0.1 seconds ago, since in collision enter we have the velocity set to the one after the impact, not before
        TakeDamage(ImpactDamage(impactVelocity));
        StopCoroutine(nameof(VelocityFromSecondsAgo));
        lastKnownVelocity = Rb.linearVelocity;
    }

    protected new void OnCollisionStay(Collision collision)
    {
        base.OnCollisionStay(collision);
        grounded = true;
        movementAcceleration = groundAcceleration;
        rotationSpeed = groundRotationSpeed;

        lastHit = Vector3.zero;

        foreach (var contact in collision.contacts)
        {               // finds midpoint from all collision contacts
            lastHit += contact.point;
        }

        lastHit = lastHit / collision.contactCount;

    }

    protected new void OnCollisionExit(Collision collision)
    {
        base.OnCollisionExit(collision);
        grounded = false;
        movementAcceleration = airAcceleration;
        rotationSpeed = airRotationSpeed;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0.0f) return;
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0.0f)
        {
            alive = false;
            StartCoroutine(TheDie());
        }
    }

    private IEnumerator TheDie()
    {
        yield return null;
        // Ruotare verso una direzione (dietro forse)
        yield return new WaitForSeconds(1.5f);

        if (OnBeforeDeath != null) // Are there any checks to make before dying (eg. Invulnerability or others)
        {
            foreach (Func<bool> guard in OnBeforeDeath.GetInvocationList()) // If there are, check if they are valid or we die for real
            {
                if (!guard()) yield break;
            }
        }

        // Actual death
        Die();
    }

    private float ImpactDamage(float velocity)
    {
        return velocity < minimumImpactVelocity ? 0 : (velocity - minimumImpactVelocity) / 2f;
    }

    private IEnumerator VelocityFromSecondsAgo(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        lastKnownVelocity = Rb.linearVelocity;
    }

    public void Die()
    {
        gameObject.SetActive(false);
        OnDeath?.Invoke(this);
    }
}
