// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBodyReferenceData : HavokData<HKLib.hk2019.hknpBodyReference> 
{
    public hknpBodyReferenceData(HavokType type, HKLib.hk2019.hknpBodyReference instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
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
            case "m_id":
            case "id":
            {
                if (value is not HKLib.hk2019.hknpBodyId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
