using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Converts a FLVER2 model into a Wavefront OBJ (+ MTL) file.
///
/// Usage (from another program):
///     FLVER2 flver = FLVER2.Read("c:\\model.flver");
///     FlverToObj.Convert(flver, "c:\\model.obj");
///
/// With custom material naming ("hkm_Matname_Matprop"), one entry per flver.Materials[i]:
///     var naming = new List&lt;FlverToObj.MaterialNaming&gt;
///     {
///         new FlverToObj.MaterialNaming("Armor", "Metal"),
///         new FlverToObj.MaterialNaming("Skin", "Cloth"),
///     };
///     FlverToObj.Convert(flver, "c:\\model.obj", naming);
///
/// Or build this file as a console app and run:
///     FlverToObj.exe input.flver output.obj
/// </summary>
public static class FlverToObj
{
    /// <summary>
    /// Pairs a material name with a material "property" tag to build the OBJ/MTL
    /// name "hkm_{MatName}_{MatProp}".
    /// </summary>
    public readonly struct MaterialNaming
    {
        public string MatName { get; }
        public string MatProp { get; }

        public MaterialNaming(string matName, string matProp)
        {
            MatName = matName;
            MatProp = matProp;
        }
    }

    /// <summary>
    /// Converts a FLVER2 to an OBJ file (and a companion MTL file with the same name).
    /// </summary>
    /// <param name="flver">The parsed FLVER2 model.</param>
    /// <param name="outputObjPath">Full path to the .obj file to write.</param>
    /// <param name="materialNaming">
    /// Optional list of (MatName, MatProp) pairs, one per entry in <see cref="FLVER2.Materials"/>
    /// (i.e. <c>materialNaming[i]</c> corresponds to <c>flver.Materials[i]</c>). Each used material
    /// is written out as "hkm_{MatName}_{MatProp}". If omitted, or shorter than the material list,
    /// materials without an entry fall back to a sanitized version of their original FLVER name.
    /// </param>
    /// <param name="flipUVsForOBJ">
    /// FLVER stores V with 0 at the top (DirectX-style); OBJ/most other tools expect 0 at the bottom.
    /// Leave this true unless your target tool expects DirectX-style UVs.
    /// </param>
    public static void Convert(FLVER2 flver, string outputObjPath, IList<MaterialNaming> materialNaming = null, bool flipUVsForOBJ = true)
    {
        string mtlPath = Path.ChangeExtension(outputObjPath, ".mtl");
        string mtlFileName = Path.GetFileName(mtlPath);

        var obj = new StringBuilder();
        var usedMaterials = new HashSet<int>();

        obj.AppendLine("# Converted from FLVER2 by FlverToObj");
        obj.AppendLine($"mtllib {mtlFileName}");
        obj.AppendLine();

        // OBJ indices are 1-based and global across the whole file, so we
        // track how many v/vt/vn entries have been written so far and
        // offset each mesh's local indices by that amount.
        int vOffset = 0;
        int vtOffset = 0;
        int vnOffset = 0;

        for (int meshIndex = 0; meshIndex < flver.Meshes.Count; meshIndex++)
        {
            FLVER2.Mesh mesh = flver.Meshes[meshIndex];
            if (mesh.Vertices == null || mesh.Vertices.Count == 0)
                continue;

            bool hasUVs = mesh.Vertices[0].UVs != null && mesh.Vertices[0].UVs.Count > 0;

            string materialName = "material_default";
            if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < flver.Materials.Count)
            {
                materialName = GetMaterialName(flver, mesh.MaterialIndex, materialNaming);
                usedMaterials.Add(mesh.MaterialIndex);
            }

            obj.AppendLine($"g mesh_{meshIndex}");
            obj.AppendLine($"usemtl {materialName}");

            // Positions
            foreach (FLVER.Vertex vert in mesh.Vertices)
            {
                Vector3 p = vert.Position;
                obj.AppendLine(FormattableString.Invariant(
                    $"v {p.X.ToString(CultureInfo.InvariantCulture)} {p.Y.ToString(CultureInfo.InvariantCulture)} {p.Z.ToString(CultureInfo.InvariantCulture)}"));
            }

            // UVs (first UV channel only)
            if (hasUVs)
            {
                foreach (FLVER.Vertex vert in mesh.Vertices)
                {
                    Vector3 uv = vert.UVs[0];
                    float v = flipUVsForOBJ ? 1f - uv.Y : uv.Y;
                    obj.AppendLine(FormattableString.Invariant(
                        $"vt {uv.X.ToString(CultureInfo.InvariantCulture)} {v.ToString(CultureInfo.InvariantCulture)}"));
                }
            }

            // Normals
            foreach (FLVER.Vertex vert in mesh.Vertices)
            {
                Vector3 n = vert.Normal;
                // Guard against zero-length normals which some layouts leave unset.
                if (n == Vector3.Zero)
                    n = new Vector3(0, 1, 0);
                else
                    n = Vector3.Normalize(n);

                obj.AppendLine(FormattableString.Invariant(
                    $"vn {n.X.ToString(CultureInfo.InvariantCulture)} {n.Y.ToString(CultureInfo.InvariantCulture)} {n.Z.ToString(CultureInfo.InvariantCulture)}"));
            }

            // Faces: pull triangle indices straight from each FaceSet.
            // Prefer the lowest-LOD (highest detail) faceset: flags == None, falling back to the first one.
            FLVER2.FaceSet faceSet = mesh.FaceSets.Find(fs => fs.Flags == FLVER2.FaceSet.FSFlags.None)
                                        ?? (mesh.FaceSets.Count > 0 ? mesh.FaceSets[0] : null);

            if (faceSet != null)
            {
                bool allowPrimitiveRestarts = mesh.Vertices.Count < ushort.MaxValue;
                List<int> triangles = faceSet.Triangulate(allowPrimitiveRestarts);

                for (int i = 0; i < triangles.Count - 2; i += 3)
                {
                    int i1 = triangles[i] + 1;
                    int i2 = triangles[i + 1] + 1;
                    int i3 = triangles[i + 2] + 1;

                    obj.AppendLine(BuildFaceLine(i1, i2, i3, vOffset, vtOffset, vnOffset, hasUVs));
                }
            }

            obj.AppendLine();

            vOffset += mesh.Vertices.Count;
            vnOffset += mesh.Vertices.Count;
            if (hasUVs)
                vtOffset += mesh.Vertices.Count;
        }

