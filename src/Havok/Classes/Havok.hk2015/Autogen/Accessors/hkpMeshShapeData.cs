// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpMeshShape>
{
    private static readonly System.Reflection.FieldInfo _padInfo = typeof(Havok.hk2015.hkpMeshShape).GetField("m_pad")!;
    public hkpMeshShapeData(Havok.hk2015.hkpMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_dispatchType":
            case "dispatchType":
            {
                value = instance.m_dispatchType;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                value = instance.m_bitsPerKey;
                return true;
            }
            case "m_shapeInfoCodecType":
            case "shapeInfoCodecType":
            {
                value = instance.m_shapeInfoCodecType;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_disableWelding":
            case "disableWelding":
            {
                value = instance.m_disableWelding;
                return true;
            }
            case "m_collectionType":
            case "collectionType":
            {
                value = instance.m_collectionType;
                return true;
            }
            case "m_scaling":
            case "scaling":
            {
                value = instance.m_scaling;
                return true;
            }
            case "m_numBitsForSubpartIndex":
            case "numBitsForSubpartIndex":
            {
                value = instance.m_numBitsForSubpartIndex;
                return true;
            }
            case "m_subparts":
            case "subparts":
            {
                value = instance.m_subparts;
                return true;
            }
            case "m_weldingInfo":
            case "weldingInfo":
            {
                value = instance.m_weldingInfo;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                value = instance.m_weldingType;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_pad":
            case "pad":
            {
                value = instance.m_pad;
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
            case "m_dispatchType":
            case "dispatchType":
            {
                if (value is not byte castValue) return false;
                instance.m_dispatchType = castValue;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                if (value is not byte castValue) return false;
                instance.m_bitsPerKey = castValue;
                return true;
            }
            case "m_shapeInfoCodecType":
            case "shapeInfoCodecType":
            {
                if (value is not byte castValue) return false;
                instance.m_shapeInfoCodecType = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_disableWelding":
            case "disableWelding":
            {
                if (value is not bool castValue) return false;
                instance.m_disableWelding = castValue;
                return true;
            }
            case "m_collectionType":
            case "collectionType":
            {
                if (value is not byte castValue) return false;
                instance.m_collectionType = castValue;
                return true;
            }
            case "m_scaling":
            case "scaling":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scaling = castValue;
                return true;
            }
            case "m_numBitsForSubpartIndex":
            case "numBitsForSubpartIndex":
            {
                if (value is not int castValue) return false;
                instance.m_numBitsForSubpartIndex = castValue;
                return true;
            }
            case "m_subparts":
            case "subparts":
            {
                if (value is not List<Havok.hk2015.hkpMeshShape.Subpart> castValue) return false;
                instance.m_subparts = castValue;
                return true;
            }
            case "m_weldingInfo":
            case "weldingInfo":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_weldingInfo = castValue;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                if (value is not byte castValue) return false;
                instance.m_weldingType = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_pad":
            case "pad":
            {
                if (value is not int[] castValue || castValue.Length != 3) return false;
                try
                {
                    _padInfo.SetValue(instance, value);
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
