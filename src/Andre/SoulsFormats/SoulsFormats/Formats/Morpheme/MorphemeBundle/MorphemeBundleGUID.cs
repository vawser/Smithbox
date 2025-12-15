namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// The MorphemeBundle header. Many times this is simply 0ed out. Contains a standard GUID.
    /// </summary>
    public struct MorphemeBundleGUID
    {
        /// <summary>
        /// GUID segment 0
        /// </summary>
        public int GUID_00;
        /// <summary>
        /// GUID segment 1
        /// </summary>
        public int GUID_04;
        /// <summary>
        /// GUID segment 2
        /// </summary>
        public int GUID_08;
        /// <summary>
        /// GUID segment 3
        /// </summary>
        public int GUID_0C;
    }
}
