// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshCutterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshCutter>
{
    public hkaiNavMeshCutterData(Havok.hk2019.hkaiNavMeshCutter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_streamingCollection":
            case "streamingCollection":
            {
                value = instance.m_streamingCollection;
                return true;
            }
            case "m_instanceCutters":
            case "instanceCutters":
            {
                value = instance.m_instanceCutters;
                return true;
            }
            case "m_compactingState":
            case "compactingState":
            {
                value = instance.m_compactingState;
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
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                value = instance.m_clearanceResetMethod;
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
            case "m_maxGarbageRatio":
            case "maxGarbageRatio":
            {
                value = instance.m_maxGarbageRatio;
                return true;
            }
            case "m_domainQuantum":
            case "domainQuantum":
            {
                value = instance.m_domainQuantum;
                return true;
            }
            case "m_defaultDynUserEdgeSetInfo":
            case "defaultDynUserEdgeSetInfo":
            {
                value = instance.m_defaultDynUserEdgeSetInfo;
                return true;
            }
            case "m_dynUserEdgeSetInfos":
            case "dynUserEdgeSetInfos":
            {
                value = instance.m_dynUserEdgeSetInfos;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                value = instance.m_modifiedSections;
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
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_instanceCutters":
            case "instanceCutters":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshInstanceCutter?> castValue) return false;
                instance.m_instanceCutters = castValue;
                return true;
            }
            case "m_compactingState":
            case "compactingState":
            {
                if (value is not Havok.hk2019.hkaiNavMeshCompactUtils.CompactingState castValue) return false;
                instance.m_compactingState = castValue;
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
                if (value is not Havok.hk2019.hkaiNavMeshEdgeMatchingParameters castValue) return false;
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
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                if (value is Havok.hk2019.hkaiWorld.ClearanceResetMethod castValue)
                {
                    instance.m_clearanceResetMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_clearanceResetMethod = (Havok.hk2019.hkaiWorld.ClearanceResetMethod)byteValue;
                    return true;
                }
                return false;
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
            case "m_maxGarbageRatio":
            case "maxGarbageRatio":
            {
                if (value is not float castValue) return false;
                instance.m_maxGarbageRatio = castValue;
                return true;
            }
            case "m_domainQuantum":
            case "domainQuantum":
            {
                if (value is not float castValue) return false;
                instance.m_domainQuantum = castValue;
                return true;
            }
            case "m_defaultDynUserEdgeSetInfo":
            case "defaultDynUserEdgeSetInfo":
            {
                if (value is not Havok.hk2019.hkaiDefaultDynamicUserEdgeSetInfo castValue) return false;
                instance.m_defaultDynUserEdgeSetInfo = castValue;
                return true;
            }
            case "m_dynUserEdgeSetInfos":
            case "dynUserEdgeSetInfos":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkHandle<uint>, Havok.hk2019.hkaiDynamicUserEdgeSetInfo> castValue) return false;
                instance.m_dynUserEdgeSetInfos = castValue;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                if (value is null)
                {
                    instance.m_modifiedSections = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiModifiedSections castValue)
                {
                    instance.m_modifiedSections = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
