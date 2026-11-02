// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPathRequestManagerData : HavokData<HKLib.hk2019.hkaiPathRequestManager> 
{
    public hkaiPathRequestManagerData(HavokType type, HKLib.hk2019.hkaiPathRequestManager instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_queues":
            case "queues":
            {
                if (instance.m_queues is not TGet castValue) return false;
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
            case "m_queues":
            case "queues":
            {
                if (value is not List<HKLib.hk2019.hkaiPathRequestQueue?> castValue) return false;
                instance.m_queues = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
