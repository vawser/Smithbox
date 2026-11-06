// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiEdgeGeometryFaceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiEdgeGeometry.Face>
{
    public hkaiEdgeGeometryFaceData(Havok.hk2019.hkaiEdgeGeometry.Face instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_faceIndex":
            case "faceIndex":
            {
                value = instance.m_faceIndex;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
                if (value is Havok.hk2019.hkaiEdgeGeometry.FaceFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2019.hkaiEdgeGeometry.FaceFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
