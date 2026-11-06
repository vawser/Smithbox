// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpConvexPolytopeShapeFaceData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpConvexPolytopeShape.Face>
{
    public hknpConvexPolytopeShapeFaceData(Havok.hk2016.hknpConvexPolytopeShape.Face instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_firstIndex":
            case "firstIndex":
            {
                value = instance.m_firstIndex;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                value = instance.m_numIndices;
                return true;
            }
            case "m_minHalfAngle":
            case "minHalfAngle":
            {
                value = instance.m_minHalfAngle;
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
            case "m_firstIndex":
            case "firstIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_firstIndex = castValue;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                if (value is not byte castValue) return false;
                instance.m_numIndices = castValue;
                return true;
            }
            case "m_minHalfAngle":
            case "minHalfAngle":
            {
                if (value is not byte castValue) return false;
                instance.m_minHalfAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
