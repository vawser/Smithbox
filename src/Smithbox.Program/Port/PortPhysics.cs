using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Reflection;
using H14 = Havok.hk2014;
using H18 = Havok.hk2018;

namespace StudioCore.Port;

/// <summary>
/// DS3 object physics (hk2014 hknp) as ER asset physics (hk2018 hknp), Smithbox Port Plan.md §4.6. A file's bodies are
/// rebuilt on fresh copies of an ER template body, material and motion properties (the AEG099_200 pot's broken state:
/// ER's defaults for quality, motion ID, drag), with DS3's motion type, mass, transform, friction, restitution and
/// motion settings. Polytopes and spheres are copied field by field (flags by name: the two SDKs number them
/// differently), with the two hk2018 additions: connectivity rebuilt from the faces, and the max allowed penetration
/// from the shape's size (Ds3ErPortSurvey erpolytopes). A single static mesh body is rebuilt with the Collision
/// Editor's builder on an ER static asset template instead.
/// </summary>
public sealed class PortPhysics
{
    /// <summary>The template binders, from the ER game: the pot (dynamic, intact + broken) and a static mesh asset.</summary>
    public const string DynamicTemplateBinder = "/asset/aeg/aeg099/aeg099_200_l.geomhkxbnd.dcx";
    public const string StaticTemplateBinder = "/asset/aeg/aeg027/aeg027_000_l.geomhkxbnd.dcx";

    private readonly byte[] DynamicTemplate;
    private readonly byte[] StaticTemplate;
    private readonly HitMaterialConfig HitConfig;
    private readonly PortLog Log;

    /// <summary>The pot's collision binder, the layout ported binders copy (names, IDs, flags).</summary>
    public BND4 BinderTemplate { get; }

