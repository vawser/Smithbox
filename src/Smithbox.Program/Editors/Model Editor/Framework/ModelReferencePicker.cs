using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// What a reference field of the model points at
/// </summary>
public enum ModelReferenceKind
{
    None,
    Material,
    Node
}

/// <summary>
/// Draws the fields of the model that refer to a material or node by index as a list of names to pick from, with a
/// Go To button. Only fields that hold an index into the model's own material or node list are handled: the skeleton
/// set's bone links index its own bone list, so they keep a plain number.
/// </summary>
public class ModelReferencePicker
{
    private readonly ModelEditorView View;

    private string _filter = "";

    public ModelReferencePicker(ModelEditorView view)
    {
        View = view;
    }

    private sealed record FieldInfo(ModelReferenceKind Kind, bool AllowNone, bool IsReadOnly);

    // The fields picked by name, by the type that holds them
    private static readonly Dictionary<(Type, string), FieldInfo> Fields = new()
    {
        [(typeof(FLVER2.Mesh), nameof(FLVER2.Mesh.MaterialIndex))] = new(ModelReferenceKind.Material, false, false),
        [(typeof(FLVER2.Mesh), nameof(FLVER2.Mesh.NodeIndex))] = new(ModelReferenceKind.Node, true, false),

        [(typeof(FLVER.Node), nameof(FLVER.Node.ParentIndex))] = new(ModelReferenceKind.Node, true, false),

        // Kept in step with the parent indices by the editor, so they are shown but not edited by hand
        [(typeof(FLVER.Node), nameof(FLVER.Node.FirstChildIndex))] = new(ModelReferenceKind.Node, true, true),
        [(typeof(FLVER.Node), nameof(FLVER.Node.NextSiblingIndex))] = new(ModelReferenceKind.Node, true, true),
        [(typeof(FLVER.Node), nameof(FLVER.Node.PreviousSiblingIndex))] = new(ModelReferenceKind.Node, true, true),

        [(typeof(FLVER.Dummy), nameof(FLVER.Dummy.ParentBoneIndex))] = new(ModelReferenceKind.Node, false, false),
        [(typeof(FLVER.Dummy), nameof(FLVER.Dummy.AttachBoneIndex))] = new(ModelReferenceKind.Node, true, false),

        [(typeof(FLVER2.SkeletonSet.Bone), nameof(FLVER2.SkeletonSet.Bone.NodeIndex))] = new(ModelReferenceKind.Node, true, false)
    };

    /// <summary>
    /// Whether the field is drawn as a picker (rather than a number), and what it picks from
    /// </summary>
    public static bool IsPickerField(PropertyInfo prop, Type valueType, out ModelReferenceKind kind)
    {
        kind = ModelReferenceKind.None;

        if (prop?.DeclaringType == null || (valueType != typeof(int) && valueType != typeof(short)))
            return false;

        if (!Fields.TryGetValue((prop.DeclaringType, prop.Name), out var info))
            return false;

        kind = info.Kind;
        return true;
    }

    /// <summary>
    /// Draws the picker. Returns true, with the new value, when an entry was picked.
    /// </summary>
    /// <param name="owner">The object holding the field, for the choices that depend on it (a node can't be its
    /// own parent)</param>
    public bool Draw(PropertyInfo prop, Type valueType, object value, object owner, bool readOnly, out object newValue)
    {
        newValue = value;

        var container = View.Selection.SelectedModelWrapper?.Container;
        var info = Fields[(prop.DeclaringType, prop.Name)];

        if (container == null)
        {
            ImGui.Text($"{value}");
            return false;
        }

        var names = GetNames(container, info.Kind);
        var current = Convert.ToInt32(value);

        var goToWidth = ImGui.CalcTextSize(Icons.Binoculars).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.SetNextItemWidth(-(goToWidth + ImGui.GetStyle().ItemSpacing.X));

        var picked = false;

        ImGui.BeginDisabled(readOnly || info.IsReadOnly);

        if (ImGui.BeginCombo("##referencePicker", GetLabel(names, current), ImGuiComboFlags.HeightLarge))
        {
            picked = DrawChoices(container, info, names, current, owner, valueType, ref newValue);

            ImGui.EndCombo();
        }

        ImGui.EndDisabled();

        if (info.IsReadOnly)
        {
            GUI.Tooltip(LOC.Get("MODEL_Properties_Reference_Maintained_TT"));
        }

        // Go To
        ImGui.SameLine();

        ImGui.BeginDisabled(current < 0 || current >= names.Count);
        if (ImGui.Button($"{Icons.Binoculars}##referenceGoTo"))
        {
            var type = info.Kind is ModelReferenceKind.Material ? "material" : "node";
            EditorCommandQueue.AddCommand($"model/select/{type}/{current}");
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("MODEL_Properties_Reference_GoTo_TT"));

        return picked;
    }

