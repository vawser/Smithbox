// Automatically Generated

namespace Havok.hk2014;

public class hkpShapeCollection : hkpShape
{
    public bool m_disableWelding;

    public hkpShapeCollection.CollectionType m_collectionType;


    public enum CollectionType : int
    {
        COLLECTION_LIST = 0,
        COLLECTION_EXTENDED_MESH = 1,
        COLLECTION_TRISAMPLED_HEIGHTFIELD = 2,
        COLLECTION_USER = 3,
        COLLECTION_SIMPLE_MESH = 4,
        COLLECTION_MESH_SHAPE = 5,
        COLLECTION_COMPRESSED_MESH = 6,
        COLLECTION_MAX = 7
    }

}
