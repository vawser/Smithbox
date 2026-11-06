// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDefaultDynamicUserEdgeSetInfoSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDefaultDynamicUserEdgeSetInfo.Section>
{
    public hkaiDefaultDynamicUserEdgeSetInfoSectionData(Havok.hk2018.hkaiDefaultDynamicUserEdgeSetInfo.Section instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_externalEdges":
            case "externalEdges":
            {
                value = instance.m_externalEdges;
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
            case "m_externalEdges":
            case "externalEdges":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiDynamicUserEdgeSetInfo.ExternalEdges?> castValue) return false;
                instance.m_externalEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
