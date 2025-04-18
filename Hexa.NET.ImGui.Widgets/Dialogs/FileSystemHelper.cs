namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui.Widgets.Extensions;
    using Hexa.NET.Utilities.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.Json.Serialization;

    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class SourceGenerationContextDictionary : JsonSerializerContext
    {
    }

    [Flags]
    public enum CommonFilePermissions
    {
        None = 0,
        OwnerRead = 1 << 0,
        OwnerWrite = 1 << 1,
        OwnerExecute = 1 << 2,
        GroupRead = 1 << 3,
        GroupWrite = 1 << 4,
        GroupExecute = 1 << 5,
        OtherRead = 1 << 6,
        OtherWrite = 1 << 7,
        OtherExecute = 1 << 8,
        OwnerFullControl = OwnerRead | OwnerWrite | OwnerExecute,
        GroupFullControl = GroupRead | GroupWrite | GroupExecute,
        OtherFullControl = OtherRead | OtherWrite | OtherExecute
    }

    public enum FileSystemItemFlags : byte
    {
        None = 0,
        Folder = 1,
        Hidden = 2,
    }

    public enum RefreshFlags
    {
        None = 0,
        Folders = 1,
        Files = 2,
        OnlyAllowFilteredExtensions = 4,
        Hidden = 8,
        SystemFiles = 16,
    }

    public class FileSystemHelper
    {
        private static FileSystemItem[] specialDirs = null!;
        private static FileSystemItem[] logicalDrives = null!;

        static FileSystemHelper()
        {
            ClearCache();
            IsCaseSensitive = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static bool IsCaseSensitive { get; }

        public static FileSystemItem[] SpecialDirs => specialDirs;

        public static FileSystemItem[] LogicalDrives => logicalDrives;

        public static string GetDownloadsFolderPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else
            {
                return string.Empty;
            }
        }

        private static readonly List<string> ignoredDrives = ["sys", "proc", "dev", "run", "snap", "tmp", "boot", "System"];

        public static void ClearCache()
        {
            List<FileSystemItem> drives = new();
            foreach (var drive in DriveInfo.GetDrives())
            {
                // shouldn't be a performance concern, but if it ever gets to the point use a trie.
                if (drive.Name.StartsWith('/') && ignoredDrives.Any(x => drive.Name.AsSpan(1).StartsWith(x.AsSpan())))
                {
                    continue;
                }

                try
                {
                    if (drive.IsReady && drive.RootDirectory != null)
                    {
                        string driveIcon = string.Empty;
                        switch (drive.DriveType)
                        {
                            case DriveType.NoRootDirectory:
                                continue;

                            case DriveType.Removable:
                                driveIcon = $"{MaterialIcons.HardDrive}";
                                break;

                            case DriveType.Fixed:
                                driveIcon = $"{MaterialIcons.HardDrive}";
                                break;

                            case DriveType.Network:
                                driveIcon = $"{MaterialIcons.SmbShare}";
                                break;

                            case DriveType.CDRom:
                                driveIcon = $"{MaterialIcons.Album}";
                                break;

                            case DriveType.Ram:
                                driveIcon = $"{MaterialIcons.Database}";
                                break;

                            default:
                            case DriveType.Unknown:
                                driveIcon = $"{MaterialIcons.DeviceUnknown}";
                                break;
                        }

                        string name = drive.VolumeLabel;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = "Local Disk";
                        }

                        if (name != drive.Name)
                        {
                            name += $" ({drive.Name})";
                        }

                        drives.Add(new FileSystemItem(drive.RootDirectory.FullName, driveIcon, name, FileSystemItemFlags.Folder));
                    }
                }
                catch (Exception)
                {
                }
            }

            logicalDrives = [.. drives];

            List<FileSystemItem> items = [];
            AddSpecialDir(items, Environment.SpecialFolder.Desktop, $"{MaterialIcons.DesktopWindows}");
            AddSpecialDir(items, GetDownloadsFolderPath, $"{MaterialIcons.Download}");
            AddSpecialDir(items, Environment.SpecialFolder.MyDocuments, $"{MaterialIcons.Description}");
            AddSpecialDir(items, Environment.SpecialFolder.MyMusic, $"{MaterialIcons.LibraryMusic}");
            AddSpecialDir(items, Environment.SpecialFolder.MyPictures, $"{MaterialIcons.Image}");
            AddSpecialDir(items, Environment.SpecialFolder.MyVideos, $"{MaterialIcons.VideoLibrary}");
            specialDirs = [.. items];

            cache.Clear();
        }

        private static void AddSpecialDir(List<FileSystemItem> items, Environment.SpecialFolder folder, string icon)
        {
            try
            {
                var path = Environment.GetFolderPath(folder);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    items.Add(new(path, icon, FileSystemItemFlags.Folder));
                }
            }
            catch (Exception)
            {
            }
        }

        private static void AddSpecialDir(List<FileSystemItem> items, Func<string> getPath, string icon)
        {
            try
            {
                var path = getPath();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    items.Add(new(path, icon, FileSystemItemFlags.Folder));
                }
            }
            catch (Exception)
            {
            }
        }

        public static IEnumerable<FileSystemItem> Refresh(string folder, RefreshFlags refreshFlags, List<string>? allowedExtensions, SearchOptions searchOptions, Func<FileMetadata, string> fileDecorator, char? folderDecorator)
        {
            StringComparison comparison = IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            bool ignoreHidden = (!searchOptions.Enabled && (refreshFlags & RefreshFlags.Hidden) == 0) || (searchOptions.Enabled && (searchOptions.Flags & SearchOptionsFlags.Hidden) == 0);
            bool onlyAllowFilteredExtensions = (refreshFlags & RefreshFlags.OnlyAllowFilteredExtensions) != 0;

            SearchOption option = searchOptions.Enabled && (searchOptions.Flags & SearchOptionsFlags.Subfolders) != 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            if (onlyAllowFilteredExtensions && allowedExtensions is null)
            {
                throw new ArgumentNullException(nameof(allowedExtensions));
            }

            int id = 0;
            bool emitFolders = (refreshFlags & RefreshFlags.Folders) != 0;
            bool emitFiles = (refreshFlags & RefreshFlags.Files) != 0;
            foreach (var metadata in FileUtils.EnumerateEntries(folder, searchOptions.Pattern, option))
            {
                var flags = metadata.Attributes;
                var isDir = (flags & FileAttributes.Directory) != 0;
                if (!isDir && !emitFiles)
                    continue;
                if (isDir && !emitFolders)
                    continue;
                if ((flags & FileAttributes.System) != 0)
                    continue;
                if ((flags & FileAttributes.Hidden) != 0 && ignoreHidden)
                    continue;
                if ((flags & FileAttributes.Device) != 0)
                    continue;

                var span = metadata.Path.AsSpan();
                var name = FileUtils.GetFileName(span);

                if (searchOptions.Filter(metadata))
                {
                    if (onlyAllowFilteredExtensions && !isDir)
                    {
                        var ext = FileUtils.GetExtension(name);
                        if (!allowedExtensions!.Contains(ext, comparison))
                        {
                            continue;
                        }
                    }
#if NET5_0_OR_GREATER
                    var itemName = option == SearchOption.AllDirectories ? $"{name}##{id++}" : name.ToString();
#else
                    var itemName = option == SearchOption.AllDirectories ? $"{name.ToString()}##{id++}" : name.ToString();
#endif
                    var decorator = isDir ? $"{folderDecorator}" : fileDecorator(metadata);
                    FileSystemItem item = new(metadata, itemName, decorator, isDir ? FileSystemItemFlags.Folder : FileSystemItemFlags.None);
                    yield return item;
                }
            }
        }

        private static readonly Dictionary<string, List<FileSystemItem>> cache = [];

        public static List<FileSystemItem> GetFileSystemEntries(string folder, RefreshFlags refreshFlags, List<string>? allowedExtensions)
        {
            if (cache.TryGetValue(folder, out var cached))
            {
                return cached;
            }

            List<FileSystemItem> items = new();

            bool folders = (refreshFlags & RefreshFlags.Folders) != 0;
            bool files = (refreshFlags & RefreshFlags.Files) != 0;
            bool onlyAllowFilteredExtensions = (refreshFlags & RefreshFlags.OnlyAllowFilteredExtensions) != 0;

            if (onlyAllowFilteredExtensions && allowedExtensions is null)
            {
                throw new ArgumentNullException(nameof(allowedExtensions));
            }

            try
            {
                foreach (var metadata in FileUtils.EnumerateEntries(folder, string.Empty, SearchOption.TopDirectoryOnly))
                {
                    var flags = metadata.Attributes;
                    if ((flags & FileAttributes.System) != 0)
                        continue;
                    if ((flags & FileAttributes.Hidden) != 0)
                        continue;
                    if ((flags & FileAttributes.Device) != 0)
                        continue;

                    var span = metadata.Path.AsSpan();
                    var name = FileUtils.GetFileName(span);

                    var itemName = name.ToString();

                    if ((flags & FileAttributes.Directory) != 0)
                    {
                        if (folders)
                        {
                            items.Add(new(metadata, itemName, $"{MaterialIcons.Folder}", FileSystemItemFlags.Folder));
                        }

                        continue;
                    }
                    else if (files)
                    {
                        if (onlyAllowFilteredExtensions)
                        {
                            var ext = FileUtils.GetExtension(span);
                            if (allowedExtensions!.Contains(ext, StringComparison.OrdinalIgnoreCase))
                            {
                                items.Add(new FileSystemItem(metadata, itemName, string.Empty, FileSystemItemFlags.None));
                            }
                        }
                        else
                        {
                            items.Add(new FileSystemItem(metadata, itemName, string.Empty, FileSystemItemFlags.None));
                        }
                    }
                }
            }
            catch
            {
            }

            items.Sort(new CompareByNameComparer<FileSystemItem>());

            cache.Add(folder, items);
            return items;
        }
    }
}