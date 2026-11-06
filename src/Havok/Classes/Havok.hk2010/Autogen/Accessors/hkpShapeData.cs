// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpShape>
{
    public hkpShapeData(Havok.hk2010.hkpShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
