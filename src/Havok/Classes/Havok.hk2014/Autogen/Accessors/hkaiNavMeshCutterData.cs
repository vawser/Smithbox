// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshCutterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshCutter>
{
    public hkaiNavMeshCutterData(Havok.hk2014.hkaiNavMeshCutter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_meshInfos":
            case "meshInfos":
            {
                value = instance.m_meshInfos;
                return true;
            }
            case "m_connectivityInfo":
            case "connectivityInfo":
            {
                value = instance.m_connectivityInfo;
                return true;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                value = instance.m_streamingCollection;
                return true;
            }
            case "m_forceRecutFaceKeys":
            case "forceRecutFaceKeys":
            {
                value = instance.m_forceRecutFaceKeys;
                return true;
            }
            case "m_forceClearanceCalcFaceKeys":
            case "forceClearanceCalcFaceKeys":
            {
                value = instance.m_forceClearanceCalcFaceKeys;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_edgeMatchParams":
            case "edgeMatchParams":
            {
                value = instance.m_edgeMatchParams;
                return true;
            }
            case "m_cutEdgeTolerance":
            case "cutEdgeTolerance":
            {
                value = instance.m_cutEdgeTolerance;
                return true;
            }
            case "m_minEdgeMatchingLength":
            case "minEdgeMatchingLength":
            {
                value = instance.m_minEdgeMatchingLength;
                return true;
            }
            case "m_smallGapFixupTolerance":
            case "smallGapFixupTolerance":
            {
                value = instance.m_smallGapFixupTolerance;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                value = instance.m_performValidationChecks;
                return true;
            }
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                value = instance.m_clearanceResetMethod;
                return true;
            }
            case "m_recomputeClearanceAfterCutting":
            case "recomputeClearanceAfterCutting":
            {
                value = instance.m_recomputeClearanceAfterCutting;
                return true;
            }
            case "m_useNewCutter":
            case "useNewCutter":
            {
                value = instance.m_useNewCutter;
                return true;
            }
            case "m_domainQuantum":
            case "domainQuantum":
            {
                value = instance.m_domainQuantum;
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
            case "m_meshInfos":
            case "meshInfos":
            {
                if (value is not List<Havok.hk2014.hkaiNavMeshCutterMeshInfo> castValue) return false;
                instance.m_meshInfos = castValue;
                return true;
            }
            case "m_connectivityInfo":
            case "connectivityInfo":
            {
                if (value is not Havok.hk2014.hkaiNavMeshCutterSavedConnectivity castValue) return false;
                instance.m_connectivityInfo = castValue;
                return true;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_forceRecutFaceKeys":
            case "forceRecutFaceKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_forceRecutFaceKeys = castValue;
                return true;
            }
            case "m_forceClearanceCalcFaceKeys":
            case "forceClearanceCalcFaceKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_forceClearanceCalcFaceKeys = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_edgeMatchParams":
            case "edgeMatchParams":
            {
                if (value is not Havok.hk2014.hkaiNavMeshEdgeMatchingParameters castValue) return false;
                instance.m_edgeMatchParams = castValue;
                return true;
            }
            case "m_cutEdgeTolerance":
            case "cutEdgeTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_cutEdgeTolerance = castValue;
                return true;
            }
            case "m_minEdgeMatchingLength":
            case "minEdgeMatchingLength":
            {
                if (value is not float castValue) return false;
                instance.m_minEdgeMatchingLength = castValue;
                return true;
            }
            case "m_smallGapFixupTolerance":
            case "smallGapFixupTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_smallGapFixupTolerance = castValue;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                if (value is not bool castValue) return false;
                instance.m_performValidationChecks = castValue;
                return true;
            }
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                if (value is Havok.hk2014.hkaiNavMeshCutter.ClearanceResetMethod castValue)
                {
                    instance.m_clearanceResetMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_clearanceResetMethod = (Havok.hk2014.hkaiNavMeshCutter.ClearanceResetMethod)byteValue;
                    return true;
                }
                return false;
            }
            case "m_recomputeClearanceAfterCutting":
            case "recomputeClearanceAfterCutting":
            {
                if (value is not bool castValue) return false;
                instance.m_recomputeClearanceAfterCutting = castValue;
                return true;
            }
            case "m_useNewCutter":
            case "useNewCutter":
            {
                if (value is not bool castValue) return false;
                instance.m_useNewCutter = castValue;
                return true;
            }
            case "m_domainQuantum":
            case "domainQuantum":
            {
                if (value is not float castValue) return false;
                instance.m_domainQuantum = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
