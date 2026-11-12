#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Havok.Builders;

/// <summary>
/// The older games' (hkp: DES, DS1, DS1R, DS2S) map collision mesh, read and rebuilt for the Collision Editor: an
/// hkpMoppBvTreeShape around a FromSoftware CustomParamStorageExtendedMeshShape. Every one of these games keeps one
/// mesh storage per collision material, each with its own subpart and material parameter (MoppSurvey fields), and a
/// triangle's shape key is its storage in the top bits (m_numBitsForSubpartIndex) and its index below.
///
/// A rebuild keeps every storage, its triangles and its material parameter as they are, and changes only the vertex
/// positions, so the shape keys stay the same. The shape, its storages and its code are copied (shallow, through
/// reflection, as the class sets name their classes differently), given the new vertices, the new bounds and a MOPP
/// compiled by <see cref="MoppCompiler"/>; the old objects are left as they were, for undo.
/// </summary>
public static class HkpMeshShapeRebuild
{
    /// <summary>A storage-extended mesh: each storage's vertices and triangles, and the key layout.</summary>
    public sealed class Mesh
    {
        public List<Vector3[]> StorageVertices = new();
        public List<List<(int A, int B, int C)>> StorageTriangles = new();
        public List<uint> StorageMaterials = new();
        public int KeyShift;

        public uint Key(int storage, int triangle) => (uint)storage << KeyShift | (uint)triangle;

        public (int Storage, int Triangle) FromKey(uint key) => ((int)(key >> KeyShift), (int)(key & ((1u << KeyShift) - 1)));
    }

    /// <summary>Whether a shape is a MOPP over a FromSoftware storage-extended mesh, the shape this rebuilds.</summary>
    public static bool IsSupported(object shape)
    {
        if (shape?.GetType().Name != "hkpMoppBvTreeShape")
            return false;

        var child = Get(Get(shape, "m_child"), "m_childShape");

        return child?.GetType().Name == "CustomParamStorageExtendedMeshShape"
            && Get(child, "m_shapestorage") is IList { Count: 0 }
            && Get(child, "m_meshstorage") is IList { Count: > 0 };
    }

    /// <summary>The mesh of a supported shape, or null.</summary>
    public static Mesh Read(object moppShape)
    {
        if (!IsSupported(moppShape))
            return null;

        var child = Get(Get(moppShape, "m_child"), "m_childShape");
        var storages = (IList)Get(child, "m_meshstorage");
        var parameters = Get(child, "m_materialArray") as IList;
        var mesh = new Mesh { KeyShift = 32 - Convert.ToInt32(Get(child, "m_numBitsForSubpartIndex")) };

        for (var s = 0; s < storages.Count; s++)
        {
            var storage = storages[s];
            var vertices = ((IList<Vector4>)Get(storage, "m_vertices")).Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            var indices = Indices(storage);
            var triangles = new List<(int, int, int)>(indices.Count / 4);

            // Four indices a triangle, the fourth padding
            for (var i = 0; i + 3 < indices.Count; i += 4)
                triangles.Add(((int)indices[i], (int)indices[i + 1], (int)indices[i + 2]));

            mesh.StorageVertices.Add(vertices);
            mesh.StorageTriangles.Add(triangles);

            var parameter = parameters != null && s < parameters.Count ? parameters[s] : null;
            mesh.StorageMaterials.Add(GetMaterial(parameter));
        }

        return mesh;
    }

    /// <summary>
    /// A new shape like the old, with each storage's vertices replaced (same counts) and a MOPP over its triangles,
    /// leaving out the keys in skip (triangles with no area left). Null if the shape isn't supported or the counts differ.
    /// </summary>
    public static object Rebuild(object moppShape, IReadOnlyList<Vector3[]> storageVertices, ISet<uint> skip = null)
    {
        var mesh = Read(moppShape);

        if (mesh == null || storageVertices.Count != mesh.StorageVertices.Count)
            return null;

        for (var s = 0; s < storageVertices.Count; s++)
        {
            if (storageVertices[s].Length != mesh.StorageVertices[s].Length)
                return null;
        }

        var oldContainer = Get(moppShape, "m_child");
        var oldChild = Get(oldContainer, "m_childShape");

        // The storages, with the new vertices (w kept)
        var oldStorages = (IList)Get(oldChild, "m_meshstorage");
        var newStorages = (IList)Activator.CreateInstance(oldStorages.GetType());

