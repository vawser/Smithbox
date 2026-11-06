// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarGeometryPrimitivesPlaneInt64Vector4Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane.Int64Vector4>
{
    private static readonly System.Reflection.FieldInfo _vecInfo = typeof(Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane.Int64Vector4).GetField("m_vec")!;
    public hkcdPlanarGeometryPrimitivesPlaneInt64Vector4Data(Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane.Int64Vector4 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vec":
            case "vec":
            {
                value = instance.m_vec;
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
            case "m_vec":
            case "vec":
            {
                if (value is not long[] castValue || castValue.Length != 4) return false;
                try
                {
                    _vecInfo.SetValue(instance, value);
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
