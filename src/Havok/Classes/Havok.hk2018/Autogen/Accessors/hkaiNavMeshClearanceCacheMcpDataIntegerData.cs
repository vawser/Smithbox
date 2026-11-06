// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshClearanceCacheMcpDataIntegerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshClearanceCache.McpDataInteger>
{
    public hkaiNavMeshClearanceCacheMcpDataIntegerData(Havok.hk2018.hkaiNavMeshClearanceCache.McpDataInteger instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_interpolant":
            case "interpolant":
            {
                value = instance.m_interpolant;
                return true;
            }
            case "m_clearance":
            case "clearance":
            {
                value = instance.m_clearance;
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
            case "m_interpolant":
            case "interpolant":
            {
                if (value is not byte castValue) return false;
                instance.m_interpolant = castValue;
                return true;
            }
            case "m_clearance":
            case "clearance":
            {
                if (value is not byte castValue) return false;
                instance.m_clearance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
