// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiObstacleGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiObstacleGenerator>
{
    public hkaiObstacleGeneratorData(Havok.hk2018.hkaiObstacleGenerator instance) : base(instance) { }

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
            case "m_useSpheres":
            case "useSpheres":
            {
                value = instance.m_useSpheres;
                return true;
            }
            case "m_useBoundaries":
            case "useBoundaries":
            {
                value = instance.m_useBoundaries;
                return true;
            }
            case "m_clipBoundaries":
            case "clipBoundaries":
            {
                value = instance.m_clipBoundaries;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                value = instance.m_spheres;
                return true;
            }
            case "m_boundaries":
            case "boundaries":
            {
                value = instance.m_boundaries;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
                if (value is not List<Havok.hk2018.hkaiAvoidanceSolver.SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_boundaries":
            case "boundaries":
            {
                if (value is not List<Havok.hk2018.hkaiAvoidanceSolver.BoundaryObstacle> castValue) return false;
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
            default:
            return false;
        }
    }

}
