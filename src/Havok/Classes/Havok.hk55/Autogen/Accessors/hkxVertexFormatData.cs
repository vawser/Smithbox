// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxVertexFormatData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxVertexFormat>
{
    public hkxVertexFormatData(Havok.hk55.hkxVertexFormat instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_stride":
            case "stride":
            {
                value = instance.m_stride;
                return true;
            }
            case "m_positionOffset":
            case "positionOffset":
            {
                value = instance.m_positionOffset;
                return true;
            }
            case "m_normalOffset":
            case "normalOffset":
            {
                value = instance.m_normalOffset;
                return true;
            }
            case "m_tangentOffset":
            case "tangentOffset":
            {
                value = instance.m_tangentOffset;
                return true;
            }
            case "m_binormalOffset":
            case "binormalOffset":
            {
                value = instance.m_binormalOffset;
                return true;
            }
            case "m_numBonesPerVertex":
            case "numBonesPerVertex":
            {
                value = instance.m_numBonesPerVertex;
                return true;
            }
            case "m_boneIndexOffset":
            case "boneIndexOffset":
            {
                value = instance.m_boneIndexOffset;
                return true;
            }
            case "m_boneWeightOffset":
            case "boneWeightOffset":
            {
                value = instance.m_boneWeightOffset;
                return true;
            }
            case "m_numTextureChannels":
            case "numTextureChannels":
            {
                value = instance.m_numTextureChannels;
                return true;
            }
            case "m_tFloatCoordOffset":
            case "tFloatCoordOffset":
            {
                value = instance.m_tFloatCoordOffset;
                return true;
            }
            case "m_tQuantizedCoordOffset":
            case "tQuantizedCoordOffset":
            {
                value = instance.m_tQuantizedCoordOffset;
                return true;
            }
            case "m_colorOffset":
            case "colorOffset":
            {
                value = instance.m_colorOffset;
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
            case "m_stride":
            case "stride":
            {
                if (value is not byte castValue) return false;
                instance.m_stride = castValue;
                return true;
            }
            case "m_positionOffset":
            case "positionOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_positionOffset = castValue;
                return true;
            }
            case "m_normalOffset":
            case "normalOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_normalOffset = castValue;
                return true;
            }
            case "m_tangentOffset":
            case "tangentOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_tangentOffset = castValue;
                return true;
            }
            case "m_binormalOffset":
            case "binormalOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_binormalOffset = castValue;
                return true;
            }
            case "m_numBonesPerVertex":
            case "numBonesPerVertex":
            {
                if (value is not byte castValue) return false;
                instance.m_numBonesPerVertex = castValue;
                return true;
            }
            case "m_boneIndexOffset":
            case "boneIndexOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_boneIndexOffset = castValue;
                return true;
            }
            case "m_boneWeightOffset":
            case "boneWeightOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_boneWeightOffset = castValue;
                return true;
            }
            case "m_numTextureChannels":
            case "numTextureChannels":
            {
                if (value is not byte castValue) return false;
                instance.m_numTextureChannels = castValue;
                return true;
            }
            case "m_tFloatCoordOffset":
            case "tFloatCoordOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_tFloatCoordOffset = castValue;
                return true;
            }
            case "m_tQuantizedCoordOffset":
            case "tQuantizedCoordOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_tQuantizedCoordOffset = castValue;
                return true;
            }
            case "m_colorOffset":
            case "colorOffset":
            {
                if (value is not byte castValue) return false;
                instance.m_colorOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
