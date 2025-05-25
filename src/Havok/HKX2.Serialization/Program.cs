using SoulsFormats;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;
using HKX2;

namespace HKX2ReadWrite
{
    class Program
    {
        static void Main(string[] args)
        {
            var hkxpath = args[0];
            using var file = MemoryMappedFile.CreateFromFile(hkxpath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            var des = new HKX2.PackFileDeserializer();
            var root = (hkRootLevelContainer)des.Deserialize(br);

            // Strip some stuff
            /*var v = (hknpPhysicsSceneData)root.m_namedVariants[0].m_variant;
                foreach (fsnpCustomParamCompressedMeshShape s in v.m_systemDatas[0].m_referencedObjects)
                {
                    var bvh = s.m_data.getMeshBVH();
                    s.m_triangleIndexToShapeKey = null;
                    s.m_pParam = null;
                    s.m_edgeWeldingMap.m_primaryKeyToIndex = null;
                    s.m_edgeWeldingMap.m_secondaryKeyMask = 0;
                    s.m_edgeWeldingMap.m_sencondaryKeyBits = 0;
                    s.m_edgeWeldingMap.m_valueAndSecondaryKeys = null;
                    s.m_quadIsFlat.m_storage.m_numBits = 0;
                    s.m_quadIsFlat.m_storage.m_words = null;
                    s.m_triangleIsInterior.m_storage.m_numBits = 0;
                    s.m_triangleIsInterior.m_storage.m_words = null;
                    s.m_numShapeKeyBits = 0;
                    s.m_data.m_meshTree.m_bitsPerKey = 0;
                    s.m_data.m_meshTree.m_maxKeyValue = 0;
                    s.m_data.m_meshTree.m_bitsPerKey = 0;
                    s.m_data.m_meshTree.m_numPrimitiveKeys = 0;
                    s.m_data.m_meshTree.m_domain.m_min.W = 1.0f;
                    s.m_data.m_meshTree.m_domain.m_max.W = 1.0f;
                    s.m_data.m_simdTree.m_nodes = null;
                    foreach (var sec in s.m_data.m_meshTree.m_sections)
                    {
                        sec.m_leafIndex = 0;
                        sec.m_flags = 0;
                        var sbvh = sec.getSectionBVH();
                    }
                }*/

            using (FileStream s2 = File.Create(args[1]))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, s2);
                var s = new HKX2.PackFileSerializer();
                s.Serialize(root, bw);
            }
        }
    }
}
