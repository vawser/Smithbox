// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxBoneGroupDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxBoneGroupData>
{
    public hkxBoneGroupDataData(Havok.hk2019.hkxBoneGroupData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_groupNames":
            case "groupNames":
            {
                value = instance.m_groupNames;
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
            case "m_groupNames":
            case "groupNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_groupNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
