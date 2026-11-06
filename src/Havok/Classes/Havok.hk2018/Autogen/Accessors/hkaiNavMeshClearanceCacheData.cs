// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshClearanceCacheData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshClearanceCache>
{
    public hkaiNavMeshClearanceCacheData(Havok.hk2018.hkaiNavMeshClearanceCache instance) : base(instance) { }

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
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                value = instance.m_clearanceCeiling;
                return true;
            }
            case "m_clearanceIntToRealMultiplier":
            case "clearanceIntToRealMultiplier":
            {
                value = instance.m_clearanceIntToRealMultiplier;
                return true;
            }
            case "m_clearanceRealToIntMultiplier":
            case "clearanceRealToIntMultiplier":
            {
                value = instance.m_clearanceRealToIntMultiplier;
                return true;
            }
            case "m_faceOffsets":
            case "faceOffsets":
            {
                value = instance.m_faceOffsets;
                return true;
            }
            case "m_edgePairClearances":
            case "edgePairClearances":
            {
                value = instance.m_edgePairClearances;
                return true;
            }
            case "m_unusedEdgePairElements":
            case "unusedEdgePairElements":
            {
                value = instance.m_unusedEdgePairElements;
                return true;
            }
            case "m_mcpData":
            case "mcpData":
            {
                value = instance.m_mcpData;
                return true;
            }
            case "m_vertexClearances":
            case "vertexClearances":
            {
                value = instance.m_vertexClearances;
                return true;
            }
            case "m_uncalculatedFacesLowerBound":
            case "uncalculatedFacesLowerBound":
            {
                value = instance.m_uncalculatedFacesLowerBound;
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
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                if (value is not float castValue) return false;
                instance.m_clearanceCeiling = castValue;
                return true;
            }
            case "m_clearanceIntToRealMultiplier":
            case "clearanceIntToRealMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_clearanceIntToRealMultiplier = castValue;
                return true;
            }
            case "m_clearanceRealToIntMultiplier":
            case "clearanceRealToIntMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_clearanceRealToIntMultiplier = castValue;
                return true;
            }
            case "m_faceOffsets":
            case "faceOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_faceOffsets = castValue;
                return true;
            }
            case "m_edgePairClearances":
            case "edgePairClearances":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_edgePairClearances = castValue;
                return true;
            }
            case "m_unusedEdgePairElements":
            case "unusedEdgePairElements":
            {
                if (value is not int castValue) return false;
                instance.m_unusedEdgePairElements = castValue;
                return true;
            }
            case "m_mcpData":
            case "mcpData":
            {
                if (value is not List<Havok.hk2018.hkaiNavMeshClearanceCache.McpDataInteger> castValue) return false;
                instance.m_mcpData = castValue;
                return true;
            }
            case "m_vertexClearances":
            case "vertexClearances":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_vertexClearances = castValue;
                return true;
            }
            case "m_uncalculatedFacesLowerBound":
            case "uncalculatedFacesLowerBound":
            {
                if (value is not int castValue) return false;
                instance.m_uncalculatedFacesLowerBound = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
