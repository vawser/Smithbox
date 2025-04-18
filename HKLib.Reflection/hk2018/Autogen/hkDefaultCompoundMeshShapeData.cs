// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkDefaultCompoundMeshShapeData : HavokData<hkDefaultCompoundMeshShape> 
{
    public hkDefaultCompoundMeshShapeData(HavokType type, hkDefaultCompoundMeshShape instance) : base(type, instance) {}

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
            case "m_shapes":
            case "shapes":
            {
                if (instance.m_shapes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultChildTransforms":
            case "defaultChildTransforms":
            {
                if (instance.m_defaultChildTransforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (instance.m_sections is not TGet castValue) return false;
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
            case "m_shapes":
            case "shapes":
            {
                if (value is not List<hkMeshShape?> castValue) return false;
                instance.m_shapes = castValue;
                return true;
            }
            case "m_defaultChildTransforms":
            case "defaultChildTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_defaultChildTransforms = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<hkDefaultCompoundMeshShape.MeshSection> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
