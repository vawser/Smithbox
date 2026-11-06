// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdCompressedAabbCodecsAabb4BytesCodecData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdCompressedAabbCodecs.Aabb4BytesCodec>
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(Havok.hk2019.hkcdCompressedAabbCodecs.Aabb4BytesCodec).GetField("m_xyz")!;
    public hkcdCompressedAabbCodecsAabb4BytesCodecData(Havok.hk2019.hkcdCompressedAabbCodecs.Aabb4BytesCodec instance) : base(instance) { }

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
            case "m_data":
            case "data":
            {
                if (value is not byte castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
