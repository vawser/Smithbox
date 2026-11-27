#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Utilities;

/// <summary>
///     Havok cloth relates bones and vertices to triangle frames built from the mesh (Character Scale.md §4.6).
///     A frame's axes a_i are unit vectors, edge vectors (length¹) or the cross product of two edges (length²): power
///     k_i. A stored matrix M = R · A⁻¹ (row vectors: a bone R in the frame, or with R = I the frame's inverse) then
///     changes under a uniform scale s as column i × s^-k_i and translation component i × s^(1 - k_i). The 3x3's
///     inverse has rows a_i · Rᵀ, which keep the a_i's lengths and cross products, so k is read from the matrix itself.
/// </summary>
public static class ClothFrame
{
    private const float Tolerance = 2e-3f;

    /// <summary>The power of length of each frame axis (0 unit, 1 edge, 2 cross product), or null if singular</summary>
    public static int[]? AxisPowers(Matrix4x4 m)
    {
        var a = new Matrix4x4(m.M11, m.M12, m.M13, 0, m.M21, m.M22, m.M23, 0, m.M31, m.M32, m.M33, 0, 0, 0, 0, 1);
        if (!Matrix4x4.Invert(a, out var inv))
            return null;

        var rows = new[]
        {
            new Vector3(inv.M11, inv.M12, inv.M13),
            new Vector3(inv.M21, inv.M22, inv.M23),
            new Vector3(inv.M31, inv.M32, inv.M33)
        };
        if (!float.IsFinite(rows[0].X + rows[1].Y + rows[2].Z))
            return null;

        var k = new int[3];
        for (var i = 0; i < 3; i++)
        {
            var r = rows[i];
            var length = r.Length();
            if (MathF.Abs(length - 1f) < Tolerance)
            {
                k[i] = 0;
                continue;
            }

            var cross = Vector3.Cross(rows[(i + 1) % 3], rows[(i + 2) % 3]);
            var near = MathF.Min((r - cross).Length(), (r + cross).Length());
            k[i] = near < Tolerance * length ? 2 : 1;
        }

        return k;
    }

    /// <summary>
    ///     The axis powers of a mesh-to-mesh operator's triangle frames, from its m_scaleNormalBehaviour: IGNORE a
    ///     normalized normal (1, 1, 0), APPLY the edges' cross product (1, 1, 2), INVERT its inverse (1, 1, -2). Read
    ///     against each frame matrix in ER, DS3, SDT, NR and BB (§4.6). Mesh-bone operators: (1, 1, 2).
    /// </summary>
    public static int[] OperatorPowers(object op)
    {
        var flag = op.GetType().GetField("m_scaleNormalBehaviour")?.GetValue(op);
        var value = flag == null ? 1 : Convert.ToInt32(flag);
        return value switch
        {
            0 => new[] { 1, 1, 0 },
            2 => new[] { 1, 1, -2 },
            _ => new[] { 1, 1, 2 }
        };
    }

