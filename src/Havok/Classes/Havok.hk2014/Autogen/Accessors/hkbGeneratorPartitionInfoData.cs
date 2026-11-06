// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbGeneratorPartitionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbGeneratorPartitionInfo>
{
    private static readonly System.Reflection.FieldInfo _boneMaskInfo = typeof(Havok.hk2014.hkbGeneratorPartitionInfo).GetField("m_boneMask")!;
    private static readonly System.Reflection.FieldInfo _partitionMaskInfo = typeof(Havok.hk2014.hkbGeneratorPartitionInfo).GetField("m_partitionMask")!;
    public hkbGeneratorPartitionInfoData(Havok.hk2014.hkbGeneratorPartitionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneMask":
            case "boneMask":
            {
                value = instance.m_boneMask;
                return true;
            }
            case "m_partitionMask":
            case "partitionMask":
            {
                value = instance.m_partitionMask;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                value = instance.m_numBones;
                return true;
            }
            case "m_numMaxPartitions":
            case "numMaxPartitions":
            {
                value = instance.m_numMaxPartitions;
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
