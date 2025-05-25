// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStorageSetupMeshSectionSectionEdgeSelectionChannelData : HavokData<hclStorageSetupMeshSection.SectionEdgeSelectionChannel> 
{
    public hclStorageSetupMeshSectionSectionEdgeSelectionChannelData(HavokType type, hclStorageSetupMeshSection.SectionEdgeSelectionChannel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeIndices":
            case "edgeIndices":
            {
                if (instance.m_edgeIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