        for (var s = 0; s < oldStorages.Count; s++)
        {
            var storage = Clone(oldStorages[s]);
            var old = (IList<Vector4>)Get(oldStorages[s], "m_vertices");
            var vertices = new List<Vector4>(old.Count);

            for (var v = 0; v < old.Count; v++)
                vertices.Add(new Vector4(storageVertices[s][v], old[v].W));

            Set(storage, "m_vertices", vertices);
            newStorages.Add(storage);
        }

        var child = Clone(oldChild);
        Set(child, "m_meshstorage", newStorages);

        // The MOPP over every triangle that still has an area
        var triangles = new List<MoppCompiler.Triangle>();

        for (var s = 0; s < mesh.StorageTriangles.Count; s++)
        {
            var vertices = storageVertices[s];

            for (var t = 0; t < mesh.StorageTriangles[s].Count; t++)
            {
                var key = mesh.Key(s, t);
                var (a, b, c) = mesh.StorageTriangles[s][t];

                if (skip != null && skip.Contains(key))
                    continue;

                if (a >= vertices.Length || b >= vertices.Length || c >= vertices.Length)
                    continue;

                triangles.Add(new MoppCompiler.Triangle(vertices[a], vertices[b], vertices[c], key));
            }
        }

        return Finish(moppShape, oldContainer, oldChild, child, storageVertices, triangles);
    }

    /// <summary>
    /// Whether a material parameter keeps its material in its primitive data, as DS2S's do: four bytes, the material
    /// as a big-endian 16-bit value then 00 0B (l01_0200: 00 14 00 0B), with the name data 0. DES, DS1 and DS1R keep
    /// it in the name data (1, 2, 301: the OBJ2HKX material IDs).
    /// </summary>
    private static bool IsPrimitiveMaterial(object parameter)
    {
        return Get(parameter, "m_primitiveDataBuffer") is IList { Count: 4 }
            && Convert.ToUInt32(Get(parameter, "m_materialNameData")) == 0;
    }

    /// <summary>A material parameter's material (see IsPrimitiveMaterial); 0 for none.</summary>
    public static uint GetMaterial(object parameter)
    {
        if (parameter == null)
            return 0;

        if (IsPrimitiveMaterial(parameter))
        {
            var bytes = (IList)Get(parameter, "m_primitiveDataBuffer");
            return (uint)(Convert.ToByte(bytes[0]) << 8 | Convert.ToByte(bytes[1]));
        }

        return Convert.ToUInt32(Get(parameter, "m_materialNameData"));
    }

    /// <summary>Gives a (copied) material parameter a material, where its kind of parameter keeps it.</summary>
    private static void SetMaterial(object parameter, uint material)
    {
        if (IsPrimitiveMaterial(parameter))
        {
            var old = ((IList)Get(parameter, "m_primitiveDataBuffer")).Cast<object>().Select(Convert.ToByte).ToList();
            Set(parameter, "m_primitiveDataBuffer", new List<byte> { (byte)(material >> 8), (byte)material, old[2], old[3] });
        }
        else
        {
            Set(parameter, "m_materialNameData", material);
        }
    }

    // A storage holds at most this many vertices (16-bit indices) and a shape this many storages (12 subpart bits)
    private const int MaxStorageVertices = 65535;

