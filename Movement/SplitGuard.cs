/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections.Generic;
using UnityEngine;

/* This is one, and the only for now, guard that prevents the enemy's death, actually splitting it,
 * The strategy is: the enemies are not created and destroyed dinamically, because an array is created, with as many game objects as that type of player can have simultaneously
 * 
 * So when an enemy with this guard dies, it can't proc the respawn, but it has to 'stay alive' but in stasis, divide itself, and wait for all the
 * children to die, to actually die. This can go on for as many levels as specified below (remainingSplits)
 */
public class SplitGuard : MonoBehaviour
{
    [SerializeField] private GameObject prefab;  // Prefab of the enemy holding the script (need to create clones at runtime)
    private MovableEntity entity;                // The enemy holding the script is also a movable entity, we listen for it's death to intercept it
    [SerializeField] int remainingSplits;        // How many levels of clones will be generated
    [SerializeField] public int childrenOnSplit; // How many clones created after this death
    private SplitGuard father;                   // Reference to the enemy that was used to create this clone, if we are in a clone
    private List<SplitGuard> children;           // List of clones (both father and children are used for terms since we can see the whole structure as a tree data structure)

    private void Awake()
    {
        entity = GetComponent<MovableEntity>();
        entity.OnBeforeDeath += Split;
    } 

    public void AddGuard()
    {
        entity.OnBeforeDeath += Split;
    }

    private bool Split()
    {
        DisableEverything();

        if (remainingSplits <= 0) { EnableEverything();  return true; } // Can actually die

        GameObject child;
        SplitGuard sg;

        children = new List<SplitGuard>();
        for (int i = 0; i < childrenOnSplit; i++)
        {
            child = Instantiate(prefab, transform.position + Random.insideUnitSphere, transform.rotation);
            child.transform.localScale = transform.localScale * 0.5f;
            child.GetComponent<Enemy>().maxHealth = GetComponent<Enemy>().maxHealth * 0.8f;

            sg = child.GetComponent<SplitGuard>();
            sg.remainingSplits = remainingSplits - 1;

            sg.father = this;
            sg.EnableEverything();
            children.Add(sg);

            MovableEntity childE = child.GetComponent<MovableEntity>();
            childE.OnDeath += OnChildDeath;
            childE.groundAcceleration = entity.groundAcceleration * 1.2f;
        }

        return false;
    }

    // A children has died, update the counter, if all are dead, we die too
    private void OnChildDeath(MovableEntity child)
    {

        children.Remove(child.GetComponent<SplitGuard>());
        Destroy(child.gameObject);

        if (children.Count == 0)
        {
            if (father == null)
            {
                // Actually die
                EnableEverything();
                entity.Die();
            }
            else
            {
                // We are a clone, notify father
                father.OnChildDeath(GetComponent<MovableEntity>());
            }
        }
    }

    // Disables ALL scripts but this, so we can listen for clones death
    private void DisableEverything()
    {
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this) script.enabled = false;
        }

        Collider c = GetComponent<Collider>(); if(c) c.enabled = false;

        Rigidbody r = GetComponent<Rigidbody>(); if(r) r.isKinematic = true;
        
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Enables ALL enemy scripts
    private void EnableEverything()
    {
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this) script.enabled = true;
        }

        Collider c = GetComponent<Collider>(); if (c) c.enabled = true;

        Rigidbody r = GetComponent<Rigidbody>(); if (r) r.isKinematic = false;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
