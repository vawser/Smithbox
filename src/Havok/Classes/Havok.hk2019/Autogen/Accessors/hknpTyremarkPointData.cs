// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpTyremarkPointData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpTyremarkPoint>
{
    public hknpTyremarkPointData(Havok.hk2019.hknpTyremarkPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pointLeft":
            case "pointLeft":
            {
                value = instance.m_pointLeft;
                return true;
            }
            case "m_pointRight":
            case "pointRight":
            {
                value = instance.m_pointRight;
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
            case "m_pointLeft":
            case "pointLeft":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointLeft = castValue;
                return true;
            }
            case "m_pointRight":
            case "pointRight":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointRight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
