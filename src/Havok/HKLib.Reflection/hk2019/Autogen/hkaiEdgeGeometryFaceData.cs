// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiEdgeGeometryFaceData : HavokData<HKLib.hk2019.hkaiEdgeGeometry.Face> 
{
    public hkaiEdgeGeometryFaceData(HavokType type, HKLib.hk2019.hkaiEdgeGeometry.Face instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceIndex":
            case "faceIndex":
            {
                if (instance.m_faceIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                if (value is not uint castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_faceIndex":
            case "faceIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_faceIndex = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is HKLib.hk2019.hkaiEdgeGeometry.FaceFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (HKLib.hk2019.hkaiEdgeGeometry.FaceFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
