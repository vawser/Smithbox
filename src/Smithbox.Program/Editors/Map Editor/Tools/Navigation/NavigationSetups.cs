using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The jump and drop links of DS3, BB and SDT: user edge setups (hkaiUserEdgeSetupArray), which the game turns into user
// edges when it loads the navmesh, as their meshes have none baked in. Kept free of the rest of Smithbox, so
// Tools/NavSurvey can link it. See Documentation/Claude/Navigation Editor Other Games Plan.md, N5.
//
// How the game's setups sit (NavSurvey setups; DS3 650, SDT 1,288, BB 62 in the surveyed maps):
// - every BB and SDT setup and 452 of DS3's 650 are in the mesh's own (local) space; the rest of DS3's are world space;
// - box A is on a ledge: its centre near a boundary edge of the mesh (median 0.34 m inside it in DS3, 0.25 m in BB,
//   0.92 m in SDT), at the edge's height, its x axis along the edge;
// - box B is where the link lands, at the height of the surface there (median 0.06 m above), on the same mesh or another
//   (40 %), turned as box A is (DS3 and BB: every one);
// - A to B links are drops (cost 0, user data 0); links both ways are 0 in DS3 and BB, and in SDT cost 0.9 times the
//   distance between the boxes each way, with user data 1 (628 of 628);
// - world up (0, 1, 0), force align on.

/// <summary>What adding or removing a setup did, or why it was refused.</summary>
public sealed class NavSetupResult
{
    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; set; }

    public NavFileChange File { get; set; }

    public bool Ok => Error == null;
}

public static class NavSetups
{
    /// <summary>The setups of a navmesh file, or null if it has no setup array.</summary>
    public static IHavokNavUserEdgeSetups Of(Havok.HavokDocument document) => document?.ObjectsOf<IHavokNavUserEdgeSetups>().FirstOrDefault();

    /// <summary>How far inside its edge box A's centre goes: the game's median.</summary>
    public static float Inset(NavGameProfile profile) => profile.Game == "SDT" ? 0.92f : 0.3f;

    /// <summary>The half extents new boxes get: the game's median.</summary>
    public static Vector4 HalfExtents(NavGameProfile profile) => profile.Game == "SDT" ? new Vector4(0.786f, 0.929f, 0.821f, 0) : new Vector4(0.5f, 0.6f, 0.5f, 0);

    /// <summary>
    /// A setup from a face of a mesh to a point, both in the mesh's space: box A on the face's boundary edge nearest the
    /// start point, a little inside it and along it, box B at the target turned the same way. Null when the face has no
    /// boundary edge (a link starts at a ledge).
    /// </summary>
    public static HavokUserEdgeSetup Make(IHavokNavMeshEdit mesh, int startFace, Vector3 start, Vector3 target, bool bothWays, NavGameProfile profile)
    {
        if (startFace < 0 || startFace >= mesh.m_faces.Count)
            return null;

        var face = mesh.m_faces[startFace];
        var centre = NavigationOverlayBuilder.FaceCentre(mesh, startFace);
        float best = float.MaxValue;
        Vector3 onEdge = default, along = default;

        for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
        {
            var edge = mesh.m_edges[e];
            if (edge.m_oppositeFace != 0xFFFFFFFF)
                continue;

            var a = V(mesh, edge.m_a);
            var b = V(mesh, edge.m_b);
            var ab = b - a;
            if (ab.LengthSquared() < 1e-8f)
                continue;

            float t = Math.Clamp(Vector3.Dot(start - a, ab) / ab.LengthSquared(), 0, 1);
            var q = a + ab * t;
            float d = Vector3.Distance(start, q);
            if (d < best)
            {
                best = d;
                onEdge = q;
                along = Vector3.Normalize(new Vector3(ab.X, 0, ab.Z));
            }
        }

        if (best == float.MaxValue || along == Vector3.Zero)
            return null;

        // Into the face from the edge, in XZ
        var inward = new Vector3(-along.Z, 0, along.X);
        if (Vector3.Dot(inward, new Vector3(centre.X - onEdge.X, 0, centre.Z - onEdge.Z)) < 0)
            inward = -inward;

        var a0 = onEdge + inward * Inset(profile);
        var rotation = Turn(along);
        var half = HalfExtents(profile);

        // SDT's links both ways cost 0.9 times their length, each way
        bool sdtBoth = bothWays && profile.Game == "SDT";
        float cost = sdtBoth ? 0.9f * Vector3.Distance(a0, target) : 0;

        return new HavokUserEdgeSetup(
            rotation * Matrix4x4.CreateTranslation(a0), half,
            rotation * Matrix4x4.CreateTranslation(target), half,
            sdtBoth ? 1u : 0u, sdtBoth ? 1u : 0u,
            cost, cost,
            new Vector4(0, 1, 0, 0), new Vector4(0, 1, 0, 0),
            bothWays ? HavokUserEdgeDirection.Bidirectional : HavokUserEdgeDirection.AToB,
            WorldSpace: false,
            ForceAlign: true);
    }

