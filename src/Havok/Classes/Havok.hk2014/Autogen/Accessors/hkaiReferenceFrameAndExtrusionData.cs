// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiReferenceFrameAndExtrusionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiReferenceFrameAndExtrusion>
{
    public hkaiReferenceFrameAndExtrusionData(Havok.hk2014.hkaiReferenceFrameAndExtrusion instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_cellExtrusion":
            case "cellExtrusion":
            {
                value = instance.m_cellExtrusion;
                return true;
            }
            case "m_silhouetteRadiusExpasion":
            case "silhouetteRadiusExpasion":
            {
                value = instance.m_silhouetteRadiusExpasion;
                return true;
            }
            case "m_upTransformMethod":
            case "upTransformMethod":
            {
                value = instance.m_upTransformMethod;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_cellExtrusion":
            case "cellExtrusion":
            {
                if (value is not float castValue) return false;
                instance.m_cellExtrusion = castValue;
                return true;
            }
            case "m_silhouetteRadiusExpasion":
            case "silhouetteRadiusExpasion":
            {
                if (value is not float castValue) return false;
                instance.m_silhouetteRadiusExpasion = castValue;
                return true;
            }
            case "m_upTransformMethod":
            case "upTransformMethod":
            {
                if (value is Havok.hk2014.hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod castValue)
                {
                    instance.m_upTransformMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_upTransformMethod = (Havok.hk2014.hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
