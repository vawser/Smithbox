// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbComputeRotationFromAxisAngleModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbComputeRotationFromAxisAngleModifierInternalState>
{
    public hkbComputeRotationFromAxisAngleModifierInternalStateData(Havok.hk2014.hkbComputeRotationFromAxisAngleModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
