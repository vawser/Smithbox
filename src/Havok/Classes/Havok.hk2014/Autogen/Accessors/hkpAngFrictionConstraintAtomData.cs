// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpAngFrictionConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpAngFrictionConstraintAtom>
{
    public hkpAngFrictionConstraintAtomData(Havok.hk2014.hkpAngFrictionConstraintAtom instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
                return true;
            }
            case "m_firstFrictionAxis":
            case "firstFrictionAxis":
            {
                value = instance.m_firstFrictionAxis;
                return true;
            }
            case "m_numFrictionAxes":
            case "numFrictionAxes":
            {
                value = instance.m_numFrictionAxes;
                return true;
            }
            case "m_maxFrictionTorque":
            case "maxFrictionTorque":
            {
                value = instance.m_maxFrictionTorque;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2014.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not byte castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_firstFrictionAxis":
            case "firstFrictionAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_firstFrictionAxis = castValue;
                return true;
            }
            case "m_numFrictionAxes":
            case "numFrictionAxes":
            {
                if (value is not byte castValue) return false;
                instance.m_numFrictionAxes = castValue;
                return true;
            }
            case "m_maxFrictionTorque":
            case "maxFrictionTorque":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionTorque = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
