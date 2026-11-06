// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMemoryMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMemoryMeshShape>
{
    public hkMemoryMeshShapeData(Havok.hk2018.hkMemoryMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
                return true;
            }
            case "m_indices16":
            case "indices16":
            {
                value = instance.m_indices16;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                value = instance.m_indices32;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2018.hkMemoryMeshShape.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_indices16":
            case "indices16":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_indices16 = castValue;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_indices32 = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
