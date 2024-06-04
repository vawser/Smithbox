// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkRootLevelContainerData : HavokData<hkRootLevelContainer> 
{
    public hkRootLevelContainerData(HavokType type, hkRootLevelContainer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_namedVariants":
            case "namedVariants":
            {
                if (instance.m_namedVariants is not TGet castValue) return false;
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
            case "m_namedVariants":
            case "namedVariants":
            {
                if (value is not List<hkRootLevelContainer.NamedVariant> castValue) return false;
                instance.m_namedVariants = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
