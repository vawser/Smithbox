// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpConvexHullEdgeData : HavokData<HKLib.hk2019.hknpConvexHull.Edge> 
{
    public hknpConvexHullEdgeData(HavokType type, HKLib.hk2019.hknpConvexHull.Edge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndex":
            case "faceIndex":
            {
                if (instance.m_faceIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (instance.m_edgeIndex is not TGet castValue) return false;
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
            case "m_faceIndex":
            case "faceIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_faceIndex = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_edgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
