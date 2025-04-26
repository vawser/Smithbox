using SoulsFormats;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO.VFS;

/// <summary>
/// Virtual file system used to abstract file system operations on a variety of sources, such as raw filesystem, a zip
/// file, or binders.
/// 
/// Within the context of a virtual filesystem object, all paths are relative to the object that accepts the path.
/// Paths are sometimes allowed to have a leading or trailing slash, but no guarantees are made.
/// All functions should work with either "/" or "\" as the path separator, or a mix of both.
/// </summary>
public abstract class VirtualFileSystem : IDisposable
{
    private HashSet<IDisposable> disposables = [];

    /// <summary>
    /// Is the file system readonly
    /// </summary>
    public abstract bool IsReadOnly { get; }

    /// <summary>
    /// The directory representing the root of this filesystem
    /// </summary>
    public abstract VirtualDirectory FsRoot { get; }

    public void AddDisposable(IDisposable d)
    {
        disposables.Add(d);
    }

    public void RemoveDisposable(IDisposable d)
    {
        disposables.Remove(d);
    }

    /// <summary>
    /// Returns true if a given file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool FileExists(string path) => FileExists(new VFSPath(path));

    /// <summary>
    /// Returns true if a given file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public abstract bool FileExists(VFSPath path);

    /// <summary>
    /// Gets a file from a given path, if that file can be found
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public bool TryGetFile(string path, [MaybeNullWhen(false)] out VirtualFile file)
        => TryGetFile(new VFSPath(path), out file);

    /// <summary>
    /// Reads a SoulsFile from a given path.
    /// Will throw an exception if the file does not exist.
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T ReadSoulsFile<T>(string path) where T : SoulsFile<T>, new()
        => SoulsFile<T>.Read(ReadFileOrThrow(path));

    public Memory<byte> ReadFileOrThrow(string path)
    {
        var data = ReadFile(path);
        if (!data.HasValue) throw new FileNotFoundException($"Could not find file {path}");
        return data.Value;
    }

    /// <summary>
    /// Attempts to read a file from a given path.
    /// Returns null if the file cannot be found.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Memory<byte>? ReadFile(string path)
        => GetFile(path)?.GetData();

    /// <summary>
    /// Gets the VirtualFile object that represents a file at a given path.
    /// Throws a FileNotFoundException if the file cannot be found.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public VirtualFile GetFileOrThrow(string path)
    {
        var ans = GetFile(path);
        if (ans == null) throw new FileNotFoundException($"Could not find file {path}");
        return ans;
    }

    /// <summary>
    /// Gets the VirtualFile object that represents a file at a given path.
    /// Returns null if the file cannot be found.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public VirtualFile? GetFile(string path)
    {
        TryGetFile(path, out var f);
        return f;
    }

    /// <summary>
    /// Gets a file from a given path, if that file can be found
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public abstract bool TryGetFile(VFSPath path, [MaybeNullWhen(false)] out VirtualFile file);

    public IMemoryOwner<byte> MemoryMapFileOrThrow(string path)
    {
        var data = MemoryMapFile(path);
        if (data == null) throw new FileNotFoundException($"Could not find file {path}");
        return data;
    }

    /// <summary>
    /// Memory map the data of this file.
    /// </summary>
    /// <returns></returns>
    public IMemoryOwner<byte>? MemoryMapFile(string path)
    {
        TryMemoryMapFile(path, out var d);
        return d;
    }

    /// <summary>
    /// Memory map the data of this file.
    /// </summary>
    /// <returns></returns>
    public bool TryMemoryMapFile(string path, [MaybeNullWhen(false)] out IMemoryOwner<byte> data)
        => TryMemoryMapFile(new VFSPath(path), out data);

    /// <summary>
    /// Memory map the data of this file.
    /// </summary>
    /// <returns></returns>
    public bool TryMemoryMapFile(VFSPath path, [MaybeNullWhen(false)] out IMemoryOwner<byte> data)
    {
        if (!TryGetFile(path, out var f))
        {
            data = null;
            return false;
        }

        data = f.MemoryMapData();
        return true;
    }

    /// <summary>
    /// Returns true if a given directory exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public bool DirectoryExists(string path) => DirectoryExists(new VFSPath(path));

    /// <summary>
    /// Returns true if a given directory exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public abstract bool DirectoryExists(VFSPath path);

    /// <summary>
    /// Gets the VirtualDirectory object that represents a given directory.
    /// Returns null if the directory cannot be found.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public VirtualDirectory? GetDirectory(string path)
    {
        return GetDirectory(new VFSPath(path));
    }

    /// <summary>
    /// Gets the VirtualDirectory object that represents a given directory.
    /// Returns null if the directory cannot be found.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public VirtualDirectory? GetDirectory(VFSPath path)
    {
        var currDir = FsRoot;
        foreach (var dir in path.directories)
        {
            if (currDir.TryGetDirectory(dir, out var d))
            {
                currDir = d;
            }
            else return null;
        }

        return currDir;
    }

