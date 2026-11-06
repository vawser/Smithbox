// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarCsgOperandData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarCsgOperand>
{
    public hkcdPlanarCsgOperandData(Havok.hk2019.hkcdPlanarCsgOperand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_solid":
            case "solid":
            {
                value = instance.m_solid;
                return true;
            }
            case "m_danglingGeometry":
            case "danglingGeometry":
            {
                value = instance.m_danglingGeometry;
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
            case "m_solid":
            case "solid":
            {
                if (value is null)
                {
                    instance.m_solid = default;
                    return true;
                }
                if (value is Havok.hk2019.hkcdPlanarSolid castValue)
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
                if (value is Havok.hk2019.hkcdPlanarGeometry castValue)
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
