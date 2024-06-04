// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMinMaxQuadTreeData : HavokData<hknpMinMaxQuadTree> 
{
    public hknpMinMaxQuadTreeData(HavokType type, hknpMinMaxQuadTree instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_coarseTreeData":
            case "coarseTreeData":
            {
                if (instance.m_coarseTreeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
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
            case "m_invMultiplier":
            case "invMultiplier":
            {
                if (instance.m_invMultiplier is not TGet castValue) return false;
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
            case "m_coarseTreeData":
            case "coarseTreeData":
            {
                if (value is not List<hknpMinMaxQuadTree.MinMaxLevel> castValue) return false;
                instance.m_coarseTreeData = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                if (value is not float castValue) return false;
                instance.m_multiplier = castValue;
                return true;
            }
            case "m_invMultiplier":
            case "invMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_invMultiplier = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
