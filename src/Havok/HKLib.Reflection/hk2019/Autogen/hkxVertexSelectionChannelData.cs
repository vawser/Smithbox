// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxVertexSelectionChannelData : HavokData<HKLib.hk2019.hkxVertexSelectionChannel> 
{
    public hkxVertexSelectionChannelData(HavokType type, HKLib.hk2019.hkxVertexSelectionChannel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_selectedVertices":
            case "selectedVertices":
            {
                if (instance.m_selectedVertices is not TGet castValue) return false;
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
            case "m_selectedVertices":
            case "selectedVertices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_selectedVertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
