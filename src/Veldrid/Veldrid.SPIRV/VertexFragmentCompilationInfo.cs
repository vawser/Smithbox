namespace Veldrid.SPIRV
{
    /// <summary>
    /// The output of a cross-compile operation of a vertex and fragment shader from SPIR-V to some target language.
    /// </summary>
    public class VertexFragmentCompilationResult
    {
        /// <summary>
        /// The translated vertex shader source code.
        /// </summary>
        public string VertexShader { get; }
        /// <summary>
        /// The translated fragment shader source code.
        /// </summary>
        public string FragmentShader { get; }
        /// <summary>
        /// Information about the resources used in the compiled shaders.
        /// </summary>
        public SpirvReflection Reflection { get; }

        internal VertexFragmentCompilationResult(
            string vertexCode,
            string fragmentCode)
            : this(vertexCode, fragmentCode, null)
        {
        }

        internal VertexFragmentCompilationResult(
            string vertexCode,
            string fragmentCode,
            SpirvReflection reflection)
        {
            VertexShader = vertexCode;
            FragmentShader = fragmentCode;
            Reflection = reflection;
        }
    }
}
