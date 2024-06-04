// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshCutConfigurationBigFaceInfoData : HavokData<hkaiNavMeshCutConfiguration.BigFaceInfo> 
{
    public hkaiNavMeshCutConfigurationBigFaceInfoData(HavokType type, hkaiNavMeshCutConfiguration.BigFaceInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_stitchCount":
            case "stitchCount":
            {
                if (instance.m_stitchCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isCutFace":
            case "isCutFace":
            {
                if (instance.m_isCutFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasDynamicUserEdges":
            case "hasDynamicUserEdges":
            {
                if (instance.m_hasDynamicUserEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeIsCut":
            case "edgeIsCut":
            {
                if (instance.m_edgeIsCut is not TGet castValue) return false;
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
            case "m_stitchCount":
            case "stitchCount":
            {
                if (value is not int castValue) return false;
                instance.m_stitchCount = castValue;
                return true;
            }
            case "m_isCutFace":
            case "isCutFace":
            {
                if (value is not bool castValue) return false;
                instance.m_isCutFace = castValue;
                return true;
            }
            case "m_hasDynamicUserEdges":
            case "hasDynamicUserEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_hasDynamicUserEdges = castValue;
                return true;
            }
            case "m_edgeIsCut":
            case "edgeIsCut":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_edgeIsCut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
