// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;

namespace HKLib.Reflection.hk2018;

internal class hknpCompressedMeshShapeDataData : HavokData<HKLib.hk2018.hknpCompressedMeshShapeData> 
{
    public hknpCompressedMeshShapeDataData(HavokType type, HKLib.hk2018.hknpCompressedMeshShapeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_meshTree":
            case "meshTree":
            {
                if (value is not hknpCompressedMeshShapeTree castValue) return false;
                instance.m_meshTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                if (value is not Connectivity castValue) return false;
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
