// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpMinMaxQuadTreeMinMaxLevelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpMinMaxQuadTree.MinMaxLevel>
{
    public hknpMinMaxQuadTreeMinMaxLevelData(Havok.hk2014.hknpMinMaxQuadTree.MinMaxLevel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minMaxData":
            case "minMaxData":
            {
                value = instance.m_minMaxData;
                return true;
            }
            case "m_xRes":
            case "xRes":
            {
                value = instance.m_xRes;
                return true;
            }
            case "m_zRes":
            case "zRes":
            {
                value = instance.m_zRes;
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
            case "m_minMaxData":
            case "minMaxData":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_minMaxData = castValue;
                return true;
            }
            case "m_xRes":
            case "xRes":
            {
                if (value is not ushort castValue) return false;
                instance.m_xRes = castValue;
                return true;
            }
            case "m_zRes":
            case "zRes":
            {
                if (value is not ushort castValue) return false;
                instance.m_zRes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
