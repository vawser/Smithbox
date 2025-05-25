// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaMeshBindingMappingData : HavokData<hkaMeshBinding.Mapping> 
{
    public hkaMeshBindingMappingData(HavokType type, hkaMeshBinding.Mapping instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mapping":
            case "mapping":
            {
                if (instance.m_mapping is not TGet castValue) return false;
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
            case "m_mapping":
            case "mapping":
            {
                if (value is not List<short> castValue) return false;
                instance.m_mapping = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
