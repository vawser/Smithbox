// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpBallAndSocketConstraintDataData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpBallAndSocketConstraintData>
{
    public hkpBallAndSocketConstraintDataData(Havok.hk55.hkpBallAndSocketConstraintData instance) : base(instance) { }

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
            case "m_atoms":
            case "atoms":
            {
                value = instance.m_atoms;
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
            case "m_atoms":
            case "atoms":
            {
                if (value is not Havok.hk55.hkpBallAndSocketConstraintData.Atoms castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
