// Automatically Generated

namespace HKLib.hk2018;

public class hknp1dAngularFollowCamCinfo : IHavokObject
{
    public float m_yawCorrection;

    public float m_yawSignCorrection;

    public Vector4 m_upDirWS = new();

    public Vector4 m_rigidBodyForwardDir = new();

    public List<hknp1dAngularFollowCamCinfo.CameraSet> m_set = new();


    public class CameraSet : IHavokObject
    {
        public Vector4 m_positionUS = new();

        public Vector4 m_lookAtUS = new();

        public float m_fov;

        public float m_velocity;

        public float m_speedInfluenceOnCameraDirection;

        public float m_angularRelaxation;

    }


}

