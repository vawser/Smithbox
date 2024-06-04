// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkSerialize.Detail;

namespace HKLib.Reflection.hk2018;

internal class hkSerializeDetailIdFromPointerData : HavokData<IdFromPointer> 
{
    public hkSerializeDetailIdFromPointerData(HavokType type, IdFromPointer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
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
            case "m_id":
            case "id":
            {
                if (value is not uint castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
