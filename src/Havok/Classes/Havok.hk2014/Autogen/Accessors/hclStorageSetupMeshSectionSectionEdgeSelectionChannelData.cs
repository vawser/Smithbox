// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStorageSetupMeshSectionSectionEdgeSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStorageSetupMeshSection.SectionEdgeSelectionChannel>
{
    public hclStorageSetupMeshSectionSectionEdgeSelectionChannelData(Havok.hk2014.hclStorageSetupMeshSection.SectionEdgeSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeIndices":
            case "edgeIndices":
            {
                value = instance.m_edgeIndices;
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
            case "m_edgeIndices":
            case "edgeIndices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_edgeIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
