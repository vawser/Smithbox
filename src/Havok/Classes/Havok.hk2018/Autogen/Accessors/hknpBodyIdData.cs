// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpBodyIdData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpBodyId>
{
    public hknpBodyIdData(Havok.hk2018.hknpBodyId instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_serialAndIndex":
            case "serialAndIndex":
            {
                value = instance.m_serialAndIndex;
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
            case "m_serialAndIndex":
            case "serialAndIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_serialAndIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
