// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVertexCopySetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVertexCopySetupObject>
{
    public hclVertexCopySetupObjectData(Havok.hk2018.hclVertexCopySetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                value = instance.m_inputBufferSetupObject;
                return true;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                value = instance.m_outputBufferSetupObject;
                return true;
            }
            case "m_copyNormals":
            case "copyNormals":
            {
                value = instance.m_copyNormals;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_inputBufferSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferSetupObject":
            case "outputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_outputBufferSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_copyNormals":
            case "copyNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_copyNormals = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
