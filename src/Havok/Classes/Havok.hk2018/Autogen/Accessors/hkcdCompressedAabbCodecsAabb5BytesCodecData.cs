// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdCompressedAabbCodecsAabb5BytesCodecData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdCompressedAabbCodecs.Aabb5BytesCodec>
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(Havok.hk2018.hkcdCompressedAabbCodecs.Aabb5BytesCodec).GetField("m_xyz")!;
    public hkcdCompressedAabbCodecsAabb5BytesCodecData(Havok.hk2018.hkcdCompressedAabbCodecs.Aabb5BytesCodec instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xyz":
            case "xyz":
            {
                value = instance.m_xyz;
                return true;
            }
            case "m_hiData":
            case "hiData":
            {
                value = instance.m_hiData;
                return true;
            }
            case "m_loData":
            case "loData":
            {
                value = instance.m_loData;
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
                if (value is not byte castValue) return false;
                instance.m_loData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
