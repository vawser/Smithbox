// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpAngConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpAngConstraintAtom>
{
    public hkpAngConstraintAtomData(Havok.hk2014.hkpAngConstraintAtom instance) : base(instance) { }

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
            case "m_firstConstrainedAxis":
            case "firstConstrainedAxis":
            {
                value = instance.m_firstConstrainedAxis;
                return true;
            }
            case "m_numConstrainedAxes":
            case "numConstrainedAxes":
            {
                value = instance.m_numConstrainedAxes;
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
            case "m_firstConstrainedAxis":
            case "firstConstrainedAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_firstConstrainedAxis = castValue;
                return true;
            }
            case "m_numConstrainedAxes":
            case "numConstrainedAxes":
            {
                if (value is not byte castValue) return false;
                instance.m_numConstrainedAxes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
