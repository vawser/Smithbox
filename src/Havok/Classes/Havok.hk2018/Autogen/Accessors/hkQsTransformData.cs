// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkQsTransformData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkQsTransform>
{
    public hkQsTransformData(Havok.hk2018.hkQsTransform instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                value = instance.m_rotation;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                value = instance.m_scale;
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
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
