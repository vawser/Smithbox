using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Which version of the groups the viewport tests.
/// </summary>
public enum DisplayGroupLoaderMode
{
    /// <summary>The groups exactly as stored in the MSB.</summary>
    Stored,

    /// <summary>The loader with its state byte at 0 (the value GameMan starts with): alternate groups merged in.</summary>
    Merged,

    /// <summary>The loader with its state byte at 1: alternate display groups replace the display groups.</summary>
    Replaced,

    /// <summary>The loader with its state byte at 0xFF: no alternate groups.</summary>
    NoAlternate
}

/// <summary>
/// Which pair of group arrays the Display Groups list's edit columns work on.
/// </summary>
public enum DisplayGroupEditSet
{
    Main,
    Alternate,
    AlternateMask,
    Backread,
    Navmesh
}

/// <summary>
/// Reads group fields from MSB parts by name, through at most one nested struct, with the lookups cached per type.
/// </summary>
public static class DisplayGroupFields
{
    private static readonly ConcurrentDictionary<(Type, string), Func<object, object>> Getters = new();

    public static object Get(object part, string name)
    {
        if (part == null)
            return null;

        var getter = Getters.GetOrAdd((part.GetType(), name), key => Build(key.Item1, key.Item2));
        return getter?.Invoke(part);
    }

    public static uint[] GetArray(object part, string name)
    {
        return Get(part, name) as uint[];
    }

    public static bool Has(object part, string name)
    {
        return part != null && Getters.GetOrAdd((part.GetType(), name), key => Build(key.Item1, key.Item2)) != null;
    }

    private static Func<object, object> Build(Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        var direct = type.GetProperty(name, flags);
        if (direct != null)
            return o => direct.GetValue(o);

        foreach (var outer in type.GetProperties(flags))
        {
            var pt = outer.PropertyType;
            if (outer.GetIndexParameters().Length > 0 || !pt.IsClass || pt == typeof(string) || pt.IsArray || !pt.IsNested)
                continue;

            var inner = pt.GetProperty(name, flags);
            if (inner != null)
            {
                return o =>
                {
                    var s = outer.GetValue(o);
                    return s == null ? null : inner.GetValue(s);
                };
            }
        }

        return null;
    }

    /// <summary>
    /// The names of the arrays an edit set works on: the first for the Draw column, the second for the Display
    /// column (null when the set has one array).
    /// </summary>
    public static (string first, string second) ArrayNames(DisplayGroupEditSet set) => set switch
    {
        DisplayGroupEditSet.Alternate => ("AltDrawGroups", "AltDisplayGroups"),
        DisplayGroupEditSet.AlternateMask => ("AltDrawGroupsMask", "AltDisplayGroupsMask"),
        DisplayGroupEditSet.Backread => ("BackreadGroups", null),
        DisplayGroupEditSet.Navmesh => ("NvmGroups", null),
        _ => ("DrawGroups", null) // Main uses the entity's resolved arrays instead
    };

    /// <summary>
    /// Applies the ER loader (MsbResCap, 0x140CF9B62) to a part's groups. NR and DB_NT share its layout and are
    /// assumed to share its rules. Returns null when the part has no alternate groups, or the mode is Stored.
    /// </summary>
    public static (uint[] draw, uint[] display)? ApplyLoader(Entity owner, uint[] storedDraw, uint[] storedDisplay,
        DisplayGroupLoaderMode mode)
    {
        if (mode == DisplayGroupLoaderMode.Stored || owner?.WrappedObject == null || storedDraw == null)
            return null;

        var part = owner.WrappedObject;

        if (Get(part, "UseAltGroups") is not byte useAlt)
            return null;

        bool noMerge = Get(part, "NoAltGroupMerge") is byte nm && nm != 0;

        var draw = (uint[])storedDraw.Clone();
        var display = storedDisplay != null ? (uint[])storedDisplay.Clone() : new uint[draw.Length];

        if (useAlt != 0)
        {
            var altDisplay = GetArray(part, "AltDisplayGroups");
            var altDisplayMask = GetArray(part, "AltDisplayGroupsMask");
            var altDraw = GetArray(part, "AltDrawGroups");
            var altDrawMask = GetArray(part, "AltDrawGroupsMask");

            if (mode == DisplayGroupLoaderMode.Replaced && altDisplay != null)
            {
                for (int i = 0; i < display.Length && i < altDisplay.Length; i++)
                    display[i] = altDisplay[i];
            }
            else if (mode == DisplayGroupLoaderMode.Merged && !noMerge)
            {
                if (altDisplay != null && altDisplayMask != null)
                {
                    for (int i = 0; i < display.Length && i < altDisplay.Length; i++)
                        display[i] = (display[i] & altDisplayMask[i]) | (altDisplay[i] & ~altDisplayMask[i]);
                }

                if (altDraw != null && altDrawMask != null)
                {
                    for (int i = 0; i < draw.Length && i < altDraw.Length; i++)
                        draw[i] = (draw[i] & altDrawMask[i]) | (altDraw[i] & ~altDrawMask[i]);
                }
            }
        }

        // A collision always draws in the groups it displays (all loader states)
        if (EntityHelper.IsPartCollision(owner))
        {
            for (int i = 0; i < draw.Length && i < display.Length; i++)
                draw[i] |= display[i];
        }

        return (draw, display);
    }
}
