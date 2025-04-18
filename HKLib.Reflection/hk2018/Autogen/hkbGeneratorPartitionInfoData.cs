// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGeneratorPartitionInfoData : HavokData<hkbGeneratorPartitionInfo> 
{
    private static readonly System.Reflection.FieldInfo _boneMaskInfo = typeof(hkbGeneratorPartitionInfo).GetField("m_boneMask")!;
    private static readonly System.Reflection.FieldInfo _partitionMaskInfo = typeof(hkbGeneratorPartitionInfo).GetField("m_partitionMask")!;
    public hkbGeneratorPartitionInfoData(HavokType type, hkbGeneratorPartitionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneMask":
            case "boneMask":
            {
                if (instance.m_boneMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partitionMask":
            case "partitionMask":
            {
                if (instance.m_partitionMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (instance.m_numBones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numMaxPartitions":
            case "numMaxPartitions":
            {
                if (instance.m_numMaxPartitions is not TGet castValue) return false;
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
            case "m_boneMask":
            case "boneMask":
            {
                if (value is not uint[] castValue || castValue.Length != 8) return false;
                try
                {
                    _boneMaskInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_partitionMask":
            case "partitionMask":
            {
                if (value is not uint[] castValue || castValue.Length != 1) return false;
                try
                {
                    _partitionMaskInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numBones":
            case "numBones":
            {
                if (value is not short castValue) return false;
                instance.m_numBones = castValue;
                return true;
            }
            case "m_numMaxPartitions":
            case "numMaxPartitions":
            {
                if (value is not short castValue) return false;
                instance.m_numMaxPartitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
