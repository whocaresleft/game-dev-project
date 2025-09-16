/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;
using TMPro;
using System.Collections;

// Prints particular hints based on certain events
public class Hint : MonoBehaviour
{
    private TextMeshProUGUI hintText;
    void Start()
    {
        hintText = GetComponent<TextMeshProUGUI>();

        hintText.text = "Bring the fuel tank to the rocket";
        BlockDispatcher.Subscribe("clear", () => Clear());
        BlockDispatcher.Subscribe("planet2-arrival", () => StartCoroutine(HandlePlanet2()));
        BlockDispatcher.Subscribe("larvae", () => StartCoroutine(HandlePlanet3()));
        BlockDispatcher.Subscribe("cannon-2", () => StartCoroutine(HandleCannon2()));
        BlockDispatcher.Subscribe("ghost-free", () => StartCoroutine(HandlePlanetCastle()));
        BlockDispatcher.Subscribe("cannon-3", () => StartCoroutine(HandleCannon3()));
        BlockDispatcher.Subscribe("grav-now", () => StartCoroutine(HandlePlanetLast()));        
        BlockDispatcher.Subscribe("game-won", () => Clear());
    }

    private void Clear()
    {
        hintText.text = "";
    }
    

    private IEnumerator HandlePlanet2() {
        hintText.text = ""; 
        yield return new WaitForSeconds(4);
        hintText.text = "Look for the cannon on the other planet"; 
    }

    private IEnumerator HandlePlanet3()
    {
        hintText.text = "";
        yield return new WaitForSeconds(10);
        hintText.text = "Kill the black slugs";
    }

    private IEnumerator HandleCannon2()
    {
        hintText.text = "";
        yield return new WaitForSeconds(2);
        hintText.text = "Escape this planet! Look for the cannon";
    }

    private IEnumerator HandlePlanetCastle()
    {
        hintText.text = "";
        yield return new WaitForSeconds(14);
        hintText.text = "Kill the ghosts";
    }

    private IEnumerator HandleCannon3()
    {
        hintText.text = "";
        yield return new WaitForSeconds(2);
        hintText.text = "Find the cannon and escape from this planet!";
    }

    private IEnumerator HandlePlanetLast()
    {
        hintText.text = "";
        yield return new WaitForSeconds(11);
        hintText.text = "Find and defeat the last enemy";
    }
}
