// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStateChooserWrapperData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStateChooserWrapper>
{
    public hkbStateChooserWrapperData(Havok.hk2014.hkbStateChooserWrapper instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wrappedChooser":
            case "wrappedChooser":
            {
                value = instance.m_wrappedChooser;
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
            case "m_wrappedChooser":
            case "wrappedChooser":
            {
                if (value is null)
                {
                    instance.m_wrappedChooser = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbStateChooser castValue)
                {
                    instance.m_wrappedChooser = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
