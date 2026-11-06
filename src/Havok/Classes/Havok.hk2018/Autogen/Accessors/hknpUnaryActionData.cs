// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpUnaryActionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpUnaryAction>
{
    public hknpUnaryActionData(Havok.hk2018.hknpUnaryAction instance) : base(instance) { }

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
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                value = instance.m_bodyId;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
