// Automatically Generated

namespace HKLib.hk2019.hknpShapeViewer;

public class Options : hkVdb.Tweakable
{
    public hknpShapeViewer.Options.LevelOfDetail m_levelOfDetail = new();

    public hknpShapeViewer.Options.ConvexRadiusDisplayMode m_convexRadiusDisplayMode = new();

    public bool m_drawEdges;

    public bool m_drawInteriorTriangles;

    public bool m_usePreIntegrationTransform;


    public class ConvexRadiusDisplayMode : IHavokObject
    {
        public bool m_rounded;

        public bool m_planar;

        public bool m_none;

    }

    public class LevelOfDetail : IHavokObject
    {
        public bool m_high;

        public bool m_medium;

        public bool m_low;

        public bool m_user0;

        public bool m_user1;

        public bool m_user2;

        public bool m_user3;

        public bool m_user4;

    }

}
