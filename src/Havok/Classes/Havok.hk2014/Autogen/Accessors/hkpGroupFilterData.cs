// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpGroupFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpGroupFilter>
{
    private static readonly System.Reflection.FieldInfo _prepadInfo = typeof(Havok.hk2014.hkpGroupFilter).GetField("m_prepad")!;
    private static readonly System.Reflection.FieldInfo _postpadInfo = typeof(Havok.hk2014.hkpGroupFilter).GetField("m_postpad")!;
    private static readonly System.Reflection.FieldInfo _collisionLookupTableInfo = typeof(Havok.hk2014.hkpGroupFilter).GetField("m_collisionLookupTable")!;
    private static readonly System.Reflection.FieldInfo _pad256Info = typeof(Havok.hk2014.hkpGroupFilter).GetField("m_pad256")!;
    public hkpGroupFilterData(Havok.hk2014.hkpGroupFilter instance) : base(instance) { }

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
            case "m_nextFreeSystemGroup":
            case "nextFreeSystemGroup":
            {
                value = instance.m_nextFreeSystemGroup;
                return true;
            }
            case "m_collisionLookupTable":
            case "collisionLookupTable":
            {
                value = instance.m_collisionLookupTable;
                return true;
            }
            case "m_pad256":
            case "pad256":
            {
                value = instance.m_pad256;
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
            case "m_nextFreeSystemGroup":
            case "nextFreeSystemGroup":
            {
                if (value is not int castValue) return false;
                instance.m_nextFreeSystemGroup = castValue;
                return true;
            }
            case "m_collisionLookupTable":
            case "collisionLookupTable":
            {
                if (value is not uint[] castValue || castValue.Length != 32) return false;
                try
                {
                    _collisionLookupTableInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_pad256":
            case "pad256":
            {
                if (value is not Vector4[] castValue || castValue.Length != 4) return false;
                try
                {
                    _pad256Info.SetValue(instance, value);
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