    public PortPhysics(PortGame output, HitMaterialConfig hitConfig, PortLog log)
    {
        HitConfig = hitConfig;
        Log = log;
        BinderTemplate = BND4.Read(output.ReadVanilla(DynamicTemplateBinder) ?? throw new InvalidOperationException($"{DynamicTemplateBinder} not found in the ER game"));
        DynamicTemplate = Bytes(BinderTemplate.Files.First(f => f.Name.EndsWith("_1_L.hkx", StringComparison.OrdinalIgnoreCase)));
        var staticBnd = BND4.Read(output.ReadVanilla(StaticTemplateBinder) ?? throw new InvalidOperationException($"{StaticTemplateBinder} not found in the ER game"));
        StaticTemplate = Bytes(staticBnd.Files.First(f => f.Name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase)));
    }

    public static byte[] Bytes(BinderFile f)
    {
        var b = f.Bytes.ToArray();
        return DCX.Is(b) ? DCX.Decompress(b).ToArray() : b;
    }

    /// <summary>
    /// One DS3 physics file as an ER one, or null if nothing could be built. Body names go through rename (each body
    /// drives the FLVER bone of its name); a file with one body (the intact state) names it singleName instead.
    /// </summary>
    public byte[] Convert(byte[] ds3Hkx, Func<string, string> rename, string singleName, string label, PortObjectStats stats, List<Vector3> renderTriangles = null)
    {
        var doc = Havok.Files.HavokFile.Read(ds3Hkx, Havok.HavokProfile.DS3);
        var system = doc.Objects.OfType<H14.hknpPhysicsSystemData>().FirstOrDefault();
        if (system == null) { Log.Warn($"{label}: no physics system"); return null; }
        var bodies = system.m_bodyCinfos.Where(b => b != null).ToList();
        if (bodies.Count == 0) { Log.Warn($"{label}: no bodies"); return null; }

        string NameOf(H14.hknpBodyCinfo b) => bodies.Count == 1 && singleName != null ? singleName : rename(b.m_name ?? "");

        // A single static mesh body: the Collision Editor's builder on the static asset template
        if (bodies.Count == 1 && bodies[0].m_shape is IHavokCompressedMesh && !IsMoving(system, bodies[0]))
            return StaticMesh(bodies[0], NameOf(bodies[0]), label, stats, renderTriangles);

        var target = Havok.Files.HavokFile.Read(DynamicTemplate, Havok.HavokProfile.ER);
        var targetSystem = target.Objects.OfType<H18.hknpPhysicsSystemData>().First();
        var bodyTemplate = targetSystem.m_bodyCinfos[0];
        var materialTemplate = targetSystem.m_materials[0];
        var motionTemplate = targetSystem.m_motionProperties[0];
        targetSystem.m_bodyCinfos.Clear();
        targetSystem.m_materials.Clear();
        targetSystem.m_motionProperties.Clear();
        targetSystem.m_constraintCinfos.Clear();

        foreach (var b in bodies)
        {
            var shape = Shape(b.m_shape, out var why);
            if (shape == null)
            {
                Log.Warn($"{label}: body '{b.m_name}' ({b.m_shape?.GetType().Name}) left out: {why}");
                stats.Note($"physics body left out: {b.m_shape?.GetType().Name} ({why})");
                continue;
            }

            var index = targetSystem.m_bodyCinfos.Count;
            var motion = b.m_motionId < (uint)system.m_motionCinfos.Count ? system.m_motionCinfos[(int)b.m_motionId] : null;
            var dynamic = motion != null && motion.m_inverseMass > 0;

            var body = Clone(bodyTemplate);
            body.m_shape = shape;
            body.m_name = NameOf(b);
            body.m_motionType = (byte)(motion == null ? 0 : dynamic ? 2 : 1);
            body.m_mass = dynamic ? 1 / motion.m_inverseMass : 0;
            body.m_position = b.m_position;
            body.m_orientation = b.m_orientation;
            body.m_materialId = (ushort)index;
            body.m_motionPropertiesId = (ushort)index;
            targetSystem.m_bodyCinfos.Add(body);
            stats.Note($"physics body: {(motion == null ? "static" : dynamic ? "dynamic" : "keyframed")} {shape.GetType().Name}");

            var material = Clone(materialTemplate);
            if (b.m_materialId < system.m_materials.Count)
            {
                var m = system.m_materials[b.m_materialId];
                material.m_dynamicFriction = m.m_dynamicFriction;
                material.m_staticFriction = m.m_staticFriction;
                material.m_restitution = m.m_restitution;
            }
            targetSystem.m_materials.Add(material);

            var props = Clone(motionTemplate);
            if (motion != null && motion.m_motionPropertiesId < system.m_motionProperties.Count)
            {
                var p = system.m_motionProperties[motion.m_motionPropertiesId];
                props.m_gravityFactor = p.m_gravityFactor;
                props.m_timeFactor = p.m_timeFactor;
                props.m_maxLinearSpeed = p.m_maxLinearSpeed;
                props.m_maxAngularSpeed = p.m_maxAngularSpeed;
                props.m_linearDamping = p.m_linearDamping;
                props.m_angularDamping = p.m_angularDamping;
            }
            targetSystem.m_motionProperties.Add(props);
        }
        if (system.m_constraintCinfos.Count > 0)
        {
            Log.Warn($"{label}: {system.m_constraintCinfos.Count} constraints left out");
            stats.Note("physics constraints left out");
        }
        if (targetSystem.m_bodyCinfos.Count == 0) return null;

        var written = Havok.Files.HavokFile.Write(target);
        Check(written, targetSystem.m_bodyCinfos.Select(b => b.m_name).ToList(), label);
        return written;
    }

    private static bool IsMoving(H14.hknpPhysicsSystemData system, H14.hknpBodyCinfo b) => b.m_motionId < (uint)system.m_motionCinfos.Count;

    /// <summary>A static triangle mesh body, rebuilt in ER's class set with the hit material remap.</summary>
    private byte[] StaticMesh(H14.hknpBodyCinfo body, string name, string label, PortObjectStats stats, List<Vector3> renderTriangles)
    {
        var mesh = (IHavokCompressedMesh)body.m_shape;
        var transform = CollisionGeometry.BodyTransform(body);
        var byKey = CollisionEditorShapes.GetMaterialsByKey(body.m_shape);
        var shift = 32 - mesh.BitsPerKey;
        var vertices = new List<Vector3>();
        var materials = new List<uint>();
        foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(mesh))
        {
            vertices.Add(Vector3.Transform(a, transform));
            vertices.Add(Vector3.Transform(b, transform));
            vertices.Add(Vector3.Transform(c, transform));
            materials.Add(HitConfig.Map(byKey.TryGetValue(key >> shift, out var dm) ? dm : 0, out _));
        }
        // Some DS3 object meshes (small props, a few primitives) don't decode in the hk2014 classes (their shared
        // vertex index reads as garbage: Ds3ErPortSurvey objmesh): their collision is built from the render mesh
        if (materials.Count == 0 && renderTriangles is { Count: > 0 })
        {
            var surface = byKey.Values.GroupBy(v => v).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            vertices = renderTriangles;
            materials = Enumerable.Repeat(HitConfig.Map(surface, out _), renderTriangles.Count / 3).ToList();
            Log.Line($"  {label}: the DS3 mesh doesn't decode; collision built from the render mesh ({materials.Count} triangles)");
            stats.Note("physics body: static mesh from the render mesh (DS3 mesh doesn't decode)");
        }
        if (materials.Count == 0) { Log.Warn($"{label}: static mesh with no triangles"); return null; }

        var template = Havok.Files.HavokFile.Read(StaticTemplate, Havok.HavokProfile.ER);
        if (!CollisionEditorShapes.MakeNewCollision(template, "000", vertices, Enumerable.Range(0, vertices.Count).ToList(), materials))
        {
            Log.Warn($"{label}: the Collision Editor's builder refused the mesh ({materials.Count} triangles)");
            return null;
        }
        foreach (var b in template.Objects.OfType<H18.hknpBodyCinfo>()) b.m_name = name;
        stats.Note("physics body: static mesh (rebuilt)");
        var written = Havok.Files.HavokFile.Write(template);
        Check(written, [name], label);
        return written;
    }

    /// <summary>Read back: the bodies, by name, and every shape decodes.</summary>
    private void Check(byte[] written, List<string> names, string label)
    {
        var back = Havok.Files.HavokFile.Read(written, Havok.HavokProfile.ER);
        var bodies = back.Objects.OfType<H18.hknpBodyCinfo>().ToList();
        Log.Check(bodies.Select(b => b.m_name).SequenceEqual(names), $"{label}: physics read back with its {names.Count} bodies");
        Log.Check(bodies.All(b => b.m_shape != null), $"{label}: every body has a shape");
    }

    // ---- shapes ---------------------------------------------------------------------------------------------------

    private static H18.hknpShape Shape(H14.hknpShape shape, out string why)
    {
        why = null;
        switch (shape)
        {
            case H14.hknpConvexPolytopeShape p:
                if (p.m_vertices.Count > 255) { why = $"{p.m_vertices.Count} vertices (ER's face indices are bytes)"; return null; }
                return Polytope(p);
            case H14.hknpSphereShape s:
                return Sphere(s);
            case IHavokCompressedMesh:
                why = "a triangle mesh in a multi-body or moving file";
                return null;
            default:
                why = "no converter for this shape";
                return null;
        }
    }

    private static H18.hknpConvexPolytopeShape Polytope(H14.hknpConvexPolytopeShape p)
    {
        var s = new H18.hknpConvexPolytopeShape
        {
            m_flags = Flags(p.m_flags),
            m_type = H18.hknpShapeType.Enum.CONVEX_POLYTOPE,
            m_numShapeKeyBits = p.m_numShapeKeyBits,
            m_dispatchType = H18.hknpCollisionDispatchType.Enum.CONVEX,
            m_convexRadius = p.m_convexRadius,
            m_userData = p.m_userData,
            m_properties = Properties(p.m_properties),
            m_vertices = p.m_vertices.ToList(),
            m_planes = p.m_planes.ToList(),
            m_faces = p.m_faces.Select(f => new H18.hknpConvexPolytopeShape.Face { m_firstIndex = f.m_firstIndex, m_numIndices = f.m_numIndices, m_minHalfAngle = f.m_minHalfAngle }).ToList(),
            m_indices = p.m_indices.ToList()
        };

        var faces = p.m_faces.Select(f => (f.m_firstIndex, f.m_numIndices)).ToList();
        var (vertexEdges, faceLinks) = PolytopeConnectivity.Build(faces, p.m_indices, p.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList());
        s.m_connectivity = new H18.hknpConvexPolytopeShape.Connectivity
        {
            m_vertexEdges = vertexEdges.Select(e => new H18.hknpConvexPolytopeShape.Connectivity.Edge { m_faceIndex = e.Face, m_edgeIndex = e.Edge }).ToList(),
            m_faceLinks = faceLinks.Select(e => new H18.hknpConvexPolytopeShape.Connectivity.Edge { m_faceIndex = e.Face, m_edgeIndex = e.Edge }).ToList()
        };
        s.m_maxAllowedPenetration = MaxAllowedPenetration(p.m_vertices, p.m_planes, p.m_convexRadius);
        return s;
    }

    private static H18.hknpSphereShape Sphere(H14.hknpSphereShape p) => new()
    {
        m_flags = Flags(p.m_flags),
        m_type = H18.hknpShapeType.Enum.SPHERE,
        m_numShapeKeyBits = p.m_numShapeKeyBits,
        m_dispatchType = H18.hknpCollisionDispatchType.Enum.CONVEX,
        m_convexRadius = p.m_convexRadius,
        m_userData = p.m_userData,
        m_properties = Properties(p.m_properties),
        m_vertices = p.m_vertices.ToList(),
        m_maxAllowedPenetration = p.m_convexRadius / 3
    };

    /// <summary>
    /// ER's polytopes: penetration / inscribed radius has a median of 0.33 (Ds3ErPortSurvey erpolytopes; boxes with a
    /// convex radius come out at (inscribed + convex radius) / 3, e.g. 0.065 + 0.01 -> 0.0248). Inscribed radius: the
    /// centroid's distance to its nearest plane.
    /// </summary>
    private static float MaxAllowedPenetration(List<Vector4> vertices, List<Vector4> planes, float convexRadius)
    {
        var centre = vertices.Aggregate(Vector3.Zero, (a, v) => a + new Vector3(v.X, v.Y, v.Z)) / Math.Max(1, vertices.Count);
        var inner = planes.Count == 0 ? 0 : planes.Min(p => -(p.X * centre.X + p.Y * centre.Y + p.Z * centre.Z + p.W));
        if (inner <= 0)
        {
            var min = vertices.Aggregate(new Vector3(float.MaxValue), (a, v) => Vector3.Min(a, new Vector3(v.X, v.Y, v.Z)));
            var max = vertices.Aggregate(new Vector3(float.MinValue), (a, v) => Vector3.Max(a, new Vector3(v.X, v.Y, v.Z)));
            inner = MathF.Min(max.X - min.X, MathF.Min(max.Y - min.Y, max.Z - min.Z)) / 2;
        }
        return (inner + convexRadius) / 3;
    }

    /// <summary>The flag set by name: hk2018 renumbered the bits above USE_SMALL_FACE_INDICES.</summary>
    private static H18.hknpShape.FlagsEnum Flags(H14.hknpShape.FlagsEnum flags)
    {
        H18.hknpShape.FlagsEnum result = 0;
        foreach (H14.hknpShape.FlagsEnum f in Enum.GetValues(typeof(H14.hknpShape.FlagsEnum)))
            if (flags.HasFlag(f) && Enum.TryParse<H18.hknpShape.FlagsEnum>(f.ToString(), out var g))
                result |= g;
        return result;
    }

    /// <summary>The shape's mass properties (the only property DS3 objects carry), copied into hk2018's classes.</summary>
    private static H18.hkRefCountedProperties Properties(H14.hkRefCountedProperties properties)
    {
        if (properties == null) return null;
        var result = new H18.hkRefCountedProperties();
        foreach (var e in properties.m_entries)
        {
            if (e.m_object is not H14.hknpShapeMassProperties mp) continue;
            var copy = new H18.hknpShapeMassProperties();
            var a = mp.m_compressedMassProperties;
            var b = copy.m_compressedMassProperties;
            Array.Copy(a.m_centerOfMass, b.m_centerOfMass.m_values, 4);
            Array.Copy(a.m_inertia, b.m_inertia.m_values, 4);
            Array.Copy(a.m_majorAxisSpace, b.m_majorAxisSpace, 4);
            b.m_mass = a.m_mass;
            b.m_volume = a.m_volume;
            result.m_entries.Add(new H18.hkRefCountedProperties.Entry { m_object = copy, m_key = e.m_key, m_flags = e.m_flags });
        }
        return result.m_entries.Count > 0 ? result : null;
    }

    private static readonly MethodInfo MemberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>A shallow copy of a template object (its nested settings objects are shared, as Havok allows).</summary>
    private static T Clone<T>(T o) => (T)MemberwiseCloneMethod.Invoke(o, null);
}

