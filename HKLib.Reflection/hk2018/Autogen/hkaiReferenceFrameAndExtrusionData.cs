// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiReferenceFrameAndExtrusionData : HavokData<hkaiReferenceFrameAndExtrusion> 
{
    private static readonly System.Reflection.FieldInfo _cellExtrusionsInfo = typeof(hkaiReferenceFrameAndExtrusion).GetField("m_cellExtrusions")!;
    public hkaiReferenceFrameAndExtrusionData(HavokType type, hkaiReferenceFrameAndExtrusion instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellExtrusions":
            case "cellExtrusions":
            {
                if (instance.m_cellExtrusions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteRadiusExpasion":
            case "silhouetteRadiusExpasion":
            {
                if (instance.m_silhouetteRadiusExpasion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_upTransformMethod":
            case "upTransformMethod":
            {
                if (instance.m_upTransformMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_upTransformMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod castValue)
                {
                    instance.m_upTransformMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_upTransformMethod = (hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
