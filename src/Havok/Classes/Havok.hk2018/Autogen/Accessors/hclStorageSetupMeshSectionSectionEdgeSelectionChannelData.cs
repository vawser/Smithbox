// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStorageSetupMeshSectionSectionEdgeSelectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStorageSetupMeshSection.SectionEdgeSelectionChannel>
{
    public hclStorageSetupMeshSectionSectionEdgeSelectionChannelData(Havok.hk2018.hclStorageSetupMeshSection.SectionEdgeSelectionChannel instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
