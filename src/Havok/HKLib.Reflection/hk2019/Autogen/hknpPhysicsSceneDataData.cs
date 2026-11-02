// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpPhysicsSceneDataData : HavokData<HKLib.hk2019.hknpPhysicsSceneData> 
{
    public hknpPhysicsSceneDataData(HavokType type, HKLib.hk2019.hknpPhysicsSceneData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_systemDatas":
            case "systemDatas":
            {
                if (value is not List<HKLib.hk2019.hknpPhysicsSystemData?> castValue) return false;
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
                if (value is HKLib.hk2019.hknpRefWorldCinfo castValue)
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
