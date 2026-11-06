// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBreakableShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBreakableShape>
{
    public hkpBreakableShapeData(Havok.hk2014.hkpBreakableShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_physicsShape":
            case "physicsShape":
            {
                value = instance.m_physicsShape;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
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
            case "m_physicsShape":
            case "physicsShape":
            {
                if (value is null)
                {
                    instance.m_physicsShape = default;
                    return true;
                }
                if (value is Havok.hk2014.hkcdShape castValue)
                {
                    instance.m_physicsShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (value is null)
                {
                    instance.m_material = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpBreakableMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
