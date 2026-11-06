// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpPairCollisionFilterMapPairFilterKeyOverrideTypeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpPairCollisionFilter.MapPairFilterKeyOverrideType>
{
    public hknpPairCollisionFilterMapPairFilterKeyOverrideTypeData(Havok.hk2014.hknpPairCollisionFilter.MapPairFilterKeyOverrideType instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numElems":
            case "numElems":
            {
                value = instance.m_numElems;
                return true;
            }
            case "m_hashMod":
            case "hashMod":
            {
                value = instance.m_hashMod;
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
