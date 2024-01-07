using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A BND or BXF container of generic files.
    /// </summary>
    public interface IBinder : IDisposable
    {
        /// <summary>
        /// Flags indicating features of the binder.
        /// </summary>
        Binder.Format Format { get; set; }

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Files in this binder.
        /// </summary>
        List<BinderFile> Files { get; set; }
    }
}
