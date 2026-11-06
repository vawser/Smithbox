// Automatically Generated

namespace Havok.hk2014;

public class hkaiReferenceFrameAndExtrusion : IHavokObject
{
    public Vector4 m_up = new();

    public float m_cellExtrusion;

    public float m_silhouetteRadiusExpasion;

    public hkaiReferenceFrameAndExtrusion.UpVectorTransformMethod m_upTransformMethod;


    public enum UpVectorTransformMethod : int
    {
        USE_GLOBAL_UP = 0,
        USE_INSTANCE_TRANSFORM = 1,
        USE_FACE_NORMAL = 2
    }

}
