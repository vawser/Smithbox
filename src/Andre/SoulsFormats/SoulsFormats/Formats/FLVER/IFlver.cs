using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A common interface for the basic features of FLVER0 and FLVER2.
    /// </summary>
    public interface IFlver
    {
        /// <summary>
        /// Joints available for vertices and dummy points to be attached to.
        /// </summary>
        IReadOnlyList<FLVER.Bone> Bones { get; }

        /// <summary>
        /// Dummy points used to determine hitboxes, particle effects, etc.
        /// </summary>
        IReadOnlyList<FLVER.Dummy> Dummies { get; }

        /// <summary>
        /// Materials that determine rendering of meshes.
        /// </summary>
        IReadOnlyList<IFlverMaterial> Materials { get; }

        /// <summary>
        /// Actual geometry of the model.
        /// </summary>
        IReadOnlyList<IFlverMesh> Meshes { get; }
    }

    /// <summary>
    /// Determines rendering properties of a mesh.
    /// </summary>
    public interface IFlverMaterial
    {
        /// <summary>
        /// Name of the material, mostly non-functional but may include special flags.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Name of an MTD file which configures the shader to use.
        /// </summary>
        string MTD { get; }

        /// <summary>
        /// Various texture maps applied to the mesh.
        /// </summary>
        IReadOnlyList<IFlverTexture> Textures { get; }
    }

    /// <summary>
    /// A single texture map used by a material.
    /// </summary>
    public interface IFlverTexture
    {
        /// <summary>
        /// Indicates the type of texture map this is.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Network path to the texture file; only the filename without extension is actually used.
        /// </summary>
        string Path { get; }
    }

    /// <summary>
    /// A segment of model geometry.
    /// </summary>
    public interface IFlverMesh
    {
        /// <summary>
        /// Indicates whether the mesh is already in bind pose or not, among other things (probably).
        /// </summary>
        byte Dynamic { get; }

        /// <summary>
        /// Index in the flver's material list to apply to this mesh.
        /// </summary>
        int MaterialIndex { get; }

        /// <summary>
        /// Points making up the mesh's shape.
        /// </summary>
        IReadOnlyList<FLVER.Vertex> Vertices { get; }
    }
}
