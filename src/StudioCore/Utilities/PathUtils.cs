using System.IO;
using System.Runtime.CompilerServices;

namespace StudioCore.Utilities
{
    /// <summary>
    /// Utilities for paths and backing up files.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// Backup a file on the specified path with a .bak extension, copying if specified, renaming otherwise.<br/>
        /// File is not overwritten if it already exists.
        /// </summary>
        /// <param name="path">The path to backup.</param>
        /// <param name="copy">Whether or not to copy instead of rename.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BackupFile(string path, bool copy = false)
            => MoveFileToBackup(path, ".bak", copy, false);

        /// <summary>
        /// Backup a file on the specified path with a .temp extension.<br/>
        /// File is copied to the temp path.<br/>
        /// File is overwritten if it already exists.
        /// </summary>
        /// <param name="path">The path to backup.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BackupTempFile(string path)
            => MoveFileToBackup(path, ".temp", true, true);

        /// <summary>
        /// Backup a file on the specified path with a .prev extension.<br/>
        /// File is copied to the prev path.<br/>
        /// File is overwritten if it already exists.
        /// </summary>
        /// <param name="path">The path to backup.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BackupPrevFile(string path)
            => MoveFileToBackup(path, ".prev", true, true);

        /// <summary>
        /// Backup a file on the specified path using the specified extension.<br/>
        /// Will be copied if specified, renamed otherwise.<br/>
        /// Will be overwritten if specified, ignored otherwise.
        /// </summary>
        /// <param name="path">The path to backup.</param>
        /// <param name="extension">The extension to append.</param>
        /// <param name="copy">Whether or not to copy instead of rename.</param>
        /// <param name="overwrite">Whether or not to overwrite the backup if it already exists.</param>
        public static void MoveFileToBackup(string path, string extension, bool copy = false, bool overwrite = false)
        {
            if (File.Exists(path))
            {
                string newPath = path + extension;
                if (!File.Exists(newPath) || overwrite)
                {
                    if (copy)
                    {
                        File.Copy(path, newPath, true);
                    }
                    else
                    {
                        File.Move(path, newPath, true);
                    }
                }
            }
        }
    }
}
