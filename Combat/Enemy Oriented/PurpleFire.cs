/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

public class PurpleFire : MonoBehaviour 
{
    private float lifetime;     // How much the fireball exists for
    private float speed;        // How fast the fireball travels
    private Vector3 direction;  // The direction the fireball travers to
    public static event System.Action<Player> OnPlayerHit;  // Event that flags the player as hit

    public void Initialize(float lifetime, float speed, Vector3 direction)
    {
        this.lifetime = lifetime;
        this.speed = speed;
        this.direction = direction;
        Destroy(gameObject, this.lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponent<Player>();
        if (p != null)
        {
            // A player was hit
            OnPlayerHit?.Invoke(p);
            Destroy(gameObject);
        }
    }
}
