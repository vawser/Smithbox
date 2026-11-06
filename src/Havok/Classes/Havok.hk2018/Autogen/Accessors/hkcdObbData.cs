// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdObbData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdObb>
{
    public hkcdObbData(Havok.hk2018.hkcdObb instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
