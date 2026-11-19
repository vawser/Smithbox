using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP mode's geometry helpers (Phase G5), at the end of the Models tab: a ladder strip at each selected ladder
/// object, and a drop strip from the ledge nearest the picked point to the floor below it. Both cut their joins into
/// the floors, so enemies can path onto them; each is one undoable edit. See NgpStripEdit.
/// </summary>
public partial class NavigationTool
{
    private float NgpDropRun = 0.5f;
    private float NgpDropMax = 30f;

    private static readonly Regex NgpLadderModel = new(@"^o\d\d_09\d\d$", RegexOptions.IgnoreCase);

    /// <summary>A selected object's foot, +Z and top height, map-local (the NGP's own space).</summary>
    private sealed record NgpLadderSource(string Name, string Model, Vector3 Foot, Vector3 Forward, float Top);

    private void DisplayNgpStrips(MapContainer map, NgpMap data)
    {
        DisplayNgpLadder(map, data);
        DisplayNgpDrop(map, data);
    }

    #region Ladder

    private void DisplayNgpLadder(MapContainer map, NgpMap data)
    {
        // Ladder
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Ladder_Header"), LOC.Get("MAP_Navigation_Ngp_Ladder_Header_TT"));

        var sources = SelectedNgpLadders(data);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Ladder_Selected", sources.Count));

        foreach (var s in sources.Take(6))
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Ladder_Source", s.Name, s.Model, (s.Top - s.Foot.Y).ToString("0.00")));
            if (!NgpLadderModel.IsMatch(s.Model ?? ""))
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Ngp_Ladder_Not_Ladder_Model", s.Name));
        }

        if (HasNgpVertexEdits)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Ngp_Strips_Pending"));

        GUI.ConditionalMultiButtonInput("navigationNgpLadderActions",
            "navigationNgpLadderAdd", LOC.Get("MAP_Navigation_Ngp_Ladder_Add"), LOC.Get("MAP_Navigation_Ngp_Ladder_Add_TT"),
            () => AddNgpLadders(map, data, sources), sources.Count > 0 && !HasNgpVertexEdits);

        if (sources.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Ladder_Select"));
    }

    /// <summary>
    /// The selected objects drawn with a model: their position, +Z and the top of their bounds, in the NGP's space.
    /// Vanilla ladders sit at their object (median 3 cm), from its height (4 cm), facing its +Z.
    /// </summary>
    private List<NgpLadderSource> SelectedNgpLadders(NgpMap data)
    {
        var result = new List<NgpLadderSource>();
        if (data == null || !Matrix4x4.Invert(data.World, out var toLocal))
            return result;

        foreach (var entity in View.ViewportSelection.GetFilteredSelection<MsbEntity>())
        {
            if (entity.WrappedObject is not MSB2.Part.Object part)
                continue;

            var world = entity.GetWorldMatrix();
            var foot = Vector3.Transform(world.Translation, toLocal);
            var forward = Vector3.TransformNormal(Vector3.UnitZ, world);
            forward.Y = 0;
            if (forward.LengthSquared() < 1e-8f)
                continue;

            // The top of its bounds, else 3 m up
            float top = foot.Y + 3f;
            if (entity.RenderSceneMesh?.GetLocalBounds() is BoundingBox box && box.Max.Y > box.Min.Y)
            {
                top = float.MinValue;
                for (int c = 0; c < 8; c++)
                {
                    var corner = new Vector3((c & 1) == 0 ? box.Min.X : box.Max.X, (c & 2) == 0 ? box.Min.Y : box.Max.Y, (c & 4) == 0 ? box.Min.Z : box.Max.Z);
                    top = MathF.Max(top, Vector3.Transform(Vector3.Transform(corner, world), toLocal).Y);
                }
            }

            result.Add(new NgpLadderSource(part.Name, part.ModelName, foot, Vector3.Normalize(forward), top));
        }

        return result;
    }

    private void AddNgpLadders(MapContainer map, NgpMap data, List<NgpLadderSource> sources)
    {
        var ngp = data.Ngp;
        var lines = new List<string>();
        bool anyError = false;
        int fallback = PickedNgpMesh(data)?.Index ?? -1;

        foreach (var s in sources)
        {
            NgpStripResult r;
            try
            {
                r = NgpStripEdit.WithLadder(ngp, s.Foot, s.Forward, s.Top, fallback);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Edit_Commit_Failed", data.FileName), e);
                r = new NgpStripResult { Problem = "Failed" };
            }

            if (r.Ngp == null)
            {
                anyError = true;
                lines.Add(LOC.Get($"MAP_Navigation_Ngp_Strip_Problem_{r.Problem}", s.Name));
                continue;
            }

            ngp = r.Ngp;
            string Joined(bool joined, int mesh) => joined ? LOC.Get("MAP_Navigation_Ngp_Ladder_Joined", mesh) : LOC.Get("MAP_Navigation_Ngp_Ladder_Unjoined");
            lines.Add(LOC.Get("MAP_Navigation_Ngp_Ladder_Added", s.Name, r.Mesh, (r.Top - r.Bottom).ToString("0.00"),
                Joined(r.BottomJoined, r.BottomMesh), Joined(r.TopJoined, r.TopMesh)));

            if (!r.BottomJoined || !r.TopJoined)
                anyError = true;
            if (r.MinSlope < 80)
                lines.Add(LOC.Get("MAP_Navigation_Ngp_Ladder_Leans", s.Name, r.MinSlope.ToString("0")));
            foreach (var note in r.Notes)
                Smithbox.Log(this, $"Ladder {s.Name}: {note}");
        }

        ExecuteNgpStrip(map, data, ngp, string.Join("\n", lines), anyError);
    }

