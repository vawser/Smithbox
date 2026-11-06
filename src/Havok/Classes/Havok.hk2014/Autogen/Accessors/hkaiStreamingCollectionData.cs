// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiStreamingCollectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiStreamingCollection>
{
    public hkaiStreamingCollectionData(Havok.hk2014.hkaiStreamingCollection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isTemporary":
            case "isTemporary":
            {
                value = instance.m_isTemporary;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                value = instance.m_tree;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                value = instance.m_instances;
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
            case "m_isTemporary":
            case "isTemporary":
            {
                if (value is not bool castValue) return false;
                instance.m_isTemporary = castValue;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                if (value is null)
                {
                    instance.m_tree = default;
                    return true;
                }
                if (value is Havok.hk2014.hkcdDynamicAabbTree castValue)
                {
                    instance.m_tree = castValue;
                    return true;
                }
                return false;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<Havok.hk2014.hkaiStreamingCollectionInstanceInfo> castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
