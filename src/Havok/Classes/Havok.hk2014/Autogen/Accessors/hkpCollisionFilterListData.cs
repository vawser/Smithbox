// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCollisionFilterListData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCollisionFilterList>
{
    private static readonly System.Reflection.FieldInfo _prepadInfo = typeof(Havok.hk2014.hkpCollisionFilterList).GetField("m_prepad")!;
    private static readonly System.Reflection.FieldInfo _postpadInfo = typeof(Havok.hk2014.hkpCollisionFilterList).GetField("m_postpad")!;
    public hkpCollisionFilterListData(Havok.hk2014.hkpCollisionFilterList instance) : base(instance) { }

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
            case "m_collisionFilters":
            case "collisionFilters":
            {
                value = instance.m_collisionFilters;
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
            case "m_collisionFilters":
            case "collisionFilters":
            {
                if (value is not List<Havok.hk2014.hkpCollisionFilter?> castValue) return false;
                instance.m_collisionFilters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
