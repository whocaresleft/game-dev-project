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
using System.Collections.Generic;

// This script is ONLY used for the editor, this adds at 'runtime' (only editor's game's simulation) the needed layers for the execution, later, for a real build, the layers need to exist already
[DefaultExecutionOrder(-100)]
public class LayerChecker : MonoBehaviour
{
    [SerializeField] private List<string> neededLayers; // List of the layers to add

    void Start()
    {
        foreach(string l in neededLayers)
            if (LayerMask.NameToLayer(l) == -1)
                CreateLayer(l);
    }
    private void CreateLayer(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        // Reference to the asset manager and layer property
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        // Check if the layer 'name' already exists, we know that the layers 0 to 2 and 4-5 are reserved, so we can skip those
        SerializedProperty sp = layersProp.GetArrayElementAtIndex(3);
        if (sp != null && sp.stringValue == name) return; // Already existed
        for (int i = 6; i < layersProp.arraySize; i++)
        {
            sp = layersProp.GetArrayElementAtIndex(i);
            if (sp != null && sp.stringValue == name) return; // Already existed
        }

        // We know that it does not exist, so we find an empty slot to store it in
        sp = layersProp.GetArrayElementAtIndex(3);
        if (sp != null && string.IsNullOrEmpty(sp.stringValue))
        {
            sp.stringValue = name;
            tagManager.ApplyModifiedProperties();
            return;
        }

        for (int i = 6; i < layersProp.arraySize; i++)
        {
            sp = layersProp.GetArrayElementAtIndex(i);
            if (sp != null && string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = name;
                tagManager.ApplyModifiedProperties();
                break;
            }
        }
    }
}
#endif