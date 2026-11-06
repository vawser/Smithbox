// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpPhysicsSceneDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpPhysicsSceneData>
{
    public hknpPhysicsSceneDataData(Havok.hk2016.hknpPhysicsSceneData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_systemDatas":
            case "systemDatas":
            {
                value = instance.m_systemDatas;
                return true;
            }
            case "m_worldCinfo":
            case "worldCinfo":
            {
                value = instance.m_worldCinfo;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_systemDatas":
            case "systemDatas":
            {
                if (value is not List<Havok.hk2016.hknpPhysicsSystemData?> castValue) return false;
                instance.m_systemDatas = castValue;
                return true;
            }
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (value is null)
                {
                    instance.m_worldCinfo = default;
                    return true;
                }
                if (value is Havok.hk2016.hknpRefWorldCinfo castValue)
                {
                    instance.m_worldCinfo = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
