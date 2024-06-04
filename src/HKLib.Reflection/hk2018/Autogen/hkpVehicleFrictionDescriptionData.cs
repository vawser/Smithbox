// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpVehicleFrictionDescriptionData : HavokData<hkpVehicleFrictionDescription> 
{
    private static readonly System.Reflection.FieldInfo _axleDescrInfo = typeof(hkpVehicleFrictionDescription).GetField("m_axleDescr")!;
    public hkpVehicleFrictionDescriptionData(HavokType type, hkpVehicleFrictionDescription instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelDistance":
            case "wheelDistance":
            {
                if (instance.m_wheelDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chassisMassInv":
            case "chassisMassInv":
            {
                if (instance.m_chassisMassInv is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_axleDescr":
            case "axleDescr":
            {
                if (instance.m_axleDescr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is not hkpVehicleFrictionDescription.AxisDescription[] castValue || castValue.Length != 2) return false;
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
