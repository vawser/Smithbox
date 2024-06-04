// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkcdCompressedAabbCodecs;

namespace HKLib.Reflection.hk2018;

internal class hkcdCompressedAabbCodecsAabb6BytesCodecData : HavokData<Aabb6BytesCodec> 
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(Aabb6BytesCodec).GetField("m_xyz")!;
    public hkcdCompressedAabbCodecsAabb6BytesCodecData(HavokType type, Aabb6BytesCodec instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xyz":
            case "xyz":
            {
                if (instance.m_xyz is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hiData":
            case "hiData":
            {
                if (instance.m_hiData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_loData":
            case "loData":
            {
                if (instance.m_loData is not TGet castValue) return false;
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
            case "m_xyz":
            case "xyz":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _xyzInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_hiData":
            case "hiData":
            {
                if (value is not byte castValue) return false;
                instance.m_hiData = castValue;
                return true;
            }
            case "m_loData":
            case "loData":
            {
                if (value is not ushort castValue) return false;
                instance.m_loData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