    private bool DrawChoices(ModelContainer container, FieldInfo info, List<string> names, int current, object owner,
        Type valueType, ref object newValue)
    {
        // A fresh search each time the list opens, ready to type into
        if (ImGui.IsWindowAppearing())
        {
            _filter = "";
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##referenceFilter", LOC.Get("MODEL_Properties_Reference_Filter_Hint"), ref _filter, 255);

        // A node can't sit under itself or anything below it
        var blocked = owner is FLVER.Node node && info.Kind is ModelReferenceKind.Node
            ? GetSelfAndDescendants(container, node)
            : new HashSet<int>();

        var choices = new List<int>();

        if (info.AllowNone)
        {
            choices.Add(-1);
        }

        for (var i = 0; i < names.Count; i++)
        {
            if (string.IsNullOrEmpty(_filter) || GetLabel(names, i).Contains(_filter, StringComparison.OrdinalIgnoreCase))
            {
                choices.Add(i);
            }
        }

        var picked = false;

        // Skeletons run to hundreds of bones, so only the visible rows are drawn
        var clipper = new ImGuiListClipper();
        clipper.Begin(choices.Count);

        while (clipper.Step())
        {
            for (var row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var index = choices[row];
                var isBlocked = blocked.Contains(index);

                ImGui.BeginDisabled(isBlocked);

                if (ImGui.Selectable($"{GetLabel(names, index)}##choice{index}", index == current))
                {
                    newValue = valueType == typeof(short) ? (object)(short)index : index;
                    picked = index != current;
                }

                ImGui.EndDisabled();

                if (index == current)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
        }

        clipper.End();

        return picked;
    }

    private static List<string> GetNames(ModelContainer container, ModelReferenceKind kind)
    {
        return kind switch
        {
            ModelReferenceKind.Material => container.Materials.Select(e => ((FLVER2.Material)e.WrappedObject).Name).ToList(),
            ModelReferenceKind.Node => container.Nodes.Select(e => ((FLVER.Node)e.WrappedObject).Name).ToList(),
            _ => new List<string>()
        };
    }

    private static string GetLabel(List<string> names, int index)
    {
        if (index < 0)
            return $"{index}: {LOC.Get("MODEL_Properties_Reference_None")}";

        if (index >= names.Count)
            return $"{index}: {LOC.Get("MODEL_Properties_Reference_Missing")}";

        return $"{index}: {names[index]}";
    }

    /// <summary>
    /// The node and every node below it, by their indices
    /// </summary>
    public static HashSet<int> GetSelfAndDescendants(ModelContainer container, FLVER.Node node)
    {
        var nodes = container.Nodes.Select(e => (FLVER.Node)e.WrappedObject).ToList();
        var result = new HashSet<int>();

        var start = nodes.IndexOf(node);

        if (start < 0)
            return result;

        result.Add(start);

        // Repeat until nothing is added, as a child may come before its parent in the list
        var added = true;

        while (added)
        {
            added = false;

            for (var i = 0; i < nodes.Count; i++)
            {
                if (!result.Contains(i) && result.Contains(nodes[i].ParentIndex))
                {
                    result.Add(i);
                    added = true;
                }
            }
        }

        return result;
    }
}
