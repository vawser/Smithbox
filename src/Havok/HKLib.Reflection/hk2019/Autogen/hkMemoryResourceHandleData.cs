// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkMemoryResourceHandleData : HavokData<HKLib.hk2019.hkMemoryResourceHandle> 
{
    public hkMemoryResourceHandleData(HavokType type, HKLib.hk2019.hkMemoryResourceHandle instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_variant":
            case "variant":
            {
                if (value is null)
                {
                    instance.m_variant = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkReferencedObject castValue)
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
                if (value is not List<HKLib.hk2019.hkMemoryResourceHandle.ExternalLink> castValue) return false;
                instance.m_references = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
