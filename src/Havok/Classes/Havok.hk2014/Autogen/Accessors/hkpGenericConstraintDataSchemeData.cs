// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpGenericConstraintDataSchemeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpGenericConstraintDataScheme>
{
    public hkpGenericConstraintDataSchemeData(Havok.hk2014.hkpGenericConstraintDataScheme instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_commands":
            case "commands":
            {
                value = instance.m_commands;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                value = instance.m_motors;
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
            case "m_data":
            case "data":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_commands":
            case "commands":
            {
                if (value is not List<int> castValue) return false;
                instance.m_commands = castValue;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                if (value is not List<Havok.hk2014.hkpConstraintMotor?> castValue) return false;
                instance.m_motors = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
