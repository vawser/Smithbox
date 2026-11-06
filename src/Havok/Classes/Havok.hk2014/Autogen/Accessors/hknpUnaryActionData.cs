// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpUnaryActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpUnaryAction>
{
    public hknpUnaryActionData(Havok.hk2014.hknpUnaryAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is not uint castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
