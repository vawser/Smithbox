using SoulsFormats.Util;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Keybinds.InputManager;

namespace StudioCore.Keybinds;


public static class InputManager
{
    public class KeyBinding
    {
        public Key Key;
        public bool Ctrl;
        public bool Shift;
        public bool Alt;

        public KeyBinding Clone()
        {
            return new KeyBinding
            {
                Key = Key,
                Ctrl = Ctrl,
                Shift = Shift,
                Alt = Alt
            };
        }
    }

    public static readonly KeybindStore _bindings = new();
    public static readonly KeybindStore _defaultBindings = new();

    private static string KeybindPath = "";

    public static Vector2 MousePosition => _mouseCurrent.Position;
    public static Vector2 MouseDelta => _mouseCurrent.Delta;
    public static float MouseWheelDelta => _mouseCurrent.WheelDelta;

    private static MouseSnapshot _mouseCurrent = new();
    private static MouseSnapshot _mousePrevious = new();

    private static KeyboardSnapshot _current = new();
    private static KeyboardSnapshot _previous = new();

    private static readonly Dictionary<Key, float> _keyHoldTime = new();

    private static double _delta;

    public static InputSnapshot InputSnapshot;
    public static KeyboardSnapshot Current => _current;

    public static void Init()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        KeybindPath = Path.Combine(folder, "Keybinds.json");

        DefaultKeyBindings.CreateDefaultBindings();

        if (File.Exists(KeybindPath))
        {
            Load(KeybindPath);
        }

