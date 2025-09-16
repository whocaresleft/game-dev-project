/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using UnityEngine;

public class ThrowFire : Combat
{
    [SerializeField, ReadOnly] private float maxtime;       // Lifetime of the fireball
    [SerializeField, ReadOnly] private float travelspeed;   // Travel speed of the fireball
    [SerializeField] private float impactDamage;            // Damage the fireball deals on first contact
    [SerializeField] private float dotDamageDPS;            // Damage dealt each second after impact
    [SerializeField] private float dotDuration;             // Damage over time duration, how many ticks
    [SerializeField, ReadOnly] private float tickRate;      // How many seconds a tick is
    private Enemy e;                                        // Enemy the script is attached to, need to use its looking direction
    private GameObject fireballPrefab;                      // Prefab of the fireball
    private GameObject fireball;                            // Reference of the thrown fireball

    private void Start()
    {
        e = gameObject.GetComponent<Enemy>();
        fireballPrefab = Resources.Load<GameObject>("Prefabs/Utils/Purplefire");
        fireball = null;
    }

    public override void Attack()
    {
        fireball = Instantiate(fireballPrefab, transform.position + transform.forward * 3f, Quaternion.identity) as GameObject;
        fireball.GetComponent<PurpleFire>().Initialize(maxtime, travelspeed, e.checkDirection.normalized);
    }

    public override bool AttackCondition()
    {
        return fireball == null;
    }

    private void OnEnable()
    {
        PurpleFire.OnPlayerHit += ApplyDamage;
        if (fireball) Destroy(fireball); // If an enemy is being disabled and reenabled while a fireball exists, delete it
    }
    private void OnDisable()
    {
        PurpleFire.OnPlayerHit -= ApplyDamage;
    }

    private void ApplyDamage(Player p)
    {
        // Apply damage once and then overtime
        p.TakeDamage(impactDamage);
        StartCoroutine(DOT(p, dotDamageDPS, dotDuration));
    }

    private IEnumerator DOT(Player p, float damagePerSecond, float duration)
    {
        yield return null;
        float timer = duration;

        // Damage per tick = DPS * tickRate
        // Tick count = duration / tickRate
        int tickCount = Mathf.CeilToInt(duration / tickRate);
        float damagePerTick = damagePerSecond * tickRate;

        for (int i = 0; i < tickCount; i++)
        {
            p.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(tickRate);
        }
    }
}
