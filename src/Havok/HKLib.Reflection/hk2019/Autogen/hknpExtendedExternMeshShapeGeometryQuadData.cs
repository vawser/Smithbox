// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpExtendedExternMeshShapeGeometryQuadData : HavokData<HKLib.hk2019.hknpExtendedExternMeshShapeGeometry.Quad> 
{
    private static readonly System.Reflection.FieldInfo _verticesInfo = typeof(HKLib.hk2019.hknpExtendedExternMeshShapeGeometry.Quad).GetField("m_vertices")!;
    public hknpExtendedExternMeshShapeGeometryQuadData(HavokType type, HKLib.hk2019.hknpExtendedExternMeshShapeGeometry.Quad instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
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
            case "m_vertices":
            case "vertices":
            {
                if (value is not Vector4[] castValue || castValue.Length != 4) return false;
                try
                {
                    _verticesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
