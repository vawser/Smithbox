// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkDataObjectTypeAttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkDataObjectTypeAttribute>
{
    public hkDataObjectTypeAttributeData(Havok.hk2014.hkDataObjectTypeAttribute instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_typeName":
            case "typeName":
            {
                value = instance.m_typeName;
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
            case "m_typeName":
            case "typeName":
            {
                if (value is null)
                {
                    instance.m_typeName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_typeName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
