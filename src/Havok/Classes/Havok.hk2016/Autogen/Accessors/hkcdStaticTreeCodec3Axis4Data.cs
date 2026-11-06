// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkcdStaticTreeCodec3Axis4Data : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkcdStaticTree.Codec3Axis4>
{
    private static readonly System.Reflection.FieldInfo _xyzInfo = typeof(Havok.hk2016.hkcdStaticTree.Codec3Axis4).GetField("m_xyz")!;
    public hkcdStaticTreeCodec3Axis4Data(Havok.hk2016.hkcdStaticTree.Codec3Axis4 instance) : base(instance) { }

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
