// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpPhysicsSceneDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpPhysicsSceneData>
{
    public hknpPhysicsSceneDataData(Havok.hk2019.hknpPhysicsSceneData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_systemDatas":
            case "systemDatas":
            {
                if (value is not List<Havok.hk2019.hknpPhysicsSystemData?> castValue) return false;
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
                if (value is Havok.hk2019.hknpRefWorldCinfo castValue)
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
