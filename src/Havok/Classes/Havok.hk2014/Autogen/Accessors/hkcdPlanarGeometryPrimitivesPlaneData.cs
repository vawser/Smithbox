// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdPlanarGeometryPrimitivesPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdPlanarGeometryPrimitives.Plane>
{
    private static readonly System.Reflection.FieldInfo _iEqnInfo = typeof(Havok.hk2014.hkcdPlanarGeometryPrimitives.Plane).GetField("m_iEqn")!;
    public hkcdPlanarGeometryPrimitivesPlaneData(Havok.hk2014.hkcdPlanarGeometryPrimitives.Plane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_iEqn":
            case "iEqn":
            {
                value = instance.m_iEqn;
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
            case "m_iEqn":
            case "iEqn":
            {
                if (value is not long[] castValue || castValue.Length != 4) return false;
                try
                {
                    _iEqnInfo.SetValue(instance, value);
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
