// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpPairCollisionFilterData : HavokData<HKLib.hk2019.hknpPairCollisionFilter> 
{
    public hknpPairCollisionFilterData(HavokType type, HKLib.hk2019.hknpPairCollisionFilter instance) : base(type, instance) {}

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
            case "m_childFilter":
            case "childFilter":
            {
                if (instance.m_childFilter is null)
                {
                    return true;
                }
                if (instance.m_childFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_childFilter":
            case "childFilter":
            {
                if (value is null)
                {
                    instance.m_childFilter = default;
                    return true;
                }
                if (value is HKLib.hk2019.hknpCollisionFilter castValue)
                {
                    instance.m_childFilter = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
