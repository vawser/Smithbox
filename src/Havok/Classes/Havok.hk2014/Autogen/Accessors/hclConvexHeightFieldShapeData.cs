// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclConvexHeightFieldShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclConvexHeightFieldShape>
{
    private static readonly System.Reflection.FieldInfo _facesInfo = typeof(Havok.hk2014.hclConvexHeightFieldShape).GetField("m_faces")!;
    public hclConvexHeightFieldShapeData(Havok.hk2014.hclConvexHeightFieldShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_res":
            case "res":
            {
                value = instance.m_res;
                return true;
            }
            case "m_resIncBorder":
            case "resIncBorder":
            {
                value = instance.m_resIncBorder;
                return true;
            }
            case "m_floatCorrectionOffset":
            case "floatCorrectionOffset":
            {
                value = instance.m_floatCorrectionOffset;
                return true;
            }
            case "m_heights":
            case "heights":
            {
                value = instance.m_heights;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                value = instance.m_faces;
                return true;
            }
            case "m_localToMapTransform":
            case "localToMapTransform":
            {
                value = instance.m_localToMapTransform;
                return true;
            }
            case "m_localToMapScale":
            case "localToMapScale":
            {
                value = instance.m_localToMapScale;
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
