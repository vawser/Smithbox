// Automatically Generated

namespace HKLib.hk2018;

public class hknpLinearSurfaceVelocity : hknpSurfaceVelocity
{
    public hknpSurfaceVelocity.Space m_space;

    public hknpLinearSurfaceVelocity.ProjectMethod m_projectMethod;

    public float m_maxVelocityScale;

    public Vector4 m_velocityMeasurePlane = new();

    public Vector4 m_velocity = new();


    public enum ProjectMethod : int
    {
        VELOCITY_PROJECT = 0,
        VELOCITY_RESCALE = 1
    }

}

