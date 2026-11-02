// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxEdgeSelectionChannelData : HavokData<HKLib.hk2019.hkxEdgeSelectionChannel> 
{
    public hkxEdgeSelectionChannelData(HavokType type, HKLib.hk2019.hkxEdgeSelectionChannel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_selectedEdges":
            case "selectedEdges":
            {
                if (instance.m_selectedEdges is not TGet castValue) return false;
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
            case "m_selectedEdges":
            case "selectedEdges":
            {
                if (value is not List<int> castValue) return false;
                instance.m_selectedEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
