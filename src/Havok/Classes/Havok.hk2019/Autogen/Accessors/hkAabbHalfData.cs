// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkAabbHalfData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkAabbHalf>
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(Havok.hk2019.hkAabbHalf).GetField("m_data")!;
    public hkAabbHalfData(Havok.hk2019.hkAabbHalf instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_data":
            case "data":
            {
                if (value is not ushort[] castValue || castValue.Length != 8) return false;
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
