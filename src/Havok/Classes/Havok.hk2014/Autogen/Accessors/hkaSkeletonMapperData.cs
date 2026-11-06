// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaSkeletonMapperData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaSkeletonMapper>
{
    public hkaSkeletonMapperData(Havok.hk2014.hkaSkeletonMapper instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mapping":
            case "mapping":
            {
                value = instance.m_mapping;
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
            case "m_mapping":
            case "mapping":
            {
                if (value is not Havok.hk2014.hkaSkeletonMapperData castValue) return false;
                instance.m_mapping = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
