using System.Numerics;
using Havok.Shared;

namespace Havok.hk2019;

// A compressed mesh's extern shapes: convex shapes (boxes, cylinders, hulls) placed with the mesh, which its own
// primitives don't hold (some AC6 assets). Their hulls are read by the convex shape's own view.
public partial class hknpCompressedMeshShape : IHavokExternShapes
{
    // Empty free list slots are unused; a zero scale means none
    IEnumerable<(object Shape, Matrix4x4 Transform)> IHavokExternShapes.ExternShapes =>
        m_externShapes.Where(e => e?.m_shape != null && e.m_isEmpty == 0).Select(e =>
        {
            var scale = new Vector3(e.m_scale.X, e.m_scale.Y, e.m_scale.Z);
            if (scale == Vector3.Zero) scale = Vector3.One;
            return ((object)e.m_shape!, Matrix4x4.CreateScale(scale) * HavokViewMath.Clean(e.m_transform));
        });
}
