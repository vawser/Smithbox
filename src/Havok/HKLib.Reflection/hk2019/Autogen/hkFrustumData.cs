// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkFrustumData : HavokData<HKLib.hk2019.hkFrustum> 
{
    private static readonly System.Reflection.FieldInfo _planesInfo = typeof(HKLib.hk2019.hkFrustum).GetField("m_planes")!;
    public hkFrustumData(HavokType type, HKLib.hk2019.hkFrustum instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                if (instance.m_planes is not TGet castValue) return false;
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
            case "m_planes":
            case "planes":
            {
                if (value is not HKLib.hk2019.hkPlane[] castValue || castValue.Length != 6) return false;
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
