// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMaterialDescriptorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMaterialDescriptor>
{
    public hknpMaterialDescriptorData(Havok.hk2019.hknpMaterialDescriptor instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                value = instance.m_materialId;
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
            case "m_material":
            case "material":
            {
                if (value is null)
                {
                    instance.m_material = default;
                    return true;
                }
                if (value is Havok.hk2019.hknpRefMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_materialId":
            case "materialId":
            {
                if (value is not ushort castValue) return false;
                instance.m_materialId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
