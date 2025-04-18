// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavMeshDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshDebugUtilsFaceNormalSettingsData : HavokData<FaceNormalSettings> 
{
    public hkaiNavMeshDebugUtilsFaceNormalSettingsData(HavokType type, FaceNormalSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_highlightFaceNormals":
            case "highlightFaceNormals":
            {
                if (instance.m_highlightFaceNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawFaceNormal":
            case "drawFaceNormal":
            {
                if (instance.m_drawFaceNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleMultiplier":
            case "scaleMultiplier":
            {
                if (instance.m_scaleMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosAngleThreshold":
            case "cosAngleThreshold":
            {
                if (instance.m_cosAngleThreshold is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_highlightFaceNormals":
            case "highlightFaceNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_highlightFaceNormals = castValue;
                return true;
            }
            case "m_drawFaceNormal":
            case "drawFaceNormal":
            {
                if (value is not bool castValue) return false;
                instance.m_drawFaceNormal = castValue;
                return true;
            }
            case "m_scaleMultiplier":
            case "scaleMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_scaleMultiplier = castValue;
                return true;
            }
            case "m_cosAngleThreshold":
            case "cosAngleThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_cosAngleThreshold = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
