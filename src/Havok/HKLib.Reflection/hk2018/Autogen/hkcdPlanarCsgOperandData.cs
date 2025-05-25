// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarCsgOperandData : HavokData<hkcdPlanarCsgOperand> 
{
    public hkcdPlanarCsgOperandData(HavokType type, hkcdPlanarCsgOperand instance) : base(type, instance) {}

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
            case "m_solid":
            case "solid":
            {
                if (instance.m_solid is null)
                {
                    return true;
                }
                if (instance.m_solid is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_danglingGeometry":
            case "danglingGeometry":
            {
                if (instance.m_danglingGeometry is null)
                {
                    return true;
                }
                if (instance.m_danglingGeometry is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_solid":
            case "solid":
            {
                if (value is null)
                {
                    instance.m_solid = default;
                    return true;
                }
                if (value is hkcdPlanarSolid castValue)
                {
                    instance.m_solid = castValue;
                    return true;
                }
                return false;
            }
            case "m_danglingGeometry":
            case "danglingGeometry":
            {
                if (value is null)
                {
                    instance.m_danglingGeometry = default;
                    return true;
                }
                if (value is hkcdPlanarGeometry castValue)
                {
                    instance.m_danglingGeometry = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
