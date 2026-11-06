// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimatedMatrixData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimatedMatrix>
{
    public hkxAnimatedMatrixData(Havok.hk2019.hkxAnimatedMatrix instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_matrices":
            case "matrices":
            {
                value = instance.m_matrices;
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
            case "m_matrices":
            case "matrices":
            {
                if (value is not List<float> castValue) return false;
                instance.m_matrices = castValue;
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
