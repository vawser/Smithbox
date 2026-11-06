// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpWorldSnapshotConstraintGroupInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpWorldSnapshot.ConstraintGroupInfo>
{
    public hknpWorldSnapshotConstraintGroupInfoData(Havok.hk2019.hknpWorldSnapshot.ConstraintGroupInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                value = instance.m_multiplier;
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
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2019.hknpConstraintGroupId castValue) return false;
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