    /// <summary>
    /// A new shape of the template's class set from triangles (indices in threes into vertices, a material each): one
    /// storage per material (more if a material has over 65535 vertices), each storage and material parameter copied
    /// from the template's (a parameter with the same material if it has one, else its first, given the material),
    /// with vertices of their own, a subpart each, the bounds and a compiled MOPP. Keys gives each input triangle's
    /// shape key; triangles with no area get none (uint.MaxValue) and are left out. Null if the template isn't
    /// supported or nothing is left.
    /// </summary>
    public static object BuildNew(object templateShape, IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices,
        IReadOnlyList<uint> materials, out uint[] keys)
    {
        keys = Enumerable.Repeat(uint.MaxValue, indices.Count / 3).ToArray();

        if (!IsSupported(templateShape))
            return null;

        var oldContainer = Get(templateShape, "m_child");
        var oldChild = Get(oldContainer, "m_childShape");
        var oldStorages = (IList)Get(oldChild, "m_meshstorage");
        var oldParameters = (IList)Get(oldChild, "m_materialArray");
        var oldSubparts = (IList)Get(oldChild, "m_trianglesSubparts");
        var keyShift = 32 - Convert.ToInt32(Get(oldChild, "m_numBitsForSubpartIndex"));

        if (oldStorages.Count == 0 || oldParameters == null || oldParameters.Count == 0 || oldSubparts.Count == 0)
            return null;

        // The triangles of each material, in input order, split where a storage would pass its vertex limit
        var groups = new List<(uint Material, List<int> Triangles, Dictionary<int, int> Remap)>();

        for (var t = 0; t < indices.Count / 3; t++)
        {
            var a = vertices[indices[t * 3]];
            var b = vertices[indices[t * 3 + 1]];
            var c = vertices[indices[t * 3 + 2]];

            if (Vector3.Cross(b - a, c - a).LengthSquared() < 1e-12f)
                continue;

            var material = t < materials.Count ? materials[t] : materials.LastOrDefault();
            var group = groups.LastOrDefault(g => g.Material == material);

            if (group.Triangles == null || group.Remap.Count + 3 > MaxStorageVertices || group.Triangles.Count >= (1 << keyShift) - 1)
            {
                group = (material, new List<int>(), new Dictionary<int, int>());
                groups.Add(group);
            }

            group.Triangles.Add(t);

            for (var j = 0; j < 3; j++)
                group.Remap.TryAdd(indices[t * 3 + j], group.Remap.Count);
        }

        if (groups.Count == 0 || groups.Count > 1 << (32 - keyShift))
            return null;

        var newStorages = (IList)Activator.CreateInstance(oldStorages.GetType());
        var newParameters = (IList)Activator.CreateInstance(oldParameters.GetType());
        var newSubparts = (IList)Activator.CreateInstance(oldSubparts.GetType());
        var storageVertices = new List<Vector3[]>();
        var triangles = new List<MoppCompiler.Triangle>();

        for (var s = 0; s < groups.Count; s++)
        {
            var (material, groupTriangles, remap) = groups[s];

            // The vertices this storage uses, in first-use order
            var local = new Vector3[remap.Count];

            foreach (var (global, index) in remap)
                local[index] = vertices[global];

            storageVertices.Add(local);

            var storage = Clone(oldStorages[0]);
            Set(storage, "m_vertices", local.Select(v => new Vector4(v, 0)).ToList());

            // Four 16-bit indices a triangle, the fourth padding; every other list empty, of its own
            var indices16 = new List<ushort>(groupTriangles.Count * 4);

            for (var i = 0; i < groupTriangles.Count; i++)
            {
                var t = groupTriangles[i];
                indices16.Add((ushort)remap[indices[t * 3]]);
                indices16.Add((ushort)remap[indices[t * 3 + 1]]);
                indices16.Add((ushort)remap[indices[t * 3 + 2]]);
                indices16.Add(0);

                var key = (uint)s << keyShift | (uint)i;
                keys[t] = key;
                triangles.Add(new MoppCompiler.Triangle(vertices[indices[t * 3]], vertices[indices[t * 3 + 1]], vertices[indices[t * 3 + 2]], key));
            }

            foreach (var field in storage.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.Name is "m_vertices" || !typeof(IList).IsAssignableFrom(field.FieldType))
                    continue;

                field.SetValue(storage, field.Name == "m_indices16" ? indices16 : Activator.CreateInstance(field.FieldType));
            }

            newStorages.Add(storage);

            // A parameter of the same material keeps every byte the games put in it
            var template = oldParameters.Cast<object>().FirstOrDefault(p => p != null && GetMaterial(p) == material) ?? oldParameters[0];
            var parameter = Clone(template);

            if (GetMaterial(parameter) != material)
                SetMaterial(parameter, material);

            // One byte a vertex (0xFF, as most of the games' vertices have) where the template has one
            if (Convert.ToInt32(Get(parameter, "m_vertexDataStride")) == 1)
                Set(parameter, "m_vertexDataBuffer", Enumerable.Repeat((byte)0xFF, local.Length).ToList());
            else
                Set(parameter, "m_vertexDataBuffer", new List<byte>());

            Set(parameter, "m_primitiveDataBuffer", ((IList)Get(parameter, "m_primitiveDataBuffer")).Cast<object>().Select(Convert.ToByte).ToList());
            newParameters.Add(parameter);

            var subpart = Clone(oldSubparts[0]);
            SetConverted(subpart, "m_numTriangleShapes", groupTriangles.Count);
            SetConverted(subpart, "m_numVertices", local.Length);
            newSubparts.Add(subpart);
        }

        var child = Clone(oldChild);
        Set(child, "m_meshstorage", newStorages);
        Set(child, "m_materialArray", newParameters);
        Set(child, "m_trianglesSubparts", newSubparts);
        Set(child, "m_embeddedTrianglesSubpart", Clone(newSubparts[0]));

        if (Get(child, "m_weldingInfo") is IList welding)
            Set(child, "m_weldingInfo", Activator.CreateInstance(welding.GetType()));

