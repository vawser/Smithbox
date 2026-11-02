// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdCompressedAabbCodecsCompressedAabbCodecData : HavokData<HKLib.hk2019.hkcdCompressedAabbCodecs.CompressedAabbCodec> 
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(HKLib.hk2019.hkcdCompressedAabbCodecs.CompressedAabbCodec).GetField("m_xyz")!;
    public hkcdCompressedAabbCodecsCompressedAabbCodecData(HavokType type, HKLib.hk2019.hkcdCompressedAabbCodecs.CompressedAabbCodec instance) : base(type, instance) {}

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
            default:
            return false;
        }
    }

}
