// Automatically Generated

namespace HKLib.hk2018;

public class hkbRadialSelectorGenerator : hkbGenerator, hkbVerifiable
{
    public List<hkbRadialSelectorGenerator.GeneratorPair> m_generatorPairs = new();

    public float m_angle;

    public float m_radius;


    public class GeneratorPair : IHavokObject
    {
        public readonly hkbRadialSelectorGenerator.GeneratorInfo[] m_generators = new hkbRadialSelectorGenerator.GeneratorInfo[2];

        public float m_minAngle;

        public float m_maxAngle;

    }


    public class GeneratorInfo : IHavokObject
    {
        public hkbGenerator? m_generator;

        public float m_angle;

        public float m_radialSpeed;

    }


}

