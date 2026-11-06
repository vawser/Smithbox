// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkReflectDataFieldDeclData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkReflect.DataFieldDecl>
{
    public hkReflectDataFieldDeclData(Havok.hk2019.hkReflect.DataFieldDecl instance) : base(instance) { }

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
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is Havok.hk2019.IHavokObject castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
