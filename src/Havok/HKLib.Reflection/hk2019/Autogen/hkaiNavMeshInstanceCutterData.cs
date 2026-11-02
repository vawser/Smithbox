// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshInstanceCutterData : HavokData<HKLib.hk2019.hkaiNavMeshInstanceCutter> 
{
    public hkaiNavMeshInstanceCutterData(HavokType type, HKLib.hk2019.hkaiNavMeshInstanceCutter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_sectionIdx":
            case "sectionIdx":
            {
                if (instance.m_sectionIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cutConfiguration":
            case "cutConfiguration":
            {
                if (instance.m_cutConfiguration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_addStreamingFaces":
            case "addStreamingFaces":
            {
                if (instance.m_addStreamingFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_removeStreamingFaces":
            case "removeStreamingFaces":
            {
                if (instance.m_removeStreamingFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modifiedFaces":
            case "modifiedFaces":
            {
                if (instance.m_modifiedFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_needMatchStreamingFaceIndices":
            case "needMatchStreamingFaceIndices":
            {
                if (instance.m_needMatchStreamingFaceIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                if (instance.m_modifiedAabbs is not TGet castValue) return false;
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
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIdx":
            case "sectionIdx":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIdx = castValue;
                return true;
            }
            case "m_cutConfiguration":
            case "cutConfiguration":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshCutConfiguration castValue) return false;
                instance.m_cutConfiguration = castValue;
                return true;
            }
            case "m_addStreamingFaces":
            case "addStreamingFaces":
            {
                if (value is not List<int> castValue) return false;
                instance.m_addStreamingFaces = castValue;
                return true;
            }
            case "m_removeStreamingFaces":
            case "removeStreamingFaces":
            {
                if (value is not List<int> castValue) return false;
                instance.m_removeStreamingFaces = castValue;
                return true;
            }
            case "m_modifiedFaces":
            case "modifiedFaces":
            {
                if (value is not HKLib.hk2019.hkHashSet<int> castValue) return false;
                instance.m_modifiedFaces = castValue;
                return true;
            }
            case "m_needMatchStreamingFaceIndices":
            case "needMatchStreamingFaceIndices":
            {
                if (value is not HKLib.hk2019.hkHashSet<int> castValue) return false;
                instance.m_needMatchStreamingFaceIndices = castValue;
                return true;
            }
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                if (value is not List<HKLib.hk2019.hkAabb> castValue) return false;
                instance.m_modifiedAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
