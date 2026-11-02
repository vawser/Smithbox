// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpGroupCollisionFilterData : HavokData<HKLib.hk2019.hknpGroupCollisionFilter> 
{
    private static readonly System.Reflection.FieldInfo _collisionLookupTableInfo = typeof(HKLib.hk2019.hknpGroupCollisionFilter).GetField("m_collisionLookupTable")!;
    public hknpGroupCollisionFilterData(HavokType type, HKLib.hk2019.hknpGroupCollisionFilter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_nextFreeSystemGroup":
            case "nextFreeSystemGroup":
            {
                if (instance.m_nextFreeSystemGroup is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionLookupTable":
            case "collisionLookupTable":
            {
                if (instance.m_collisionLookupTable is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hknpCollisionFilter.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (HKLib.hk2019.hknpCollisionFilter.Type)byteValue;
                    return true;
                }
                return false;
            }
            case "m_nextFreeSystemGroup":
            case "nextFreeSystemGroup":
            {
                if (value is not int castValue) return false;
                instance.m_nextFreeSystemGroup = castValue;
                return true;
            }
            case "m_collisionLookupTable":
            case "collisionLookupTable":
            {
                if (value is not uint[] castValue || castValue.Length != 32) return false;
                try
                {
                    _collisionLookupTableInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
