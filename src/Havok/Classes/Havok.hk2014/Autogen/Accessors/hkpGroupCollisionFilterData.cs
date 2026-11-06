// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpGroupCollisionFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpGroupCollisionFilter>
{
    private static readonly System.Reflection.FieldInfo _prepadInfo = typeof(Havok.hk2014.hkpGroupCollisionFilter).GetField("m_prepad")!;
    private static readonly System.Reflection.FieldInfo _postpadInfo = typeof(Havok.hk2014.hkpGroupCollisionFilter).GetField("m_postpad")!;
    private static readonly System.Reflection.FieldInfo _collisionGroupsInfo = typeof(Havok.hk2014.hkpGroupCollisionFilter).GetField("m_collisionGroups")!;
    public hkpGroupCollisionFilterData(Havok.hk2014.hkpGroupCollisionFilter instance) : base(instance) { }

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
            case "m_noGroupCollisionEnabled":
            case "noGroupCollisionEnabled":
            {
                value = instance.m_noGroupCollisionEnabled;
                return true;
            }
            case "m_collisionGroups":
            case "collisionGroups":
            {
                value = instance.m_collisionGroups;
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
            case "m_noGroupCollisionEnabled":
            case "noGroupCollisionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_noGroupCollisionEnabled = castValue;
                return true;
            }
            case "m_collisionGroups":
            case "collisionGroups":
            {
                if (value is not uint[] castValue || castValue.Length != 32) return false;
                try
                {
                    _collisionGroupsInfo.SetValue(instance, value);
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
