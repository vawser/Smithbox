// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclPlaneShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclPlaneShape>
{
    public hclPlaneShapeData(Havok.hk2014.hclPlaneShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planeEquation":
            case "planeEquation":
            {
                value = instance.m_planeEquation;
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
            case "m_planeEquation":
            case "planeEquation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_planeEquation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
