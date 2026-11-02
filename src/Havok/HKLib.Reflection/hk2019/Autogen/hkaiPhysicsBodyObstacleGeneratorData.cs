// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPhysicsBodyObstacleGeneratorData : HavokData<HKLib.hk2019.hkaiPhysicsBodyObstacleGenerator> 
{
    public hkaiPhysicsBodyObstacleGeneratorData(HavokType type, HKLib.hk2019.hkaiPhysicsBodyObstacleGenerator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clipLineSegments":
            case "clipLineSegments":
            {
                if (instance.m_clipLineSegments is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (instance.m_spheres is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lineSegments":
            case "lineSegments":
            {
                if (instance.m_lineSegments is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                if (instance.m_localAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityThreshold":
            case "velocityThreshold":
            {
                if (instance.m_velocityThreshold is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_lineSegments":
            case "lineSegments":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.LineSegmentObstacle> castValue) return false;
                instance.m_lineSegments = castValue;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_localAabb = castValue;
                return true;
            }
            case "m_velocityThreshold":
            case "velocityThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_velocityThreshold = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
