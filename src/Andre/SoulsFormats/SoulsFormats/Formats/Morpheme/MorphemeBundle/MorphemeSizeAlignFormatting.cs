namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Structure for Morpheme data and alignment info.
    /// </summary>
    public struct MorphemeSizeAlignFormatting
    {
        /// <summary>
        /// Size of this bundle structure
        /// </summary>
        public long dataSize { get; set; }

        /// <summary>
        /// Alignment for this bundle structure. 
        /// </summary>
        public int dataAlignment { get; set; }

        /// <summary>
        /// Unknown, possibly a second alignment for large file chunks?
        /// </summary>
        public int unkValue { get; set; }

    }
}
