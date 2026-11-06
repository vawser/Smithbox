// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkFrustumData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkFrustum>
{
    private static readonly System.Reflection.FieldInfo _planesInfo = typeof(Havok.hk2019.hkFrustum).GetField("m_planes")!;
    public hkFrustumData(Havok.hk2019.hkFrustum instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
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
            case "m_planes":
            case "planes":
            {
                if (value is not Havok.hk2019.hkPlane[] castValue || castValue.Length != 6) return false;
                try
                {
                    _planesInfo.SetValue(instance, value);
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
