// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxVertexSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxVertexSelectionChannel>
{
    public hkxVertexSelectionChannelData(Havok.hk55.hkxVertexSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_selectedVertices":
            case "selectedVertices":
            {
                value = instance.m_selectedVertices;
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
