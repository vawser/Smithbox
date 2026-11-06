// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiDirectedGraphInstanceFreeBlockListData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiDirectedGraphInstanceFreeBlockList>
{
    public hkaiDirectedGraphInstanceFreeBlockListData(Havok.hk2014.hkaiDirectedGraphInstanceFreeBlockList instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blocks":
            case "blocks":
            {
                value = instance.m_blocks;
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
            case "m_blocks":
            case "blocks":
            {
                if (value is not List<int> castValue) return false;
                instance.m_blocks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
