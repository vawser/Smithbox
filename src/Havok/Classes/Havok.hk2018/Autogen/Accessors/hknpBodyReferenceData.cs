// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpBodyReferenceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpBodyReference>
{
    public hknpBodyReferenceData(Havok.hk2018.hknpBodyReference instance) : base(instance) { }

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
            case "m_id":
            case "id":
            {
                value = instance.m_id;
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
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
