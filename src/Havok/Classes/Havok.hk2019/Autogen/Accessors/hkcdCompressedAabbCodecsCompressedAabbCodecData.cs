// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdCompressedAabbCodecsCompressedAabbCodecData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdCompressedAabbCodecs.CompressedAabbCodec>
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(Havok.hk2019.hkcdCompressedAabbCodecs.CompressedAabbCodec).GetField("m_xyz")!;
    public hkcdCompressedAabbCodecsCompressedAabbCodecData(Havok.hk2019.hkcdCompressedAabbCodecs.CompressedAabbCodec instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
