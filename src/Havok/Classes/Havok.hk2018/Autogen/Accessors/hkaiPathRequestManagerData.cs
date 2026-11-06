// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPathRequestManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPathRequestManager>
{
    public hkaiPathRequestManagerData(Havok.hk2018.hkaiPathRequestManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_queues":
            case "queues":
            {
                if (value is not List<Havok.hk2018.hkaiPathRequestQueue?> castValue) return false;
                instance.m_queues = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
