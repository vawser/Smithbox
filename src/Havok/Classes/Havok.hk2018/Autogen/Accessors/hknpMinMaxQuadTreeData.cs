// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpMinMaxQuadTreeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpMinMaxQuadTree>
{
    public hknpMinMaxQuadTreeData(Havok.hk2018.hknpMinMaxQuadTree instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_coarseTreeData":
            case "coarseTreeData":
            {
                value = instance.m_coarseTreeData;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                value = instance.m_multiplier;
                return true;
            }
            case "m_invMultiplier":
            case "invMultiplier":
            {
                value = instance.m_invMultiplier;
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
            case "m_coarseTreeData":
            case "coarseTreeData":
            {
                if (value is not List<Havok.hk2018.hknpMinMaxQuadTree.MinMaxLevel> castValue) return false;
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
