// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxEdgeSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxEdgeSelectionChannel>
{
    public hkxEdgeSelectionChannelData(Havok.hk2019.hkxEdgeSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_selectedEdges":
            case "selectedEdges":
            {
                value = instance.m_selectedEdges;
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
