// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSemanticsAttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSemanticsAttribute>
{
    public hkSemanticsAttributeData(Havok.hk2019.hkSemanticsAttribute instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkSemanticsAttribute.Semantics castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2019.hkSemanticsAttribute.Semantics)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
