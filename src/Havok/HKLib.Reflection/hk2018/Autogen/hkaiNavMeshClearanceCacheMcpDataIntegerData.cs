// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshClearanceCacheMcpDataIntegerData : HavokData<hkaiNavMeshClearanceCache.McpDataInteger> 
{
    public hkaiNavMeshClearanceCacheMcpDataIntegerData(HavokType type, hkaiNavMeshClearanceCache.McpDataInteger instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_interpolant":
            case "interpolant":
            {
                if (instance.m_interpolant is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearance":
            case "clearance":
            {
                if (instance.m_clearance is not TGet castValue) return false;
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
