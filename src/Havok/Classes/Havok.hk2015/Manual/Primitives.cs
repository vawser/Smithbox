using System.Numerics;
using Havok.Shared;

namespace Havok.hk2015;

// The hkp physics bodies and primitive shapes of objects (DS1R), as Havok.Shared views, so CollisionGeometry can make
// triangles of them. Hand-written: the classes are generated as partial for them. The same views are in hk55, hk2010
// and hk2014.

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
        HavokViewMath.FourTransposedPoints(m_rotatedVertices.Select(m => (Matrix4x4)m), m_numVertices);
}

public partial class hkpConvexTranslateShape : IHavokCompoundShape
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokCompoundShape.Instances =>
        m_childShape.m_childShape == null ? Array.Empty<(object, Matrix4x4)>()
            : new[] { ((object)m_childShape.m_childShape, Matrix4x4.CreateTranslation(m_translation.X, m_translation.Y, m_translation.Z)) };
}

public partial class hkpConvexTransformShape : IHavokCompoundShape
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokCompoundShape.Instances =>
        m_childShape.m_childShape == null ? Array.Empty<(object, Matrix4x4)>()
            : new[] { ((object)m_childShape.m_childShape, HavokViewMath.QsTransform(m_transform.m_translation, m_transform.m_rotation, m_transform.m_scale)) };
}

// The MOPP tree wraps the shape it speeds up
public partial class hkpMoppBvTreeShape : IHavokCompoundShape
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokCompoundShape.Instances =>
        m_child.m_childShape == null ? Array.Empty<(object, Matrix4x4)>() : new[] { ((object)m_child.m_childShape, Matrix4x4.Identity) };
}
