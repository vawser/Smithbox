// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class TempAllocatedClassData : Havok.Formats.HavokFieldAccess<Havok.hk2019.TempAllocatedClass>
{
    public TempAllocatedClassData(Havok.hk2019.TempAllocatedClass instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_int":
            case "int":
            {
                value = instance.m_int;
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
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
