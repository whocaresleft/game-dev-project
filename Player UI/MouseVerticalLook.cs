/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using UnityEngine;

public class MouseVerticalLook : MonoBehaviour {
    

    [SerializeField] public float verticalSensitivity;
    [SerializeField] private float minimumVert; 
    [SerializeField] private float maximumVert;
    private float verticalRot = 0f; // vertical rotation angle
    private short inverted = -1;

    private void Update()
    { 
        verticalRot += inverted * Input.GetAxis("Mouse Y") * verticalSensitivity;
        // Clamp the vertical angle between minimum and maximum limits
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);
        // Create a new vector from the stored rotation values.
        this.transform.localEulerAngles = new Vector3(verticalRot, 0, 0);
    }

    public void InvertYAxis()
    {
        inverted *= -1;
    }
}
