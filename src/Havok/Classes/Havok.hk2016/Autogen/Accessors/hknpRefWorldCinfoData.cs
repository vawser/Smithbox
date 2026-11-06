// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpRefWorldCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpRefWorldCinfo>
{
    public hknpRefWorldCinfoData(Havok.hk2016.hknpRefWorldCinfo instance) : base(instance) { }

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
            case "m_info":
            case "info":
            {
                value = instance.m_info;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (value is not Havok.hk2016.hknpWorldCinfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
