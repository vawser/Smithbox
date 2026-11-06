// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpCircularSurfaceVelocityData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpCircularSurfaceVelocity>
{
    public hknpCircularSurfaceVelocityData(Havok.hk2014.hknpCircularSurfaceVelocity instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_velocityIsLocalSpace":
            case "velocityIsLocalSpace":
            {
                value = instance.m_velocityIsLocalSpace;
                return true;
            }
            case "m_pivot":
            case "pivot":
            {
                value = instance.m_pivot;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                value = instance.m_angularVelocity;
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
            case "m_velocityIsLocalSpace":
            case "velocityIsLocalSpace":
            {
                if (value is not bool castValue) return false;
                instance.m_velocityIsLocalSpace = castValue;
                return true;
            }
            case "m_pivot":
            case "pivot":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pivot = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
