using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Material tab's Replace Materials section (Model Variants Plan.md §6, V5): every face of one collision material
/// given another, in each layer of the model at once (an asset's high and low), as pending repaints that the header's
/// Commit rebuilds. One step instead of painting every face, for "a wooden version of this stone crate".
/// </summary>
public partial class ModelCollisionTool
{
    private uint? RemapFrom;
    private int RemapTo = -1;

    // A picker of its own: the Paint Material section's draws the same widget IDs in this window, and shares its filter
    private CollisionMaterialPicker RemapPicker;
    private bool RemapHigh = true;
    private bool RemapLow = true;

    /// <summary>The materials of the model's layers, with their face counts per layer.</summary>
    private List<(uint Material, Dictionary<HavokCollisionType, int> Faces)> GetRemapLegend()
    {
        var legend = new Dictionary<uint, Dictionary<HavokCollisionType, int>>();

        foreach (var type in LayerTypes())
        {
            var layer = GetLayer(type);

            if (layer == null)
                continue;

            foreach (var body in layer.Bodies)
            {
                foreach (var (material, faces) in body.MaterialTriangleCounts)
                {
                    if (!legend.TryGetValue(material, out var perLayer))
                        legend[material] = perLayer = new();

                    perLayer[type] = perLayer.GetValueOrDefault(type) + faces;
                }
            }
        }

        return legend.OrderBy(e => e.Key).Select(e => (e.Key, e.Value)).ToList();
    }

    private void DisplayRemap()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_CollisionTool_Remap_Header"), LOC.Get("MODEL_CollisionTool_Remap_Header_TT"));

        var legend = GetRemapLegend();

        if (legend.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Legend_None"));
            return;
        }

        var names = GetMaterialNames();
        var layered = LayerTypes().Count() > 1;

        // From: one of the materials the model has
        GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Remap_From"));

        foreach (var (material, faces) in legend)
        {
            DisplaySwatch(material, "Remap");
            ImGui.SameLine();

            var count = layered
                ? LOC.Get("MODEL_CollisionTool_Remap_Faces_Layered", faces.GetValueOrDefault(HavokCollisionType.High), faces.GetValueOrDefault(HavokCollisionType.Low))
                : LOC.Get("MODEL_CollisionTool_Remap_Faces", faces.Values.Sum());

            if (ImGui.Selectable($"{names.Describe(material)} ({count})##modelCollisionRemapFrom{material}", RemapFrom == material))
                RemapFrom = material;
        }

        if (RemapFrom is not { } from || legend.All(e => e.Material != from))
        {
            RemapFrom = null;
            return;
        }

        // To
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Remap_To", names.Describe(from)));

        if (RemapTo < 0)
            RemapTo = (int)from;

        // The picker's widgets have fixed IDs (##collisionMaterialSurface…), also drawn by Paint Material above
        RemapPicker ??= new CollisionMaterialPicker(Project);
        ImGui.PushID("modelCollisionRemapPicker");
        RemapPicker.Display(ref RemapTo, null);
        ImGui.PopID();

        if (layered)
        {
            ImGui.Checkbox($"{LOC.Get("MODEL_CollisionTool_Remap_High")}##modelCollisionRemapHigh", ref RemapHigh);
            ImGui.SameLine();
            ImGui.Checkbox($"{LOC.Get("MODEL_CollisionTool_Remap_Low")}##modelCollisionRemapLow", ref RemapLow);
            GUI.Tooltip(LOC.Get("MODEL_CollisionTool_Remap_Layers_TT"));
        }

        var target = (uint)System.Math.Max(0, RemapTo);
        var types = LayerTypes().Where(t => !layered || (t == HavokCollisionType.High ? RemapHigh : RemapLow)).ToList();
        var affected = legend.First(e => e.Material == from).Faces.Where(f => types.Contains(f.Key)).Sum(f => f.Value);
        // Replace commits at once, as its own undo step: other pending edits would be swept into it
        var pending = Layers.Values.Any(l => l.IsEdited);

        if (pending)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_CollisionTool_Remap_Pending"));

        var canApply = target != from && affected > 0 && !pending;

        GUI.ConditionalMultiButtonInput("modelCollisionRemapActions", "modelCollisionRemapApply",
            LOC.Get("MODEL_CollisionTool_Remap_Apply", affected), LOC.Get("MODEL_CollisionTool_Remap_Apply_TT"),
            () => ApplyRemap(from, target, types), canApply);
    }

    /// <summary>
    /// Gives every face of a material another, in the given layers, and commits it at once as one undo step (in the view's
    /// history, as Commit is). Only when nothing else is pending, so the step holds the replace alone; a rebuild that
    /// fails puts the faces back.
    /// </summary>
    private void ApplyRemap(uint from, uint to, List<HavokCollisionType> types)
    {
        if (Layers.Values.Any(l => l.IsEdited))
            return;

        var changed = 0;

        foreach (var type in types)
        {
            var layer = GetLayer(type);

            if (layer == null)
                continue;

            foreach (var body in layer.Bodies)
            {
                for (var t = 0; t < body.TriangleCount; t++)
                {
                    if (body.Materials[t] == from && body.SetMaterial(t, to))
                        changed++;
                }
            }
        }

        if (changed == 0)
            return;

        var names = GetMaterialNames();
        var label = LOC.Get("MODEL_CollisionTool_History_Remap", names.Describe(from), names.Describe(to));

        if (!CommitChanges(label, LOC.Get("MODEL_CollisionTool_Remap_Done", changed, names.Describe(from), names.Describe(to))))
        {
            // The rebuild failed (reported): nothing else was pending, so every repaint here is the replace's
            foreach (var layer in Layers.Values)
            {
                foreach (var body in layer.Bodies)
                    body.Reset();
            }

            return;
        }

        // The replaced material is gone from the list: pick the new one, so another replace starts from it
        RemapFrom = to;
        RemapTo = -1;
    }
}
