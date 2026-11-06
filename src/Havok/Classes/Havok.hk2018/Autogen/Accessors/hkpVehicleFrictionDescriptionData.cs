// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpVehicleFrictionDescriptionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpVehicleFrictionDescription>
{
    private static readonly System.Reflection.FieldInfo _axleDescrInfo = typeof(Havok.hk2018.hkpVehicleFrictionDescription).GetField("m_axleDescr")!;
    public hkpVehicleFrictionDescriptionData(Havok.hk2018.hkpVehicleFrictionDescription instance) : base(instance) { }

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
            case "m_wheelDistance":
            case "wheelDistance":
            {
                value = instance.m_wheelDistance;
                return true;
            }
            case "m_chassisMassInv":
            case "chassisMassInv":
            {
                value = instance.m_chassisMassInv;
                return true;
            }
            case "m_axleDescr":
            case "axleDescr":
            {
                value = instance.m_axleDescr;
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
            case "m_wheelDistance":
            case "wheelDistance":
            {
                if (value is not float castValue) return false;
                instance.m_wheelDistance = castValue;
                return true;
            }
            case "m_chassisMassInv":
            case "chassisMassInv":
            {
                if (value is not float castValue) return false;
                instance.m_chassisMassInv = castValue;
                return true;
            }
            case "m_axleDescr":
            case "axleDescr":
            {
                if (value is not Havok.hk2018.hkpVehicleFrictionDescription.AxisDescription[] castValue || castValue.Length != 2) return false;
                try
                {
                    _axleDescrInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
