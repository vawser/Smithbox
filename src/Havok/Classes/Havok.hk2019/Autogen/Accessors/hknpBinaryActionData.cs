// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpBinaryActionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpBinaryAction>
{
    public hknpBinaryActionData(Havok.hk2019.hknpBinaryAction instance) : base(instance) { }

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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                value = instance.m_bodyIdA;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                value = instance.m_bodyIdB;
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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
