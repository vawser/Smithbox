namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.Utilities.IO;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Security.AccessControl;
    using System.Text.Json;

    public struct FileSystemItem : IEquatable<FileSystemItem>, IFileSystemItem
    {
        private string path;
        private string icon;
        private string name;
        private FileSystemItemFlags flags;
        private DateTime dateModified;
        private string type;
        private long size;
        private CommonFilePermissions permissions;

        public FileSystemItem(string path, string icon, string name, FileSystemItemFlags flags)
        {
            this.path = path;
            this.icon = icon;
            this.name = name;
            this.flags = flags;
            dateModified = path.TryReturn(File.GetLastWriteTime);

            if (IsFile)
            {
                size = path.TryReturn(FileUtils.GetFileSize);
                type = DetermineFileType(FileUtils.GetExtension(path.AsSpan()));
            }
            else
            {
                type = "File Folder";
            }
        }

        public FileSystemItem(FileMetadata metadata, string name, string icon, FileSystemItemFlags flags)
        {
            path = metadata.Path.ToString();
            this.icon = icon;
            this.name = name;
            this.flags = flags;
            dateModified = metadata.LastWriteTime;

            if (IsFile)
            {
                size = metadata.Size;
                type = DetermineFileType(FileUtils.GetExtension(path.AsSpan()));
            }
            else
            {
                type = "File Folder";
            }
        }

        public FileSystemItem(string path, string icon, FileSystemItemFlags flags)
        {
            this.path = path;
            this.icon = icon;
            name = System.IO.Path.GetFileName(path);
            this.flags = flags;

            dateModified = path.TryReturn(File.GetLastWriteTime);
            if (IsFile)
            {
                size = path.TryReturn(FileUtils.GetFileSize);
                type = DetermineFileType(FileUtils.GetExtension(path.AsSpan()));
            }
            else
            {
                type = "File Folder";
            }
        }

#if NET5_0_OR_GREATER

        private static CommonFilePermissions ConvertUnixPermissions(UnixFileMode permissions)
        {
            CommonFilePermissions result = CommonFilePermissions.None;

            if ((permissions & UnixFileMode.UserRead) != 0)
                result |= CommonFilePermissions.OwnerRead;
            if ((permissions & UnixFileMode.UserWrite) != 0)
                result |= CommonFilePermissions.OwnerWrite;
            if ((permissions & UnixFileMode.UserExecute) != 0)
                result |= CommonFilePermissions.OwnerExecute;
            if ((permissions & UnixFileMode.GroupRead) != 0)
                result |= CommonFilePermissions.GroupRead;
            if ((permissions & UnixFileMode.GroupWrite) != 0)
                result |= CommonFilePermissions.GroupWrite;
            if ((permissions & UnixFileMode.GroupExecute) != 0)
                result |= CommonFilePermissions.GroupExecute;
            if ((permissions & UnixFileMode.OtherRead) != 0)
                result |= CommonFilePermissions.OtherRead;
            if ((permissions & UnixFileMode.OtherWrite) != 0)
                result |= CommonFilePermissions.OtherWrite;
            if ((permissions & UnixFileMode.OtherExecute) != 0)
                result |= CommonFilePermissions.OtherExecute;

            return result;
        }

        [SupportedOSPlatform("windows")]
        private static CommonFilePermissions ConvertWindowsPermissions(FileSecurity security)
        {
            CommonFilePermissions result = CommonFilePermissions.None;

            var rules = security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
            string currentUser = Environment.UserName;
            string usersGroup = "Users";

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.AccessControlType == AccessControlType.Allow)
                {
                    string identity = rule.IdentityReference.Value;

                    if (identity.Equals(currentUser, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Owner permissions
                        if ((rule.FileSystemRights & FileSystemRights.ReadData) != 0)
                            result |= CommonFilePermissions.OwnerRead;
                        if ((rule.FileSystemRights & FileSystemRights.WriteData) != 0)
                            result |= CommonFilePermissions.OwnerWrite;
                        if ((rule.FileSystemRights & FileSystemRights.ExecuteFile) != 0)
                            result |= CommonFilePermissions.OwnerExecute;
                    }
                    else if (identity.Equals(usersGroup, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Other permissions
                        if ((rule.FileSystemRights & FileSystemRights.ReadData) != 0)
                            result |= CommonFilePermissions.OtherRead;
                        if ((rule.FileSystemRights & FileSystemRights.WriteData) != 0)
                            result |= CommonFilePermissions.OtherWrite;
                        if ((rule.FileSystemRights & FileSystemRights.ExecuteFile) != 0)
                            result |= CommonFilePermissions.OtherExecute;
                    }
                    else
                    {
                        // Group permissions (simplified for example purposes)
                        if ((rule.FileSystemRights & FileSystemRights.ReadData) != 0)
                            result |= CommonFilePermissions.GroupRead;
                        if ((rule.FileSystemRights & FileSystemRights.WriteData) != 0)
                            result |= CommonFilePermissions.GroupWrite;
                        if ((rule.FileSystemRights & FileSystemRights.ExecuteFile) != 0)
                            result |= CommonFilePermissions.GroupExecute;
                    }
                }
            }

            return result;
        }

#endif

        public readonly bool IsFile => (flags & FileSystemItemFlags.Folder) == 0;

        public readonly bool IsFolder => (flags & FileSystemItemFlags.Folder) != 0;

        public readonly bool IsHidden => (flags & FileSystemItemFlags.Hidden) != 0;

        public string Path { readonly get => path; set => path = value; }

        public string Icon { readonly get => icon; set => icon = value; }

        public string Name { readonly get => name; set => name = value; }

        public FileSystemItemFlags Flags { readonly get => flags; set => flags = value; }

        public DateTime DateModified { readonly get => dateModified; set => dateModified = value; }

        public string Type { readonly get => type; set => type = value; }

        public long Size { readonly get => size; set => size = value; }

        public CommonFilePermissions Permissions { readonly get => permissions; set => permissions = value; }

        static FileSystemItem()
        {
            LoadFileTypes();
        }

        public static string DetermineFileType(ReadOnlySpan<char> extension)
        {
            if (extension.IsEmpty)
            {
                return "File";
            }

            ulong hash = GetSpanHash(extension[1..]);

            if (fileTypes.TryGetValue(hash, out var type))
            {
                return type;
            }

#if NET5_0_OR_GREATER
            type = $"{extension} File"; // generic type name
#else
            type = $"{extension.ToString()} File"; // generic type name
#endif
            fileTypes.TryAdd(hash, type);

            return type;
        }

        private static void LoadFileTypes()
        {
            Stream? stream = null;
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                string resourceName = "Hexa.NET.ImGui.Widgets.assets.fileTypes.json";

                stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    return;
                }

                var fileTypeDict = JsonSerializer.Deserialize(stream, SourceGenerationContextDictionary.Default.DictionaryStringString);

                if (fileTypeDict != null)
                {
                    foreach (var kvp in fileTypeDict)
                    {
                        fileTypes[GetSpanHash(kvp.Key.AsSpan()[1..])] = kvp.Value;
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                stream?.Close();
            }
        }

        private static readonly ConcurrentDictionary<ulong, string> fileTypes = [];

        public static void RegisterFileType(string extension, string type)
        {
            fileTypes[GetSpanHash(extension.AsSpan())] = type; // overwrite existing type
        }

        public static ulong GetSpanHash(ReadOnlySpan<char> span, bool caseSensitive = false)
        {
            ulong hash = 7;
            foreach (var c in span)
            {
                var cc = c;
                if (!caseSensitive)
                {
                    cc = char.ToLower(c);
                }
                hash = hash * 31 + cc;
            }
            return hash;
        }

        public static int CompareByBase(FileSystemItem a, FileSystemItem b)
        {
            if (a.IsFolder && !b.IsFolder)
            {
                return -1;
            }
            if (!a.IsFolder && b.IsFolder)
            {
                return 1;
            }
            return 0;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is FileSystemItem item && Equals(item);
        }

        public readonly bool Equals(FileSystemItem other)
        {
            return Path == other.Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public static bool operator ==(FileSystemItem left, FileSystemItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FileSystemItem left, FileSystemItem right)
        {
            return !(left == right);
        }
    }
}