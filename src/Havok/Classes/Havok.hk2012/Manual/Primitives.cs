using System.Numerics;
using Havok.Shared;

namespace Havok.hk2012;

// The hkp physics bodies and primitive shapes of objects (DS2), as Havok.Shared views, so CollisionGeometry can make
// triangles of them. Hand-written: the classes are generated as partial for them. The same views are in hk55, hk2010,
// hk2014 and hk2015. DS2's files use no hkpConvexTransformShape, so it isn't in this class set.

public partial class hkpRigidBody : IHavokBody
{
    object? IHavokBody.Shape => m_collidable.m_shape;

    Vector3 IHavokBody.Position => HavokViewMath.Clean(m_motion.m_motionState.m_transform).Translation;

    Quaternion IHavokBody.Orientation => HavokViewMath.Rotation(m_motion.m_motionState.m_transform);
}

// The convex radius rounds the box's edges; its extents are taken out to it
public partial class hkpBoxShape : IHavokBox
{
    Vector3 IHavokBox.HalfExtents => new Vector3(m_halfExtents.X, m_halfExtents.Y, m_halfExtents.Z) + new Vector3(m_radius);
}

public partial class hkpCylinderShape : IHavokCylinder
{
    Vector3 IHavokCylinder.A => new(m_vertexA.X, m_vertexA.Y, m_vertexA.Z);

    Vector3 IHavokCylinder.B => new(m_vertexB.X, m_vertexB.Y, m_vertexB.Z);

    float IHavokCylinder.Radius => m_cylRadius + m_radius;
}

public partial class hkpCapsuleShape : IHavokCapsule
{
    Vector3 IHavokCapsule.A => new(m_vertexA.X, m_vertexA.Y, m_vertexA.Z);

    Vector3 IHavokCapsule.B => new(m_vertexB.X, m_vertexB.Y, m_vertexB.Z);

    float IHavokCapsule.Radius => m_radius;
}

public partial class hkpSphereShape : IHavokSphere
{
    float IHavokSphere.Radius => m_radius;
}

public partial class hkpConvexVerticesShape : IHavokConvexPoints
{
    IReadOnlyList<Vector3> IHavokConvexPoints.Points =>
        HavokViewMath.FourTransposedPoints(m_rotatedVertices.Select(Rows), m_numVertices);

    // hk_2012 keeps each block of four points as hkFourTransposedPoints: x, y and z of the four, one vector each
    private static Matrix4x4 Rows(hkFourTransposedPoints p) => new(
        p.m_vertices[0].X, p.m_vertices[0].Y, p.m_vertices[0].Z, p.m_vertices[0].W,
        p.m_vertices[1].X, p.m_vertices[1].Y, p.m_vertices[1].Z, p.m_vertices[1].W,
        p.m_vertices[2].X, p.m_vertices[2].Y, p.m_vertices[2].Z, p.m_vertices[2].W,
        0, 0, 0, 1);
}

public partial class hkpConvexTranslateShape : IHavokCompoundShape
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokCompoundShape.Instances =>
        m_childShape.m_childShape == null ? Array.Empty<(object, Matrix4x4)>()
            : new[] { ((object)m_childShape.m_childShape, Matrix4x4.CreateTranslation(m_translation.X, m_translation.Y, m_translation.Z)) };
}

public partial class hkpListShape : IHavokCompoundShape
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokCompoundShape.Instances =>
        m_childInfo.Where(c => c.m_shape != null).Select(c => ((object)c.m_shape!, Matrix4x4.Identity));
}

// The MOPP tree wraps the shape it speeds up
public partial class hkpMoppBvTreeShape : IHavokCompoundShape
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokCompoundShape.Instances =>
        m_child.m_childShape == null ? Array.Empty<(object, Matrix4x4)>() : new[] { ((object)m_child.m_childShape, Matrix4x4.Identity) };
}
