// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclUpdateVertexFramesSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclUpdateVertexFramesSetupObject>
{
    public hclUpdateVertexFramesSetupObjectData(Havok.hk2014.hclUpdateVertexFramesSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_buffer":
            case "buffer":
            {
                value = instance.m_buffer;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
                return true;
            }
            case "m_updateNormals":
            case "updateNormals":
            {
                value = instance.m_updateNormals;
                return true;
            }
            case "m_updateTangents":
            case "updateTangents":
            {
                value = instance.m_updateTangents;
                return true;
            }
            case "m_updateBiTangents":
            case "updateBiTangents":
            {
                value = instance.m_updateBiTangents;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_buffer":
            case "buffer":
            {
                if (value is null)
                {
                    instance.m_buffer = default;
                    return true;
                }
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
                {
                    instance.m_buffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_updateNormals":
            case "updateNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_updateNormals = castValue;
                return true;
            }
            case "m_updateTangents":
            case "updateTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_updateTangents = castValue;
                return true;
            }
            case "m_updateBiTangents":
            case "updateBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_updateBiTangents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
