// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshCutterData : HavokData<hkaiNavMeshCutter> 
{
    public hkaiNavMeshCutterData(HavokType type, hkaiNavMeshCutter instance) : base(type, instance) {}

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
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (instance.m_streamingCollection is null)
                {
                    return true;
                }
                if (instance.m_streamingCollection is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_instanceCutters":
            case "instanceCutters":
            {
                if (instance.m_instanceCutters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_compactingState":
            case "compactingState":
            {
                if (instance.m_compactingState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeMatchParams":
            case "edgeMatchParams":
            {
                if (instance.m_edgeMatchParams is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cutEdgeTolerance":
            case "cutEdgeTolerance":
            {
                if (instance.m_cutEdgeTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minEdgeMatchingLength":
            case "minEdgeMatchingLength":
            {
                if (instance.m_minEdgeMatchingLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                if (instance.m_clearanceResetMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_clearanceResetMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_smallGapFixupTolerance":
            case "smallGapFixupTolerance":
            {
                if (instance.m_smallGapFixupTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                if (instance.m_performValidationChecks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxGarbageRatio":
            case "maxGarbageRatio":
            {
                if (instance.m_maxGarbageRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_domainQuantum":
            case "domainQuantum":
            {
                if (instance.m_domainQuantum is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultDynUserEdgeSetInfo":
            case "defaultDynUserEdgeSetInfo":
            {
                if (instance.m_defaultDynUserEdgeSetInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynUserEdgeSetInfos":
            case "dynUserEdgeSetInfos":
            {
                if (instance.m_dynUserEdgeSetInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                if (instance.m_modifiedSections is null)
                {
                    return true;
                }
                if (instance.m_modifiedSections is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_instanceCutters":
            case "instanceCutters":
            {
                if (value is not List<hkaiNavMeshInstanceCutter?> castValue) return false;
                instance.m_instanceCutters = castValue;
                return true;
            }
            case "m_compactingState":
            case "compactingState":
            {
                if (value is not hkaiNavMeshCompactUtils.CompactingState castValue) return false;
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
                if (value is not hkaiNavMeshEdgeMatchingParameters castValue) return false;
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
                if (value is hkaiWorld.ClearanceResetMethod castValue)
                {
                    instance.m_clearanceResetMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_clearanceResetMethod = (hkaiWorld.ClearanceResetMethod)byteValue;
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
                if (value is not hkaiDefaultDynamicUserEdgeSetInfo castValue) return false;
                instance.m_defaultDynUserEdgeSetInfo = castValue;
                return true;
            }
            case "m_dynUserEdgeSetInfos":
            case "dynUserEdgeSetInfos":
            {
                if (value is not hkHashMap<hkHandle<uint>, hkaiDynamicUserEdgeSetInfo> castValue) return false;
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
                if (value is hkaiModifiedSections castValue)
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
