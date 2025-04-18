// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTransformSetUsageData : HavokData<hclTransformSetUsage> 
{
    private static readonly System.Reflection.FieldInfo _perComponentFlagsInfo = typeof(hclTransformSetUsage).GetField("m_perComponentFlags")!;
    public hclTransformSetUsageData(HavokType type, hclTransformSetUsage instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_perComponentFlags":
            case "perComponentFlags":
            {
                if (instance.m_perComponentFlags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_perComponentTransformTrackers":
            case "perComponentTransformTrackers":
            {
                if (instance.m_perComponentTransformTrackers is not TGet castValue) return false;
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
            case "m_perComponentFlags":
            case "perComponentFlags":
            {
                if (value is not byte[] castValue || castValue.Length != 2) return false;
                try
                {
                    _perComponentFlagsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_perComponentTransformTrackers":
            case "perComponentTransformTrackers":
            {
                if (value is not List<hclTransformSetUsage.TransformTracker> castValue) return false;
                instance.m_perComponentTransformTrackers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
