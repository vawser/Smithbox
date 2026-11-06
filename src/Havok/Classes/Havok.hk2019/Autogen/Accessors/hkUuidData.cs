// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkUuidData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkUuid>
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(Havok.hk2019.hkUuid).GetField("m_data")!;
    public hkUuidData(Havok.hk2019.hkUuid instance) : base(instance) { }

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
                if (value is not uint[] castValue || castValue.Length != 4) return false;
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
