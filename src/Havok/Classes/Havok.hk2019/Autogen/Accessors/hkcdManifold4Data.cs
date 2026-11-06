// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdManifold4Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdManifold4>
{
    private static readonly System.Reflection.FieldInfo _distancesInfo = typeof(Havok.hk2019.hkcdManifold4).GetField("m_distances")!;
    private static readonly System.Reflection.FieldInfo _positionsInfo = typeof(Havok.hk2019.hkcdManifold4).GetField("m_positions")!;
    public hkcdManifold4Data(Havok.hk2019.hkcdManifold4 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numPoints":
            case "numPoints":
            {
                value = instance.m_numPoints;
                return true;
            }
            case "m_minimumDistance":
            case "minimumDistance":
            {
                value = instance.m_minimumDistance;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                value = instance.m_normal;
                return true;
            }
            case "m_weldNormal":
            case "weldNormal":
            {
                value = instance.m_weldNormal;
                return true;
            }
            case "m_distances":
            case "distances":
            {
                value = instance.m_distances;
                return true;
            }
            case "m_positions":
            case "positions":
            {
                value = instance.m_positions;
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
            case "m_numPoints":
            case "numPoints":
            {
                if (value is not int castValue) return false;
                instance.m_numPoints = castValue;
                return true;
            }
            case "m_minimumDistance":
            case "minimumDistance":
            {
                if (value is not float castValue) return false;
                instance.m_minimumDistance = castValue;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_normal = castValue;
                return true;
            }
            case "m_weldNormal":
            case "weldNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_weldNormal = castValue;
                return true;
            }
            case "m_distances":
            case "distances":
            {
                if (value is not float[] castValue || castValue.Length != 4) return false;
                try
                {
                    _distancesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_positions":
            case "positions":
            {
                if (value is not Vector4[] castValue || castValue.Length != 4) return false;
                try
                {
                    _positionsInfo.SetValue(instance, value);
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
