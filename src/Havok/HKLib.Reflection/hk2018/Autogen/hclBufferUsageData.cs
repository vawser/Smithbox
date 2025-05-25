// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBufferUsageData : HavokData<hclBufferUsage> 
{
    private static readonly System.Reflection.FieldInfo _perComponentFlagsInfo = typeof(hclBufferUsage).GetField("m_perComponentFlags")!;
    public hclBufferUsageData(HavokType type, hclBufferUsage instance) : base(type, instance) {}

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
            case "m_trianglesRead":
            case "trianglesRead":
            {
                if (instance.m_trianglesRead is not TGet castValue) return false;
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
                if (value is not byte[] castValue || castValue.Length != 4) return false;
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
            case "m_trianglesRead":
            case "trianglesRead":
            {
                if (value is not bool castValue) return false;
                instance.m_trianglesRead = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
