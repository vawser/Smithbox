// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStorageSetupMeshSectionSectionVertexSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStorageSetupMeshSection.SectionVertexSelectionChannel>
{
    public hclStorageSetupMeshSectionSectionVertexSelectionChannelData(Havok.hk2014.hclStorageSetupMeshSection.SectionVertexSelectionChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndices":
            case "vertexIndices":
            {
                value = instance.m_vertexIndices;
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
            case "m_vertexIndices":
            case "vertexIndices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_vertexIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
