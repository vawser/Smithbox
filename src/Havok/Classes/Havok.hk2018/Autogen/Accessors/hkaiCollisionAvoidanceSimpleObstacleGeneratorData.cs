// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCollisionAvoidanceSimpleObstacleGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCollisionAvoidance.SimpleObstacleGenerator>
{
    public hkaiCollisionAvoidanceSimpleObstacleGeneratorData(Havok.hk2018.hkaiCollisionAvoidance.SimpleObstacleGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_clipLineSegments":
            case "clipLineSegments":
            {
                value = instance.m_clipLineSegments;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                value = instance.m_spheres;
                return true;
            }
            case "m_lineSegments":
            case "lineSegments":
            {
                value = instance.m_lineSegments;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                value = instance.m_localAabb;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_clipLineSegments":
            case "clipLineSegments":
            {
                if (value is not bool castValue) return false;
                instance.m_clipLineSegments = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2018.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_lineSegments":
            case "lineSegments":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.LineSegmentObstacle> castValue) return false;
                instance.m_lineSegments = castValue;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_localAabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
