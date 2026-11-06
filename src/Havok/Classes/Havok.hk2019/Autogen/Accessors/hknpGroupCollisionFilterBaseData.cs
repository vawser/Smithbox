// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpGroupCollisionFilterBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpGroupCollisionFilterBase>
{
    private static readonly System.Reflection.FieldInfo _collisionLookupTableInfo = typeof(Havok.hk2019.hknpGroupCollisionFilterBase).GetField("m_collisionLookupTable")!;
    public hknpGroupCollisionFilterBaseData(Havok.hk2019.hknpGroupCollisionFilterBase instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hknpCollisionFilter.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2019.hknpCollisionFilter.Type)byteValue;
                    return true;
                }
                return false;
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
            default:
            return false;
        }
    }

}
