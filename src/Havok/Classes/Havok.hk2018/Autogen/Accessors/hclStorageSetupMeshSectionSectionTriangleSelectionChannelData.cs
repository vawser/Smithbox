// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStorageSetupMeshSectionSectionTriangleSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStorageSetupMeshSection.SectionTriangleSelectionChannel>
{
    public hclStorageSetupMeshSectionSectionTriangleSelectionChannelData(Havok.hk2018.hclStorageSetupMeshSection.SectionTriangleSelectionChannel instance) : base(instance) { }

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
            case "m_triangleIndices":
            case "triangleIndices":
            {
                value = instance.m_triangleIndices;
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
            case "m_triangleIndices":
            case "triangleIndices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_triangleIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
