/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

// This script handles the visual part of the grappling hook
public class GrapplingHandler : MonoBehaviour
{
    private Transform cam;      // Used to determine where to render the rope
    private GameObject grapplingHook; // Actual model of the grappling hook
    private GrapplingHook gh;   // Player script for the grappling hook
    private GameObject hookPrefab; // Prefab for the grappling hook, is created if not already present
    [SerializeField] private Transform hookTip;   // Where to actually start rendering the rope
    private LineRenderer rope;   // Renders the rope
    [SerializeField] private Material ropeMaterial; // Material used to color the rope

    private void Start()
    {
        cam = transform.GetChild(1);
        gh = GetComponent<GrapplingHook>();
        hookPrefab = Resources.Load<GameObject>("Prefabs/Utils/GrapplingHook");
        AttachHook(cam);
    }

    private void OnEnable()
    {
        gh = GetComponent<GrapplingHook>();
        EnableHook();
    }

    private void OnDisable()
    {
        DisableHook();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (grapplingHook.activeSelf)
            {
                DisableHook();
            }
            else
            {
                EnableHook();
            }
        }
    }

    private void EnableHook()
    {
        if (rope)
        {
            rope.positionCount = 0;
            rope.enabled = false;
        }
        if (grapplingHook) grapplingHook.SetActive(true);
        gh.enabled = true;
    }

    private void DisableHook()
    {
        if (rope)
        {
            rope.positionCount = 0;
            rope.enabled = false;
        }
        if (grapplingHook) grapplingHook.SetActive(false);
        gh.enabled = false;
    }

    public void DrawHook(Vector3 target)
    {
        rope.positionCount = 2;
        rope.SetPosition(0, hookTip.position);
        rope.SetPosition(1, target);
        rope.enabled = true;

         HideTip();
    }

    public void EraseHook()
    {
        rope.enabled = false;
        rope.positionCount = 0;
    }

    public void ShowTip()
    {
        hookTip.gameObject.SetActive(true);
    }

    public void HideTip()
    {
        hookTip.gameObject.SetActive(false);
    }

    private void AttachHook(Transform ancestor)
    {
        // Create objects
        grapplingHook = GameObject.Instantiate(hookPrefab) as GameObject;
        grapplingHook.layer = LayerMask.NameToLayer("On Top");
        grapplingHook.transform.parent = ancestor;

        grapplingHook.transform.localPosition = new Vector3(-0.813491285f, -0.352355957f, 0.584091306f);
        grapplingHook.transform.localRotation = new Quaternion(-0.0459629335f, 0.0933209881f, -0.000983806094f, 0.99457413f);


        rope = grapplingHook.GetComponent<LineRenderer>();
        if (rope == null)
        {
            rope = grapplingHook.AddComponent<LineRenderer>();
        }

        rope.enabled = false;
        rope.startWidth = 0.1f;
        rope.endWidth = 0.1f;
        rope.material = ropeMaterial;

        hookTip = grapplingHook.transform.GetChild(0);
    }
}
