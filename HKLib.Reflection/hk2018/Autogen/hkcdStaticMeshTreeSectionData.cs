// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkcdCompressedAabbCodecs;
using HKLib.hk2018.hkcdStaticMeshTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdStaticMeshTreeSectionData : HavokData<Section> 
{
    private static readonly System.Reflection.FieldInfo _codecParmsInfo = typeof(Section).GetField("m_codecParms")!;
    public hkcdStaticMeshTreeSectionData(HavokType type, Section instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                if (instance.m_nodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (instance.m_domain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_codecParms":
            case "codecParms":
            {
                if (instance.m_codecParms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstPackedVertexIndex":
            case "firstPackedVertexIndex":
            {
                if (instance.m_firstPackedVertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstSharedVertexIndex":
            case "firstSharedVertexIndex":
            {
                if (instance.m_firstSharedVertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstPrimitiveIndex":
            case "firstPrimitiveIndex":
            {
                if (instance.m_firstPrimitiveIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstDataRunIndex":
            case "firstDataRunIndex":
            {
                if (instance.m_firstDataRunIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPackedVertices":
            case "numPackedVertices":
            {
                if (instance.m_numPackedVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                if (instance.m_numPrimitives is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numDataRuns":
            case "numDataRuns":
            {
                if (instance.m_numDataRuns is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_page":
            case "page":
            {
                if (instance.m_page is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leafIndex":
            case "leafIndex":
            {
                if (instance.m_leafIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerData":
            case "layerData":
            {
                if (instance.m_layerData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Aabb4BytesCodec> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            case "m_codecParms":
            case "codecParms":
            {
                if (value is not float[] castValue || castValue.Length != 6) return false;
                try
                {
                    _codecParmsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_firstPackedVertexIndex":
            case "firstPackedVertexIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstPackedVertexIndex = castValue;
                return true;
            }
            case "m_firstSharedVertexIndex":
            case "firstSharedVertexIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstSharedVertexIndex = castValue;
                return true;
            }
            case "m_firstPrimitiveIndex":
            case "firstPrimitiveIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstPrimitiveIndex = castValue;
                return true;
            }
            case "m_firstDataRunIndex":
            case "firstDataRunIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstDataRunIndex = castValue;
                return true;
            }
            case "m_numPackedVertices":
            case "numPackedVertices":
            {
                if (value is not byte castValue) return false;
                instance.m_numPackedVertices = castValue;
                return true;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                if (value is not byte castValue) return false;
                instance.m_numPrimitives = castValue;
                return true;
            }
            case "m_numDataRuns":
            case "numDataRuns":
            {
                if (value is not byte castValue) return false;
                instance.m_numDataRuns = castValue;
                return true;
            }
            case "m_page":
            case "page":
            {
                if (value is not byte castValue) return false;
                instance.m_page = castValue;
                return true;
            }
            case "m_leafIndex":
            case "leafIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_leafIndex = castValue;
                return true;
            }
            case "m_layerData":
            case "layerData":
            {
                if (value is not byte castValue) return false;
                instance.m_layerData = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not byte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
