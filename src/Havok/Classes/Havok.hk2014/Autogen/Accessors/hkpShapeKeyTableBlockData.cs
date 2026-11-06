// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpShapeKeyTableBlockData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpShapeKeyTableBlock>
{
    private static readonly System.Reflection.FieldInfo _slotsInfo = typeof(Havok.hk2014.hkpShapeKeyTableBlock).GetField("m_slots")!;
    public hkpShapeKeyTableBlockData(Havok.hk2014.hkpShapeKeyTableBlock instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_slots":
            case "slots":
            {
                value = instance.m_slots;
                return true;
            }
            case "m_next":
            case "next":
            {
                value = instance.m_next;
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
            case "m_slots":
            case "slots":
            {
                if (value is not uint[] castValue || castValue.Length != 63) return false;
                try
                {
                    _slotsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_next":
            case "next":
            {
                if (value is null)
                {
                    instance.m_next = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShapeKeyTableBlock castValue)
                {
                    instance.m_next = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
