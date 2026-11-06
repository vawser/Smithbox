// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshDebugUtilsFaceNormalSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshDebugUtils.FaceNormalSettings>
{
    public hkaiNavMeshDebugUtilsFaceNormalSettingsData(Havok.hk2018.hkaiNavMeshDebugUtils.FaceNormalSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_highlightFaceNormals":
            case "highlightFaceNormals":
            {
                value = instance.m_highlightFaceNormals;
                return true;
            }
            case "m_drawFaceNormal":
            case "drawFaceNormal":
            {
                value = instance.m_drawFaceNormal;
                return true;
            }
            case "m_scaleMultiplier":
            case "scaleMultiplier":
            {
                value = instance.m_scaleMultiplier;
                return true;
            }
            case "m_cosAngleThreshold":
            case "cosAngleThreshold":
            {
                value = instance.m_cosAngleThreshold;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
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
