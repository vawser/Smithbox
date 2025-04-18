// Automatically Generated

namespace HKLib.hk2018.hkgpCgo;

public class Config : hkReferencedObject
{
    public hkgpCgo.Config.VertexSemantic m_semantic;

    public hkgpCgo.Config.VertexCombinator m_combinator;

    public float m_maxDistance;

    public float m_maxShrink;

    public float m_maxAngle;

    public float m_minEdgeRatio;

    public float m_maxAngleDrift;

    public float m_weldDistance;

    public float m_updateThreshold;

    public float m_degenerateTolerance;

    public float m_flatAngle;

    public int m_maxVertices;

    public bool m_inverseOrientation;

    public bool m_proportionalShrinking;

    public bool m_multiPass;

    public bool m_protectNakedBoundaries;

    public bool m_protectMaterialBoundaries;

    public bool m_decimateComponents;

    public hkgpCgo.Config.SolverAccuracy m_solverAccuracy;

    public float m_minDistance;

    public float m_minConvergence;

    public bool m_project;

    public bool m_buildClusters;

    public bool m_useAccumulatedError;

    public bool m_useLegacySolver;


    public enum SolverAccuracy : int
    {
        SA_FAST = 4,
        SA_NORMAL = 8,
        SA_ACCURATE = 16,
        SA_HIGH = 64
    }

    public enum VertexCombinator : int
    {
        VC_MIN = 0,
        VC_MAX = 1,
        VC_MEAN = 2
    }

    public enum VertexSemantic : int
    {
        VS_NONE = 0,
        VS_WEIGHT = 1,
        VS_DISTANCE = 2,
        VS_FACTOR = 3,
        VS_OFFSET = 4
    }

}

