// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxCameraData : HavokData<HKLib.hk2019.hkxCamera> 
{
    public hkxCameraData(HavokType type, HKLib.hk2019.hkxCamera instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_from":
            case "from":
            {
                if (instance.m_from is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_focus":
            case "focus":
            {
                if (instance.m_focus is not TGet castValue) return false;
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
            case "m_fov":
            case "fov":
            {
                if (instance.m_fov is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_far":
            case "far":
            {
                if (instance.m_far is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_near":
            case "near":
            {
                if (instance.m_near is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftHanded":
            case "leftHanded":
            {
                if (instance.m_leftHanded is not TGet castValue) return false;
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
