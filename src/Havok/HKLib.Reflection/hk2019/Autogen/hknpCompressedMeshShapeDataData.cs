// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpCompressedMeshShapeDataData : HavokData<HKLib.hk2019.hknpCompressedMeshShapeData> 
{
    public hknpCompressedMeshShapeDataData(HavokType type, HKLib.hk2019.hknpCompressedMeshShapeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_meshTree":
            case "meshTree":
            {
                if (instance.m_meshTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (instance.m_simdTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                if (instance.m_connectivity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasSimdTree":
            case "hasSimdTree":
            {
                if (instance.m_hasSimdTree is not TGet castValue) return false;
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
            case "m_meshTree":
            case "meshTree":
            {
                if (value is not HKLib.hk2019.hknpCompressedMeshShapeTree castValue) return false;
                instance.m_meshTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not HKLib.hk2019.hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                if (value is not HKLib.hk2019.hkcdStaticMeshTree.Connectivity castValue) return false;
                instance.m_connectivity = castValue;
                return true;
            }
            case "m_hasSimdTree":
            case "hasSimdTree":
            {
                if (value is not bool castValue) return false;
                instance.m_hasSimdTree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
