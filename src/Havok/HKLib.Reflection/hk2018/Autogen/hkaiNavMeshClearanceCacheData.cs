// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshClearanceCacheData : HavokData<hkaiNavMeshClearanceCache> 
{
    public hkaiNavMeshClearanceCacheData(HavokType type, hkaiNavMeshClearanceCache instance) : base(type, instance) {}

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
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                if (instance.m_clearanceCeiling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceIntToRealMultiplier":
            case "clearanceIntToRealMultiplier":
            {
                if (instance.m_clearanceIntToRealMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceRealToIntMultiplier":
            case "clearanceRealToIntMultiplier":
            {
                if (instance.m_clearanceRealToIntMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceOffsets":
            case "faceOffsets":
            {
                if (instance.m_faceOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgePairClearances":
            case "edgePairClearances":
            {
                if (instance.m_edgePairClearances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_unusedEdgePairElements":
            case "unusedEdgePairElements":
            {
                if (instance.m_unusedEdgePairElements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mcpData":
            case "mcpData":
            {
                if (instance.m_mcpData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexClearances":
            case "vertexClearances":
            {
                if (instance.m_vertexClearances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uncalculatedFacesLowerBound":
            case "uncalculatedFacesLowerBound":
            {
                if (instance.m_uncalculatedFacesLowerBound is not TGet castValue) return false;
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
                if (value is not List<hkaiNavMeshClearanceCache.McpDataInteger> castValue) return false;
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
