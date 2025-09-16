/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] private bool canShoot;             // Can this cannon shoot?
    [SerializeField] private string eventToTrigger;     // Name of the event the cannon will trigger after throwing the player
    [SerializeField] private Vector3 throwPosition;     // Position the player is placed in before being thrown
    [SerializeField] private Vector3 throwRotation;     // Rotation the player is being faced before being thrown
    [SerializeField] private Transform destination;     // Determines the direction the cannon throws the player to 
    [SerializeField] private float intensity;           // Cannon's force at throwing the player
    private Rigidbody thingToShoot;                     // Rigidbody of the player to shoot
    [SerializeField] private Cannon nextCannon;         // Does this cannon come after, in progression, this one?
    [SerializeField] private List<Planet> toEnable;     // List of planets to enable (for optimization)
    [SerializeField] private List<Planet> toDisable;    // List of planets to disable (for optimization)
    [SerializeField] private TextMeshProUGUI hint;      // String text that should help the player figure out what to do (this is just the field that will contain the text)
    [SerializeField] private AudioSource shotFired;
    private void OnEnable()
    {
        CannonActivate.OnClickActivate += (Rigidbody r) => { thingToShoot = r;  StartShoot(); };
    }

    private void OnDisable()
    {
        CannonActivate.OnClickActivate -= (Rigidbody r) => { thingToShoot = r; StartShoot(); };
    }

    private void StartShoot()
    {
        if (!canShoot) return;
        StartCoroutine(Shoot());
    }
    private IEnumerator Shoot()
    {
        yield return null;
        BlockDispatcher.Trigger(eventToTrigger); // Trigger the event
        toEnable.ForEach(p => p.enabled = true);

        canShoot = false;

        thingToShoot.GetComponent<Player>().canGiveInput = false;
        yield return new WaitForSeconds(1.4f);
        thingToShoot.isKinematic = true;
        thingToShoot.constraints = RigidbodyConstraints.FreezeAll;
        yield return new WaitForSeconds(0.1f);
        thingToShoot.transform.position = throwPosition;
        thingToShoot.transform.rotation = Quaternion.Euler(throwRotation.x, throwRotation.y, throwRotation.z);
        thingToShoot.isKinematic = false;
        yield return new WaitForSeconds(1.5f);

        thingToShoot.constraints = RigidbodyConstraints.FreezeRotation;
        thingToShoot.GetComponent<Player>().canGiveInput = true;
        yield return new WaitForEndOfFrame();
        if (shotFired != null) shotFired.Play();
        thingToShoot.AddForce((destination.position-thingToShoot.transform.position).normalized * intensity, ForceMode.VelocityChange);
        if(nextCannon) nextCannon.enabled = true;

        yield return new WaitForSeconds(1.0f);
        toDisable.ForEach(p => p.enabled = false);
        this.enabled = false;
    }

    public bool CanShoot() => canShoot;
    public void CanShoot(bool value) => canShoot = value;
}