    /// <summary>
    /// Tries to get the VirtualDirectory object that represents a given directory.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public bool TryGetDirectory(string path, [MaybeNullWhen(false)] out VirtualDirectory directory)
    {
        directory = GetDirectory(path);
        return directory != null;
    }

    /// <summary>
    /// Returns an enumerator over all the files in this filesystem
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<VirtualFile> EnumerateFiles();

    /// <summary>
    /// Equivalent to Directory.GetFileSystemEntries with regex (but only returns files, not directories)
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public IEnumerable<string> GetFileNamesMatching(string directoryPath, [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
        => GetDirectory(directoryPath)?.GetFileNamesMatching(regex).Select(p => Path.Combine(directoryPath, p)) ?? Array.Empty<string>();

    /// <summary>
    /// Equivalent to Directory.GetFileSystemEntries with regex
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public IEnumerable<string> GetFileSystemEntriesMatching(string directoryPath, [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
        => GetDirectory(directoryPath)?.GetFileSystemEntriesMatching(regex).Select(p => Path.Combine(directoryPath, p)) ?? Array.Empty<string>();

    public IEnumerable<string> GetFileNamesMatchingRecursive(string directoryPath,
        [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
    {
        if (!TryGetDirectory(directoryPath, out var dir))
            return Array.Empty<string>();
        var ans = dir.GetFileNamesMatching(regex).Select(p => Path.Combine(directoryPath, p)).ToList();

        List<(string, VirtualDirectory)> pathStack = [(directoryPath, dir)];
        while (pathStack.Count != 0)
        {
            var (curr, currDir) = pathStack[^1];
            pathStack.RemoveAt(pathStack.Count - 1);
            ans.AddRange(currDir.GetFileNamesMatching(regex).Select(p => Path.Combine(curr, p)));
            foreach (var (n, d) in currDir.EnumerateDirectories())
            {
                pathStack.Add((Path.Combine(curr, n), d));
            }
        }

        return ans;
    }

    public IEnumerable<string> GetFileSystemEntriesMatchingRecursive(string directoryPath,
        [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
    {
        if (!TryGetDirectory(directoryPath, out var dir))
            return Array.Empty<string>();
        var ans = dir.GetFileSystemEntriesMatching(regex).Select(p => Path.Combine(directoryPath, p)).ToList();

        List<(string, VirtualDirectory)> pathStack = [(directoryPath, dir)];
        while (pathStack.Count != 0)
        {
            var (curr, currDir) = pathStack[^1];
            pathStack.RemoveAt(pathStack.Count - 1);
            ans.AddRange(currDir.GetFileSystemEntriesMatching(regex).Select(p => Path.Combine(curr, p)));
            foreach (var (n, d) in currDir.EnumerateDirectories())
            {
                pathStack.Add((Path.Combine(curr, n), d));
            }
        }

        return ans;
    }

    /// <summary>
    /// Finds all files in a directory that have one of the given extensions.
    /// Returns strings are full paths, not just file names.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public IEnumerable<string> GetFileNamesWithExtensions(string directoryPath, params string[] extensions)
        => extensions
            .Select(e => $".*{e.Replace(".", "\\.")}$")
            .Select(regex => GetFileNamesMatching(directoryPath, regex))
            .Aggregate((a, b) => a.Concat(b))
            .Distinct();

    #region WriteOperations

    /// <summary>
    /// Attempts to write all the supplied data to the specified file.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public void WriteFile(string path, byte[] data)
        => WriteFile(new VFSPath(path), data);

    /// <summary>
    /// Attempts to write all the supplied data to the specified file.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public void WriteFile(VFSPath path, byte[] data)
        => GetOrCreateFile(path).WriteData(data);

    public VirtualFile GetOrCreateFile(string path)
        => GetOrCreateFile(new VFSPath(path));

    public VirtualFile GetOrCreateFile(VFSPath path)
        => GetOrCreateDirectory(path).GetOrCreateFile(path.fileName!);

    /// <summary>
    /// Creates the specified directory, and any parent directories, if any of them do not exist.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    public void GetOrCreateDirectory(string path)
        => GetOrCreateDirectory(new VFSPath(path));

    /// <summary>
    /// Creates the specified directory, and any parent directories, if any of them do not exist.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    public VirtualDirectory GetOrCreateDirectory(VFSPath path)
        => path.directories.Aggregate(FsRoot, (current, dir) => current.GetOrCreateDirectory(dir));

    /// <summary>
    /// Attempt to delete a given file.
    /// Does nothing if the file cannot be found.
    /// May throw exceptions.
    /// </summary>
    /// <param name="path"></param>
    public virtual void Delete(string path)
        => GetFile(path)?.Delete();

    public virtual void Copy(string from, string to)
        => WriteFile(to, GetFile(from).GetData().ToArray());

    public virtual void Move(string from, string to)
    {
        WriteFile(to, GetFile(from).GetData().ToArray());
        Delete(from);
    }

    #endregion

    public struct VFSPath(string[] directories, string? fileName)
    {
        public string[] directories = directories;
        public string? fileName = fileName;

        public VFSPath(params string[] pathComponents) :
            this(
                pathComponents.Length == 0 ? pathComponents : (pathComponents[^1].Contains('.') ? pathComponents[0..^1] : pathComponents),
                pathComponents.Length == 0 ? null : (pathComponents[^1].Contains('.') ? pathComponents[^1] : null))
        { }

        public VFSPath(string path) : this(path.Replace('\\', '/').Trim().Trim('/').Split('/').Where(s => s != "").ToArray()) { }

        public override string ToString()
        {
            return ("/" + string.Join('/', directories) + "/" + (fileName ?? "")).Replace("//", "/");
        }
    }

    internal static NotSupportedException ThrowWriteNotSupported()
        => new("Attempted to write to a read-only file or filesystem.");

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var d in disposables)
        {
            d.Dispose();
        }
    }
}

public abstract class VirtualDirectory
{

    public abstract bool IsReadOnly { get; }

    /// <summary>
    /// Returns true if a given file exists within this directory.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public abstract bool FileExists(string fileName);

    /// <summary>
    /// Gets the binary data of a file in this directory, if that file can be found
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileData"></param>
    /// <returns>true if the file was found, false otherwise</returns>
    public abstract bool TryGetFile(string fileName, [MaybeNullWhen(false)] out VirtualFile file);

    public VirtualFile? GetFile(string fileName)
    {
        return TryGetFile(fileName, out var d) ? d : null;
    }

    /// <summary>
    /// Returns true if a given directory exists within this directory.
    /// Only one level of directories may be queried, i.e. the path may not contain '/'.
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public abstract bool DirectoryExists(string directoryName);

    public abstract bool TryGetDirectory(string directoryName, [MaybeNullWhen(false)] out VirtualDirectory directory);

    public VirtualDirectory? GetDirectory(string directoryName)
    {
        return TryGetDirectory(directoryName, out var d) ? d : null;
    }

    /// <summary>
    /// Returns an enumerator over the directories contained by this directory.
    /// The enumerator returns tuples of the directory name followed by the directory object.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<(string, VirtualDirectory)> EnumerateDirectories();

    /// <summary>
    /// Returns an enumerator over the names of the directories contained by this directory.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<string> EnumerateDirectoryNames();

    /// <summary>
    /// Returns an enumerator over the names of the files contained by this directory.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<string> EnumerateFileNames();

    /// <summary>
    /// Returns an enumerator over the files contained by this directory.
    /// The enumerator returns tuples of the file name followed by the file object.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<(string, VirtualFile)> EnumerateFiles();

    public IEnumerable<string> GetFileNamesMatching([StringSyntax(StringSyntaxAttribute.Regex)] string regex) =>
        EnumerateFileNames().Where(s => Regex.IsMatch(s, regex));


    public IEnumerable<string> GetFileSystemEntriesMatching([StringSyntax(StringSyntaxAttribute.Regex)] string regex) =>
        EnumerateFileNames().Concat(EnumerateDirectoryNames()).Where(s => Regex.IsMatch(s, regex));

    #region WritingOperations

    /// <summary>
    /// Attempts to get a directory with a specified name.
    /// If the directory does not already exist, an attempt is made to create that directory.
    /// May throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public abstract VirtualDirectory GetOrCreateDirectory(string directoryName);

    /// <summary>
    /// Gets a file with a given name, or if the file does not exist, attempts to create it.
    /// May throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public abstract VirtualFile GetOrCreateFile(string fileName);

    #endregion

}

public abstract class VirtualFile
{
    public abstract bool IsReadOnly { get; }

    /// <summary>
    /// Attempt to get the binary data of this file.
    /// </summary>
    /// <returns></returns>
    public abstract Memory<byte> GetData();

    /// <summary>
    /// Memory map the data of this file.
    /// </summary>
    /// <returns></returns>
    public abstract IMemoryOwner<byte> MemoryMapData();

    /// <summary>
    /// Attempt to write binary data to a file.
    /// Guaranteed to throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    /// <param name="data"></param>
    public virtual void WriteData(byte[] data)
    {
        throw VirtualFileSystem.ThrowWriteNotSupported();
    }

    /// <summary>
    /// Attempt to delete this file.
    /// Guaranteed to throw NotSupportedException if IsReadOnly is true.
    /// </summary>
    public virtual void Delete()
    {
        throw VirtualFileSystem.ThrowWriteNotSupported();
    }
}