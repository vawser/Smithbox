// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpTyremarksWheelData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpTyremarksWheel>
{
    public hknpTyremarksWheelData(Havok.hk2018.hknpTyremarksWheel instance) : base(instance) { }

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
            case "m_currentPosition":
            case "currentPosition":
            {
                value = instance.m_currentPosition;
                return true;
            }
            case "m_numPoints":
            case "numPoints":
            {
                value = instance.m_numPoints;
                return true;
            }
            case "m_tyremarkPoints":
            case "tyremarkPoints":
            {
                value = instance.m_tyremarkPoints;
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
            case "m_currentPosition":
            case "currentPosition":
            {
                if (value is not int castValue) return false;
                instance.m_currentPosition = castValue;
                return true;
            }
            case "m_numPoints":
            case "numPoints":
            {
                if (value is not int castValue) return false;
                instance.m_numPoints = castValue;
                return true;
            }
            case "m_tyremarkPoints":
            case "tyremarkPoints":
            {
                if (value is not List<Havok.hk2018.hknpTyremarkPoint> castValue) return false;
                instance.m_tyremarkPoints = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
