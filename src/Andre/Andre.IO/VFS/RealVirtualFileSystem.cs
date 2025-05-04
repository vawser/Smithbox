using DotNext.IO.MemoryMappedFiles;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO.VFS
{
    public class RealVirtualFileSystem : VirtualFileSystem
    {
        private bool isReadOnly;
        public override bool IsReadOnly => isReadOnly;

        private string rootPath;
        private RealVirtualDirectory fsRoot;
        public override VirtualDirectory FsRoot => fsRoot;

        public RealVirtualFileSystem(string rootPath, bool isReadOnly)
        {
            this.rootPath = rootPath;
            this.isReadOnly = isReadOnly;
            fsRoot = new RealVirtualDirectory(rootPath, this.isReadOnly);
        }

        public override bool FileExists(VirtualFileSystem.VFSPath path)
        {
            return File.Exists(Path.Combine(rootPath, path.ToString().TrimStart('/').TrimStart('\\')));
        }

        public override bool TryGetFile(VirtualFileSystem.VFSPath path, [MaybeNullWhen(false)] out VirtualFile file)
        {
            if (FileExists(path))
            {
                file = new RealVirtualFile(Path.Combine(rootPath, path.ToString().TrimStart('/').TrimStart('\\')), isReadOnly);
                return true;
            }

            file = null;
            return false;
        }

        public override bool DirectoryExists(VirtualFileSystem.VFSPath path)
        {
            return Directory.Exists(Path.Combine(rootPath, path.ToString().TrimStart('/').TrimStart('\\')));
        }

        public override IEnumerable<VirtualFile> EnumerateFiles()
        {
            throw new NotImplementedException();
        }

        public override void Delete(string path)
        {
            if (isReadOnly) throw ThrowWriteNotSupported();
            path = path.TrimStart('/').TrimStart('\\');
            File.Delete(Path.Combine(rootPath, path));
        }

        public override void Copy(string from, string to)
        {
            if (isReadOnly) throw ThrowWriteNotSupported();
            from = from.TrimStart('/').TrimStart('\\');
            to = to.TrimStart('/').TrimStart('\\');
            File.Copy(Path.Combine(rootPath, from), Path.Combine(rootPath, to), true);
        }

        public override void Move(string from, string to)
        {
            if (isReadOnly) throw ThrowWriteNotSupported();
            from = from.TrimStart('/').TrimStart('\\');
            to = to.TrimStart('/').TrimStart('\\');
            File.Move(Path.Combine(rootPath, from), Path.Combine(rootPath, to), true);
        }

        public class RealVirtualDirectory(string path, bool isReadOnly) : VirtualDirectory
        {
            public string path = path;
            public bool isReadOnly = isReadOnly;

            public override bool IsReadOnly => isReadOnly;

            public override bool FileExists(string fileName)
            {
                return File.Exists(Path.Combine(path, fileName.TrimStart('/').TrimStart('\\')));
            }

            public override bool TryGetFile(string fileName, out VirtualFile file)
            {
                if (!FileExists(fileName))
                {
                    file = null;
                    return false;
                }

                file = new RealVirtualFile(Path.Combine(path, fileName.TrimStart('/').TrimStart('\\')), isReadOnly);
                return true;
            }

            public override bool DirectoryExists(string directoryName)
            {
                return Directory.Exists(Path.Combine(path, directoryName.TrimStart('/').TrimStart('\\')));
            }

            public override bool TryGetDirectory(string directoryName, [MaybeNullWhen(false)] out VirtualDirectory directory)
            {
                if (!DirectoryExists(directoryName))
                {
                    directory = null;
                    return false;
                }

                directory = new RealVirtualDirectory(Path.Combine(path, directoryName.TrimStart('/').TrimStart('\\')), isReadOnly);
                return true;
            }

            public override VirtualDirectory GetOrCreateDirectory(string directoryName)
            {
                directoryName = directoryName.TrimStart('/').TrimStart('\\');
                string newPath = Path.Combine(path, directoryName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                return new RealVirtualDirectory(newPath, isReadOnly);
            }

            public override VirtualFile GetOrCreateFile(string fileName)
            {
                fileName = fileName.TrimStart('/').TrimStart('\\');
                string filePath = Path.Combine(path, fileName);
                if (TryGetFile(filePath, out var file))
                {
                    return file;
                }
                if (isReadOnly) throw ThrowWriteNotSupported();
                File.Create(filePath).Dispose();
                if (!TryGetFile(filePath, out file))
                {
                    throw new($"Failed to create file \"{filePath}\"... somehow?");
                }
                return file;
            }

            public override IEnumerable<(string, VirtualDirectory)> EnumerateDirectories()
            {
                return EnumerateDirectoryNames().Select(s => (s, new RealVirtualDirectory(s, isReadOnly) as VirtualDirectory));
            }

            public override IEnumerable<string> EnumerateDirectoryNames()
            {
                return Directory.EnumerateDirectories(path).Select(d => Path.GetFileNameWithoutExtension(d + ".dummy"));
            }

            public override IEnumerable<string> EnumerateFileNames()
            {
                return Directory.EnumerateFiles(path).Select(Path.GetFileName);
            }

            public override IEnumerable<(string, VirtualFile)> EnumerateFiles()
            {
                return EnumerateFileNames().Select(s => (s, new RealVirtualFile(s, isReadOnly) as VirtualFile));
            }
        }

        public class RealVirtualFile(string path, bool isReadOnly) : VirtualFile
        {
            public string path = path;
            public bool isReadOnly = isReadOnly;
            public override bool IsReadOnly => isReadOnly;

            public FileStream GetFileStream()
                => File.OpenRead(path);

            public override Memory<byte> GetData()
            {
                /*using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
                using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
                return accessor.Memory;*/
                var data = File.ReadAllBytes(path);
                return new Memory<byte>(data);
            }

            public override IMemoryOwner<byte> MemoryMapData()
            {
                var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open);
                return mmf.CreateMemoryAccessor(access: MemoryMappedFileAccess.Read);
            }

            public override void WriteData(byte[] data)
            {
                if (isReadOnly) throw ThrowWriteNotSupported();
                File.WriteAllBytes(path, data);
            }

            public override void Delete()
            {
                if (isReadOnly) throw ThrowWriteNotSupported();
                File.Delete(path);
            }
        }
    }
}