    /// <summary>
    ///     Scales a cloth file's operators (§4.6): triangle-frame matrices by their axis powers; deformer local
    ///     vectors by where they live (object or bone space: positions × s; triangle space: position component i
    ///     × s^(1 - k_i), normal, tangent and bitangent component i × s^-k_i). Returns how many values changed.
    /// </summary>
    public static int ScaleOperators(Havok.HavokDocument document, float s)
    {
        var changed = 0;
        foreach (var op in document.Objects)
        {
            var name = op.GetType().Name;
            if (!name.StartsWith("hcl", StringComparison.Ordinal) || !name.EndsWith("Operator", StringComparison.Ordinal))
                continue;

            var meshMesh = name.Contains("MeshMeshDeform", StringComparison.Ordinal);
            var boneSpace = name.StartsWith("hclBoneSpace", StringComparison.Ordinal);
            var k = name is "hclMeshBoneDeformOperator" or "hclSimpleMeshBoneDeformOperator" ? new[] { 1, 1, 2 } : OperatorPowers(op);

            // Triangle frames: object-space mesh-to-mesh inverse frames; mesh-bone bones in triangle frames
            changed += ScaleMatrices(op, "m_triangleFromMeshTransforms", k, s);
            changed += ScaleMatrices(op, "m_localBoneTransforms", k, s);
            if (op.GetType().GetField("m_triangleBonePairs")?.GetValue(op) is IList pairs)
                foreach (var pair in pairs)
                    changed += ScaleMatrixField(pair, "m_localBoneTransform", k, s);

            // Local vectors in triangle coordinates (bone-space mesh-to-mesh, and the older hclMeshMeshDeformOperator)
            var triangleSpace = meshMesh && (boneSpace || name == "hclMeshMeshDeformOperator");
            var position = triangleSpace ? k.Select(p => MathF.Pow(s, 1 - p)).ToArray() : new[] { s, s, s };
            var direction = triangleSpace ? k.Select(p => MathF.Pow(s, -p)).ToArray() : null;

            if (op.GetType().GetField("m_triangleVertexPairs")?.GetValue(op) is IList vertexPairs)
                foreach (var pair in vertexPairs)
                {
                    changed += ScaleVectorField(pair, "m_localPosition", position);
                    if (direction != null)
                        changed += ScaleVectorField(pair, "m_localNormal", direction);
                }

            foreach (var field in op.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetValue(op) is not IList blocks || blocks.Count == 0 || blocks[0] == null ||
                    !blocks[0]!.GetType().Name.StartsWith("LocalBlock", StringComparison.Ordinal))
                    continue;
                foreach (var block in blocks)
                {
                    changed += ScaleVectorArray(block!, "m_localPosition", position);
                    if (direction == null)
                        continue;
                    changed += ScaleVectorArray(block!, "m_localNormal", direction);
                    changed += ScaleVectorArray(block!, "m_localTangent", direction);
                    changed += ScaleVectorArray(block!, "m_localBiTangent", direction);
                }
            }
        }

        return changed;
    }

    private static int ScaleMatrices(object owner, string field, int[] k, float s)
    {
        if (owner.GetType().GetField(field)?.GetValue(owner) is not List<Matrix4x4> list)
            return 0;
        for (var i = 0; i < list.Count; i++)
            list[i] = ScaleFrame(list[i], k, s);
        return list.Count;
    }

    private static int ScaleMatrixField(object owner, string field, int[] k, float s)
    {
        var info = owner.GetType().GetField(field);
        if (info?.GetValue(owner) is not Matrix4x4 m)
            return 0;
        info.SetValue(owner, ScaleFrame(m, k, s));
        return 1;
    }

    // A Vector4 field (W kept: 1 for a position, 0 for a direction) scaled per component
    private static int ScaleVectorField(object owner, string field, float[] f)
    {
        var info = owner.GetType().GetField(field);
        if (info?.GetValue(owner) is not Vector4 v)
            return 0;
        info.SetValue(owner, new Vector4(v.X * f[0], v.Y * f[1], v.Z * f[2], v.W));
        return 1;
    }

    // An array of Vector4 or of hkPackedVector3 (decoded, scaled per component, packed again)
    private static int ScaleVectorArray(object owner, string field, float[] f)
    {
        switch (owner.GetType().GetField(field)?.GetValue(owner))
        {
            case Vector4[] vectors:
                for (var i = 0; i < vectors.Length; i++)
                    vectors[i] = new Vector4(vectors[i].X * f[0], vectors[i].Y * f[1], vectors[i].Z * f[2], vectors[i].W);
                return vectors.Length;
            case Array packed:
                var n = 0;
                foreach (var item in packed)
                {
                    if (item?.GetType().GetField("m_values")?.GetValue(item) is not short[] { Length: 4 } values)
                        continue;
                    var v = HavokFieldScale.Unpack(values);
                    HavokFieldScale.Pack(values, new Vector3(v.X * f[0], v.Y * f[1], v.Z * f[2]));
                    n++;
                }
                return n;
            default:
                return 0;
        }
    }

    /// <summary>A frame-relative matrix under a uniform scale s, given its axis powers</summary>
    public static Matrix4x4 ScaleFrame(Matrix4x4 m, int[] k, float s)
    {
        float Col(int i) => MathF.Pow(s, -k[i]);
        float T(int i) => MathF.Pow(s, 1 - k[i]);

        m.M11 *= Col(0); m.M21 *= Col(0); m.M31 *= Col(0);
        m.M12 *= Col(1); m.M22 *= Col(1); m.M32 *= Col(1);
        m.M13 *= Col(2); m.M23 *= Col(2); m.M33 *= Col(2);
        m.M41 *= T(0);
        m.M42 *= T(1);
        m.M43 *= T(2);
        return m;
    }
}
