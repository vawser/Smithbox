// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpExtendedExternMeshShapeGeometryQuadData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpExtendedExternMeshShapeGeometry.Quad>
{
    private static readonly System.Reflection.FieldInfo _verticesInfo = typeof(Havok.hk2018.hknpExtendedExternMeshShapeGeometry.Quad).GetField("m_vertices")!;
    public hknpExtendedExternMeshShapeGeometryQuadData(Havok.hk2018.hknpExtendedExternMeshShapeGeometry.Quad instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
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
