// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkMemoryResourceContainerData : HavokData<HKLib.hk2019.hkMemoryResourceContainer> 
{
    public hkMemoryResourceContainerData(HavokType type, HKLib.hk2019.hkMemoryResourceContainer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_resourceHandles":
            case "resourceHandles":
            {
                if (instance.m_resourceHandles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is not TGet castValue) return false;
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
            case "m_resourceHandles":
            case "resourceHandles":
            {
                if (value is not List<HKLib.hk2019.hkMemoryResourceHandle?> castValue) return false;
                instance.m_resourceHandles = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not List<HKLib.hk2019.hkMemoryResourceContainer?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
