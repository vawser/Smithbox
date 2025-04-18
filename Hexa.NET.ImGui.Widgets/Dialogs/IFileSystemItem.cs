namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    public interface IFileSystemItem
    {
        string Path { get; }

        string Icon { get; }

        string Name { get; }

        FileSystemItemFlags Flags { get; }

        DateTime DateModified { get; }

        string Type { get; }

        long Size { get; }

        CommonFilePermissions Permissions { get; }

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER

        public bool IsFile => (Flags & FileSystemItemFlags.Folder) == 0;

        public bool IsFolder => (Flags & FileSystemItemFlags.Folder) != 0;

        public bool IsHidden => (Flags & FileSystemItemFlags.Hidden) != 0;

#else
        public bool IsFile { get; }

        public bool IsFolder { get; }

        public bool IsHidden { get; }
#endif
    }

    public static class BaseComparer
    {
        public static int CompareByBase(IFileSystemItem a, IFileSystemItem b)
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
    }
}