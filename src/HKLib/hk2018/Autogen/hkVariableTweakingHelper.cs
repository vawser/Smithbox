// Automatically Generated

namespace HKLib.hk2018;

public class hkVariableTweakingHelper : IHavokObject
{
    public List<hkVariableTweakingHelper.BoolVariableInfo> m_boolVariableInfo = new();

    public List<hkVariableTweakingHelper.IntVariableInfo> m_intVariableInfo = new();

    public List<hkVariableTweakingHelper.RealVariableInfo> m_realVariableInfo = new();

    public List<hkVariableTweakingHelper.Vector4VariableInfo> m_vector4VariableInfo = new();


    public class Vector4VariableInfo : IHavokObject
    {
        public string? m_name;

        public float m_x;

        public float m_y;

        public float m_z;

        public float m_w;

        public bool m_tweakOn;

    }


    public class RealVariableInfo : IHavokObject
    {
        public string? m_name;

        public float m_value;

        public bool m_tweakOn;

    }


    public class IntVariableInfo : IHavokObject
    {
        public string? m_name;

        public int m_value;

        public bool m_tweakOn;

    }


    public class BoolVariableInfo : IHavokObject
    {
        public string? m_name;

        public bool m_value;

        public bool m_tweakOn;

    }


}

