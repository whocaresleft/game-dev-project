/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/* This script only works in the editor, enabling the user editor to manage the positions the enemy can spawn in directly using the editor
 * Giving the options to save the created the points onto a file, provided a name, or to load points from a file, to edit them perhaps
 */
public class SpawnPointManager : MonoBehaviour
{
    [SerializeField] private GameObject spawnPoints;    // Gameobject that holds all the spawn points as empty children
    [SerializeField] private string fileName;           // Name of the file that contains, or will contain, the spawn points
    [SerializeField] private bool drawGizmos;           // Flag used to draw a sphere corrisponding on each point in the editor (better visibility to manage them)

    private void Start()
    {
        if (spawnPoints && spawnPoints.transform.childCount > 0)
            DeletePlaceholders();
    }

    [ContextMenu("Save points to JSON")] // We add the option directly in the editor, this button calls this function
    private void SaveSpawnPoints()
    {
        SpawnPointList collection = new SpawnPointList();
        for(int i = 0; i < spawnPoints.transform.childCount; i++)
        {
            collection.points.Add(new SpawnPoint(spawnPoints.transform.GetChild(i).position));
        }

        string json = JsonUtility.ToJson(collection, true);
        string dirPath = Path.Combine(Application.dataPath, "Resources/Profiles/Spawns");
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        string path = Path.Combine(dirPath, fileName + ".json");
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    [ContextMenu("Load points from JSON")] // We add the option directly in the editor, this button calls this function
    private void LoadSpawnPoints()
    {
        string dirPath = Path.Combine(Application.dataPath, "Resources/Profiles/Spawns");
        string path = Path.Combine(dirPath, fileName + ".json");
        string json = File.ReadAllText(path);
        SpawnPointList collection = JsonUtility.FromJson<SpawnPointList>(json);

        spawnPoints = new GameObject("placeholders father");
        for (int i = 0; i < collection.points.Count; i++)
        {
            GameObject c = new GameObject("placehoilder" + i);
            c.transform.position = collection.points[i].ToVector3();
            c.transform.parent = spawnPoints.transform;
        }
    }

    [ContextMenu("Clear objects")] // We add the option directly in the editor, this button calls this function
    private void DeletePlaceholders()
    {
        drawGizmos = false;
        for (int i = 0; i < spawnPoints.transform.childCount; i++)
            if (spawnPoints.transform.GetChild(i) != null)
                DestroyImmediate(spawnPoints.transform.GetChild(i).gameObject);
        DestroyImmediate(spawnPoints);
    }

    private void OnDrawGizmos()
    {
        if(spawnPoints != null)
            for (int i = 0; i < spawnPoints.transform.childCount; i++)
            {
                if (drawGizmos)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(spawnPoints.transform.GetChild(i).position, 0.5f);
                }
            }
    }
}
#endif