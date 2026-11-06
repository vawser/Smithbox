// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclNamedSetupMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclNamedSetupMesh>
{
    public hclNamedSetupMeshData(Havok.hk2016.hclNamedSetupMesh instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_meshName":
            case "meshName":
            {
                value = instance.m_meshName;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
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
            case "m_meshName":
            case "meshName":
            {
                if (value is null)
                {
                    instance.m_meshName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_meshName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