    #endregion

    #region Drop

    private void DisplayNgpDrop(MapContainer map, NgpMap data)
    {
        // Drop
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Drop_Header"), LOC.Get("MAP_Navigation_Ngp_Drop_Header_TT"));

        var ledge = NgpLedge(data);
        if (ledge is var (mesh, a, b))
        {
            var m = data.Meshes[mesh].Mesh;
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Drop_Ledge", mesh, data.Meshes[mesh].Label, Vector3.Distance(m.Vertices[a], m.Vertices[b]).ToString("0.00")));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Drop_Pick"));
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationNgpDropRun", ref NgpDropRun, 0.1f, 0.5f, $"{LOC.Get("MAP_Navigation_Ngp_Drop_Run")} %.2f m"))
            NgpDropRun = Math.Clamp(NgpDropRun, 0f, 5f);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Drop_Run_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationNgpDropMax", ref NgpDropMax, 1f, 5f, $"{LOC.Get("MAP_Navigation_Ngp_Drop_Max")} %.1f m"))
            NgpDropMax = Math.Clamp(NgpDropMax, 1f, 200f);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Drop_Max_TT"));

        GUI.ConditionalMultiButtonInput("navigationNgpDropActions",
            "navigationNgpDropAdd", LOC.Get("MAP_Navigation_Ngp_Drop_Add"), LOC.Get("MAP_Navigation_Ngp_Drop_Add_TT"),
            () => AddNgpDrop(map, data, ledge.Value), ledge != null && !HasNgpVertexEdits);
    }

    /// <summary>The open edge (not a gate's) of the picked navmesh nearest the picked point: the ledge a drop starts from.</summary>
    private (int Mesh, int A, int B)? NgpLedge(NgpMap data)
    {
        if (data == null || NgpContainer == null || NgpContainer.Name != data.MapId || PickedNgpMesh(data) is not NgpMesh picked
            || !Matrix4x4.Invert(data.World, out var toLocal))
            return null;

        var point = Vector3.Transform(NgpPickedPoint, toLocal);
        return NgpStripEdit.NearestOpenEdge(data.Ngp, picked.Index, point) is var (a, b) ? (picked.Index, a, b) : null;
    }

    private void AddNgpDrop(MapContainer map, NgpMap data, (int Mesh, int A, int B) ledge)
    {
        NgpStripResult r;
        try
        {
            r = NgpStripEdit.WithDrop(data.Ngp, ledge.Mesh, ledge.A, ledge.B, NgpDropRun, 0.5f, NgpDropMax);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Edit_Commit_Failed", data.FileName), e);
            r = new NgpStripResult { Problem = "Failed" };
        }

        if (r.Ngp == null)
        {
            SetNgpMessage(LOC.Get($"MAP_Navigation_Ngp_Strip_Problem_{r.Problem}", data.Meshes[ledge.Mesh].Label), true);
            return;
        }

        var text = LOC.Get("MAP_Navigation_Ngp_Drop_Added", ledge.Mesh, (r.Top - r.Bottom).ToString("0.00"), r.BottomMesh, r.StripFaces);
        if (r.BottomMesh != ledge.Mesh)
            text += " " + LOC.Get("MAP_Navigation_Ngp_Drop_Gate", r.BottomMesh);
        ExecuteNgpStrip(map, data, r.Ngp, text, false);
    }

    #endregion

    /// <summary>Records a strip edit as one undoable action; picks are cleared, since the cut renumbers faces.</summary>
    private void ExecuteNgpStrip(MapContainer map, NgpMap data, NGP after, string message, bool warn)
    {
        if (ReferenceEquals(after, data.Ngp))
        {
            SetNgpMessage(message, true);
            return;
        }

        if (NgpContainer != map)
            RebuildNgpOverlay(map);

        View.ViewportActionManager.ExecuteAction(new NavigationNgpEditAction(map, Ngp, data.Ngp, after, OnNgpEdited));
        SetNgpPicked(-1, -1, default);
        SelectNgpGate(-1);

        SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Strip_Done", message, data.Ngp.Gates.Count, after.Gates.Count), warn);
        Smithbox.Log(this, NgpMessage);
    }
}
