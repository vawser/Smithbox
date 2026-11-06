// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticTreeCodec3AxisData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticTreeCodec3Axis>
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(Havok.hk2014.hkcdStaticTreeCodec3Axis).GetField("m_xyz")!;
    public hkcdStaticTreeCodec3AxisData(Havok.hk2014.hkcdStaticTreeCodec3Axis instance) : base(instance) { }

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
