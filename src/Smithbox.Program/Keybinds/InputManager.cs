using StudioCore.Application;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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

    public class MouseBinding
    {
        public MouseButton Key;
        public bool Ctrl;
        public bool Shift;
        public bool Alt;

        public MouseBinding Clone()
        {
            return new MouseBinding
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

    public static readonly MousebindStore _mouseBindings = new();
    public static readonly MousebindStore _defaultMouseBindings = new();

    private static string KeybindPath = "";
    private static string MousebindPath = "";

    public static Vector2 MousePosition => _mouseCurrent.Position;
    public static Vector2 MouseDelta => _mouseCurrent.Delta;
    public static float MouseWheelDelta => _mouseCurrent.WheelDelta;

    private static MouseSnapshot _mouseCurrent = new();
    private static MouseSnapshot _mousePrevious = new();

    private static KeyboardSnapshot _current = new();
    private static KeyboardSnapshot _previous = new();

    private static readonly Dictionary<Key, float> _keyHoldTime = new();
    private static readonly Dictionary<MouseButton, float> _mouseButtonHoldTime = new();

    private static double _delta;

    public static InputSnapshot InputSnapshot;

    public static KeyboardSnapshot Current => _current;
    public static MouseSnapshot MouseCurrent => _mouseCurrent;
    public static IReadOnlyDictionary<KeybindID, List<KeyBinding>> Bindings => _bindings.Entries;
    public static IReadOnlyDictionary<MousebindID, List<MouseBinding>> MouseBindings => _mouseBindings.Entries;

    // Init runs before the logger exists, so what it has to say waits here until FlushPendingLogs
    private static readonly List<(LogLevel Level, string Message, Exception Exception)> PendingLogs = new();

    private static void LogLater(LogLevel level, string message, Exception exception = null)
    {
        PendingLogs.Add((level, message, exception));
    }

    /// <summary>
    /// Logs what <see cref="Init"/> found while loading the bindings. Called once the logger has been set up.
    /// </summary>
    public static void FlushPendingLogs()
    {
        foreach (var (level, message, exception) in PendingLogs)
        {
            Smithbox.Log<Smithbox>(message, level, exception);
        }

        PendingLogs.Clear();
    }

    public static void Init()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        KeybindPath = Path.Combine(folder, "Keybinds.json");

        DefaultKeyBindings.CreateDefaultBindings();

        // A file that can't be read is left as it is, rather than saved over with the defaults, so the bindings
        // in it aren't lost
        var keybindsRead = true;
        var mousebindsRead = true;

        if (File.Exists(KeybindPath))
        {
            try
            {
                LoadKeybinds(KeybindPath);
            }
            catch (Exception ex)
            {
                keybindsRead = false;
                LogLater(LogLevel.Error, LOC.Get("INPUT_Keybinds_Failed_Load"), ex);
            }
        }

        MousebindPath = Path.Combine(folder, "Mousebinds.json");

        DefaultMouseBindings.CreateDefaultBindings();

        if (File.Exists(MousebindPath))
        {
            try
            {
                LoadMousebinds(MousebindPath);
            }
            catch (Exception ex)
            {
                mousebindsRead = false;
                LogLater(LogLevel.Error, LOC.Get("INPUT_Mousebinds_Failed_Load"), ex);
            }
        }

        if (keybindsRead)
        {
            SaveKeybinds();
        }

        if (mousebindsRead)
        {
            SaveMousebinds();
        }
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


    public static bool IsMouseDown(MousebindID action)
    {
        if (_mouseBindings.Entries.ContainsKey(action))
        {
            return _mouseBindings.Entries[action].Any(IsMouseBindingDown);
        }

        return false;
    }

    public static bool IsMousePressed(MousebindID action)
    {
        if (_mouseBindings.Entries.ContainsKey(action))
        {
            return _mouseBindings.Entries[action].Any(b =>
                 IsMouseBindingDown(b) &&
                 !_mousePrevious.IsDown(b.Key));
        }

        return false;
    }

    public static bool IsMousePressedOrRepeated(
    MousebindID action,
    float initialDelay = 0.35f,
    float repeatRate = 0.075f)
    {
        if (!_mouseBindings.Entries.TryGetValue(action, out var bindings))
            return false;

        foreach (var b in bindings)
        {
            if (!IsMouseBindingDown(b))
                continue;

            // Initial press
            if (!_mousePrevious.IsDown(b.Key))
                return true;

            // Held repeat
            if (_mouseButtonHoldTime.TryGetValue(b.Key, out float t))
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

    public static bool IsMouseReleased(MousebindID action)
    {
        if (_mouseBindings.Entries.ContainsKey(action))
        {
            return _mouseBindings.Entries[action].Any(b =>
            !_mouseCurrent.IsDown(b.Key) &&
            _mousePrevious.IsDown(b.Key));
        }

        return false;
    }


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

    public static void BindMouseKey(MousebindID action, MouseBinding binding)
    {
        if (!_mouseBindings.Entries.TryGetValue(action, out var list))
            _mouseBindings.Entries[action] = list = new();

        list.Add(binding);

        if (!_defaultMouseBindings.Entries.TryGetValue(action, out var defaultList))
            _defaultMouseBindings.Entries[action] = defaultList = new();

        _defaultMouseBindings.Entries[action].Add(binding.Clone());
    }

    // ---------------- Serialization ----------------

    public static void SaveKeybinds()
    {
        var filestring = JsonSerializer.Serialize(_bindings, ProjectJsonSerializerContext.Default.KeybindStore);

        var dir = Path.GetDirectoryName(KeybindPath);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(KeybindPath, filestring);
    }
    public static void SaveMousebinds()
    {
        var filestring = JsonSerializer.Serialize(_mouseBindings, ProjectJsonSerializerContext.Default.MousebindStore);

        var dir = Path.GetDirectoryName(MousebindPath);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(MousebindPath, filestring);
    }

    public static void LoadKeybinds(string path)
    {
        LoadBindings(path, _bindings.Entries, ProjectJsonSerializerContext.Default.ListKeyBinding);
    }

    public static void LoadMousebinds(string path)
    {
        LoadBindings(path, _mouseBindings.Entries, ProjectJsonSerializerContext.Default.ListMouseBinding);
    }

    /// <summary>
    /// Reads the saved bindings over the defaults, one entry at a time. The entries are keyed by the name of their
    /// binding, so a file saved before a binding was removed or renamed still names it. Such an entry, or one that
    /// can't be read, is skipped and logged, rather than failing the whole file: the file is saved again straight
    /// after loading, so a failed load would replace every binding the user had set with the defaults.
    /// </summary>
    private static void LoadBindings<TId, TBinding>(string path, Dictionary<TId, List<TBinding>> target,
        JsonTypeInfo<List<TBinding>> typeInfo) where TId : struct, Enum
    {
        if (!File.Exists(path))
            return;

        using var document = JsonDocument.Parse(File.ReadAllText(path));

        if (!document.RootElement.TryGetProperty("Entries", out var entries) || entries.ValueKind != JsonValueKind.Object)
            return;

        var skipped = new List<string>();

        foreach (var entry in entries.EnumerateObject())
        {
            // Only a name the enum still has: TryParse also takes numbers, which could stand for any binding
            if (!Enum.TryParse<TId>(entry.Name, out var id) || id.ToString() != entry.Name || !target.ContainsKey(id))
            {
                skipped.Add(entry.Name);
                continue;
            }

            try
            {
                var bindings = entry.Value.Deserialize(typeInfo);

                if (bindings != null)
                {
                    target[id] = bindings;
                }
            }
            catch (JsonException)
            {
                skipped.Add(entry.Name);
            }
        }

        if (skipped.Count > 0)
        {
            LogLater(LogLevel.Information, LOC.Get("INPUT_Bindings_Skipped", Path.GetFileName(path), string.Join(", ", skipped)));
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
        if (b.Key == Key.Unknown)
            return false;

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
        if (b.Key == Key.Unknown)
            return false;

        if (!_current.IsKeyDown(b.Key))
            return false;

        return true;
    }


    private static bool IsMouseBindingDown(MouseBinding b)
    {
        if (b.Key == MouseButton.None)
            return false;

        if (!_mouseCurrent.IsDown(b.Key))
            return false;

        if (b.Ctrl && !CtrlDown())
            return false;

        if (b.Shift && !ShiftDown())
            return false;

        if (b.Alt && !AltDown())
            return false;


        return true;
    }

    private static bool IsMouseBindingDown_IgnoreModifiers(MouseBinding b)
    {
        if (b.Key == MouseButton.None)
            return false;

        if (!_mouseCurrent.IsDown(b.Key))
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
        if (curBind.Ctrl) s += LOC.Get("KEY_Modifier_Ctrl_Plus");
        if (curBind.Shift) s += LOC.Get("KEY_Modifier_Shift_Plus");
        if (curBind.Alt) s += LOC.Get("KEY_Modifier_Alt_Plus");
        s += curBind.Key.ToString();
        return s;
    }
    public static string GetMouseHint(MousebindID action)
    {
        var curBind = _mouseBindings.Entries[action].FirstOrDefault();

        string s = "";
        if (curBind.Ctrl) s += LOC.Get("KEY_Modifier_Ctrl_Plus");
        if (curBind.Shift) s += LOC.Get("KEY_Modifier_Shift_Plus");
        if (curBind.Alt) s += LOC.Get("KEY_Modifier_Alt_Plus");
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
public class MousebindStore
{
    public Dictionary<MousebindID, List<MouseBinding>> Entries { get; set; } = new();
}