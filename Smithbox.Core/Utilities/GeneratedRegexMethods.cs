using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Utilities
{
    /// <summary>
    /// A collection of generated regex methods for faster regex.
    /// </summary>
    public static partial class GeneratedRegexMethods
    {
        /// <summary>
        /// Gets a regex for general dark souls map IDs.
        /// </summary>
        /// <returns>A <see cref="Regex"/>.</returns>
        [GeneratedRegex(@"^m\d{2}_\d{2}_\d{2}_\d{2}$")]
        public static partial Regex DSMapRegex();

        /// <summary>
        /// Gets a regex for general 4thgen Armored Core map IDs.
        /// </summary>
        /// <returns>A <see cref="Regex"/>.</returns>
        [GeneratedRegex(@"^m\d{3}")]
        public static partial Regex AC4MapRegex();

        /// <summary>
        /// Gets a regex for general 5thgen Armored Core map IDs.
        /// </summary>
        /// <returns>A <see cref="Regex"/>.</returns>
        [GeneratedRegex(@"^m\d{4}")]
        public static partial Regex AC5MapRegex();

        public static bool IsMapId(string value)
            => DSMapRegex().IsMatch(value) ||
            AC4MapRegex().IsMatch(value) ||
            AC5MapRegex().IsMatch(value);
    }
}