        Save();
    }

    public static void Update(Sdl2Window window, InputSnapshot snapshot, double delta)
    {
        InputSnapshot = snapshot;
        _delta = delta;

        // ---- Keyboard ----
        _previous = _current;
        _current = _current.Clone();

        // Apply key state changes
        foreach (var keyEvent in snapshot.KeyEvents)
        {
            if (keyEvent.Down)
                _current.SetKeyDown(keyEvent.Key);
            else
                _current.SetKeyUp(keyEvent.Key);
        }

        // Update hold times
        foreach (var key in _current.GetDownKeys())
        {
            if (!_keyHoldTime.ContainsKey(key))
                _keyHoldTime[key] = 0f;

            _keyHoldTime[key] += (float)delta;
        }

        // Cleanup released keys
        foreach (var key in _keyHoldTime.Keys.ToList())
        {
            if (!_current.IsKeyDown(key))
                _keyHoldTime.Remove(key);
        }

        // ---- Mouse ----
        _mousePrevious = _mouseCurrent;
        _mouseCurrent = _mouseCurrent.Clone();

        Vector2 newPos = snapshot.MousePosition;
        _mouseCurrent.Delta = newPos - _mousePrevious.Position;
        _mouseCurrent.Position = newPos;

        _mouseCurrent.WheelDelta = snapshot.WheelDelta;

        foreach (var me in snapshot.MouseEvents)
        {
            if (me.Down)
                _mouseCurrent.SetDown(me.MouseButton);
            else
                _mouseCurrent.SetUp(me.MouseButton);
        }
    }

    // ---------------- Actions ----------------

    public static bool IsDown(KeybindID action)
    {
        if (_bindings.Entries.ContainsKey(action))
        {
            return _bindings.Entries[action].Any(IsBindingDown);
        }

        return false;
    }

    public static bool IsPressed(KeybindID action)
    {
        if (_bindings.Entries.ContainsKey(action))
        {
            return _bindings.Entries[action].Any(b =>
                 IsBindingDown(b) &&
                 !_previous.IsKeyDown(b.Key));
        }

        return false;
    }

    public static bool IsPressedOrRepeated(
    KeybindID action,
    float initialDelay = 0.35f,
    float repeatRate = 0.075f)
    {
        if (!_bindings.Entries.TryGetValue(action, out var bindings))
            return false;

        foreach (var b in bindings)
        {
            if (!IsBindingDown(b))
                continue;

            // Initial press
            if (!_previous.IsKeyDown(b.Key))
                return true;

            // Held repeat
            if (_keyHoldTime.TryGetValue(b.Key, out float t))
            {
                if (t > initialDelay)
                {
                    float repeatTime = t - initialDelay;
                    return (repeatTime % repeatRate) < _delta;
                }
            }
        }

        return false;
    }

    public static bool IsReleased(KeybindID action)
    {
        if (_bindings.Entries.ContainsKey(action))
        {
            return _bindings.Entries[action].Any(b =>
            !_current.IsKeyDown(b.Key) &&
            _previous.IsKeyDown(b.Key));
        }

        return false;
    }

    public static bool IsKeyDown(Key key)
    => _current.IsKeyDown(key);

    public static bool IsKeyPressed(Key key)
        => _current.IsKeyDown(key) && !_previous.IsKeyDown(key);

    public static bool IsKeyReleased(Key key)
        => !_current.IsKeyDown(key) && _previous.IsKeyDown(key);

    public static bool IsMouseDown(MouseButton button)
    => _mouseCurrent.IsDown(button);

    public static bool IsMousePressed(MouseButton button)
        => _mouseCurrent.IsDown(button) && !_mousePrevious.IsDown(button);

    public static bool IsMouseReleased(MouseButton button)
        => !_mouseCurrent.IsDown(button) && _mousePrevious.IsDown(button);

    // ---------------- Bindings ----------------

    public static void Bind(KeybindID action, KeyBinding binding)
    {
        if (!_bindings.Entries.TryGetValue(action, out var list))
            _bindings.Entries[action] = list = new();

        list.Add(binding);

        if (!_defaultBindings.Entries.TryGetValue(action, out var defaultList))
            _defaultBindings.Entries[action] = defaultList = new();

        _defaultBindings.Entries[action].Add(binding.Clone());
    }

    public static IReadOnlyDictionary<KeybindID, List<KeyBinding>> Bindings => _bindings.Entries;

    // ---------------- Serialization ----------------

    public static void Save()
    {
        var filestring = JsonSerializer.Serialize(_bindings, ProjectJsonSerializerContext.Default.KeybindStore);

        var dir = Path.GetDirectoryName(KeybindPath);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(KeybindPath, filestring);
    }

    public static void Load(string path)
    {
        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize(json, ProjectJsonSerializerContext.Default.KeybindStore);

        if (data != null)
        {
            foreach (var kv in data.Entries)
            {
                _bindings.Entries[kv.Key] = kv.Value;
            }
        }
    }

    // ---------------- Helpers ----------------

    public static bool IsPressed_IgnoreModifiers(KeybindID action)
    {
        if (_bindings.Entries.ContainsKey(action))
        {
            return _bindings.Entries[action].Any(
                b => IsBindingDown_IgnoreModifiers(b) &&
                 !_previous.IsKeyDown(b.Key));
        }

        return false;
    }

    private static bool IsBindingDown(KeyBinding b)
    {
        if (!_current.IsKeyDown(b.Key))
            return false;

        if (b.Ctrl && !CtrlDown())
            return false;

        if (b.Shift && !ShiftDown())
            return false;

        if (b.Alt && !AltDown())
            return false;


        return true;
    }

    private static bool IsBindingDown_IgnoreModifiers(KeyBinding b)
    {
        if (!_current.IsKeyDown(b.Key))
            return false;

        return true;
    }

    private static bool CtrlDown()
        => _current.IsKeyDown(Key.LControl) || _current.IsKeyDown(Key.RControl);

    private static bool ShiftDown()
        => _current.IsKeyDown(Key.LShift) || _current.IsKeyDown(Key.RShift);

    private static bool AltDown()
        => _current.IsKeyDown(Key.LAlt) || _current.IsKeyDown(Key.RAlt);


    public static string GetHint(KeybindID action)
    {
        var curBind = _bindings.Entries[action].FirstOrDefault();

        string s = "";
        if (curBind.Ctrl) s += "Ctrl+";
        if (curBind.Shift) s += "Shift+";
        if (curBind.Alt) s += "Alt+";
        s += curBind.Key.ToString();
        return s;
    }

    public static bool HasArrowSelection()
    {
        if (IsPressedOrRepeated(KeybindID.Up) ||
            IsPressedOrRepeated(KeybindID.Down))
        {
            return true;
        }

        return false;
    }

    public static bool HasCtrlDown()
    {
        if (CtrlDown())
        {
            return true;
        }

        return false;
    }

    public static bool HasShiftDown()
    {
        if (ShiftDown())
        {
            return true;
        }

        return false;
    }

    public static bool HasAltDown()
    {
        if (AltDown())
        {
            return true;
        }

        return false;
    }
}

public sealed class MouseSnapshot
{
    private readonly HashSet<MouseButton> _buttons = new();

    public Vector2 Position;
    public Vector2 Delta;
    public float WheelDelta;

    public bool IsDown(MouseButton button) => _buttons.Contains(button);

    public void SetDown(MouseButton button) => _buttons.Add(button);
    public void SetUp(MouseButton button) => _buttons.Remove(button);

    public IEnumerable<MouseButton> GetDownButtons() => _buttons;

    public MouseSnapshot Clone()
    {
        var copy = new MouseSnapshot
        {
            Position = Position,
            Delta = Vector2.Zero,
            WheelDelta = 0f
        };

        foreach (var b in _buttons)
            copy._buttons.Add(b);

        return copy;
    }
}

public sealed class KeyboardSnapshot
{
    private readonly HashSet<Key> _down = new();

    public bool IsKeyDown(Key key) => _down.Contains(key);

    public void SetKeyDown(Key key) => _down.Add(key);
    public void SetKeyUp(Key key) => _down.Remove(key);

    public IEnumerable<Key> GetDownKeys() => _down;

    public KeyboardSnapshot Clone()
    {
        var copy = new KeyboardSnapshot();
        foreach (var key in _down)
            copy._down.Add(key);
        return copy;
    }
}

public class KeybindStore
{
    public Dictionary<KeybindID, List<KeyBinding>> Entries { get; set; } = new();
}