/// <summary>
/// An hk2018 polytope's connectivity from its faces: for each face edge (face f, edge e: index e to e+1), the face and
/// edge on the other side; for each vertex, a face edge that starts at it. Vertices no face uses (the padding to a
/// multiple of 4) take the edge of the vertex at the same position, as ER's own files do. Checked against ER's stored
/// connectivity (Ds3ErPortSurvey erpolytopes: face links equal in every file).
/// </summary>
public static class PolytopeConnectivity
{
    public static (List<(ushort Face, byte Edge)> VertexEdges, List<(ushort Face, byte Edge)> FaceLinks) Build(
        List<(ushort First, byte Count)> faces, IList<byte> indices, List<Vector3> vertices)
    {
        var byEdge = new Dictionary<(int, int), (ushort, byte)>();
        for (int f = 0; f < faces.Count; f++)
            for (int e = 0; e < faces[f].Count; e++)
                byEdge[(indices[faces[f].First + e], indices[faces[f].First + (e + 1) % faces[f].Count])] = ((ushort)f, (byte)e);

        var links = new List<(ushort, byte)>();
        for (int f = 0; f < faces.Count; f++)
            for (int e = 0; e < faces[f].Count; e++)
            {
                int a = indices[faces[f].First + e], b = indices[faces[f].First + (e + 1) % faces[f].Count];
                links.Add(byEdge.TryGetValue((b, a), out var other) ? other : ((ushort)f, (byte)e));
            }

        var vertexEdges = new (ushort, byte)?[vertices.Count];
        for (int f = 0; f < faces.Count; f++)
            for (int e = 0; e < faces[f].Count; e++)
            {
                int a = indices[faces[f].First + e];
                if (a < vertices.Count && vertexEdges[a] == null) vertexEdges[a] = ((ushort)f, (byte)e);
            }
        for (int i = 0; i < vertices.Count; i++)
            if (vertexEdges[i] == null)
            {
                var twin = Enumerable.Range(0, vertices.Count).FirstOrDefault(j => vertexEdges[j] != null && vertices[j] == vertices[i], -1);
                vertexEdges[i] = twin >= 0 ? vertexEdges[twin] : ((ushort)0, (byte)0);
            }
        return (vertexEdges.Select(v => v.Value).ToList(), links);
    }
}

/// <summary>Counts for the objects stage's report.</summary>
public sealed class PortObjectStats
{
    public readonly SortedDictionary<string, int> Notes = new();
    public void Note(string text) => Notes[text] = Notes.GetValueOrDefault(text) + 1;
}
