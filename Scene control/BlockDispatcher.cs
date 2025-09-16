/*
 * Copyright (c) 2025 Biribo' Francesco - Palumbo Dario
 *
 * Permission to use, copy, modify, and distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;
using System.Collections.Generic;

// Class that handles the custom event system, based on string literal names, so it can be used for profiles
public static class BlockDispatcher
{
    private static Dictionary<string, Action> events = new Dictionary<string, Action>(); // Each string literal has a function associated
    public static void Subscribe(string eventName, Action callback) // An object can add it's function to be called when the associated event is triggered
    {
        if (!events.ContainsKey(eventName))
        {
            events[eventName] = () => { };
        }
        events[eventName] += callback;
    }

    public static void Unsubscribe(string eventName, Action callback) // An object can remove it's function from the event call
    {
        if (events.ContainsKey(eventName))
        {
            events[eventName] -= callback;
        }
    }

    public static void Trigger(string eventName) // Triggers a particular event, from name, executing the actions associated with such event
    {
        if (events.TryGetValue(eventName, out Action action))
        {
            action?.Invoke();
        }
    }

    public static void ClearAll()
    {
        events.Clear();
    }
}