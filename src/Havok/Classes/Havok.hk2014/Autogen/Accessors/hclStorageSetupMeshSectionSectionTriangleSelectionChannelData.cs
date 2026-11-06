// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStorageSetupMeshSectionSectionTriangleSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStorageSetupMeshSection.SectionTriangleSelectionChannel>
{
    public hclStorageSetupMeshSectionSectionTriangleSelectionChannelData(Havok.hk2014.hclStorageSetupMeshSection.SectionTriangleSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