        // A cached count is kept up to date; the "not cached" markers (0, int.MinValue) are left
        var cached = Convert.ToInt32(Get(child, "m_cachedNumChildShapes"));

        if (cached != 0 && cached != int.MinValue)
            SetConverted(child, "m_cachedNumChildShapes", triangles.Count);

        return Finish(templateShape, oldContainer, oldChild, child, storageVertices, triangles);
    }

    /// <summary>The shape around a new mesh shape: its bounds, its container and a MOPP over the triangles.</summary>
    private static object Finish(object oldShape, object oldContainer, object oldChild, object child,
        IReadOnlyList<Vector3[]> storageVertices, List<MoppCompiler.Triangle> triangles)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var v in storageVertices.SelectMany(x => x))
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        if (min.X <= max.X)
        {
            // Widened by the triangle radius, which w holds too (as the games' shapes have it)
            var radius = Convert.ToSingle(Get(oldChild, "m_triangleRadius") ?? 0f);
            Set(child, "m_aabbHalfExtents", new Vector4((max - min) / 2 + new Vector3(radius), radius));
            Set(child, "m_aabbCenter", new Vector4((max + min) / 2, 0));
        }

        var container = Clone(oldContainer);
        Set(container, "m_childShape", child);

        var compiled = MoppCompiler.Compile(triangles);

        var oldCode = Get(oldShape, "m_code");
        var code = Clone(oldCode);
        var info = Clone(Get(oldCode, "m_info"));
        Set(info, "m_offset", compiled.Offset);
        Set(code, "m_info", info);
        Set(code, "m_data", new List<byte>(compiled.Code));

        // The compiler writes no chunks (DES's codes, which have no build type, have none either)
        var buildType = Field(code.GetType(), "m_buildType");

        if (buildType != null)
        {
            buildType.SetValue(code, buildType.FieldType.IsEnum
                ? Enum.ToObject(buildType.FieldType, 1)
                : Convert.ChangeType(1, buildType.FieldType));
        }

        var shape = Clone(oldShape);
        Set(shape, "m_code", code);
        Set(shape, "m_child", container);

        return shape;
    }

    /// <summary>
    /// Names an hkpRigidBody (the games name a file's body after its model, with the file's layer letter:
    /// h0029B1, l01_0200). False if the body isn't one.
    /// </summary>
    public static bool SetBodyName(object body, string name)
    {
        if (body?.GetType().Name != "hkpRigidBody" || Field(body.GetType(), "m_name") == null)
            return false;

        Set(body, "m_name", name);
        return true;
    }

    /// <summary>The name of an hkpRigidBody, or null.</summary>
    public static string GetBodyName(object body) => body?.GetType().Name == "hkpRigidBody" ? Get(body, "m_name") as string : null;

    private static void SetConverted(object o, string name, int value)
    {
        var field = Field(o.GetType(), name);

        if (field != null)
            field.SetValue(o, Convert.ChangeType(value, field.FieldType));
    }

    /// <summary>
    /// Puts a shape on an hkpRigidBody (its collidable's shape). False if the body isn't one, or the shape isn't of
    /// the body's class set.
    /// </summary>
    public static bool SetBodyShape(object body, object shape)
    {
        if (body?.GetType().Name != "hkpRigidBody" || shape == null)
            return false;

        var collidable = Get(body, "m_collidable");
        var field = collidable == null ? null : Field(collidable.GetType(), "m_shape");

        if (field == null || !field.FieldType.IsInstanceOfType(shape))
            return false;

        field.SetValue(collidable, shape);
        return true;
    }

    private static List<long> Indices(object storage)
    {
        foreach (var name in new[] { "m_indices8", "m_indices16", "m_indices32" })
        {
            if (Get(storage, name) is IList { Count: > 0 } list)
                return list.Cast<object>().Select(Convert.ToInt64).ToList();
        }

        return new List<long>();
    }

    private static readonly MethodInfo MemberwiseCloneMethod =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

    private static object Clone(object o) => o == null ? null : MemberwiseCloneMethod.Invoke(o, null);

    private static FieldInfo Field(Type type, string name) => type.GetField(name, BindingFlags.Public | BindingFlags.Instance);

    private static object Get(object o, string name) => o == null ? null : Field(o.GetType(), name)?.GetValue(o);

    private static void Set(object o, string name, object value)
    {
        var field = Field(o.GetType(), name) ?? throw new MissingFieldException(o.GetType().Name, name);
        field.SetValue(o, value);
    }
}
