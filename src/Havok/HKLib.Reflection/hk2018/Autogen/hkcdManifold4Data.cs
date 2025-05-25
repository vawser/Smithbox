// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdManifold4Data : HavokData<hkcdManifold4> 
{
    private static readonly System.Reflection.FieldInfo _distancesInfo = typeof(hkcdManifold4).GetField("m_distances")!;
    private static readonly System.Reflection.FieldInfo _positionsInfo = typeof(hkcdManifold4).GetField("m_positions")!;
    public hkcdManifold4Data(HavokType type, hkcdManifold4 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numPoints":
            case "numPoints":
            {
                if (instance.m_numPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minimumDistance":
            case "minimumDistance":
            {
                if (instance.m_minimumDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                if (instance.m_normal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weldNormal":
            case "weldNormal":
            {
                if (instance.m_weldNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distances":
            case "distances":
            {
                if (instance.m_distances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positions":
            case "positions":
            {
                if (instance.m_positions is not TGet castValue) return false;
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
