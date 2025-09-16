/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System.Collections;
using UnityEngine;

// Class that plays music tracks based on events
public class Music : MonoBehaviour
{
    [SerializeField] private AudioSource audioPlayer;

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip slugTrack;
    [SerializeField] private AudioClip ghostTrack;
    [SerializeField] private AudioClip bigbossTrack;

    void Start()
    {
        audioPlayer = GetComponent<AudioSource>();

        PlayBackgroundMusic();
        BlockDispatcher.Subscribe("larvae", () => StartCoroutine(SlugBossMusic()));
        BlockDispatcher.Subscribe("cannon-2", () => PlayBackgroundMusic());
        BlockDispatcher.Subscribe("ghost-free", () => StartCoroutine(GhostBossMusic()));
        BlockDispatcher.Subscribe("cannon-3", () => PlayBackgroundMusic());
        BlockDispatcher.Subscribe("grav-now", () => StartCoroutine(FinalBossMusic()));
        BlockDispatcher.Subscribe("game-won", () => audioPlayer.Stop());
    }

    private void PlayBackgroundMusic()
    {
        audioPlayer.clip = backgroundMusic;
        audioPlayer.Play();
    }

    private IEnumerator SlugBossMusic()
    {
        yield return new WaitForSeconds(10);
        audioPlayer.clip = slugTrack;
        audioPlayer.Play();
    }

    private IEnumerator GhostBossMusic()
    {
        yield return new WaitForSeconds(14);
        audioPlayer.clip = ghostTrack;
        audioPlayer.Play();
    }

    private IEnumerator FinalBossMusic()
    {
        yield return new WaitForSeconds(11);
        audioPlayer.clip = bigbossTrack;
        audioPlayer.Play();
    }
}
