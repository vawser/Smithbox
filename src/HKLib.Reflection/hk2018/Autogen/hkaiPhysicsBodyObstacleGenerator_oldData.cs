// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiAvoidanceSolver;

namespace HKLib.Reflection.hk2018;

internal class hkaiPhysicsBodyObstacleGenerator_oldData : HavokData<hkaiPhysicsBodyObstacleGenerator_old> 
{
    public hkaiPhysicsBodyObstacleGenerator_oldData(HavokType type, hkaiPhysicsBodyObstacleGenerator_old instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useSpheres":
            case "useSpheres":
            {
                if (instance.m_useSpheres is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useBoundaries":
            case "useBoundaries":
            {
                if (instance.m_useBoundaries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clipBoundaries":
            case "clipBoundaries":
            {
                if (instance.m_clipBoundaries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
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
            case "m_boundaries":
            case "boundaries":
            {
                if (instance.m_boundaries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_useSpheres":
            case "useSpheres":
            {
                if (value is not bool castValue) return false;
                instance.m_useSpheres = castValue;
                return true;
            }
            case "m_useBoundaries":
            case "useBoundaries":
            {
                if (value is not bool castValue) return false;
                instance.m_useBoundaries = castValue;
                return true;
            }
            case "m_clipBoundaries":
            case "clipBoundaries":
            {
                if (value is not bool castValue) return false;
                instance.m_clipBoundaries = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (value is not List<SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_boundaries":
            case "boundaries":
            {
                if (value is not List<BoundaryObstacle> castValue) return false;
                instance.m_boundaries = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
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
