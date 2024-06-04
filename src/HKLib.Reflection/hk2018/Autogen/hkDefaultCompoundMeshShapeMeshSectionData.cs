// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkDefaultCompoundMeshShapeMeshSectionData : HavokData<hkDefaultCompoundMeshShape.MeshSection> 
{
    public hkDefaultCompoundMeshShapeMeshSectionData(HavokType type, hkDefaultCompoundMeshShape.MeshSection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shapeIndex":
            case "shapeIndex":
            {
                if (instance.m_shapeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (instance.m_sectionIndex is not TGet castValue) return false;
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
            case "m_shapeIndex":
            case "shapeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_shapeIndex = castValue;
                return true;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
