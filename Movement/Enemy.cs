/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

// Class that implements the FSM that manages the enemy behaviour

public class Enemy : MovableEntity
{
    public enum State { idle, chase, recon, search };   // Each enemy can be in either of 4 states
    [Header("States")]  
    public State state; // State the enemy is ib

    [Header("Idle")]
    public float idleTimer;     // How much time the enemy has wandered around
    public float idleDuration;  // How much time the enemy wanders around

    [Header("Recon")]
    private Quaternion rotation;    // How much to rotate before checking
    public float FOV;               // How much is actually visible to the enemy?
    public Vector3 checkDirection;  // Direction the enemy is checking for a player
    public float viewDistance;      // How far the enemy sees
    public float hearDistance;      // How far the enemy 'hears' (detects the player even if not visible, for example if the player tries to sneak behind the enemy)

    [Header("Chase")]
    public Transform player;        // Reference to the player's transform, the enemy actually knows where the player is, it just 'acts fair' by limiting his choises based on FOV, hearing and view distance

    [Header("Search")]
    public float searchTimer;       // How much did the enemy search for the player
    public float maxSearchTime;     // How much does the enemy search at max?
    public Vector3 lastSeenPoint;   // Last position the player was spotted

    [Header("Optional - Combat")]
    public Combat combat;           // This script defines the enemy's attacking method

    [Header("Optional - Audio")]
    public AudioSource source;      // Audio source for the enemy (sound he makes)


    // ------------------------------------------------------------------------------------------------

    private new void Start()
    {
        base.Start();
        player = FindAnyObjectByType<Player>().transform;
        state = State.idle;

        if (combat == null) combat = gameObject.AddComponent<Peace>();

        source = gameObject.GetComponent<AudioSource>();
        source.playOnAwake = false;
        if (source != null) Invoke("PlaySound", 10);
    }


    // ------------------------------------------------------------------------------------------------

    private void PlaySound() {
        source.Play();
        Invoke("PlaySound", Random.Range(7, 10));
    }

    private void OnDestroy()
    {
        CancelInvoke("PlaySound");
    }

    // The enemy is actually implemented as a Finite State Machine
    protected override void CalculateMovementDirection()
    {
        switch (state)
        {
            case State.idle:
                Idle();
                break;

            case State.chase:
                Chase();
                break;

            case State.recon:
                Recon();
                break;
            case State.search:
                Search();
                break;
        }
    }

    // ------------------------------------------------------------------------------------------------

    // Enemy behaviour in every state, as well as transitions based on conditions
    private void Idle()
    {
        idleTimer += Time.deltaTime;
        movementDirection = transform.forward;
        combat.enabled = false;

        if (DetectPlayer())
        {
            state = State.chase;
        }

        else if (idleTimer > idleDuration)
        {
            idleTimer = 0.0f;
            state = State.recon;
        }
    }
    private void Chase()
    {
        // Keep rotating towards the player and follow him
        RotateTowardsPlayer();
        movementDirection = transform.forward;
        combat.enabled = true;


        if (!DetectPlayer())
        {
            lastSeenPoint = player.position;
            state = State.search;
        }
    }
    private void Recon()
    {
        rotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.up);
        transform.rotation = rotation * transform.rotation;
        movementDirection = Vector3.zero;
        combat.enabled = false;

        if (DetectPlayer())
        {
            state = State.chase;
        }
        else
        {
            idleDuration = Random.Range(3.0f, 6.0f);
            state = State.idle;
        }
    }
    private void Search()
    {
        searchTimer += Time.deltaTime;
        transform.rotation = Quaternion.LookRotation((lastSeenPoint - transform.position).normalized, transform.up);
        movementDirection = transform.forward;
        combat.enabled = false;

        if (DetectPlayer())
        {
            searchTimer = 0.0f;
            state = State.chase;
        }
        if (Vector3.Distance(transform.position, lastSeenPoint) < 0.5f)
        {
            searchTimer = 0.0f;
            state = State.recon;
        }
        if (searchTimer >= maxSearchTime)
        {
            searchTimer = 0.0f;
            state = State.recon;
        }
    }
    //----------------------------------------
    private bool DetectPlayer()
    {
        checkDirection = player.position - transform.position;

        if (checkDirection.magnitude <= hearDistance) return true;

        if (checkDirection.magnitude > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, checkDirection);
        if (angle > FOV / 2.0f) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, checkDirection.normalized, out hit, viewDistance))
        {
            if (hit.transform.gameObject == player.gameObject) return true;
        }
        return false;
    }

    // Rotation was 'delicate' as we need to keep our rotation consistent both towards the player as well as the gravity of the player the enemy is on
    private void RotateTowardsPlayer()
    {
        Vector3 playerDirection = checkDirection.normalized;

        Quaternion rotation = Quaternion.LookRotation(playerDirection, transform.up);
        Vector3 localEuler = (Quaternion.Inverse(transform.rotation) * rotation).eulerAngles;

        localEuler.x = 0;
        localEuler.z = 0;

        Quaternion globalRotation = transform.rotation * Quaternion.Euler(localEuler);

        transform.rotation = globalRotation;
    }
}
