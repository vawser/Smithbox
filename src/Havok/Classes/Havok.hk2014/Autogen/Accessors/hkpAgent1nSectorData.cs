// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpAgent1nSectorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpAgent1nSector>
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(Havok.hk2014.hkpAgent1nSector).GetField("m_data")!;
    public hkpAgent1nSectorData(Havok.hk2014.hkpAgent1nSector instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bytesAllocated":
            case "bytesAllocated":
            {
                value = instance.m_bytesAllocated;
                return true;
            }
            case "m_pad0":
            case "pad0":
            {
                value = instance.m_pad0;
                return true;
            }
            case "m_pad1":
            case "pad1":
            {
                value = instance.m_pad1;
                return true;
            }
            case "m_pad2":
            case "pad2":
            {
                value = instance.m_pad2;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_bytesAllocated":
            case "bytesAllocated":
            {
                if (value is not uint castValue) return false;
                instance.m_bytesAllocated = castValue;
                return true;
            }
            case "m_pad0":
            case "pad0":
            {
                if (value is not uint castValue) return false;
                instance.m_pad0 = castValue;
                return true;
            }
            case "m_pad1":
            case "pad1":
            {
                if (value is not uint castValue) return false;
                instance.m_pad1 = castValue;
                return true;
            }
            case "m_pad2":
            case "pad2":
            {
                if (value is not uint castValue) return false;
                instance.m_pad2 = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not byte[] castValue || castValue.Length != 496) return false;
                try
                {
                    _dataInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
