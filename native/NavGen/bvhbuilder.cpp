#include "bvh/bvh.hpp"
#include "bvh/vector.hpp"
#include "bvh/triangle.hpp"
#include "bvh/sweep_sah_builder.hpp"

#define DllExport extern "C"  __declspec( dllexport )

using Scalar = float;
using Vector3 = bvh::Vector3<Scalar>;
using Triangle = bvh::Triangle<Scalar>;
using Ray = bvh::Ray<Scalar>;
using Bvh = bvh::Bvh<Scalar>;

Bvh bbvh;

DllExport bool BuildBVHForMesh(float* verts, int vcount, unsigned short* indices, int icount)
{
	// Triangle array for processing
	std::vector<Triangle> triangles;
	for (int i = 0; i < icount / 3; i++)
	{
		int i1 = indices[i * 3];
		int i2 = indices[i * 3 + 1];
		int i3 = indices[i * 3 + 2];
		float v1x = verts[i1 * 3];
		float v1y = verts[i1 * 3 + 1];
		float v1z = verts[i1 * 3 + 2];
		float v2x = verts[i2 * 3];
		float v2y = verts[i2 * 3 + 1];
		float v2z = verts[i2 * 3 + 2];
		float v3x = verts[i3 * 3];
		float v3y = verts[i3 * 3 + 1];
		float v3z = verts[i3 * 3 + 2];
		triangles.emplace_back(
			Vector3(v1x, v1y, v1z),
			Vector3(v2x, v2y, v2z),
			Vector3(v3x, v3y, v3z)
		);
	}

	// Build bvh
	bvh::SweepSahBuilder<Bvh> builder(bbvh);
	builder.max_leaf_size = 1;
	auto [bboxes, centers] = bvh::compute_bounding_boxes_and_centers(triangles.data(), triangles.size());
	auto globalBBox = bvh::compute_bounding_boxes_union(bboxes.get(), triangles.size());
	builder.build(globalBBox, bboxes.get(), centers.get(), triangles.size());

	for (int i = 0; i < bbvh.node_count; i++)
	{
		if (bbvh.nodes[i].is_leaf)
		{
			bbvh.nodes[i].first_child_or_primitive = bbvh.primitive_indices[bbvh.nodes[i].first_child_or_primitive];
		}
	}

	return true;
}

DllExport size_t GetNodeSize()
{
	return sizeof(Bvh::Node);
}

DllExport size_t GetBVHSize()
{
	return bbvh.node_count;
}

DllExport void GetBVHNodes(void* buffer)
{
	memcpy(buffer, bbvh.nodes.get(), bbvh.node_count * sizeof(Bvh::Node));
}