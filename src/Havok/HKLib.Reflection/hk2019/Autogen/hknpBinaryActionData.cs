// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBinaryActionData : HavokData<HKLib.hk2019.hknpBinaryAction> 
{
    public hknpBinaryActionData(HavokType type, HKLib.hk2019.hknpBinaryAction instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (instance.m_bodyIdA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (instance.m_bodyIdB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not HKLib.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not HKLib.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
