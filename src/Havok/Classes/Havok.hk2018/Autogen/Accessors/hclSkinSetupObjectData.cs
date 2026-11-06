// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSkinSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSkinSetupObject>
{
    public hclSkinSetupObjectData(Havok.hk2018.hclSkinSetupObject instance) : base(instance) { }

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
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                value = instance.m_transformSetSetup;
                return true;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                value = instance.m_referenceBufferSetup;
                return true;
            }
            case "m_outputBufferSetup":
            case "outputBufferSetup":
            {
                value = instance.m_outputBufferSetup;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
                return true;
            }
            case "m_skinNormals":
            case "skinNormals":
            {
                value = instance.m_skinNormals;
                return true;
            }
            case "m_skinTangents":
            case "skinTangents":
            {
                value = instance.m_skinTangents;
                return true;
            }
            case "m_skinBiTangents":
            case "skinBiTangents":
            {
                value = instance.m_skinBiTangents;
                return true;
            }
            case "m_useDualQuaternionMethod":
            case "useDualQuaternionMethod":
            {
                value = instance.m_useDualQuaternionMethod;
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
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                if (value is null)
                {
                    instance.m_transformSetSetup = default;
                    return true;
                }
                if (value is Havok.hk2018.hclTransformSetSetupObject castValue)
                {
                    instance.m_transformSetSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (value is null)
                {
                    instance.m_referenceBufferSetup = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_referenceBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputBufferSetup":
            case "outputBufferSetup":
            {
                if (value is null)
                {
                    instance.m_outputBufferSetup = default;
                    return true;
                }
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_outputBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_skinNormals":
            case "skinNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_skinNormals = castValue;
                return true;
            }
            case "m_skinTangents":
            case "skinTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_skinTangents = castValue;
                return true;
            }
            case "m_skinBiTangents":
            case "skinBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_skinBiTangents = castValue;
                return true;
            }
            case "m_useDualQuaternionMethod":
            case "useDualQuaternionMethod":
            {
                if (value is not bool castValue) return false;
                instance.m_useDualQuaternionMethod = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
