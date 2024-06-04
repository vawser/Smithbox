// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavVolumeDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeDebugUtilsGeometryDisplaySettingsData : HavokData<GeometryDisplaySettings> 
{
    public hkaiNavVolumeDebugUtilsGeometryDisplaySettingsData(HavokType type, GeometryDisplaySettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_color":
            case "color":
            {
                if (instance.m_color is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawLines":
            case "drawLines":
            {
                if (instance.m_drawLines is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lineColor":
            case "lineColor":
            {
                if (instance.m_lineColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorVariance":
            case "colorVariance":
            {
                if (instance.m_colorVariance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sortGeometry":
            case "sortGeometry":
            {
                if (instance.m_sortGeometry is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sortDirection":
            case "sortDirection":
            {
                if (instance.m_sortDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (instance.m_displayOffset is not TGet castValue) return false;
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
            case "m_color":
            case "color":
            {
                if (value is not Color castValue) return false;
                instance.m_color = castValue;
                return true;
            }
            case "m_drawLines":
            case "drawLines":
            {
                if (value is not bool castValue) return false;
                instance.m_drawLines = castValue;
                return true;
            }
            case "m_lineColor":
            case "lineColor":
            {
                if (value is not Color castValue) return false;
                instance.m_lineColor = castValue;
                return true;
            }
            case "m_colorVariance":
            case "colorVariance":
            {
                if (value is not float castValue) return false;
                instance.m_colorVariance = castValue;
                return true;
            }
            case "m_sortGeometry":
            case "sortGeometry":
            {
                if (value is not bool castValue) return false;
                instance.m_sortGeometry = castValue;
                return true;
            }
            case "m_sortDirection":
            case "sortDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_sortDirection = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_displayOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
