using System.Diagnostics.CodeAnalysis;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO.VFS
{
    public class CompundVirtualFileSystem : VirtualFileSystem
    {
        private List<VirtualFileSystem> innerFileSystems;

        public CompundVirtualFileSystem(List<VirtualFileSystem> innerFileSystems)
        {
            this.innerFileSystems = innerFileSystems;
            List<VirtualDirectory> tmp = [];
            foreach (var fs in this.innerFileSystems)
            {
                tmp.Add(fs.FsRoot);
            }
            this.fsRoot = new(tmp, IsReadOnly, null);
        }

        public override bool IsReadOnly => innerFileSystems[0].IsReadOnly;
        private CompoundVirtualDirectory fsRoot;
        public override VirtualDirectory FsRoot => fsRoot;
        public override bool FileExists(VirtualFileSystem.VFSPath path)
        {
            foreach (var fs in innerFileSystems)
            {
                if (fs.FileExists(path)) return true;
            }

            return false;
        }

        public override bool TryGetFile(VirtualFileSystem.VFSPath path, out VirtualFile file)
        {
            foreach (var fs in this.innerFileSystems)
            {
                if (fs.TryGetFile(path, out var tmp))
                {
                    file = tmp;
                    return true;
                }
            }

            file = null;
            return false;
        }

        public override bool DirectoryExists(VirtualFileSystem.VFSPath path)
        {
            foreach (var fs in innerFileSystems)
            {
                if (fs.DirectoryExists(path)) return true;
            }

            return false;
        }

        public override IEnumerable<VirtualFile> EnumerateFiles()
        {
            List<VirtualFile> files = [];
            List<VirtualDirectory> dirs = [fsRoot];
            while (dirs.Count != 0)
            {
                var dir = dirs[^1];
                dirs.RemoveAt(dirs.Count - 1);
                dirs.AddRange(dir.EnumerateDirectories().Select(t => t.Item2));
                files.AddRange(dir.EnumerateFiles().Select(t => t.Item2));
            }

            return files;
        }

        private class CompoundVirtualDirectory(List<VirtualDirectory> backingDirs, bool isReadOnly, CompoundVirtualDirectory? parent) : VirtualDirectory
        {
            private List<VirtualDirectory> backingDirs = backingDirs;
            private bool isReadOnly = isReadOnly;
            public override bool IsReadOnly => isReadOnly;
            private CompoundVirtualDirectory? parent = parent;

            public override bool FileExists(string fileName)
            {
                return TryGetFile(fileName, out var _);
            }

            public override bool TryGetFile(string fileName, [MaybeNullWhen(false)] out VirtualFile file)
            {
                foreach (var dir in backingDirs)
                {
                    if (!dir.TryGetFile(fileName, out var f))
                        continue;
                    file = f;
                    return true;
                }

                file = null;
                return false;
            }

            public override bool DirectoryExists(string directoryName)
            {
                return TryGetDirectory(directoryName, out var _);
            }

            public override bool TryGetDirectory(string directoryName, [MaybeNullWhen(false)] out VirtualDirectory directory)
            {
                List<VirtualDirectory> dirs = new();
                bool hasIter = false;
                bool hasWriteFs = false;
                foreach (var d in this.backingDirs)
                {
                    if (d.TryGetDirectory(directoryName, out var tmp))
                    {
                        dirs.Add(tmp);
                        if (!hasIter && !isReadOnly) hasWriteFs = true;
                    }
                    hasIter = true;
                }

                if (dirs.Count == 0)
                {
                    directory = null;
                    return false;
                }

                directory = new CompoundVirtualDirectory(dirs, hasWriteFs, this);
                return true;
            }

            public override IEnumerable<(string, VirtualDirectory)> EnumerateDirectories()
            {
                List<(string, VirtualDirectory)> ans = [];
                foreach (var dir in EnumerateDirectoryNames())
                {
                    List<VirtualDirectory> inner = [];
                    bool hasIter = false;
                    bool hasWriteFs = false;
                    foreach (var par in this.backingDirs)
                    {
                        if (par.TryGetDirectory(dir, out var tmp))
                        {
                            inner.Add(tmp);
                            if (!hasIter && !isReadOnly) hasWriteFs = true;
                        }
                        hasIter = true;
                    }
                    ans.Add((dir, new CompoundVirtualDirectory(inner, hasWriteFs, this)));
                }

                return ans;
            }

            public override IEnumerable<string> EnumerateDirectoryNames()
            {
                HashSet<string> childDirs = [];
                foreach (var d in backingDirs)
                {
                    foreach (var cd in d.EnumerateDirectoryNames())
                    {
                        childDirs.Add(cd);
                    }
                }

                return childDirs;
            }

            public override IEnumerable<string> EnumerateFileNames()
            {
                HashSet<string> children = [];
                foreach (var d in backingDirs)
                {
                    foreach (var cd in d.EnumerateFileNames())
                    {
                        children.Add(cd);
                    }
                }

                return children;
            }

            public override IEnumerable<(string, VirtualFile)> EnumerateFiles()
            {
                List<(string, VirtualFile)> ans = [];
                foreach (var child in EnumerateFileNames())
                {
                    foreach (var d in backingDirs)
                    {
                        if (d.TryGetFile(child, out var f))
                        {
                            ans.Add((child, f));
                            break;
                        }
                    }
                }

                return ans;
            }

            public override VirtualDirectory GetOrCreateDirectory(string directoryName)
            {
                if (isReadOnly)
                {
                    if (!TryGetDirectory(directoryName, out var dir))
                    {
                        throw ThrowWriteNotSupported();
                    }
                    return dir;
                }

                throw new NotImplementedException();
            }

            public override VirtualFile GetOrCreateFile(string fileName)
            {
                if (isReadOnly)
                {
                    if (!TryGetFile(fileName, out var file))
                    {
                        throw ThrowWriteNotSupported();
                    }

                    return file;
                }
                throw new NotImplementedException();
            }
        }

    }
}