// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class DebugAllocatedClassData : Havok.Formats.HavokFieldAccess<Havok.hk2019.DebugAllocatedClass>
{
    public DebugAllocatedClassData(Havok.hk2019.DebugAllocatedClass instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_float":
            case "float":
            {
                value = instance.m_float;
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
            case "m_float":
            case "float":
            {
                if (value is not float castValue) return false;
                instance.m_float = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
