// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiUserEdgeSetupArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiUserEdgeSetupArray>
{
    public hkaiUserEdgeSetupArrayData(Havok.hk2014.hkaiUserEdgeSetupArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeSetups":
            case "edgeSetups":
            {
                value = instance.m_edgeSetups;
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
            case "m_edgeSetups":
            case "edgeSetups":
            {
                if (value is not List<Havok.hk2014.hkaiUserEdgeUtils.UserEdgeSetup> castValue) return false;
                instance.m_edgeSetups = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
