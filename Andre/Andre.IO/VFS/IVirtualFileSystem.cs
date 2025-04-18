namespace Andre.IO.VFS;

/// <summary>
/// Virtual file system used to abstract file system operations on a variety of sources, such as raw filesystem, a zip
/// file, or binders
/// </summary>
public interface IVirtualFileSystem
{
    /// <summary>
    /// Is the file system readonly
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Returns true if a given file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool FileExists(string path);

    /// <summary>
    /// Returns true if a given directory exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool DirectoryExists(string path);
}
