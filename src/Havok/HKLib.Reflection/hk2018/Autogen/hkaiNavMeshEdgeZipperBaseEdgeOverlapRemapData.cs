// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshEdgeZipper;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshEdgeZipperBaseEdgeOverlapRemapData : HavokData<BaseEdgeOverlapRemap> 
{
    public hkaiNavMeshEdgeZipperBaseEdgeOverlapRemapData(HavokType type, BaseEdgeOverlapRemap instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                if (instance.m_start is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (instance.m_end is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overlapRemap":
            case "overlapRemap":
            {
                if (instance.m_overlapRemap is not TGet castValue) return false;
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
            case "m_start":
            case "start":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            case "m_overlapRemap":
            case "overlapRemap":
            {
                if (value is not hkaiAffineMap1D castValue) return false;
                instance.m_overlapRemap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
