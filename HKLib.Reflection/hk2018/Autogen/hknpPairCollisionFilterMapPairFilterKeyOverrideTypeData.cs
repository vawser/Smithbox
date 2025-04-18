// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpPairCollisionFilterMapPairFilterKeyOverrideTypeData : HavokData<hknpPairCollisionFilter.MapPairFilterKeyOverrideType> 
{
    public hknpPairCollisionFilterMapPairFilterKeyOverrideTypeData(HavokType type, hknpPairCollisionFilter.MapPairFilterKeyOverrideType instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numElems":
            case "numElems":
            {
                if (instance.m_numElems is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hashMod":
            case "hashMod":
            {
                if (instance.m_hashMod is not TGet castValue) return false;
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
            case "m_numElems":
            case "numElems":
            {
                if (value is not int castValue) return false;
                instance.m_numElems = castValue;
                return true;
            }
            case "m_hashMod":
            case "hashMod":
            {
                if (value is not int castValue) return false;
                instance.m_hashMod = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
