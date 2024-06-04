// Automatically Generated

namespace HKLib.hk2018;

public class hkbParametricMotionGenerator : hkbProceduralBlenderGenerator, hkbVerifiable
{
    public hkbParametricMotionGenerator.MotionSpaceType m_motionSpace;

    public List<hkbGenerator?> m_generators = new();

    public float m_xAxisParameterValue;

    public float m_yAxisParameterValue;


    public enum MotionSpaceType : int
    {
        MST_UNKNOWN = 0,
        MST_ANGULAR = 1,
        MST_DIRECTIONAL = 2
    }

    public class PrecomputeData : IHavokObject
    {
    }


}

