// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCombineTransformsModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCombineTransformsModifierInternalState>
{
    public hkbCombineTransformsModifierInternalStateData(Havok.hk2014.hkbCombineTransformsModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_translationOut":
            case "translationOut":
            {
                value = instance.m_translationOut;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                value = instance.m_rotationOut;
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
            case "m_translationOut":
            case "translationOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translationOut = castValue;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotationOut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
