using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class MsbUtils
{
    public static void EntitySelectionHandler(MapEditorView view, ViewportSelection selection, Entity entity,
        bool itemSelected, bool isItemFocused, List<WeakReference<Entity>> filteredEntityList = null)
    {
        // Up/Down arrow mass selection
        var arrowKeySelect = false;

        if (isItemFocused && InputManager.HasArrowSelection())
        {
            itemSelected = true;
            arrowKeySelect = true;
        }

        if (itemSelected)
        {
            if (arrowKeySelect)
            {
                if (InputManager.HasCtrlDown() || InputManager.HasShiftDown())
                {
                    selection.AddSelection(entity);
                }
                else
                {
                    selection.ClearSelection();
                    selection.AddSelection(entity);
                }
            }
            else if (InputManager.HasCtrlDown())
            {
                // Toggle Selection
                if (selection.GetSelection().Contains(entity))
                {
                    selection.RemoveSelection(entity);
                }
                else
                {
                    selection.AddSelection(entity);
                }
            }
            else if (selection.GetSelection().Count > 0
                     && InputManager.HasShiftDown())
            {
                // Select Range
                List<Entity> entList;
                if (filteredEntityList != null)
                {
                    entList = new();
                    foreach (WeakReference<Entity> ent in filteredEntityList)
                    {
                        if (ent.TryGetTarget(out Entity e))
                        {
                            entList.Add(e);
                        }
                    }
                }
                else
                {
                    entList = entity.Container.Objects;
                }

                var i1 = -1;

                if (entity.GetType() == typeof(MsbEntity))
                {
                    i1 = entList.IndexOf(selection.GetFilteredSelection<MsbEntity>()
                        .FirstOrDefault(fe => fe.Container == entity.Container && fe != entity.Container.RootObject));
                }

                var i2 = -1;

                if (entity.GetType() == typeof(MsbEntity))
                {
                    i2 = entList.IndexOf((MsbEntity)entity);
                }

                if (i1 != -1 && i2 != -1)
                {
                    var iStart = i1;
                    var iEnd = i2;
                    if (i2 < i1)
                    {
                        iStart = i2;
                        iEnd = i1;
                    }

                    for (var i = iStart; i <= iEnd; i++)
                    {
                        selection.AddSelection(entList[i]);
                    }
                }
                else
                {
                    selection.AddSelection(entity);
                }
            }
            else
            {
                // Exclusive Selection
                selection.ClearSelection();
                selection.AddSelection(entity);
            }
        }
    }

    public static bool Matches(string name, Type refType, IMsbEntry entry)
    {
        return entry.Name == name && refType.IsAssignableFrom(entry.GetType());
    }

    /// <summary>
    /// This will yield the value and a setter for that value for each field in `entry` that's a MSBReference
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static IEnumerable<(string, Type, Action<string>)> GetMsbReferences(IMsbEntry entry)
    {
        if(entry == null) 
            yield break;

        foreach (var property in entry.GetType().GetProperties())
        {
            if (property.GetCustomAttribute<MSBReference>() is not MSBReference msbReference) 
                continue;

            var value = property.GetValue(entry);
            if (value is string[] array)
            {
                var index = 0;
                foreach (var element in array)
                {
                    yield return (array[index], msbReference.ReferenceType, (newName) => array[index] = newName);
                    index += 1;
                }
            }
            else if (value is string str)
            {
                yield return (str, msbReference.ReferenceType, (newName) => property.SetValue(entry, newName));
            }
        }
    }

    /// <summary>
    /// Renames the target, and every reference to it within 'entries'
    /// </summary>
    public static void RenameWithRefs(IEnumerable<IMsbEntry> entries, IMsbEntry target, string newName)
    {
        foreach (var entry in entries)
        {
            foreach (var (current, type, set) in GetMsbReferences(entry))
            {
                if (Matches(current, type, target)) { set(newName); }
            }
        }
        target.Name = newName;
    }

    /// <summary>
    /// Empties out every reference in 'obj' that's not pointing to an entity that's part of 'entities'
    /// </summary>
    public static void StripMsbReference(IEnumerable<IMsbEntry> entities, IMsbEntry obj)
    {
        foreach (var (current, type, set) in GetMsbReferences(obj))
        {
            if (current is null or "") 
                continue;

            if (!entities.Any((ent) => Matches(current, type, ent)))
                set(null);
        }
    }

    /// <summary>
    /// Gets the full list of maps in the game. 
    /// Basically if there's an msb for it, it will be in this list.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetFullMapList(ProjectEntry project)
    {
        List<string> mapList = new();

        if (project.Handler.MapEditor != null)
        {
            foreach (var entry in project.Locator.MapFiles.Entries)
            {
                mapList.Add(entry.Filename);
            }
        }

        return mapList;
    }
}