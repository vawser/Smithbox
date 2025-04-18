namespace Veldrid.SPIRV
{
    /// <summary>
    /// The output of a source to SPIR-V compilation operation.
    /// </summary>
    public class SpirvCompilationResult
    {
        /// <summary>
        /// The compiled SPIR-V bytecode.
        /// </summary>
        public byte[] SpirvBytes { get; }

        /// <summary>
        /// Constructs a new <see cref="SpirvCompilationResult"/>.
        /// </summary>
        /// <param name="spirvBytes">The compiled SPIR-V bytecode.</param>
        public SpirvCompilationResult(byte[] spirvBytes)
        {
            SpirvBytes = spirvBytes;
        }
    }
}
