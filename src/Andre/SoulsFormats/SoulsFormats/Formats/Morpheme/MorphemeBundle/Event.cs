namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// A structure containing times for EventTrack structures.
    /// </summary>
    public struct Event
    {
        /// <summary>
        /// The starting time for an event.
        /// </summary>
        public float start;

        /// <summary>
        /// The amount of time the event lasts.
        /// </summary>
        public float duration;

        /// <summary>
        /// The data stored in the event.
        /// </summary>
        public int value;
    }
}
