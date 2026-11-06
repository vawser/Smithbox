// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbTransformVectorModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbTransformVectorModifierInternalState>
{
    public hkbTransformVectorModifierInternalStateData(Havok.hk2014.hkbTransformVectorModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vectorOut":
            case "vectorOut":
            {
                value = instance.m_vectorOut;
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
            case "m_vectorOut":
            case "vectorOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vectorOut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
