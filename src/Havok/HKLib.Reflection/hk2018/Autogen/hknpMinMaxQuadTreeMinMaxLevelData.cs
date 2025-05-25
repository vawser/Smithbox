// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMinMaxQuadTreeMinMaxLevelData : HavokData<hknpMinMaxQuadTree.MinMaxLevel> 
{
    public hknpMinMaxQuadTreeMinMaxLevelData(HavokType type, hknpMinMaxQuadTree.MinMaxLevel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minMaxData":
            case "minMaxData":
            {
                if (instance.m_minMaxData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_xRes":
            case "xRes":
            {
                if (instance.m_xRes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_zRes":
            case "zRes":
            {
                if (instance.m_zRes is not TGet castValue) return false;
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
