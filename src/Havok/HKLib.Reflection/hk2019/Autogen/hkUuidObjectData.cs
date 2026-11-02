// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkUuidObjectData : HavokData<HKLib.hk2019.hkUuidObject> 
{
    public hkUuidObjectData(HavokType type, HKLib.hk2019.hkUuidObject instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uuid":
            case "uuid":
            {
                if (instance.m_uuid is not TGet castValue) return false;
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
            case "m_uuid":
            case "uuid":
            {
                if (value is not HKLib.hk2019.hkUuid castValue) return false;
                instance.m_uuid = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
