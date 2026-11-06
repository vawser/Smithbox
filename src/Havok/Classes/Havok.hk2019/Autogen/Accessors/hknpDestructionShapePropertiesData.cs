// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpDestructionShapePropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpDestructionShapeProperties>
{
    public hknpDestructionShapePropertiesData(Havok.hk2019.hknpDestructionShapeProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_worldFromShape":
            case "worldFromShape":
            {
                value = instance.m_worldFromShape;
                return true;
            }
            case "m_sceneNodeUuid":
            case "sceneNodeUuid":
            {
                value = instance.m_sceneNodeUuid;
                return true;
            }
            case "m_isHierarchicalCompound":
            case "isHierarchicalCompound":
            {
                value = instance.m_isHierarchicalCompound;
                return true;
            }
            case "m_hasDestructionShapes":
            case "hasDestructionShapes":
            {
                value = instance.m_hasDestructionShapes;
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
            case "m_worldFromShape":
            case "worldFromShape":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_worldFromShape = castValue;
                return true;
            }
            case "m_sceneNodeUuid":
            case "sceneNodeUuid":
            {
                if (value is not Havok.hk2019.hkUuid castValue) return false;
                instance.m_sceneNodeUuid = castValue;
                return true;
            }
            case "m_isHierarchicalCompound":
            case "isHierarchicalCompound":
            {
                if (value is not bool castValue) return false;
                instance.m_isHierarchicalCompound = castValue;
                return true;
            }
            case "m_hasDestructionShapes":
            case "hasDestructionShapes":
            {
                if (value is not bool castValue) return false;
                instance.m_hasDestructionShapes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
