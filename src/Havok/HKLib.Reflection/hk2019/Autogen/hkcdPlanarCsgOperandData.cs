// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdPlanarCsgOperandData : HavokData<HKLib.hk2019.hkcdPlanarCsgOperand> 
{
    public hkcdPlanarCsgOperandData(HavokType type, HKLib.hk2019.hkcdPlanarCsgOperand instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_solid":
            case "solid":
            {
                if (value is null)
                {
                    instance.m_solid = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkcdPlanarSolid castValue)
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
                if (value is HKLib.hk2019.hkcdPlanarGeometry castValue)
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
