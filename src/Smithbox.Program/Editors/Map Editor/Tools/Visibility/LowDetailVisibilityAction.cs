using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Finds the map objects whose model is a low detail stand-in for other geometry, such as the distant view of an area.
/// No flag in the FLVER marks a whole model as low detail, so this goes by two signs that the models share:
/// they are named as low detail (m30_00_low_hall_03_a), or they cover a large area with few triangles.
/// </summary>
public class LowDetailVisibilityAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    private List<LowDetailCandidate> Candidates = new();

    // Parts whose model had not finished loading when last scanned, so they could not be judged
    private int UnloadedCount = 0;

    private bool HasScanned = false;

    public LowDetailVisibilityAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        GUI.WrappedText("Use these actions to hide or show map objects whose model is a low detail version of other geometry, such as the distant view of an area.");

        GUI.Spacer();
        GUI.SimpleHeader("Detection", "How a model is judged to be low detail. A model that meets either enabled check counts.");

        ImGui.Checkbox("Low Detail Naming##lowDetailNaming", ref CFG.Current.Toolbar_LowDetail_Use_Naming);
        GUI.Tooltip("Count models with a material or texture named as low detail, such as m30_00_low_hall_03_a or a name containing 'lod'.\n\nThis is rarely wrong, but only some low detail models are named this way.");

        ImGui.Checkbox("Low Triangle Density##lowDetailDensity", ref CFG.Current.Toolbar_LowDetail_Use_Density);
        GUI.Tooltip("Count large models that have few triangles for their surface area.\n\nThis finds more of the low detail models, but can also find simple geometry such as flat floors and water.");

        if (CFG.Current.Toolbar_LowDetail_Use_Density)
        {
            GUI.SetInputWidth();
            if (ImGui.DragFloat("Max Density##lowDetailMaxDensity", ref CFG.Current.Toolbar_LowDetail_Max_Density, 0.05f, 0.0f, 100.0f, "%.2f tris/m²"))
            {
                CFG.Current.Toolbar_LowDetail_Max_Density = Math.Max(0.0f, CFG.Current.Toolbar_LowDetail_Max_Density);
            }
            GUI.Tooltip("Models with fewer triangles than this per square metre of surface count as low detail.");

            GUI.SetInputWidth();
            if (ImGui.DragFloat("Min Size##lowDetailMinSize", ref CFG.Current.Toolbar_LowDetail_Min_Size, 1.0f, 0.0f, 10000.0f, "%.0f m"))
            {
                CFG.Current.Toolbar_LowDetail_Min_Size = Math.Max(0.0f, CFG.Current.Toolbar_LowDetail_Min_Size);
            }
            GUI.Tooltip("Only models whose bounding box is at least this long, corner to corner, are checked for density.\n\nSmall props are often simple, so this keeps them from counting.");
        }

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "Actions that affect every loaded map object that is detected as low detail.");

        GUI.MultiButtonInput("lowDetailActions",
            "disableVis", "Make Invisible", "Hide every map object detected as low detail.", MakeInvisibleAction,
            "enableVis", "Make Visible", "Show every map object detected as low detail.", MakeVisibleAction,
            "selectLowDetail", "Select", "Select every map object detected as low detail.", SelectAction,
            "scanLowDetail", "Scan", "List the map objects detected as low detail, without changing them.", ScanAction);

        DisplayCandidates();
    }

    private void DisplayCandidates()
    {
        if (!HasScanned)
            return;

        GUI.Spacer();
        GUI.SimpleHeader("Detected", "The map objects detected as low detail by the last action. Click one to select it.");

        GUI.WrappedText($"{Candidates.Count} detected.");

        if (UnloadedCount > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, $"{UnloadedCount} map objects were skipped, as their model had not finished loading.");
        }

        if (Candidates.Count == 0)
            return;

        var tblFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable("lowDetailCandidates", 4, tblFlags, new Vector2(0, 300 * DPI.UIScale())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Reason");
            ImGui.TableSetupColumn("Density");
            ImGui.TableSetupColumn("Size");
            ImGui.TableHeadersRow();

            for (var i = 0; i < Candidates.Count; i++)
            {
                var entry = Candidates[i];

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                var label = entry.Entity.EditorVisible ? entry.Entity.Name : $"{entry.Entity.Name} (hidden)";
                if (ImGui.Selectable($"{label}##lowDetailEntry{i}", false, ImGuiSelectableFlags.SpanAllColumns))
                {
                    View.ViewportSelection.ClearSelection();
                    View.ViewportSelection.AddSelection(entry.Entity);
                }
                GUI.Tooltip(entry.ModelName);

                ImGui.TableSetColumnIndex(1);
                ImGui.Text(entry.Reason);

                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"{entry.Density:F2}");

                ImGui.TableSetColumnIndex(3);
                ImGui.Text($"{entry.Size:F0} m");
            }

            ImGui.EndTable();
        }
    }

    public void MakeInvisibleAction()
    {
        ApplyVisibility(false);
    }

    public void MakeVisibleAction()
    {
        ApplyVisibility(true);
    }

    public void SelectAction()
    {
        if (!Scan())
            return;

        View.ViewportSelection.ClearSelection();
        View.ViewportSelection.AddSelection(Candidates.Select(x => (ISelectable)x.Entity).ToList());
    }

    public void ScanAction()
    {
        Scan();
    }

    /// <summary>
    /// The list would hold entities of a map that is gone.
    /// </summary>
    public void OnMapUnloaded(MapContainer map)
    {
        Candidates.RemoveAll(x => x.Entity.Container == map);
    }

    /// <summary>
    /// Effect
    /// </summary>
    private void ApplyVisibility(bool visible)
    {
        // Scanned each time, as models finish loading and maps come and go after the last scan
        if (!Scan())
            return;

        foreach (var entry in Candidates)
        {
            entry.Entity.EditorVisible = visible;
        }

        View.DelayPicking();
    }

    private bool Scan()
    {
        if (!Project.Handler.MapData.PrimaryBank.Maps.Values.Any(x => x.MapContainer != null))
        {
            Smithbox.LogError<LowDetailVisibilityAction>("No map loaded.");
            return false;
        }

        if (!CFG.Current.Toolbar_LowDetail_Use_Naming && !CFG.Current.Toolbar_LowDetail_Use_Density)
        {
            Smithbox.LogError<LowDetailVisibilityAction>("No detection check is enabled.");
            return false;
        }

        Candidates = new();
        UnloadedCount = 0;
        HasScanned = true;

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var map = entry.Value.MapContainer;

            if (map == null)
                continue;

            foreach (Entity obj in map.Objects)
            {
                if (!EntityHelper.IsPart(obj))
                    continue;

                // Collision and navmesh parts have no FLVER, so they are never candidates
                if (obj.RenderSceneMesh is not MeshRenderableProxy mrp ||
                    mrp.ResourceHandle is not ResourceHandle<FlverResource> handle)
                    continue;

                if (!handle.IsLoaded)
                {
                    UnloadedCount++;
                    continue;
                }

                var candidate = Evaluate(obj, mrp, handle.Get());

                if (candidate != null)
                {
                    Candidates.Add(candidate);
                }
            }
        }

        Candidates = Candidates.OrderBy(x => x.Entity.Name).ToList();

        return true;
    }

    private LowDetailCandidate Evaluate(Entity obj, MeshRenderableProxy mrp, FlverResource flver)
    {
        if (flver == null || flver.DetailTriangleCount == 0)
            return null;

        // The stats are in model space, so they are scaled to how the part is placed
        var linearScale = MathF.Cbrt(MathF.Abs(mrp.World.GetDeterminant()));
        if (linearScale <= 0.0f || float.IsNaN(linearScale))
            linearScale = 1.0f;

        var area = flver.DetailSurfaceArea * linearScale * linearScale;
        var density = area > 0.0f ? flver.DetailTriangleCount / area : float.MaxValue;
        var size = (flver.Bounds.Max - flver.Bounds.Min).Length() * linearScale;

        var isNamed = CFG.Current.Toolbar_LowDetail_Use_Naming && flver.HasLowDetailNaming;

        var isSparse = CFG.Current.Toolbar_LowDetail_Use_Density &&
            density < CFG.Current.Toolbar_LowDetail_Max_Density &&
            size >= CFG.Current.Toolbar_LowDetail_Min_Size;

        if (!isNamed && !isSparse)
            return null;

        var reason = isNamed && isSparse ? "Naming, Density" : isNamed ? "Naming" : "Density";

        return new LowDetailCandidate(obj, obj.GetPropertyValue<string>("ModelName") ?? "", reason, density, size);
    }

    private record LowDetailCandidate(Entity Entity, string ModelName, string Reason, float Density, float Size);
}
