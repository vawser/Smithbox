// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnnotationLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnnotationLibrary>
{
    public hkaiTraversalAnnotationLibraryData(Havok.hk2014.hkaiTraversalAnnotationLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_intervalPartitionLibrary":
            case "intervalPartitionLibrary":
            {
                value = instance.m_intervalPartitionLibrary;
                return true;
            }
            case "m_userEdgePairs":
            case "userEdgePairs":
            {
                value = instance.m_userEdgePairs;
                return true;
            }
            case "m_annotations":
            case "annotations":
            {
                value = instance.m_annotations;
                return true;
            }
            case "m_vectorLibrary":
            case "vectorLibrary":
            {
                value = instance.m_vectorLibrary;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_intervalPartitionLibrary":
            case "intervalPartitionLibrary":
            {
                if (value is not Havok.hk2014.hkaiIntervalPartitionLibrary castValue) return false;
                instance.m_intervalPartitionLibrary = castValue;
                return true;
            }
            case "m_userEdgePairs":
            case "userEdgePairs":
            {
                if (value is not List<Havok.hk2014.hkaiUserEdgeUtils.UserEdgePair> castValue) return false;
                instance.m_userEdgePairs = castValue;
                return true;
            }
            case "m_annotations":
            case "annotations":
            {
                if (value is not List<Havok.hk2014.hkaiTraversalAnnotationLibraryAnnotation> castValue) return false;
                instance.m_annotations = castValue;
                return true;
            }
            case "m_vectorLibrary":
            case "vectorLibrary":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vectorLibrary = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
