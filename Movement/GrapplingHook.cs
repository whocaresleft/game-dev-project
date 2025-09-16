/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour
{
    [Header("Cable Tweaks")]
    [SerializeField] private KeyCode retractCable = KeyCode.LeftShift;  // Shift for pulling ourselves towards the hooked point
    [SerializeField] private KeyCode extendCable = KeyCode.LeftControl; // Ctrl to extend the cable
    [SerializeField] private KeyCode pull = KeyCode.Mouse1;             // Right click to pull entities
    [SerializeField, ReadOnly] private float extendCableSpeed = 2.0f;   // Speed for extending the cable
    [SerializeField, ReadOnly] private float retractCableSpeed = 2.0f;  // Speed for pulling ourselves
    [SerializeField, ReadOnly] private float pullSpeed = 2.0f;          // Speed for pulling entities

    [Header("Grappling Hook")]
    [SerializeField, ReadOnly] private float hookRange; // How far the hook can hit entities/surfaces 
    private RaycastHit hitInfo; // Used to determine if something was hit
    [SerializeField, ReadOnly] private bool active; // Is the grapple in active use?
    [SerializeField, ReadOnly] private hookType type;   // Pulling (entities) or Swinging (Surfaces)
    [SerializeField] private KeyCode throwKey;          // KeyCode to throw the grapple (should be E)

    private Transform targetHit; // Transform that was hit
    private Vector3 localHookPoint; // Point hit from the local coordinates
    public Vector3 globalHookPoint { get; private set; } // Same point as ^ but in global coordinates

    public enum hookType { pulling, swinging }

    [Header("Cooldown")]
    [SerializeField, ReadOnly] private bool canThrow;   // Can the hook be thrown
    [SerializeField, ReadOnly] private float coolDown;  // How much cooldown there is

    [Header("References")]
    private GrapplingHandler handler;  // Handler for the visual aspect
    private Transform cam;             // Used to determine where to throw the hook                
    private SpringJoint joint;         // Joint used to simulate a spring, to make the grapple a little elastic
    private Rigidbody playerRb;        // Rigidbody of the player
    private Rigidbody targetRb;        // Rigidbody of the hit entity
    private AudioSource source;
    [SerializeField] private AudioClip grappleShot;
    [SerializeField] private AudioClip ropeSound;


    //----------------------------------------------------------------------------

    private void Start() 
    {
        active = false;
        canThrow = true;
        handler = GetComponent<GrapplingHandler>();
        cam = transform.GetChild(1);
        source = transform.GetChild(1).GetChild(1).GetComponent<AudioSource>();
        Invoke("GetLater", 2);
    }

    private void GetLater() { playerRb = GetComponent<Rigidbody>(); }

    private void Update()
    {
        if (Input.GetKeyDown(throwKey))
        {
            StartCoroutine(ThrowHook());
        }
        else if (Input.GetKeyUp(throwKey))
        {
            StartCoroutine(ReleaseHook());
        }
    }

    private void FixedUpdate()
    {
        if (active)
        {
            if (!targetHit || !targetHit.gameObject.activeSelf) StartCoroutine(ReleaseHook());
            globalHookPoint = targetHit.TransformPoint(localHookPoint);
            if(handler) handler.DrawHook(globalHookPoint);
            switch (type)
            {
                case hookType.pulling:
                    HandlePull();
                    break;

                case hookType.swinging:
                    joint.connectedAnchor = globalHookPoint;
                    HandleSwing();
                    break;
            }
        }
        else
        {
            targetHit = null;
            if (handler) handler.EraseHook();
        }
    }

    private IEnumerator ThrowHook()
    {
        yield return null;
        if (!canThrow || active) yield break;

        if (source != null) { 
            source.PlayOneShot(grappleShot);
            source.clip = ropeSound;
            source.Play();
        }

        if (Physics.Raycast(cam.position, cam.forward, out hitInfo, hookRange))
        {
            targetHit = hitInfo.transform;
            GameObject o = targetHit.gameObject;
            localHookPoint = targetHit.InverseTransformPoint(hitInfo.point);
            globalHookPoint = targetHit.TransformPoint(localHookPoint);

            if (o.layer == LayerMask.NameToLayer("Entity"))
            {
                type = hookType.pulling;
                targetRb = targetHit.gameObject.GetComponent<Rigidbody>();
                active = ConfigurePullingJoint();
            }
            else
            {
                type = hookType.swinging;
                active = ConfigureSwingingJoint();
            }
        }
        
        if(!active) // If it wasn't activated
        {
            if (handler) handler.DrawHook(cam.position + cam.forward * hookRange);
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(ReleaseHook());
        }
    }
    private IEnumerator ReleaseHook() 
    { 
        yield return null;
        active = false;
        if (source != null)
        {
            source.Stop();
        }
        StartCoroutine(CoolDown(coolDown));
        yield return new WaitForEndOfFrame();
        Destroy(joint);
    }

    //----------------------------------------------------------------------------

    private IEnumerator CoolDown(float cdTimer)
    {
        canThrow = false;
        yield return new WaitForSeconds(cdTimer);
        if (handler) handler.ShowTip();
        canThrow = true;
    }

    private void HandleSwing() // Swing, player can retract and extend the cable, speed is reduced each fixed update (losing momentum)
    {
        playerRb.linearVelocity = playerRb.linearVelocity * 0.97f;
        if (Input.GetKey(retractCable))
        {
            joint.minDistance = Mathf.Max(joint.minDistance - 0.5f * retractCableSpeed * Time.deltaTime, 1f);
            joint.maxDistance = Mathf.Max(joint.maxDistance - retractCableSpeed * Time.deltaTime, 2f);
            playerRb.AddForce((globalHookPoint - transform.position).normalized * retractCableSpeed * Time.deltaTime, ForceMode.Acceleration);
        }
        else if (Input.GetKey(extendCable))
        {
            joint.maxDistance += extendCableSpeed * Time.deltaTime;
        }
    }
    private void HandlePull() // Pull, player can pull an entity towards itself
    {
        if (Vector3.Distance(transform.position, globalHookPoint) < 2.0f) return;
        if (Input.GetKey(pull))
        {
            joint.minDistance = Mathf.Max(joint.minDistance - 0.3f * pullSpeed * Time.deltaTime, 0.1f);
            joint.maxDistance = Mathf.Max(joint.maxDistance - pullSpeed * Time.deltaTime, 2.0f);
            if (targetRb != null) targetRb.AddForce((transform.position - globalHookPoint).normalized * pullSpeed * Time.deltaTime, ForceMode.VelocityChange);
            else StartCoroutine(ReleaseHook());
        }
    }

    //----------------------------------------------------------------------------

    private bool ConfigureSwingingJoint() // For swinging, we consider the player lighter than the surface, so it's being pulled, to do this, the player needs to have a bigger scale, so it 'feels' stronger forces (as if it was a lighter object)
    {
        float distance = Vector3.Distance(transform.position, globalHookPoint);
        if (distance < 1f) return false; // Hook failed

        joint = transform.gameObject.AddComponent<SpringJoint>();

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = globalHookPoint;

        joint.minDistance = distance * 0.25f;
        joint.maxDistance = distance * 0.80f;

        joint.spring = 15.0f;
        joint.damper = 4.0f;
        joint.massScale = 1.0f;
        
        joint.enableCollision = true;

        return true;
    }
    private bool ConfigurePullingJoint() // For pulling, we consider the player heavier than the entity, so it pulls, to do this, the player needs to have a smaller scale, so it 'feels' lighter forces (as if it was a heavier object)
    {
        float distance = Vector3.Distance(transform.position, globalHookPoint);
        if (distance < 3f) return false; // Hook failed

        joint = transform.gameObject.AddComponent<SpringJoint>();

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = targetHit.gameObject.GetComponent<Rigidbody>();
        joint.minDistance = 0.5f;
        joint.maxDistance = distance;

        joint.massScale = 1.0f;
        joint.connectedMassScale = 1000.0f;
        joint.spring = 50f;
        joint.damper = 5f;

        joint.enableCollision = true;

        return true;
    }
}
