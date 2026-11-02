// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpWorldSnapshotConstraintGroupInfoData : HavokData<HKLib.hk2019.hknpWorldSnapshot.ConstraintGroupInfo> 
{
    public hknpWorldSnapshotConstraintGroupInfoData(HavokType type, HKLib.hk2019.hknpWorldSnapshot.ConstraintGroupInfo instance) : base(type, instance) {}

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
            case "m_multiplier":
            case "multiplier":
            {
                if (instance.m_multiplier is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hknpConstraintGroupId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                if (value is not byte castValue) return false;
                instance.m_multiplier = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
