// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpPhysicsSceneDataData : HavokData<hknpPhysicsSceneData> 
{
    public hknpPhysicsSceneDataData(HavokType type, hknpPhysicsSceneData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_systemDatas":
            case "systemDatas":
            {
                if (instance.m_systemDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (instance.m_worldCinfo is null)
                {
                    return true;
                }
                if (instance.m_worldCinfo is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_systemDatas":
            case "systemDatas":
            {
                if (value is not List<HKLib.hk2018.hknpPhysicsSystemData?> castValue) return false;
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
                if (value is hknpRefWorldCinfo castValue)
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