        File.WriteAllText(outputObjPath, obj.ToString());
        File.WriteAllText(mtlPath, BuildMtl(flver, usedMaterials, materialNaming));
    }

    /// <summary>
    /// Builds the "hkm_{MatName}_{MatProp}" name for a material, falling back to a
    /// sanitized version of the material's own FLVER name if no naming entry exists for it.
    /// </summary>
    private static string GetMaterialName(FLVER2 flver, int materialIndex, IList<MaterialNaming> materialNaming)
    {
        if (materialNaming != null && materialIndex < materialNaming.Count)
        {
            MaterialNaming naming = materialNaming[materialIndex];
            if (!string.IsNullOrWhiteSpace(naming.MatName) || !string.IsNullOrWhiteSpace(naming.MatProp))
                return $"hkm_{SanitizeName(naming.MatName)}_{SanitizeName(naming.MatProp)}";
        }

        return SanitizeName(flver.Materials[materialIndex].Name);
    }

    private static string BuildFaceLine(int i1, int i2, int i3, int vOffset, int vtOffset, int vnOffset, bool hasUVs)
    {
        string F(int localIndex)
        {
            int v = localIndex + vOffset;
            int n = localIndex + vnOffset;
            if (hasUVs)
            {
                int t = localIndex + vtOffset;
                return $"{v}/{t}/{n}";
            }
            return $"{v}//{n}";
        }

        return $"f {F(i1)} {F(i2)} {F(i3)}";
    }

    private static string BuildMtl(FLVER2 flver, HashSet<int> usedMaterialIndices, IList<MaterialNaming> materialNaming)
    {
        var mtl = new StringBuilder();
        mtl.AppendLine("# Materials referenced by the converted OBJ");
        mtl.AppendLine("# Note: this only maps material/texture names; it does not export texture image data.");
        mtl.AppendLine();

        if (usedMaterialIndices.Count == 0)
        {
            mtl.AppendLine("newmtl material_default");
            mtl.AppendLine("Kd 0.8 0.8 0.8");
            return mtl.ToString();
        }

        foreach (int matIndex in usedMaterialIndices.OrderBy(i => i))
        {
            FLVER2.Material material = flver.Materials[matIndex];
            mtl.AppendLine($"newmtl {GetMaterialName(flver, matIndex, materialNaming)}");
            mtl.AppendLine("Kd 0.8 0.8 0.8");

            // If a diffuse/albedo-looking texture is present, reference it so the
            // material can be relinked to an extracted texture file later.
            FLVER2.Texture diffuse = material.Textures.FirstOrDefault(t =>
                t.ParamName != null &&
                (t.ParamName.IndexOf("Diffuse", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    t.ParamName.IndexOf("Albedo", StringComparison.OrdinalIgnoreCase) >= 0));

            if (diffuse != null && !string.IsNullOrWhiteSpace(diffuse.Path))
            {
                string texName = Path.GetFileNameWithoutExtension(diffuse.Path) + ".png";
                mtl.AppendLine($"map_Kd {texName}");
            }

            mtl.AppendLine();
        }

        return mtl.ToString();
    }

    private static string SanitizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "material";

        var sb = new StringBuilder(name.Length);
        foreach (char c in name)
            sb.Append(char.IsWhiteSpace(c) || c == '/' || c == '\\' ? '_' : c);
        return sb.ToString();
    }
}
