// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclConvexHeightFieldShapeData : HavokData<hclConvexHeightFieldShape> 
{
    private static readonly System.Reflection.FieldInfo _facesInfo = typeof(hclConvexHeightFieldShape).GetField("m_faces")!;
    public hclConvexHeightFieldShapeData(HavokType type, hclConvexHeightFieldShape instance) : base(type, instance) {}

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
            case "m_res":
            case "res":
            {
                if (instance.m_res is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resIncBorder":
            case "resIncBorder":
            {
                if (instance.m_resIncBorder is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatCorrectionOffset":
            case "floatCorrectionOffset":
            {
                if (instance.m_floatCorrectionOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_heights":
            case "heights":
            {
                if (instance.m_heights is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (instance.m_faces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localToMapTransform":
            case "localToMapTransform":
            {
                if (instance.m_localToMapTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localToMapScale":
            case "localToMapScale":
            {
                if (instance.m_localToMapScale is not TGet castValue) return false;
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
            case "m_res":
            case "res":
            {
                if (value is not ushort castValue) return false;
                instance.m_res = castValue;
                return true;
            }
            case "m_resIncBorder":
            case "resIncBorder":
            {
                if (value is not ushort castValue) return false;
                instance.m_resIncBorder = castValue;
                return true;
            }
            case "m_floatCorrectionOffset":
            case "floatCorrectionOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_floatCorrectionOffset = castValue;
                return true;
            }
            case "m_heights":
            case "heights":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_heights = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not int[] castValue || castValue.Length != 6) return false;
                try
                {
                    _facesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_localToMapTransform":
            case "localToMapTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_localToMapTransform = castValue;
                return true;
            }
            case "m_localToMapScale":
            case "localToMapScale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localToMapScale = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
