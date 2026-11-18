#nullable enable
using Andre.Formats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The character creator's own names for its sliders, from CharMakeMenuTopParam: each slider row (commandType 1)
/// names the menu slot it edits (faceParamId), and the slider table (<see cref="FaceGenSliderTable.Slider.Slot"/>)
/// says which FaceParam slider sits in each slot. The caption is the menu row's name.
/// <para>
/// Only games whose slider table gives the slots have captions (ER). The menu's fields are found by name, so a game
/// whose menu param has other field names has none.
/// </para>
/// </summary>
public static class FaceGenMenu
{
    private const string MenuParamName = "CharMakeMenuTopParam";
    private const int SliderCommand = 1;

    private static readonly ConditionalWeakTable<ProjectEntry, Holder> Cache = new();

    private sealed class Holder
    {
        public object? Key;
        public Dictionary<int, string> BySlot = new();
    }

    /// <summary>The creator's caption for a FaceParam field, or null if the game's menu doesn't name it</summary>
    public static string? Caption(ProjectEntry project, string field)
    {
        if (FaceGenSliderTable.Get(project) is not { } table)
            return null;

        var id = FaceGenModel.SliderId(field);

        if (id < 0 || table.Get(id)?.Slot is not { } slot)
            return null;

        return Captions(project).GetValueOrDefault(slot);
    }

    /// <summary>The menu's slider captions by slot: the first named slider row of each slot</summary>
    private static Dictionary<int, string> Captions(ProjectEntry project)
    {
        var holder = Cache.GetOrCreateValue(project);
        var menu = FaceGenParams.GetParam(project, MenuParamName);

        // Made again when the menu param is replaced (a reload) or its rows change (a rename is an undo step)
        var key = (menu, menu?.Rows.Count, project.Handler.ParamEditor?.ActionManager.PeekUndoAction());

        if (Equals(holder.Key, key))
            return holder.BySlot;

        var bySlot = new Dictionary<int, string>();

        if (menu != null)
        {
            foreach (var row in menu.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.Name) ||
                    Int(row, "commandType") != SliderCommand ||
                    Int(row, "faceParamId") is not { } slot)
                {
                    continue;
                }

                bySlot.TryAdd(slot, row.Name.Trim());
            }
        }

        holder.Key = key;
        holder.BySlot = bySlot;
        return bySlot;
    }

    private static int? Int(Param.Row row, string field) =>
        FaceGenParams.CellValue(row, field) is IConvertible value ? value.ToInt32(null) : null;
}
