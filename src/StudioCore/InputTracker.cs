// The MIT License(MIT)

// Copyright(c) 2017 Eric Mellino and Veldrid contributors

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore;

public static class InputTracker
{
    private static readonly HashSet<Key> _currentlyPressedKeys = new();
    private static readonly HashSet<Key> _newKeysThisFrame = new();

    private static readonly HashSet<MouseButton> _currentlyPressedMouseButtons = new();
    private static readonly HashSet<MouseButton> _newMouseButtonsThisFrame = new();

    public static Vector2 MousePosition;
    public static Vector2 MouseDelta;

    public static float MouseScrollWheelDelta;

    public static InputSnapshot FrameSnapshot { get; private set; }

    public static bool GetKey(Key key)
    {
        return _currentlyPressedKeys.Contains(key);
    }

    public static bool GetKeyDown(Key key)
    {
        return _newKeysThisFrame.Contains(key);
    }

    public static bool GetKey_IgnoreModifier(KeyBind key)
    {
        if (!GetKey(key.PrimaryKey))
        {
            return false;
        }

        return true;
    }

    public static bool GetKey(KeyBind key)
    {
        if (!GetKey(key.PrimaryKey))
        {
            return false;
        }

        if (key.Ctrl_Pressed != (GetKey(Key.LControl) || GetKey(Key.RControl)))
        {
            return false;
        }

        if (key.Alt_Pressed != (GetKey(Key.AltLeft) || GetKey(Key.AltRight)))
        {
            return false;
        }

        if (key.Shift_Pressed != (GetKey(Key.ShiftLeft) || GetKey(Key.ShiftRight)))
        {
            return false;
        }

        return true;
    }

    public static bool GetKeyDown_IgnoreModifier(KeyBind key)
    {
        if (!GetKeyDown(key.PrimaryKey))
        {
            return false;
        }

        return true;
    }

    public static bool GetKeyDown(KeyBind key)
    {
        if (!GetKeyDown(key.PrimaryKey))
        {
            return false;
        }

        if (key.Ctrl_Pressed != (GetKey(Key.LControl) || GetKey(Key.RControl)))
        {
            return false;
        }

        if (key.Alt_Pressed != (GetKey(Key.AltLeft) || GetKey(Key.AltRight)))
        {
            return false;
        }

        if (key.Shift_Pressed != (GetKey(Key.ShiftLeft) || GetKey(Key.ShiftRight)))
        {
            return false;
        }

        return true;
    }

    public static bool GetControlShortcut(Key key)
    {
        return (GetKey(Key.LControl) || GetKey(Key.RControl)) && GetKeyDown(key);
    }

    public static bool GetAltShortcut(Key key)
    {
        return (GetKey(Key.AltLeft) || GetKey(Key.AltRight)) && GetKeyDown(key);
    }

    public static bool GetShiftShortcut(Key key)
    {
        return (GetKey(Key.ShiftLeft) || GetKey(Key.ShiftRight)) && GetKeyDown(key);
    }

    public static bool GetMouseButton(MouseButton button)
    {
        return _currentlyPressedMouseButtons.Contains(button);
    }

    public static bool GetMouseButtonDown(MouseButton button)
    {
        return _newMouseButtonsThisFrame.Contains(button);
    }

    public static float GetMouseWheelDelta()
    {
        return MouseScrollWheelDelta;
    }

    public static void UpdateFrameInput(InputSnapshot snapshot, Sdl2Window window)
    {
        FrameSnapshot = snapshot;
        _newKeysThisFrame.Clear();
        _newMouseButtonsThisFrame.Clear();

        MousePosition = snapshot.MousePosition;
        MouseDelta = window.MouseDelta;
        MouseScrollWheelDelta = snapshot.WheelDelta;
        for (var i = 0; i < snapshot.KeyEvents.Count; i++)
        {
            KeyEvent ke = snapshot.KeyEvents[i];
            if (ke.Down && ke.Key != Key.Unknown)
            {
                KeyDown(ke.Key);
            }
            else
            {
                KeyUp(ke.Key);
            }
        }

        for (var i = 0; i < snapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = snapshot.MouseEvents[i];
            if (me.Down)
            {
                MouseDown(me.MouseButton);
            }
            else
            {
                MouseUp(me.MouseButton);
            }
        }
    }

    private static void MouseUp(MouseButton mouseButton)
    {
        _currentlyPressedMouseButtons.Remove(mouseButton);
        _newMouseButtonsThisFrame.Remove(mouseButton);
    }

    private static void MouseDown(MouseButton mouseButton)
    {
        if (_currentlyPressedMouseButtons.Add(mouseButton))
        {
            _newMouseButtonsThisFrame.Add(mouseButton);
        }
    }

    private static void KeyUp(Key key)
    {
        _currentlyPressedKeys.Remove(key);
        _newKeysThisFrame.Remove(key);
    }

    private static void KeyDown(Key key)
    {
        if (_currentlyPressedKeys.Add(key))
        {
            _newKeysThisFrame.Add(key);
        }
    }

    public static KeyBind GetNewKeyBind()
    {
        Key newkey = GetNextKey();
        _newKeysThisFrame.Clear(); // Clear to prevent hotkeys from triggering

        if (newkey != Key.Unknown)
        {
            var ctrl = GetKey(Key.LControl) || GetKey(Key.RControl);
            var alt = GetKey(Key.AltLeft) || GetKey(Key.AltRight);
            var shift = GetKey(Key.ShiftLeft) || GetKey(Key.ShiftRight);
            return new KeyBind(newkey, ctrl, alt, shift);
        }

        return null;
    }

    public static Key GetNextKey()
    {
        return _newKeysThisFrame.FirstOrDefault(e =>
            e != Key.LControl && e != Key.RControl && e != Key.LAlt && e != Key.RAlt && e != Key.LShift &&
            e != Key.RShift);
    }
}
