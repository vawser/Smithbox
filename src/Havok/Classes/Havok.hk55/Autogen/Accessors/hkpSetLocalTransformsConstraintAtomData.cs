// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpSetLocalTransformsConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpSetLocalTransformsConstraintAtom>
{
    public hkpSetLocalTransformsConstraintAtomData(Havok.hk55.hkpSetLocalTransformsConstraintAtom instance) : base(instance) { }

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
            case "m_transformA":
            case "transformA":
            {
                value = instance.m_transformA;
                return true;
            }
            case "m_transformB":
            case "transformB":
            {
                value = instance.m_transformB;
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
                if (value is Havok.hk55.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk55.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_transformA":
            case "transformA":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transformA = castValue;
                return true;
            }
            case "m_transformB":
            case "transformB":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transformB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
