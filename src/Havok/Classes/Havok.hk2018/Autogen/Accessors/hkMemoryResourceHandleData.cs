// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMemoryResourceHandleData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMemoryResourceHandle>
{
    public hkMemoryResourceHandleData(Havok.hk2018.hkMemoryResourceHandle instance) : base(instance) { }

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
            case "m_variant":
            case "variant":
            {
                value = instance.m_variant;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_references":
            case "references":
            {
                value = instance.m_references;
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
            case "m_variant":
            case "variant":
            {
                if (value is null)
                {
                    instance.m_variant = default;
                    return true;
                }
                if (value is Havok.hk2018.hkReferencedObject castValue)
                {
                    instance.m_variant = castValue;
                    return true;
                }
                return false;
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
            case "m_references":
            case "references":
            {
                if (value is not List<Havok.hk2018.hkMemoryResourceHandle.ExternalLink> castValue) return false;
                instance.m_references = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
