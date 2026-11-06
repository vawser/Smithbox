// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpConstraintCollisionFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpConstraintCollisionFilter>
{
    private static readonly System.Reflection.FieldInfo _prepadInfo = typeof(Havok.hk2014.hkpConstraintCollisionFilter).GetField("m_prepad")!;
    private static readonly System.Reflection.FieldInfo _postpadInfo = typeof(Havok.hk2014.hkpConstraintCollisionFilter).GetField("m_postpad")!;
    public hkpConstraintCollisionFilterData(Havok.hk2014.hkpConstraintCollisionFilter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_prepad":
            case "prepad":
            {
                value = instance.m_prepad;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_postpad":
            case "postpad":
            {
                value = instance.m_postpad;
                return true;
            }
            case "m_childFilter":
            case "childFilter":
            {
                value = instance.m_childFilter;
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
            case "m_prepad":
            case "prepad":
            {
                if (value is not uint[] castValue || castValue.Length != 2) return false;
                try
                {
                    _prepadInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkpCollisionFilter.hkpFilterType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (Havok.hk2014.hkpCollisionFilter.hkpFilterType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_postpad":
            case "postpad":
            {
                if (value is not uint[] castValue || castValue.Length != 3) return false;
                try
                {
                    _postpadInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_childFilter":
            case "childFilter":
            {
                if (value is null)
                {
                    instance.m_childFilter = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpCollisionFilter castValue)
                {
                    instance.m_childFilter = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
