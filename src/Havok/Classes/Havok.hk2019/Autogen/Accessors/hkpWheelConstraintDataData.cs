// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpWheelConstraintDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpWheelConstraintData>
{
    public hkpWheelConstraintDataData(Havok.hk2019.hkpWheelConstraintData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                value = instance.m_numNonDirectlySolvedAtoms;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                value = instance.m_atoms;
                return true;
            }
            case "m_initialAxleInB":
            case "initialAxleInB":
            {
                value = instance.m_initialAxleInB;
                return true;
            }
            case "m_initialSteeringAxisInB":
            case "initialSteeringAxisInB":
            {
                value = instance.m_initialSteeringAxisInB;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (value is not byte castValue) return false;
                instance.m_numNonDirectlySolvedAtoms = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (value is not Havok.hk2019.hkpWheelConstraintData.Atoms castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            case "m_initialAxleInB":
            case "initialAxleInB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_initialAxleInB = castValue;
                return true;
            }
            case "m_initialSteeringAxisInB":
            case "initialSteeringAxisInB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_initialSteeringAxisInB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
