// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshCutConfigurationBigFaceInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshCutConfiguration.BigFaceInfo>
{
    public hkaiNavMeshCutConfigurationBigFaceInfoData(Havok.hk2018.hkaiNavMeshCutConfiguration.BigFaceInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_stitchCount":
            case "stitchCount":
            {
                value = instance.m_stitchCount;
                return true;
            }
            case "m_isCutFace":
            case "isCutFace":
            {
                value = instance.m_isCutFace;
                return true;
            }
            case "m_hasDynamicUserEdges":
            case "hasDynamicUserEdges":
            {
                value = instance.m_hasDynamicUserEdges;
                return true;
            }
            case "m_edgeIsCut":
            case "edgeIsCut":
            {
                value = instance.m_edgeIsCut;
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
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_edgeIsCut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
