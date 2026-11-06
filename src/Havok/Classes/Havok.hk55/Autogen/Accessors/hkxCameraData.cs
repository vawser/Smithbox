// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxCameraData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxCamera>
{
    public hkxCameraData(Havok.hk55.hkxCamera instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_from":
            case "from":
            {
                value = instance.m_from;
                return true;
            }
            case "m_focus":
            case "focus":
            {
                value = instance.m_focus;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_fov":
            case "fov":
            {
                value = instance.m_fov;
                return true;
            }
            case "m_far":
            case "far":
            {
                value = instance.m_far;
                return true;
            }
            case "m_near":
            case "near":
            {
                value = instance.m_near;
                return true;
            }
            case "m_leftHanded":
            case "leftHanded":
            {
                value = instance.m_leftHanded;
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
            case "m_from":
            case "from":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_from = castValue;
                return true;
            }
            case "m_focus":
            case "focus":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_focus = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_fov":
            case "fov":
            {
                if (value is not float castValue) return false;
                instance.m_fov = castValue;
                return true;
            }
            case "m_far":
            case "far":
            {
                if (value is not float castValue) return false;
                instance.m_far = castValue;
                return true;
            }
            case "m_near":
            case "near":
            {
                if (value is not float castValue) return false;
                instance.m_near = castValue;
                return true;
            }
            case "m_leftHanded":
            case "leftHanded":
            {
                if (value is not bool castValue) return false;
                instance.m_leftHanded = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
