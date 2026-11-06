// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiEdgePathFollowingCornerInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiEdgePath.FollowingCornerInfo>
{
    public hkaiEdgePathFollowingCornerInfoData(Havok.hk2018.hkaiEdgePath.FollowingCornerInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_data":
            case "data":
            {
                if (value is not ushort castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
