using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System.Numerics;
using System.Text.RegularExpressions;

// The structure of ER/NR fall protection files: systems, bodies, how bodies share shapes and geometry, materials.
//   CollisionSurvey ER <mapId> [max files] falldetail [file filter regex]
static class FallDetail
{
    public static void Survey(VirtualFileSystem fs, string gameName, string mapId, int maxFiles, Regex filter)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);
        var bhdPath = $"/map/{mapId.Substring(0, 3)}/{mapId}/f{mapId.Substring(1)}.hkxbhd";
        var bxf = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(Path.ChangeExtension(bhdPath, ".hkxbdt")).Value.ToArray());
        var compFile = bxf.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
        Havok.HavokCompendium compendium = null;
        if (compFile != null) { var b = compFile.Bytes.ToArray(); if (DCX.Is(b)) b = DCX.Decompress(b).ToArray(); compendium = Havok.Files.HavokFile.ReadCompendium(b, profile); }

        Console.WriteLine($"compendium {(compFile == null ? "none" : compFile.Name)}; files {bxf.Files.Count}; first names {string.Join(", ", bxf.Files.Take(3).Select(f => f.Name))}");
        var shown = 0;
        var agg = new Dictionary<string, Dictionary<string, int>>();
        void Count(string k, object v) { if (!agg.TryGetValue(k, out var d)) agg[k] = d = new(); var s = v?.ToString() ?? "null"; d[s] = d.GetValueOrDefault(s) + 1; }

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx") && filter.IsMatch(f.Name)))
        {
            var bytes = file.Bytes.ToArray();
            Count("dcx", DCX.Is(bytes));
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var doc = Havok.Files.HavokFile.Read(bytes, profile, compendium);
            var root = (hkRootLevelContainer)doc.Root;
            Count("variants", string.Join(",", root.m_namedVariants.Select(v => $"{v.m_name}:{v.m_variant?.GetType().Name}")));
            var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().FirstOrDefault();
            if (scene == null) { Count("scene", "none"); continue; }
            Count("systems", scene.m_systemDatas.Count);
            foreach (var sys in scene.m_systemDatas)
            {
                Count("sys name", sys.m_name);
                Count("sys materials", sys.m_materials.Count);
                Count("sys motionProps", sys.m_motionProperties.Count);
                Count("sys constraints", sys.m_constraintCinfos.Count);
                Count("bodies==0", sys.m_bodyCinfos.Count == 0);
                var shapes = sys.m_bodyCinfos.Select(b => b.m_shape).Distinct().Count();
                Count("distinct shapes == bodies", shapes == sys.m_bodyCinfos.Count);
                foreach (var m in sys.m_materials) Count("material", $"{m.m_name} flags {m.m_flags} fr {m.m_dynamicFriction}/{m.m_staticFriction} user {m.m_userData}");
                foreach (var body in sys.m_bodyCinfos)
                {
                    Count("body mat", body.m_materialId);
                    Count("body filter", $"0x{body.m_collisionFilterInfo:X}");
                    Count("body motion", body.m_motionId);
                    Count("body quality", body.m_qualityId);
                    Count("body flags", body.m_flags);
                    Count("body name empty", string.IsNullOrEmpty(body.m_name));
                    Count("body attached", body.m_attachedBody);
                    Count("body pos zero", body.m_position == Vector4.Zero);
                    Count("body userData", body.m_userData);
                    Count("body rest", $"cntrl {body.m_collisionCntrl} motionType {body.m_motionType} rot {body.m_orientation} linVel {body.m_linearVelocity} angVel {body.m_angularVelocity} mass {body.m_mass} massDist {body.m_massDistribution?.GetType().Name} drag {body.m_dragProperties?.GetType().Name} motionProps {body.m_motionPropertiesId} desired {body.m_desiredBodyId?.m_serialAndIndex} lookAhead {body.m_collisionLookAheadDistance} prio {body.m_activationPriority}");
                    if (body.m_shape is hknpExternMeshShape s0) Count("ems kind", $"type {s0.m_type} dispatch {s0.m_dispatchType} flags {s0.m_flags}");
                    Count("body localFrame", body.m_localFrame?.GetType().Name);
                    if (body.m_shape is hknpExternMeshShape ems)
                    {
                        var g = (ems.m_geometry as hknpDefaultExternMeshShapeGeometry)?.m_geometry;
                        if (g != null && g.m_vertices.Count >= 2) { var hgt = g.m_vertices[1].Y - g.m_vertices[0].Y; Count($"height filter 0x{body.m_collisionFilterInfo:X}", MathF.Round(hgt, 2)); }
                        Count("geom type", ems.m_geometry?.GetType().Name);
                        Count("useTriMat", (ems.m_geometry as hknpDefaultExternMeshShapeGeometry)?.m_useTriangleMaterialAsShapeTag);
                        Count("tris per body", g?.m_triangles.Count);
                        Count("verts per body", g?.m_vertices.Count);
                        Count("tri material", string.Join(",", g?.m_triangles.Select(t => t.m_material).Distinct() ?? []));
                        Count("ems flags", ems.m_flags); Count("ems keyBits", ems.m_numShapeKeyBits); Count("ems radius", ems.m_convexRadius);
                        Count("ems userData", ems.m_userData); Count("ems tagCodec", ems.m_shapeTagCodecInfo);
                        Count("ems props", ems.m_properties?.GetType().Name);
                        Count("bvd type", ems.m_boundingVolumeData?.GetType().Name);
                        Count("simd nodes", (ems.m_boundingVolumeData as hknpExternMeshShapeData)?.m_simdTree.m_nodes.Count);
                        Count("vert w", string.Join(",", g?.m_vertices.Select(v => v.W).Distinct() ?? []));
                    }
                }
                if (shown < maxFiles)
                {
                    shown++;
                    Console.WriteLine($"=== {file.Name}: bodies {sys.m_bodyCinfos.Count}, distinct shapes {shapes}");
                    foreach (var body in sys.m_bodyCinfos.Take(4))
                    {
                        var g = ((body.m_shape as hknpExternMeshShape)?.m_geometry as hknpDefaultExternMeshShapeGeometry)?.m_geometry;
                        Console.WriteLine($"  body '{body.m_name}' mat {body.m_materialId} filter 0x{body.m_collisionFilterInfo:X} pos {body.m_position} attached {body.m_attachedBody}");
                        if (g != null)
                        {
                            Console.WriteLine($"    verts: {string.Join(" ", g.m_vertices.Select(v => $"({v.X:0.##},{v.Y:0.##},{v.Z:0.##})"))}");
                            Console.WriteLine($"    tris: {string.Join(" ", g.m_triangles.Select(t => $"[{t.m_a},{t.m_b},{t.m_c} m{t.m_material}]"))}");
                        }
                    }
                }
            }
        }
        foreach (var (k, d) in agg) Console.WriteLine($"{k}: {string.Join(" | ", d.OrderByDescending(e => e.Value).Take(8).Select(e => $"{e.Key} x{e.Value}"))}");
    }
}
