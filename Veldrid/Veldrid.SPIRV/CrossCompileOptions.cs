using System;

namespace Veldrid.SPIRV
{
    /// <summary>
    /// An object used to control the parameters of shader translation from SPIR-V to some target language.
    /// </summary>
    public class CrossCompileOptions
    {
        /// <summary>
        /// Indicates whether or not the compiled shader output should include a clip-space Z-range fixup at the end of the
        /// vertex shader.
        /// If true, then the shader will include code that assumes the clip space needs to be corrected from the
        /// "wrong" range into the "right" range for the particular type of shader. For example, if an OpenGL shader is being
        /// generated, then the vertex shader will include a fixup that converts the depth range from [0, 1] to [-1, 1].
        /// If a Direct3D shader is being generated, then the vertex shader will include a fixup that converts the depth range
        /// from [-1, 1] to [0, 1].
        /// </summary>
        public bool FixClipSpaceZ { get; set; }
        /// <summary>
        /// Indicates whether or not the compiled shader output should include a fixup at the end of the vertex shader which
        /// inverts the clip-space Y value.
        /// </summary>
        public bool InvertVertexOutputY { get; set; }
        /// <summary>
        /// An array of <see cref="SpecializationConstant"/> which will be substituted into the shader as new constants. Each
        /// element in the array will be matched by ID with the SPIR-V specialization constants defined in the shader.
        /// </summary>
        public SpecializationConstant[] Specializations { get; set; }

        /// <summary>
        /// Constructs a new <see cref="CrossCompileOptions"/> with default values.
        /// </summary>
        public CrossCompileOptions()
        {
            Specializations = Array.Empty<SpecializationConstant>();
        }

        /// <summary>
        /// Constructs a new <see cref="CrossCompileOptions"/>, used to control the parameters of shader translation.
        /// </summary>
        /// <param name="fixClipSpaceZ">Indicates whether or not the compiled shader output should include a clip-space Z-range
        /// fixup at the end of the vertex shader.</param>
        /// <param name="invertVertexOutputY">Indicates whether or not the compiled shader output should include a fixup at the
        /// end of the vertex shader which inverts the clip-space Y value.</param>
        public CrossCompileOptions(bool fixClipSpaceZ, bool invertVertexOutputY)
            : this(fixClipSpaceZ, invertVertexOutputY, Array.Empty<SpecializationConstant>())
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CrossCompileOptions"/>, used to control the parameters of shader translation.
        /// </summary>
        /// <param name="fixClipSpaceZ">Indicates whether or not the compiled shader output should include a clip-space Z-range
        /// fixup at the end of the vertex shader.</param>
        /// <param name="invertVertexOutputY">Indicates whether or not the compiled shader output should include a fixup at the
        /// end of the vertex shader which inverts the clip-space Y value.</param>
        /// <param name="specializations">An array of <see cref="SpecializationConstant"/> which will be substituted into the
        /// shader as new constants.</param>
        public CrossCompileOptions(bool fixClipSpaceZ, bool invertVertexOutputY, params SpecializationConstant[] specializations)
        {
            FixClipSpaceZ = fixClipSpaceZ;
            InvertVertexOutputY = invertVertexOutputY;
            Specializations = specializations;
        }
    }
}
