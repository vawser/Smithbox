// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkFloat3Data : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkFloat3>
{
    public hkFloat3Data(Havok.hk2018.hkFloat3 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                value = instance.m_x;
                return true;
            }
            case "m_y":
            case "y":
            {
                value = instance.m_y;
                return true;
            }
            case "m_z":
            case "z":
            {
                value = instance.m_z;
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
            case "m_x":
            case "x":
            {
                if (value is not float castValue) return false;
                instance.m_x = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (value is not float castValue) return false;
                instance.m_y = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (value is not float castValue) return false;
                instance.m_z = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
