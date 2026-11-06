// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCompressedMeshShapeBigTriangleData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCompressedMeshShapeBigTriangle>
{
    public hkpCompressedMeshShapeBigTriangleData(Havok.hk2014.hkpCompressedMeshShapeBigTriangle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                value = instance.m_a;
                return true;
            }
            case "m_b":
            case "b":
            {
                value = instance.m_b;
                return true;
            }
            case "m_c":
            case "c":
            {
                value = instance.m_c;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_weldingInfo":
            case "weldingInfo":
            {
                value = instance.m_weldingInfo;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                value = instance.m_transformIndex;
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
            case "m_a":
            case "a":
            {
                if (value is not ushort castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not ushort castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_c":
            case "c":
            {
                if (value is not ushort castValue) return false;
                instance.m_c = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is not uint castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            case "m_weldingInfo":
            case "weldingInfo":
            {
                if (value is not ushort castValue) return false;
                instance.m_weldingInfo = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_transformIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
