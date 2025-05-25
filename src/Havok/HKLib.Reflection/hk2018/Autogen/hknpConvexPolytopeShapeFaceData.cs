// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpConvexPolytopeShapeFaceData : HavokData<hknpConvexPolytopeShape.Face> 
{
    public hknpConvexPolytopeShapeFaceData(HavokType type, hknpConvexPolytopeShape.Face instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_firstIndex":
            case "firstIndex":
            {
                if (instance.m_firstIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                if (instance.m_numIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minHalfAngle":
            case "minHalfAngle":
            {
                if (instance.m_minHalfAngle is not TGet castValue) return false;
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
            case "m_firstIndex":
            case "firstIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_firstIndex = castValue;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                if (value is not byte castValue) return false;
                instance.m_numIndices = castValue;
                return true;
            }
            case "m_minHalfAngle":
            case "minHalfAngle":
            {
                if (value is not byte castValue) return false;
                instance.m_minHalfAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
