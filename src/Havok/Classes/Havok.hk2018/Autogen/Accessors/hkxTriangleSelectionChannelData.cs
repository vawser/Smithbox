// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxTriangleSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxTriangleSelectionChannel>
{
    public hkxTriangleSelectionChannelData(Havok.hk2018.hkxTriangleSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
