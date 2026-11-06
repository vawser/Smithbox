// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnnotationLibraryAnnotationData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnnotationLibraryAnnotation>
{
    public hkaiTraversalAnnotationLibraryAnnotationData(Havok.hk2014.hkaiTraversalAnnotationLibraryAnnotation instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_tEquation":
            case "tEquation":
            {
                value = instance.m_tEquation;
                return true;
            }
            case "m_userdata":
            case "userdata":
            {
                value = instance.m_userdata;
                return true;
            }
            case "m_firstPartitionIndex":
            case "firstPartitionIndex":
            {
                value = instance.m_firstPartitionIndex;
                return true;
            }
            case "m_firstVectorIndex":
            case "firstVectorIndex":
            {
                value = instance.m_firstVectorIndex;
                return true;
            }
            case "m_numPartitions":
            case "numPartitions":
            {
                value = instance.m_numPartitions;
                return true;
            }
            case "m_numVectors":
            case "numVectors":
            {
                value = instance.m_numVectors;
                return true;
            }
            case "m_traversalType":
            case "traversalType":
            {
                value = instance.m_traversalType;
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
            case "m_tEquation":
            case "tEquation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_tEquation = castValue;
                return true;
            }
            case "m_userdata":
            case "userdata":
            {
                if (value is not uint castValue) return false;
                instance.m_userdata = castValue;
                return true;
            }
            case "m_firstPartitionIndex":
            case "firstPartitionIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstPartitionIndex = castValue;
                return true;
            }
            case "m_firstVectorIndex":
            case "firstVectorIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstVectorIndex = castValue;
                return true;
            }
            case "m_numPartitions":
            case "numPartitions":
            {
                if (value is not byte castValue) return false;
                instance.m_numPartitions = castValue;
                return true;
            }
            case "m_numVectors":
            case "numVectors":
            {
                if (value is not byte castValue) return false;
                instance.m_numVectors = castValue;
                return true;
            }
            case "m_traversalType":
            case "traversalType":
            {
                if (value is not byte castValue) return false;
                instance.m_traversalType = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
