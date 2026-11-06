// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpCollidableBoundingVolumeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpCollidableBoundingVolumeData>
{
    private static readonly System.Reflection.FieldInfo _minInfo = typeof(Havok.hk2010.hkpCollidableBoundingVolumeData).GetField("m_min")!;
    private static readonly System.Reflection.FieldInfo _expansionMinInfo = typeof(Havok.hk2010.hkpCollidableBoundingVolumeData).GetField("m_expansionMin")!;
    private static readonly System.Reflection.FieldInfo _maxInfo = typeof(Havok.hk2010.hkpCollidableBoundingVolumeData).GetField("m_max")!;
    private static readonly System.Reflection.FieldInfo _expansionMaxInfo = typeof(Havok.hk2010.hkpCollidableBoundingVolumeData).GetField("m_expansionMax")!;
    public hkpCollidableBoundingVolumeDataData(Havok.hk2010.hkpCollidableBoundingVolumeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                value = instance.m_min;
                return true;
            }
            case "m_expansionMin":
            case "expansionMin":
            {
                value = instance.m_expansionMin;
                return true;
            }
            case "m_expansionShift":
            case "expansionShift":
            {
                value = instance.m_expansionShift;
                return true;
            }
            case "m_max":
            case "max":
            {
                value = instance.m_max;
                return true;
            }
            case "m_expansionMax":
            case "expansionMax":
            {
                value = instance.m_expansionMax;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_min":
            case "min":
            {
                if (value is not uint[] castValue || castValue.Length != 3) return false;
                try
                {
                    _minInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_expansionMin":
            case "expansionMin":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _expansionMinInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_expansionShift":
            case "expansionShift":
            {
                if (value is not byte castValue) return false;
                instance.m_expansionShift = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not uint[] castValue || castValue.Length != 3) return false;
                try
                {
                    _maxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_expansionMax":
            case "expansionMax":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _expansionMaxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_padding":
            case "padding":
            {
                if (value is not byte castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
