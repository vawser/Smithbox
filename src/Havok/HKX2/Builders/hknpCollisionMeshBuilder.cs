using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace HKX2.Builders
{
    public class hknpCollisionMeshBuilder
    {
        private class CollisionSection
        {
            public BVNode SectionHead;
            public List<ushort> Indices;
            public HashSet<ushort> UsedIndices;

            public void GatherSectionIndices(List<ushort> indices)
            {
                Indices = new List<ushort>();
                UsedIndices = new HashSet<ushort>();

                Stack<BVNode> tstack = new Stack<BVNode>();
                tstack.Push(SectionHead);
                BVNode n;
                while (tstack.TryPop(out n))
                {
                    if (n.IsLeaf)
                    {
                        var p = n.Primitive;
                        n.Primitive = (uint)(Indices.Count / 3);
                        Indices.Add(indices[(int)p * 3]);
                        Indices.Add(indices[(int)p * 3 + 1]);
                        Indices.Add(indices[(int)p * 3 + 2]);
                        UsedIndices.Add(indices[(int)p * 3]);
                        UsedIndices.Add(indices[(int)p * 3 + 1]);
                        UsedIndices.Add(indices[(int)p * 3 + 2]);
                        if (indices[(int)p * 3] == 4 || indices[(int)p * 3 + 1] == 4 || indices[(int)p * 3 + 2] == 4)
                        {
                            n.Primitive = n.Primitive;
                        }
                    }
                    else
                    {
                        tstack.Push(n.Left);
                        tstack.Push(n.Right);
                    }
                }
            }
        }

        private List<fsnpCustomParamCompressedMeshShape> _meshes = new List<fsnpCustomParamCompressedMeshShape>();

        /// <summary>
        /// Build a material with some default values shamelessly stolen from actual
        /// DS3 cols
        /// </summary>
        /// <returns>A material</returns>
        private hknpMaterial BuildMaterial()
        {
            var mat = new hknpMaterial();
            mat.m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance = 16544;
            mat.m_dynamicFriction = 16128;
            mat.m_flags = 0;
            mat.m_fractionOfClippedImpulseToApply = 1;
            mat.m_frictionCombinePolicy = CombinePolicy.COMBINE_MIN;
            mat.m_isExclusive = 0;
            mat.m_isShared = false;
            mat.m_massChangerCategory = MassChangerCategory.MASS_CHANGER_IGNORE;
            mat.m_massChangerHeavyObjectFactor = 16256;
            mat.m_maxContactImpulse = 3.40282E+38F;
            mat.m_name = "";
            mat.m_restitution = 16076;
            mat.m_restitutionCombinePolicy = CombinePolicy.COMBINE_MAX;
            mat.m_softContactDampFactor = 0;
            mat.m_softContactForceFactor = 0;
            mat.m_softContactSeperationVelocity = new hkUFloat8();
            mat.m_softContactSeperationVelocity.m_value = 0;
            mat.m_staticFriction = 16128;
            mat.m_surfaceVelocity = null;
            mat.m_triggerManifoldTolerance = new hkUFloat8();
            mat.m_triggerManifoldTolerance.m_value = 255;
            mat.m_triggerType = TriggerType.TRIGGER_TYPE_NONE;
            mat.m_userData = 0;
            mat.m_weldingTolerance = 15692;
            return mat;
        }

        private hknpBodyCinfo BuildBodyCInfo(string name)
        {
            var cinfo = new hknpBodyCinfo();
            cinfo.m_collisionFilterInfo = 0;
            cinfo.m_collisionLookAheadDistance = 0;
            cinfo.m_flags = 1;
            cinfo.m_localFrame = null;
            cinfo.m_materialId = 0;
            cinfo.m_motionId = 2147483647;
            cinfo.m_name = name;
            cinfo.m_orientation = Quaternion.Identity;
            cinfo.m_position = new Vector4(0, 0, 0, 1);
            cinfo.m_qualityId = 255;
            cinfo.m_reservedBodyId = 2147483647;
            cinfo.m_spuFlags = 0;
            cinfo.m_userData = 0;
            return cinfo;
        }

        public ulong CompressSharedVertex(Vector3 vert, Vector3 min, Vector3 max)
        {
            float scaleX = (max.X - min.X) / (float)((1 << 21) - 1);
            float scaleY = (max.Y - min.Y) / (float)((1 << 21) - 1);
            float scaleZ = (max.Z - min.Z) / (float)((1 << 22) - 1);
            ulong x = (ulong)((vert.X - min.X) / scaleX);
            ulong y = (ulong)((vert.Y - min.Y) / scaleY);
            ulong z = (ulong)((vert.Z - min.Z) / scaleZ);
            return (x & 0x1FFFFF) | ((y & 0x1FFFFF) << 21) | ((z & 0x3FFFFF) << 42);
        }

        public uint CompressPackedVertex(Vector3 vert, Vector3 scale, Vector3 offset)
        {
            uint x = (uint)MathF.Min(MathF.Max((vert.X - offset.X), 0) / scale.X, 0x7FF);
            uint y = (uint)MathF.Min(MathF.Max((vert.Y - offset.Y), 0) / scale.Y, 0x7FF);
            uint z = (uint)MathF.Min(MathF.Max((vert.Z - offset.Z), 0) / scale.Z, 0x3FF);
            return (x & 0x7FF) | ((y & 0x7FF) << 11) | ((z & 0x3FF) << 22);
        }

        public void AddMesh(List<Vector3> verts, List<ushort> indices)
        {
            // Try and build the BVH for the mesh first
            var bv = verts.ToArray();
            var bi = indices.ToArray();
            bool didbuild = BVHNative.BuildBVHForMesh(bv, bv.Count(), bi, bi.Count());
            if (didbuild)
            {
                var nodecount = BVHNative.GetBVHSize();
                var nsize = BVHNative.GetNodeSize();
                var nodes = new NativeBVHNode[nodecount];
                BVHNative.GetBVHNodes(nodes);

                // Rebuild in friendlier tree form
                List<BVNode> bnodes = new List<BVNode>((int)nodecount);
                foreach (var n in nodes)
                {
                    var bnode = new BVNode();
                    bnode.Min = new Vector3(n.minX, n.minY, n.minZ);
                    bnode.Max = new Vector3(n.maxX, n.maxY, n.maxZ);
                    bnode.IsLeaf = n.isLeaf == 1;
                    bnode.PrimitiveCount = n.primitiveCount;
                    bnode.Primitive = n.firstChildOrPrimitive;
                    bnodes.Add(bnode);
                }
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].isLeaf == 0)
                    {
                        bnodes[i].Left = bnodes[(int)nodes[i].firstChildOrPrimitive];
                        bnodes[i].Right = bnodes[(int)nodes[i].firstChildOrPrimitive + 1];
                    }
                }

                // Split the mesh into sections using the BVH and primitive counts as
                // guidence
                bnodes[0].ComputePrimitiveCounts();
                bnodes[0].ComputeUniqueIndicesCounts(indices);
                bnodes[0].IsSectionHead = true;
                bnodes[0].AttemptSectionSplit();

                // Take out the section heads and replace them with new leafs that reference
                // the new section
                List<BVNode> sectionBVHs = new List<BVNode>();
                foreach (var node in bnodes)
                {
                    if (node.IsSectionHead)
                    {
                        var secnode = new BVNode();
                        secnode.Min = node.Min;
                        secnode.Max = node.Max;
                        secnode.PrimitiveCount = node.PrimitiveCount;
                        secnode.Left = node.Left;
                        secnode.Right = node.Right;
                        node.Left = null;
                        node.Right = null;
                        node.IsLeaf = true;
                        node.Primitive = (uint)sectionBVHs.Count();
                        sectionBVHs.Add(secnode);
                    }
                }

                List<CollisionSection> sections = new List<CollisionSection>();
                foreach (var b in sectionBVHs)
                {
                    var s = new CollisionSection();
                    s.SectionHead = b;
                    s.GatherSectionIndices(indices);
                    sections.Add(s);
                }

                // Count all the indices across sections to figure out what vertices need to be shared
                byte[] indicescount = new byte[indices.Count];
                foreach (var s in sections)
                {
                    foreach (var v in s.UsedIndices)
                    {
                        indicescount[v]++;
                    }
                }
                var shared = new HashSet<ushort>();
                for (ushort i = 0; i < indices.Count(); i++)
                {
                    if (indicescount[i] > 1)
                    {
                        shared.Add(i);
                    }
                }

                // Build shared indices mapping table and compress the shared verts
                List<ulong> sharedVerts = new List<ulong>();
                Dictionary<ushort, ushort> sharedIndexRemapTable = new Dictionary<ushort, ushort>();
                foreach (var i in shared.OrderBy(x => x))
                {
                    sharedIndexRemapTable.Add(i, (ushort)sharedVerts.Count());
                    sharedVerts.Add(CompressSharedVertex(verts[i], bnodes[0].Min, bnodes[0].Max));
                }

                // build the havok mesh
                fsnpCustomParamCompressedMeshShape mesh = new fsnpCustomParamCompressedMeshShape();
                mesh.m_convexRadius = 0.01f;
                mesh.m_dispatchType = Enum.COMPOSITE;
                mesh.m_edgeWeldingMap = new hknpSparseCompactMapunsignedshort();
                mesh.m_edgeWeldingMap.m_primaryKeyToIndex = null;
                mesh.m_edgeWeldingMap.m_secondaryKeyMask = 0;
                mesh.m_edgeWeldingMap.m_sencondaryKeyBits = 0;
                mesh.m_edgeWeldingMap.m_valueAndSecondaryKeys = null;
                mesh.m_quadIsFlat = new hkBitField();
                mesh.m_quadIsFlat.m_storage = new hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocator();
                mesh.m_quadIsFlat.m_storage.m_numBits = 0;
                mesh.m_quadIsFlat.m_storage.m_words = null;
                mesh.m_triangleIsInterior = new hkBitField();
                mesh.m_triangleIsInterior.m_storage = new hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocator();
                mesh.m_triangleIsInterior.m_storage.m_numBits = 0;
                mesh.m_triangleIsInterior.m_storage.m_words = null;
                mesh.m_triangleIndexToShapeKey = null;
                mesh.m_pParam = null;
                mesh.m_shapeTagCodecInfo = 4294967295;
                mesh.m_flags = 516;
                mesh.m_numShapeKeyBits = 5; // ?

                var meshdata = new hknpCompressedMeshShapeData();
                mesh.m_data = meshdata;

                var meshtree = new hknpCompressedMeshShapeTree();
                meshdata.m_meshTree = meshtree;
                meshtree.m_bitsPerKey = 5; // ?
                meshtree.m_domain = new hkAabb();
                meshtree.m_domain.m_min = new Vector4(bnodes[0].Min.X, bnodes[0].Min.Y, bnodes[0].Min.Z, 1.0f);
                meshtree.m_domain.m_max = new Vector4(bnodes[0].Max.X, bnodes[0].Max.Y, bnodes[0].Max.Z, 1.0f);
                meshtree.m_maxKeyValue = 30; // ?
                meshtree.m_nodes = bnodes[0].BuildAxis5Tree();
                meshtree.m_sharedVertices = sharedVerts;

                var bvh = meshdata.getMeshBVH();

                // Now let's process all the sections
                meshtree.m_packedVertices = new List<uint>();
                meshtree.m_primitiveDataRuns = new List<hknpCompressedMeshShapeTreeDataRun>();
                meshtree.m_sharedVerticesIndex = new List<ushort>();
                meshtree.m_primitives = new List<hkcdStaticMeshTreeBasePrimitive>();
                meshtree.m_sections = new List<hkcdStaticMeshTreeBaseSection>();
                foreach (var s in sections)
                {
                    var sharedindexbase = meshtree.m_sharedVerticesIndex.Count;
                    var packedvertbase = meshtree.m_packedVertices.Count;
                    var primitivesbase = meshtree.m_primitives.Count;

                    var sec = new hkcdStaticMeshTreeBaseSection();
                    var offset = s.SectionHead.Min;
                    var scale = (s.SectionHead.Max - s.SectionHead.Min) / new Vector3((float)0x7FF, (float)0x7FF, (float)0x3FF);
                    sec.m_codecParms_0 = offset.X;
                    sec.m_codecParms_1 = offset.Y;
                    sec.m_codecParms_2 = offset.Z;
                    sec.m_codecParms_3 = scale.X;
                    sec.m_codecParms_4 = scale.Y;
                    sec.m_codecParms_5 = scale.Z;
                    sec.m_domain = new hkAabb();
                    sec.m_domain.m_min = new Vector4(s.SectionHead.Min.X, s.SectionHead.Min.Y, s.SectionHead.Min.Z, 1.0f);
                    sec.m_domain.m_max = new Vector4(s.SectionHead.Max.X, s.SectionHead.Max.Y, s.SectionHead.Max.Z, 1.0f);

                    // Map the indices to either shared/packed verts and pack verts that need packing
                    var packedIndicesRemap = new Dictionary<ushort, byte>();
                    byte idxcounter = 0;
                    foreach (var idx in s.UsedIndices.OrderBy(x => x))
                    {
                        if (!shared.Contains(idx))
                        {
                            packedIndicesRemap.Add(idx, idxcounter);
                            var vert = verts[idx];
                            meshtree.m_packedVertices.Add(CompressPackedVertex(verts[idx], scale, offset));
                            var decomp = meshdata.DecompressPackedVertex(meshtree.m_packedVertices.Last(), scale, offset);
                            idxcounter++;
                        }
                    }
                    var sharedstart = idxcounter;
                    idxcounter = 0;
                    foreach (var idx in s.UsedIndices.OrderBy(x => x))
                    {
                        if (shared.Contains(idx))
                        {
                            packedIndicesRemap.Add(idx, (byte)(idxcounter + sharedstart));
                            meshtree.m_sharedVerticesIndex.Add(sharedIndexRemapTable[idx]);
                            idxcounter++;
                        }
                    }

                    sec.m_firstPackedVertex = (uint)packedvertbase;
                    sec.m_numSharedIndices = idxcounter;
                    sec.m_numPackedVertices = sharedstart;
                    sec.m_sharedVertices = new hkcdStaticMeshTreeBaseSectionSharedVertices();
                    sec.m_sharedVertices.m_data = (uint)(((uint)sharedstart & 0xFF) | ((uint)sharedindexbase << 8));

                    // Now pack the primitives
                    for (int i = 0; i < s.Indices.Count / 3; i++)
                    {
                        var p = new hkcdStaticMeshTreeBasePrimitive();
                        p.m_indices_0 = packedIndicesRemap[s.Indices[i * 3]];
                        p.m_indices_1 = packedIndicesRemap[s.Indices[i * 3 + 1]];
                        p.m_indices_2 = packedIndicesRemap[s.Indices[i * 3 + 2]];
                        p.m_indices_3 = p.m_indices_2;
                        meshtree.m_primitives.Add(p);
                    }
                    sec.m_primitives = new hkcdStaticMeshTreeBaseSectionPrimitives();
                    sec.m_primitives.m_data = (((uint)s.Indices.Count / 3) & 0xFF) | ((uint)primitivesbase << 8);

                    // Create a data run
                    sec.m_dataRuns = new hkcdStaticMeshTreeBaseSectionDataRuns();
                    sec.m_dataRuns.m_data = ((uint)meshtree.m_primitiveDataRuns.Count() << 8) | 1;
                    var run = new hknpCompressedMeshShapeTreeDataRun();
                    run.m_count = (byte)(s.Indices.Count / 3);
                    run.m_index = 0;
                    run.m_value = new hknpCompressedMeshShapeTreeDataRunData();
                    run.m_value.m_data = 65535;
                    meshtree.m_primitiveDataRuns.Add(run);

                    sec.m_nodes = s.SectionHead.BuildAxis4Tree();

                    meshtree.m_sections.Add(sec);
                }

                meshtree.m_numPrimitiveKeys = meshtree.m_primitives.Count;

                var simdtree = new hkcdSimdTree();
                meshdata.m_simdTree = simdtree;
                simdtree.m_nodes = new List<hkcdSimdTreeNode>();
                for (int i = 0; i < 2; i++)
                {
                    hkcdSimdTreeNode n = new hkcdSimdTreeNode();
                    n.m_data_0 = 0;
                    n.m_data_1 = 0;
                    n.m_data_2 = 0;
                    n.m_data_3 = 0;
                    float mi = -3.40282E+38F;
                    float ma = 3.40282E+38F;
                    n.m_hx = new Vector4(mi, mi, mi, mi);
                    n.m_hy = new Vector4(mi, mi, mi, mi);
                    n.m_hz = new Vector4(mi, mi, mi, mi);
                    n.m_lx = new Vector4(ma, ma, ma, ma);
                    n.m_ly = new Vector4(ma, ma, ma, ma);
                    n.m_lz = new Vector4(ma, ma, ma, ma);
                    simdtree.m_nodes.Add(n);
                }

                _meshes.Add(mesh);
            }
        }

        public hkRootLevelContainer CookCollision()
        {
            // Root container
            var root = new hkRootLevelContainer();
            root.m_namedVariants = new List<hkRootLevelContainerNamedVariant>();

            // Named variant
            var variant = new hkRootLevelContainerNamedVariant();
            variant.m_className = "hknpPhysicsSceneData";
            variant.m_name = "Physics Scene Data";
            root.m_namedVariants.Add(variant);

            // Physics scene data
            var physicsSceneData = new hknpPhysicsSceneData();
            variant.m_variant = physicsSceneData;
            physicsSceneData.m_systemDatas = new List<hknpPhysicsSystemData>();

            // Physics system data
            var physicsSystemData = new hknpPhysicsSystemData();
            physicsSceneData.m_systemDatas.Add(physicsSystemData);
            physicsSystemData.m_name = "Default Physics System Data";
            physicsSystemData.m_materials = new List<hknpMaterial>();
            physicsSystemData.m_bodyCinfos = new List<hknpBodyCinfo>();
            physicsSystemData.m_referencedObjects = new List<hkReferencedObject>();

            foreach (var m in _meshes)
            {
                var mat = BuildMaterial();
                var cbodyinfo = BuildBodyCInfo("l0000200");
                cbodyinfo.m_shape = m;
                physicsSystemData.m_referencedObjects.Add(m);
                physicsSystemData.m_materials.Add(mat);
                physicsSystemData.m_bodyCinfos.Add(cbodyinfo);
            }

            return root;
        }
    }
}
