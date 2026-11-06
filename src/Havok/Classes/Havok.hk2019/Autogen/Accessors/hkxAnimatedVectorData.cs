// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimatedVectorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimatedVector>
{
    public hkxAnimatedVectorData(Havok.hk2019.hkxAnimatedVector instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vectors":
            case "vectors":
            {
                value = instance.m_vectors;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                value = instance.m_hint;
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
            case "m_vectors":
            case "vectors":
            {
                if (value is not List<float> castValue) return false;
                instance.m_vectors = castValue;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                if (value is Havok.hk2019.hkxAttribute.Hint castValue)
                {
                    instance.m_hint = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_hint = (Havok.hk2019.hkxAttribute.Hint)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
