// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSplitGenerationUtilsSettingsData : HavokData<hkaiSplitGenerationUtils.Settings> 
{
    public hkaiSplitGenerationUtilsSettingsData(HavokType type, hkaiSplitGenerationUtils.Settings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simplificationOptions":
            case "simplificationOptions":
            {
                if (instance.m_simplificationOptions is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_simplificationOptions is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_splitMethod":
            case "splitMethod":
            {
                if (instance.m_splitMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_splitMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_generateClusterGraphs":
            case "generateClusterGraphs":
            {
                if (instance.m_generateClusterGraphs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredFacesPerCluster":
            case "desiredFacesPerCluster":
            {
                if (instance.m_desiredFacesPerCluster is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_borderPreserveShrinkSize":
            case "borderPreserveShrinkSize":
            {
                if (instance.m_borderPreserveShrinkSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingEdgeMatchTolerance":
            case "streamingEdgeMatchTolerance":
            {
                if (instance.m_streamingEdgeMatchTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numX":
            case "numX":
            {
                if (instance.m_numX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numY":
            case "numY":
            {
                if (instance.m_numY is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSplits":
            case "maxSplits":
            {
                if (instance.m_maxSplits is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredTrisPerChunk":
            case "desiredTrisPerChunk":
            {
                if (instance.m_desiredTrisPerChunk is not TGet castValue) return false;
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
            case "m_simplificationOptions":
            case "simplificationOptions":
            {
                if (value is hkaiSplitGenerationUtils.SplitAndGenerateOptions castValue)
                {
                    instance.m_simplificationOptions = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_simplificationOptions = (hkaiSplitGenerationUtils.SplitAndGenerateOptions)byteValue;
                    return true;
                }
                return false;
            }
            case "m_splitMethod":
            case "splitMethod":
            {
                if (value is hkaiSplitGenerationUtils.SplitMethod castValue)
                {
                    instance.m_splitMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_splitMethod = (hkaiSplitGenerationUtils.SplitMethod)byteValue;
                    return true;
                }
                return false;
            }
            case "m_generateClusterGraphs":
            case "generateClusterGraphs":
            {
                if (value is not bool castValue) return false;
                instance.m_generateClusterGraphs = castValue;
                return true;
            }
            case "m_desiredFacesPerCluster":
            case "desiredFacesPerCluster":
            {
                if (value is not int castValue) return false;
                instance.m_desiredFacesPerCluster = castValue;
                return true;
            }
            case "m_borderPreserveShrinkSize":
            case "borderPreserveShrinkSize":
            {
                if (value is not float castValue) return false;
                instance.m_borderPreserveShrinkSize = castValue;
                return true;
            }
            case "m_streamingEdgeMatchTolerance":
            case "streamingEdgeMatchTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_streamingEdgeMatchTolerance = castValue;
                return true;
            }
            case "m_numX":
            case "numX":
            {
                if (value is not int castValue) return false;
                instance.m_numX = castValue;
                return true;
            }
            case "m_numY":
            case "numY":
            {
                if (value is not int castValue) return false;
                instance.m_numY = castValue;
                return true;
            }
            case "m_maxSplits":
            case "maxSplits":
            {
                if (value is not int castValue) return false;
                instance.m_maxSplits = castValue;
                return true;
            }
            case "m_desiredTrisPerChunk":
            case "desiredTrisPerChunk":
            {
                if (value is not int castValue) return false;
                instance.m_desiredTrisPerChunk = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
