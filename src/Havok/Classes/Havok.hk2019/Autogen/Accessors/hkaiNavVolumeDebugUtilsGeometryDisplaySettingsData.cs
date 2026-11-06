// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeDebugUtilsGeometryDisplaySettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeDebugUtils.GeometryDisplaySettings>
{
    public hkaiNavVolumeDebugUtilsGeometryDisplaySettingsData(Havok.hk2019.hkaiNavVolumeDebugUtils.GeometryDisplaySettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_color":
            case "color":
            {
                value = instance.m_color;
                return true;
            }
            case "m_drawLines":
            case "drawLines":
            {
                value = instance.m_drawLines;
                return true;
            }
            case "m_lineColor":
            case "lineColor":
            {
                value = instance.m_lineColor;
                return true;
            }
            case "m_colorVariance":
            case "colorVariance":
            {
                value = instance.m_colorVariance;
                return true;
            }
            case "m_sortGeometry":
            case "sortGeometry":
            {
                value = instance.m_sortGeometry;
                return true;
            }
            case "m_sortDirection":
            case "sortDirection":
            {
                value = instance.m_sortDirection;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                value = instance.m_displayOffset;
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
