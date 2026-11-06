// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkPackedVector3Data : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkPackedVector3>
{
    private static readonly System.Reflection.FieldInfo _valuesInfo = typeof(Havok.hk2016.hkPackedVector3).GetField("m_values")!;
    public hkPackedVector3Data(Havok.hk2016.hkPackedVector3 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_values":
            case "values":
            {
                value = instance.m_values;
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
            case "m_values":
            case "values":
            {
                if (value is not short[] castValue || castValue.Length != 4) return false;
                try
                {
                    _valuesInfo.SetValue(instance, value);
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
