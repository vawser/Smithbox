// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkFloat16TransformData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkFloat16Transform>
{
    private static readonly System.Reflection.FieldInfo _elementsInfo = typeof(Havok.hk2014.hkFloat16Transform).GetField("m_elements")!;
    public hkFloat16TransformData(Havok.hk2014.hkFloat16Transform instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                value = instance.m_elements;
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
            case "m_elements":
            case "elements":
            {
                if (value is not Havok.hk2014.hkFloat16[] castValue || castValue.Length != 12) return false;
                try
                {
                    _elementsInfo.SetValue(instance, value);
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
