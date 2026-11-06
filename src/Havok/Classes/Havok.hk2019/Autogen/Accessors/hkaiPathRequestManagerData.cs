// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiPathRequestManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiPathRequestManager>
{
    public hkaiPathRequestManagerData(Havok.hk2019.hkaiPathRequestManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_queues":
            case "queues":
            {
                value = instance.m_queues;
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
            case "m_queues":
            case "queues":
            {
                if (value is not List<Havok.hk2019.hkaiPathRequestQueue?> castValue) return false;
                instance.m_queues = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