    /// <summary>A rotation whose x axis runs along a horizontal direction, y up.</summary>
    private static Matrix4x4 Turn(Vector3 along)
    {
        var z = Vector3.Cross(along, Vector3.UnitY);
        return new Matrix4x4(
            along.X, along.Y, along.Z, 0,
            0, 1, 0, 0,
            z.X, z.Y, z.Z, 0,
            0, 0, 0, 1);
    }

    private static Vector3 V(IHavokNavMeshEdit mesh, int i) => new(mesh.m_vertices[i].X, mesh.m_vertices[i].Y, mesh.m_vertices[i].Z);

    /// <summary>Box A's and B's centres of a setup, in its mesh's space (local) or the map's (world).</summary>
    public static (Vector3 A, Vector3 B) Centres(HavokUserEdgeSetup setup)
        => (setup.TransformA.Translation, setup.TransformB.Translation);

    /// <summary>The name and class name the game's files give the setup array, the last of the root's variants.</summary>
    public const string ArrayName = "hkaiUserEdgeSetupArray";

    /// <summary>
    /// A copy of a file with a setup added. A file without a setup array gets one, as the game's files with setups have
    /// it: the root's last variant, named after its class.
    /// </summary>
    public static NavSetupResult Add(NavmeshFile file, HavokUserEdgeSetup setup)
    {
        var result = new NavSetupResult();
        if (setup == null)
        {
            result.Error = "MAP_Navigation_Setup_Error_No_Ledge";
            return result;
        }

        var copy = NavModelEdit.Clone(file.Document);
        var setups = Of(copy) ?? (copy.Root as IHavokRootContainerEdit)?.AddUserEdgeSetups(ArrayName, ArrayName);
        if (setups == null)
        {
            result.Error = "MAP_Navigation_Setup_Error_No_Array";
            return result;
        }

        setups.Add(setup);
        result.File = new NavFileChange(file.Name, file.Document, NavModelEdit.Clone(copy));
        return result;
    }

    /// <summary>A copy of a file without one of its setups.</summary>
    public static NavSetupResult Remove(NavmeshFile file, int index)
    {
        var result = new NavSetupResult();
        var copy = NavModelEdit.Clone(file.Document);
        var setups = Of(copy);
        if (setups == null || index < 0 || index >= setups.Setups.Count)
        {
            result.Error = "MAP_Navigation_Setup_Error_No_Array";
            return result;
        }

        setups.RemoveAt(index);

        // No game file keeps an empty setup array
        if (setups.Setups.Count == 0 && copy.Root is IHavokRootContainerEdit root)
            root.KeepOnly(v => !ReferenceEquals(v, setups));

        result.File = new NavFileChange(file.Name, file.Document, NavModelEdit.Clone(copy));
        return result;
    }
}
