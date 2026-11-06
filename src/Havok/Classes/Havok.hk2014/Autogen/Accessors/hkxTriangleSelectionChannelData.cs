// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxTriangleSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxTriangleSelectionChannel>
{
    public hkxTriangleSelectionChannelData(Havok.hk2014.hkxTriangleSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_selectedTriangles":
            case "selectedTriangles":
            {
                value = instance.m_selectedTriangles;
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
            case "m_selectedTriangles":
            case "selectedTriangles":
            {
                if (value is not List<int> castValue) return false;
                instance.m_selectedTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
