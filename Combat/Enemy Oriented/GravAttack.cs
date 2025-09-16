/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using UnityEngine;

public class GravAttack : Combat
{
    [SerializeField] private float attackRange;                 // Range of the attack
    [SerializeField, ReadOnly] private bool midAttack;          // Flag, is the enemy attacking in this moment?
    [SerializeField, ReadOnly] private float currentDetects;    // How many times was the player detected within hearing or 'visible' range
    [SerializeField] private float neededDetects;               // How many times the player needs to be detected before being actually attacked
    private Enemy e;                                            // The enemy the script is attached to, as we need to use its looking direction and hearing range
    private Planet pl;                                          // The enemy actually becomes a planet while attacking, manipulating gravity
    private GameObject spherePrefab;                            // The enemy creates a gravitational point in front of him, we created a prefab as the creation procedure was boring and repetitive
    private GameObject sphere;                                  // Reference to said gravitational point ^

    private void Start()
    {
        spherePrefab = Resources.Load<GameObject>("Prefabs/Utils/Attack Point");
        e = gameObject.GetComponent<Enemy>();
        attackRange = e.hearDistance * 0.5f;
        currentDetects = 0;
        midAttack = false;
    }

    private void OnEnable()
    {
        currentDetects = 0;
        midAttack = false;
    }

    private void OnDisable()
    {
        currentDetects = 0;
        StopAllCoroutines();
        if (midAttack)
        {
            // If we were mid warning attack
            if (currentDetects < neededDetects) RevertStop(e.player);

            // If we were mid real attack
            else DeSetup(e.player);
        }
        midAttack = false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (midAttack)
        {
            // If we were mid warning attack
            if (currentDetects < neededDetects) RevertStop(e.player);

            // If we were mid real attack
            else RevertGrav();
        }
    }

    public override void Attack()
    {
        midAttack = true;
        Transform player = e.player; // The enemy holds a reference to the player (not really fair game)
        currentDetects = (currentDetects + 1) % (neededDetects + 1);
        if (currentDetects == neededDetects) StartCoroutine(YeetPlayer(player, 5f, 1f));    // Real attack - throw player away
        else StartCoroutine(StopPlayerForSeconds(player, 1f));  // Warning attack - stop the player's movement
    }

    public override bool AttackCondition()
    {
        return (e.checkDirection.magnitude <= attackRange) && !midAttack;
    }

    private IEnumerator YeetPlayer(Transform p, float attackPreparation, float attackDuration)
    {
        yield return null;

        // Setup the attack
        Setup(p);

        // Let the player reflect and throw him away
        yield return new WaitForSeconds(attackPreparation);

        Perform();

        yield return new WaitForSeconds(attackDuration);

        // Reset the enemy was it was before
        DeSetup(p);

        midAttack = false;
    }

    private IEnumerator StopPlayerForSeconds(Transform p, float duration)
    {
        yield return null;

        p.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        p.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        p.GetComponent<Player>().canGiveInput = false;

        yield return new WaitForSeconds(duration);

        RevertStop(p);

        midAttack = false;
    }

    private void Setup(Transform player)
    {
        // Setup the enemy to be still while the attack takes place
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.layer = 0;

        // Setup the planet script, to attract the player on its gravitational point
        pl = gameObject.AddComponent<Planet>();
        pl.enabled = false;
        pl.Radius = 2 * attackRange;
        pl.GravityAcceleration = 1000f;

        // Create a gravitational point in front of it
        sphere = GameObject.Instantiate(spherePrefab) as GameObject;
        sphere.transform.parent = transform;
        sphere.transform.position = transform.position + transform.forward * 2 + transform.up * 10;
        sphere.isStatic = true; // This shall not be moved

        Quaternion originalRotation = transform.rotation;

        player.GetComponent<Player>().canGiveInput = false;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;


        pl.enabled = true; // Enable the script to attract the player
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

        transform.rotation = originalRotation;
    }

    private void Perform()
    {
        // Invert gravity direction -> the player is thrown away, at 10 times speed (almost)
        pl.GravityAcceleration = -10f * pl.GravityAcceleration;
    }

    private void DeSetup(Transform player)
    {
        // Remove the planet script and delete the gravitational point
        RevertGrav();

        player.GetComponent<Player>().canGiveInput = true;

        // Restore enemy ability to walk
        gameObject.layer = LayerMask.NameToLayer("Entity");
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void RevertStop(Transform player)
    {
        player.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        player.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        player.GetComponent<Player>().canGiveInput = true;
    }

    private void RevertGrav()
    {
        Destroy(sphere);
        Destroy(pl);
    }
}
