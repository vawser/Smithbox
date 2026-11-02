// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class fsnpCustomMeshParameterData : HavokData<HKLib.hk2019.fsnpCustomMeshParameter> 
{
    public fsnpCustomMeshParameterData(HavokType type, HKLib.hk2019.fsnpCustomMeshParameter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_triangleDataArray":
            case "triangleDataArray":
            {
                if (instance.m_triangleDataArray is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primitiveDataArray":
            case "primitiveDataArray":
            {
                if (instance.m_primitiveDataArray is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexDataStride":
            case "vertexDataStride":
            {
                if (instance.m_vertexDataStride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleDataStride":
            case "triangleDataStride":
            {
                if (instance.m_triangleDataStride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_version":
            case "version":
            {
                if (instance.m_version is not TGet castValue) return false;
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
            case "m_triangleDataArray":
            case "triangleDataArray":
            {
                if (value is not List<HKLib.hk2019.fsnpCustomMeshParameter.TriangleData> castValue) return false;
                instance.m_triangleDataArray = castValue;
                return true;
            }
            case "m_primitiveDataArray":
            case "primitiveDataArray":
            {
                if (value is not List<HKLib.hk2019.fsnpCustomMeshParameter.PrimitiveData> castValue) return false;
                instance.m_primitiveDataArray = castValue;
                return true;
            }
            case "m_vertexDataStride":
            case "vertexDataStride":
            {
                if (value is not int castValue) return false;
                instance.m_vertexDataStride = castValue;
                return true;
            }
            case "m_triangleDataStride":
            case "triangleDataStride":
            {
                if (value is not int castValue) return false;
                instance.m_triangleDataStride = castValue;
                return true;
            }
            case "m_version":
            case "version":
            {
                if (value is not uint castValue) return false;
                instance.m_version = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
