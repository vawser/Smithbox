// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStorageSetupMeshSectionSectionVertexFloatChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStorageSetupMeshSection.SectionVertexFloatChannel>
{
    public hclStorageSetupMeshSectionSectionVertexFloatChannelData(Havok.hk2014.hclStorageSetupMeshSection.SectionVertexFloatChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexFloats":
            case "vertexFloats":
            {
                value = instance.m_vertexFloats;
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
            case "m_vertexFloats":
            case "vertexFloats":
            {
                if (value is not List<float> castValue) return false;
                instance.m_vertexFloats = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
