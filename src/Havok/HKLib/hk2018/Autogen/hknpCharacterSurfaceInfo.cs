// Automatically Generated

namespace HKLib.hk2018;

public class hknpCharacterSurfaceInfo : IHavokObject
{
    public bool m_isSurfaceDynamic;

    public hknpCharacterSurfaceInfo.SupportedState m_supportedState;

    public float m_surfaceDistanceExcess;

    public Vector4 m_surfaceNormal = new();

    public Vector4 m_surfaceVelocity = new();

    public Vector4 m_surfaceAngularVelocity = new();


    public enum SupportedState : int
    {
        UNSUPPORTED = 0,
        SLIDING = 1,
        SUPPORTED = 2
    }

}

