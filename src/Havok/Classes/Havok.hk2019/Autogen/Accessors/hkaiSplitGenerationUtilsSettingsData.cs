// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiSplitGenerationUtilsSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiSplitGenerationUtils.Settings>
{
    public hkaiSplitGenerationUtilsSettingsData(Havok.hk2019.hkaiSplitGenerationUtils.Settings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simplificationOptions":
            case "simplificationOptions":
            {
                value = instance.m_simplificationOptions;
                return true;
            }
            case "m_splitMethod":
            case "splitMethod":
            {
                value = instance.m_splitMethod;
                return true;
            }
            case "m_generateClusterGraphs":
            case "generateClusterGraphs":
            {
                value = instance.m_generateClusterGraphs;
                return true;
            }
            case "m_desiredFacesPerCluster":
            case "desiredFacesPerCluster":
            {
                value = instance.m_desiredFacesPerCluster;
                return true;
            }
            case "m_borderPreserveShrinkSize":
            case "borderPreserveShrinkSize":
            {
                value = instance.m_borderPreserveShrinkSize;
                return true;
            }
            case "m_streamingEdgeMatchTolerance":
            case "streamingEdgeMatchTolerance":
            {
                value = instance.m_streamingEdgeMatchTolerance;
                return true;
            }
            case "m_numX":
            case "numX":
            {
                value = instance.m_numX;
                return true;
            }
            case "m_numY":
            case "numY":
            {
                value = instance.m_numY;
                return true;
            }
            case "m_maxSplits":
            case "maxSplits":
            {
                value = instance.m_maxSplits;
                return true;
            }
            case "m_desiredTrisPerChunk":
            case "desiredTrisPerChunk":
            {
                value = instance.m_desiredTrisPerChunk;
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
            case "m_simplificationOptions":
            case "simplificationOptions":
            {
                if (value is Havok.hk2019.hkaiSplitGenerationUtils.SplitAndGenerateOptions castValue)
                {
                    instance.m_simplificationOptions = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_simplificationOptions = (Havok.hk2019.hkaiSplitGenerationUtils.SplitAndGenerateOptions)byteValue;
                    return true;
                }
                return false;
            }
            case "m_splitMethod":
            case "splitMethod":
            {
                if (value is Havok.hk2019.hkaiSplitGenerationUtils.SplitMethod castValue)
                {
                    instance.m_splitMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_splitMethod = (Havok.hk2019.hkaiSplitGenerationUtils.SplitMethod)byteValue;
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
