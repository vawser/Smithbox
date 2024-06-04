// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class fsnpCustomMeshParameterTriangleDataData : HavokData<fsnpCustomMeshParameter.TriangleData> 
{
    public fsnpCustomMeshParameterTriangleDataData(HavokType type, fsnpCustomMeshParameter.TriangleData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_primitiveDataIndex":
            case "primitiveDataIndex":
            {
                if (instance.m_primitiveDataIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleDataIndex":
            case "triangleDataIndex":
            {
                if (instance.m_triangleDataIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexIndexA":
            case "vertexIndexA":
            {
                if (instance.m_vertexIndexA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexIndexB":
            case "vertexIndexB":
            {
                if (instance.m_vertexIndexB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexIndexC":
            case "vertexIndexC":
            {
                if (instance.m_vertexIndexC is not TGet castValue) return false;
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
            case "m_primitiveDataIndex":
            case "primitiveDataIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_primitiveDataIndex = castValue;
                return true;
            }
            case "m_triangleDataIndex":
            case "triangleDataIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_triangleDataIndex = castValue;
                return true;
            }
            case "m_vertexIndexA":
            case "vertexIndexA":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexIndexA = castValue;
                return true;
            }
            case "m_vertexIndexB":
            case "vertexIndexB":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexIndexB = castValue;
                return true;
            }
            case "m_vertexIndexC":
            case "vertexIndexC":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexIndexC = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
