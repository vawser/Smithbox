using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The markers of the loaded model's EDGE file in the Model Editor: every entry, coloured by type, the EDGE Editor's
/// selected entry larger, in the selected colour and over the geometry. Built with the Map Editor's rules
/// (<see cref="EdgeBank.GetMarkers"/>, <see cref="MsbEntity.ApplyEdgePoints"/>), rebuilt whenever the file's
/// contents, the selection or the display settings change (so edits in the property grid show at once), and placed
/// each frame through the EDGE's space and the model's display rotation.
/// </summary>
public sealed class ModelEdgeMarkers : IDisposable
{
    private readonly List<(DebugPrimitiveRenderableProxy Mesh, Matrix4x4 Local)> Meshes = new();

    private RenderScene Scene;
    private int BuiltFor;
    private bool Built;

    /// <summary>
    /// Keeps the markers in line with a file. A null file (or a hidden filter, which the scene's draw filter handles)
    /// leaves none.
    /// </summary>
    public void Update(RenderScene scene, EDGE edge, int selected, Matrix4x4 space)
    {
        if (scene != Scene)
        {
            Clear();
            Scene = scene;
        }

        if (scene == null || edge == null)
        {
            Clear();
            return;
        }

        var key = Fingerprint(edge, selected);

        if (!Built || key != BuiltFor)
        {
            Rebuild(scene, edge, selected);
            BuiltFor = key;
            Built = true;
        }

        foreach (var (mesh, local) in Meshes)
        {
            mesh.World = local * space;
        }
    }

    private void Rebuild(RenderScene scene, EDGE edge, int selected)
    {
        Clear();

        var markers = EdgeBank.GetMarkers(edge, selected);
        var pointSize = Math.Max(CFG.Current.Viewport_Edge_Point_Size, 0.01f);
        var radius = Math.Max(CFG.Current.Viewport_Edge_Segment_Radius, 0.005f);

        foreach (var marker in markers.Markers)
        {
            var grow = marker.Selected ? 1.4f : 1.0f;
            var color = marker.Selected ? CFG.Current.Viewport_Edge_Selected_Color : MsbEntity.GetEdgeColor(marker);

            var (cylinder, local) = marker.Kind switch
            {
                EdgeMarkerKind.Point => (false, Matrix4x4.CreateScale(pointSize * grow) * Matrix4x4.CreateTranslation(marker.A)),
                EdgeMarkerKind.Pull => (false, Matrix4x4.CreateScale(pointSize * 0.6f * grow) * Matrix4x4.CreateTranslation(marker.A)),
                EdgeMarkerKind.Segment => (true, MsbEntity.GetEdgeSegmentTransform(marker.A, marker.B, radius * grow)),
                _ => (true, MsbEntity.GetEdgeSegmentTransform(marker.A, marker.B, radius * 0.4f * grow))
            };

            Add(RenderableHelper.GetEdgeMarkerWireProxy(scene, cylinder, color, marker.Selected), local);
            Add(RenderableHelper.GetEdgeMarkerFillProxy(scene, cylinder, color), local);
        }
    }

    private void Add(DebugPrimitiveRenderableProxy mesh, Matrix4x4 local)
    {
        mesh.DrawFilter = RenderFilter.EdgePoint;
        Meshes.Add((mesh, local));
    }

    public void Clear()
    {
        foreach (var (mesh, _) in Meshes)
        {
            mesh.Dispose();
        }

        Meshes.Clear();
        Built = false;
    }

    public void Dispose() => Clear();

    /// <summary>
    /// The file's contents with the selection and the display settings, which the markers are built from. Cheap
    /// enough per frame for the game's object files (at most a few hundred entries).
    /// </summary>
    public static int Fingerprint(EDGE edge, int selected)
    {
        var hash = new HashCode();

        hash.Add(edge.ID);
        hash.Add(edge.Edges.Count);
        hash.Add(selected);
        hash.Add(CFG.Current.Viewport_Edge_Point_Size);
        hash.Add(CFG.Current.Viewport_Edge_Segment_Radius);
        hash.Add(CFG.Current.Viewport_Edge_Grapple_Color);
        hash.Add(CFG.Current.Viewport_Edge_Pull_Color);
        hash.Add(CFG.Current.Viewport_Edge_Hang_Color);
        hash.Add(CFG.Current.Viewport_Edge_Hug_Color);
        hash.Add(CFG.Current.Viewport_Edge_Selected_Color);

        foreach (var entry in edge.Edges)
        {
            hash.Add(FileFingerprint(entry));
        }

        return hash.ToHashCode();
    }

    /// <summary>The contents of one entry.</summary>
    public static int FileFingerprint(EDGE.Edge entry)
    {
        return HashCode.Combine(entry.Type, entry.V1, entry.V2, entry.V3, entry.Unk2C, entry.Unk30, entry.VariationID, entry.Unk36);
    }

    /// <summary>The contents of a file alone, for caches that don't depend on the selection or display.</summary>
    public static int ContentFingerprint(EDGE edge)
    {
        var hash = new HashCode();

        hash.Add(edge.ID);
        hash.Add(edge.Edges.Count);

        foreach (var entry in edge.Edges)
        {
            hash.Add(FileFingerprint(entry));
        }

        return hash.ToHashCode();
    }
}
