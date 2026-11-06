// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiReferenceFrameAndExtrusionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiReferenceFrameAndExtrusion>
{
    private static readonly System.Reflection.FieldInfo _cellExtrusionsInfo = typeof(Havok.hk2018.hkaiReferenceFrameAndExtrusion).GetField("m_cellExtrusions")!;
    public hkaiReferenceFrameAndExtrusionData(Havok.hk2018.hkaiReferenceFrameAndExtrusion instance) : base(instance) { }

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
            case "m_cellExtrusions":
            case "cellExtrusions":
            {
                value = instance.m_cellExtrusions;
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
            case "m_cellExtrusions":
            case "cellExtrusions":
            {
                if (value is not float[] castValue || castValue.Length != 32) return false;
                try
                {
                    _cellExtrusionsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
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
                if (value is Havok.hk2018.hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod castValue)
                {
                    instance.m_upTransformMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_upTransformMethod = (Havok.hk2018.hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
