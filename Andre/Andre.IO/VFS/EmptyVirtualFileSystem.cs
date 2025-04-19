using System.Diagnostics.CodeAnalysis;

namespace Andre.IO.VFS
{
    public class EmptyVirtualFileSystem : VirtualFileSystem
    {
        public override bool IsReadOnly => true;
        private EmptyVirtualDirectory fsRoot = new();
        public override VirtualDirectory FsRoot => fsRoot;

        public static EmptyVirtualFileSystem Instance = new();
        public override bool FileExists(VFSPath path)
        {
            return false;
        }

        public override bool TryGetFile(VFSPath path, [MaybeNullWhen(false)] out VirtualFile file)
        {
            file = null;
            return false;
        }

        public override bool DirectoryExists(VFSPath path)
        {
            return false;
        }

        public override IEnumerable<VirtualFile> EnumerateFiles()
        {
            return Array.Empty<VirtualFile>();
        }

        public class EmptyVirtualDirectory : VirtualDirectory
        {
            public override bool IsReadOnly => true;
            public override bool FileExists(string fileName)
            {
                return false;
            }

            public override bool TryGetFile(string fileName, [MaybeNullWhen(false)] out VirtualFile file)
            {
                file = null;
                return false;
            }

            public override bool DirectoryExists(string directoryName)
            {
                return false;
            }

            public override bool TryGetDirectory(string directoryName, [MaybeNullWhen(false)] out VirtualDirectory directory)
            {
                directory = null;
                return false;
            }

            public override IEnumerable<(string, VirtualDirectory)> EnumerateDirectories()
            {
                return Array.Empty<(string, VirtualDirectory)>();
            }

            public override IEnumerable<string> EnumerateDirectoryNames()
            {
                return Array.Empty<string>();
            }

            public override IEnumerable<string> EnumerateFileNames()
            {
                return Array.Empty<string>();
            }

            public override IEnumerable<(string, VirtualFile)> EnumerateFiles()
            {
                return Array.Empty<(string, VirtualFile)>();
            }

            public override VirtualDirectory GetOrCreateDirectory(string directoryName)
            {
                throw ThrowWriteNotSupported();
            }

            public override VirtualFile GetOrCreateFile(string fileName)
            {
                throw ThrowWriteNotSupported();
            }
        }
    }
}