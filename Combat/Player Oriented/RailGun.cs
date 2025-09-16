/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

public class RailGun : Combat
{
    [SerializeField, ReadOnly] private float range;     // Weapon range
    [SerializeField, ReadOnly] private float damage;    // Weapon damage
    private Transform shotOrigin;   // Where is this shot coming from?
    private Transform cam;          // Where is it going? (Used to determine direction)
    [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;     // We shoot with Left click
    private RailgunVisualHandler handler;   // Script that handles the visual part of the weapon
    [SerializeField, ReadOnly] public float ShotChargeTime { get; private set; }    // How much do we have to wait after shooting to shoot again?
    public float PressTime { get; private set; }    // How much time has passed since i last shot (pressed)?

    private void Start()
    {
        range = 100f;
        damage = 30f;
        cam = transform.GetChild(1);
        shotOrigin = cam;
        handler = GetComponent<RailgunVisualHandler>();
        ShotChargeTime = 4f;
        PressTime = 0f;
    }

    private new void Update()
    {
        base.Update();
        PressTime += Time.deltaTime;
    }

    public override void Attack()
    {
        PressTime = 0f;
        if (Physics.Raycast(shotOrigin.position, cam.forward, out RaycastHit info, range, LayerMask.GetMask("Entity")))
        {
            if (handler) handler.Fire(info.point);

            // Is this attackable?
            MovableEntity me = info.transform.gameObject.GetComponent<MovableEntity>();
            if (me != null)
            {
                me.TakeDamage(damage);
            }
        }
        else
        {
            if (handler) handler.Fire(shotOrigin.position + cam.forward * range);
        }
    }

    public override bool AttackCondition()
    {
        return PressTime >= ShotChargeTime && Input.GetKeyDown(shootKey);
    }
}
