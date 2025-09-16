/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* This script:
 *      Loads enemies from profiles
 *      Creates the data structures needed to hold just the needed game objects
 *      Handles the spawn of the enemies based on events (for optimization, not spawning everything at time 0)
 *      Handles the respawn of enemies, when one dies, it checks if it can be replaced
 *      Handles difficulty
 */
public class EnemyHandler : MonoBehaviour
{
    [SerializeField] private List<string> profileNames;    // List of names of profiles to use
    [SerializeField] private string difficulty;            // Difficulty (Easy, Normal, Hard)
    [SerializeField] private List<EnemyGroup> enemyGroups; // List of groups: each enemy is part of a group, each group has a profile associated, an array to store the game objects, and an event listener and a flag to determine when to spawn enemies

    //-----------------------------------------------------------------------------------------------//

    private void Start()
    {
        difficulty = PlayerPrefs.GetString("Difficulty", "Easy"); // Default is easy
        if (profileNames != null)
        {
            enemyGroups = CreateEnemyGroups();
            CreateEnemies();
            RespawnEnemies(null);
        }
    }

    //-----------------------------------------------------------------------------------------------//

    private void CreateEnemies()
    {
        enemyGroups.ForEach(
                    group =>
                    {
                        for (int i = 0; i < group.profile.maxEnemyAtSameTime; i++)
                        {
                            group.enemyPool[i] = CreateEnemy(group);
                        }
                    }
                );
    }

    private GameObject CreateEnemy(EnemyGroup group)
    {
        GameObject enemy = Instantiate(
                                group.profile.Prefab("Prefabs/Enemies/")
                            ) as GameObject;
        enemy.SetActive(false);
        enemy.GetComponent<Enemy>().OnDeath += RespawnEnemies;
        return enemy;
    }

    //-----------------------------------------------------------------------------------------------//


    private void RespawnEnemies(MovableEntity e)
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (!group.blocked)
            {
                group.currentCount = group.enemyPool.Count(e => e.activeSelf);
                if (group.currentCount < group.profile.maxEnemyAtSameTime && group.remainingCount > 0)
                {
                    for (int i = 0; i < group.enemyPool.Length; i++)
                    {
                        if (!group.enemyPool[i].activeSelf)
                        {
                            group.enemyPool[i].transform.position = group.spawnPoints[UnityEngine.Random.Range(0, group.spawnPoints.Count)].ToVector3();
                            group.enemyPool[i].SetActive(true);
                            group.currentCount++;
                            group.remainingCount--;
                        }
                    }
                }
            }
        }
    }

    //-----------------------------------------------------------------------------------------------//

    private List<EnemyGroup> CreateEnemyGroups()
    {
        List<EnemyGroup> groups = new List<EnemyGroup>();
        EnemyGroup group;
        foreach (string profileName in profileNames)
        {
            group = CreateEnemyGroup(profileName);
            if (group != null) groups.Add(group);
        }
        return groups;
    }

    private EnemyGroup CreateEnemyGroup(string profileName)
    {
        EnemyGroup group = null;

        SpawnProfile p = LoadProfile(profileName + "(" + difficulty + ")"); // Try to load difficulty based profile (needs to be called <profile>(<difficulty>) )
        if (p != null)
        {
            group = new EnemyGroup();
            group.profile = p;
            group.enemyPool = new GameObject[group.profile.maxEnemyAtSameTime];
            group.spawnPoints = group.profile.SpawnPoints().points;
            group.currentCount = 0;
            group.remainingCount = group.profile.maxEnemyCount;
            group.blocked = group.profile.waitForEventName != null;
            BlockDispatcher.Subscribe(group.profile.waitForEventName, () => { group.blocked = false; RespawnEnemies(null); }); // Event listener, the eveng will remove the guard and proc the respawn (valid as first spawn too)
        }
        else
        {
            p = LoadProfile(profileName); // If difficulty based profile can't be found, try with the default name
            if (p != null)
            {
                group = new EnemyGroup();
                group.profile = p;
                group.enemyPool = new GameObject[group.profile.maxEnemyAtSameTime];
                group.spawnPoints = group.profile.SpawnPoints().points;
                group.currentCount = 0;
                group.remainingCount = group.profile.maxEnemyCount;
                group.blocked = group.profile.waitForEventName != null;
                BlockDispatcher.Subscribe(group.profile.waitForEventName, () => { group.blocked = false; RespawnEnemies(null); }); // Event listener, the eveng will remove the guard and proc the respawn (valid as first spawn too)
            }
        }
        return group;
    }

    private SpawnProfile LoadProfile(string profileName)
    {
        SpawnProfile prof;

        TextAsset json = Resources.Load<TextAsset>("Profiles/" + profileName);
        if (json != null)
        {
            prof = JsonUtility.FromJson<SpawnProfile>(json.text);
        }
        else
        {
            prof = null;
        }
        return prof;
    }

    //-----------------------------------------------------------------------------------------------//

    // Class that describes a group of enemies, so we can serialize and deserialize it, using the editor
    [System.Serializable]
    private class EnemyGroup
    {
        public SpawnProfile profile;         // Name of the associated profile (look below)
        public GameObject[] enemyPool;       // Array containing the gameobjects of the enemies
        public int currentCount;             // How many enemies are alive at this time
        public int remainingCount;           // How many enemies still have to be spawned
        public List<SpawnPoint> spawnPoints; // List of spawn points (deserialized from json, written in the profile)
        public bool blocked;                 // Flag that blocks the spawn, removed with the associated even, when captured
    }

    // Class that describes a profile, so we can serialize and deserialize it from json
    [System.Serializable]
    public class SpawnProfile
    {
        public string enemyPrefabName;          // Enemy to spawn
        public int maxEnemyCount;               // How many enemies to spawn in total
        public int maxEnemyAtSameTime;          // How many enemies will be present at most at each time
        public string spawnPointsFileName;      // Name of the file containing all possibile points the enemies can spawn on
        public string waitForEventName;         // Name of an event that blocks the spawn of this enemy

        public GameObject Prefab(string path) => Resources.Load<GameObject>(path + enemyPrefabName);
        public SpawnPointList SpawnPoints()
        {
            TextAsset json = Resources.Load<TextAsset>("Profiles/Spawns/" + spawnPointsFileName);
            SpawnPointList l = JsonUtility.FromJson<SpawnPointList>(json.text);
            return l;
        }
    }
}