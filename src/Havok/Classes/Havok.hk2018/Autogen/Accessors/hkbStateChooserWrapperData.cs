// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbStateChooserWrapperData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbStateChooserWrapper>
{
    public hkbStateChooserWrapperData(Havok.hk2018.hkbStateChooserWrapper instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_wrappedChooser":
            case "wrappedChooser":
            {
                if (value is null)
                {
                    instance.m_wrappedChooser = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbStateChooser castValue)
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
