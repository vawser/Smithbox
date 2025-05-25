// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpDestructionShapePropertiesData : HavokData<hknpDestructionShapeProperties> 
{
    public hknpDestructionShapePropertiesData(HavokType type, hknpDestructionShapeProperties instance) : base(type, instance) {}

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
            case "m_worldFromShape":
            case "worldFromShape":
            {
                if (instance.m_worldFromShape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sceneNodeUuid":
            case "sceneNodeUuid":
            {
                if (instance.m_sceneNodeUuid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isHierarchicalCompound":
            case "isHierarchicalCompound":
            {
                if (instance.m_isHierarchicalCompound is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasDestructionShapes":
            case "hasDestructionShapes":
            {
                if (instance.m_hasDestructionShapes is not TGet castValue) return false;
                value = castValue;
                return true;
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
                if (value is not hkUuid castValue) return false;
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
