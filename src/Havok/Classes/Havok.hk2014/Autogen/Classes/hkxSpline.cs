// Automatically Generated

namespace Havok.hk2014;

public class hkxSpline : hkReferencedObject
{
    public List<hkxSpline.ControlPoint> m_controlPoints = new();

    public bool m_isClosed;


    public class ControlPoint : IHavokObject
    {
        public Vector4 m_position = new();

        public Vector4 m_tangentIn = new();

        public Vector4 m_tangentOut = new();

        public hkxSpline.ControlType m_inType;

        public hkxSpline.ControlType m_outType;

    }

    public enum ControlType : int
    {
        BEZIER_SMOOTH = 0,
        BEZIER_CORNER = 1,
        LINEAR = 2,
        CUSTOM = 3
    }

}
