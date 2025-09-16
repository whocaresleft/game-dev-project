/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using UnityEngine;

// Script that handles the visual part of the player's railgun weapon
public class RailgunVisualHandler : MonoBehaviour
{
    private Transform cam;      // Used to determine where to render the ray
    private GameObject railgun; // Actual model of the railgun
    private RailGun rg;         // Player script for the railgun
    private GameObject railgunPrefab;   // Prefab for the railgun, is created if not already present
    private LineRenderer beam;  // Beam rendered when shooting
    [SerializeField] private Material beamMaterial; // Material for the beam, used to color the beam and the glass when the weapon is charged
    [SerializeField] private Material glassBaseMaterial; // Default material for the glass, used when the weapon is charging
    [SerializeField, ReadOnly] private float beamDuration;  // How many seconds the ray is rendered before being deleted
    [SerializeField] private AudioSource shotAudio;

    private void Start()
    {
        cam = transform.GetChild(1);
        rg = GetComponent<RailGun>();
        railgunPrefab = Resources.Load<GameObject>("Prefabs/Utils/Railgun");
        AttachGun(cam);
        shotAudio = transform.GetChild(1).GetChild(2).GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        if(railgun) railgun.SetActive(true);
    }
    private void OnDisable()
    {
        if (railgun) railgun.SetActive(false);
    }

    private void Update()
    {
        railgun.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = (rg.PressTime > rg.ShotChargeTime) ? beamMaterial : glassBaseMaterial;
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (railgun.activeSelf)
            {
                railgun.SetActive(false);
                rg.enabled = false;
            }
            else
            {
                railgun.SetActive(true);
                rg.enabled = true;
            }
        }
    }

    public void Fire(Vector3 target)
    {
        StartCoroutine(ShowBeam(target, beamDuration));
    }
    private void AttachGun(Transform ancestor)
    {
        // Create objects
        railgun = GameObject.Instantiate(railgunPrefab) as GameObject;
        railgun.layer = LayerMask.NameToLayer("On Top");
        railgun.transform.parent = ancestor;

        railgun.transform.localPosition = new Vector3(0.904999971f, -0.43599999f, 0.953999996f);
        railgun.transform.localRotation = new Quaternion(0.006776751f, -0.9802831f, 0.1411249f, -0.138141f);
        

        beam = railgun.GetComponent<LineRenderer>();
        if (beam == null)
        {
            beam = railgun.AddComponent<LineRenderer>();
        }

        beam.enabled = false;
        beam.startWidth = 0.1f;
        beam.endWidth = 0.1f;
        beam.material = beamMaterial;
    }

    private IEnumerator ShowBeam(Vector3 target, float duration)
    {
        yield return null;
        beam.positionCount = 2;
        beam.SetPosition(0, railgun.transform.position);
        beam.SetPosition(1, target);

        shotAudio.Play();

        beam.enabled = true;
        yield return new WaitForSeconds(duration);
        beam.enabled = false;
        beam.positionCount = 0;
    }
}
