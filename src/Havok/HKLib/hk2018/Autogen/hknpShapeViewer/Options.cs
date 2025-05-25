// Automatically Generated

namespace HKLib.hk2018.hknpShapeViewer;

public class Options : hkVdb.Tweakable
{
    public hknpShapeViewer.Options.LevelOfDetail m_levelOfDetail = new();

    public hknpShapeViewer.Options.ConvexRadiusDisplayMode m_convexRadiusDisplayMode = new();

    public bool m_drawEdges;

    public bool m_usePreIntegrationTransform;


    public class ConvexRadiusDisplayMode : IHavokObject
    {
        public bool m_rounded;

        public bool m_planar;

        public bool m_none;

    }


    public class LevelOfDetail : IHavokObject
    {
        public bool m_maximum;

        public bool m_high;

        public bool m_default;

        public bool m_simplified;

        public bool m_approximate;

    }


}

