// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMemoryResourceHandleData : HavokData<hkMemoryResourceHandle> 
{
    public hkMemoryResourceHandleData(HavokType type, hkMemoryResourceHandle instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variant":
            case "variant":
            {
                if (instance.m_variant is null)
                {
                    return true;
                }
                if (instance.m_variant is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_references":
            case "references":
            {
                if (instance.m_references is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkReferencedObject castValue)
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
                if (value is not List<hkMemoryResourceHandle.ExternalLink> castValue) return false;
                instance.m_references = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
