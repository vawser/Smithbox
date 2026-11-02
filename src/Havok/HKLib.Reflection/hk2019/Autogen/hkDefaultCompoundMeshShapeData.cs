// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkDefaultCompoundMeshShapeData : HavokData<HKLib.hk2019.hkDefaultCompoundMeshShape> 
{
    public hkDefaultCompoundMeshShapeData(HavokType type, HKLib.hk2019.hkDefaultCompoundMeshShape instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_shapes":
            case "shapes":
            {
                if (value is not List<HKLib.hk2019.hkMeshShape?> castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkDefaultCompoundMeshShape.MeshSection> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
