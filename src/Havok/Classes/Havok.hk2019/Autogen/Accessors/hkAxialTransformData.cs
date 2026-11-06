// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkAxialTransformData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkAxialTransform>
{
    public hkAxialTransformData(Havok.hk2019.hkAxialTransform instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotation":
            case "rotation":
            {
                value = instance.m_rotation;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
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
            case "m_rotation":
            case "rotation":
            {
                if (value is not Havok.hk2019.hkAxialRotation castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
