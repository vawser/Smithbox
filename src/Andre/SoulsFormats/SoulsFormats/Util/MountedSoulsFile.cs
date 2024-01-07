using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats;

/// <summary>
/// A file that might be "mounted" and require IO mapped memory to be alive while it is being processed.
/// </summary>
public abstract class MountedSoulsFile<TFormat> : SoulsFile<TFormat>, IDisposable
    where TFormat : MountedSoulsFile<TFormat>, new()
{
    /// <summary>
    /// Loads file data while be given a memory owner object to keep the mapped memory alive
    /// </summary>
    protected virtual void Read(BinaryReaderEx br, IMappedMemoryOwner owner)
    {
        throw new NotImplementedException("Read is not implemented for this format.");
    }
    
    /// <summary>
    /// Loads a file from a byte array, automatically decompressing it if necessary.
    /// </summary>
    public new static TFormat Read(Memory<byte> bytes)
    {
        BinaryReaderEx br = new BinaryReaderEx(false, bytes);
        TFormat file = new TFormat();
        br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
        file.Compression = compression;
        file.Read(br, null);
        return file;
    }
    
    /// <summary>
    /// Loads a file from the specified path, automatically decompressing it if necessary.
    /// </summary>
    public new static TFormat Read(string path)
    {
        using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
        BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
        TFormat ret = new TFormat();
        br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
        ret.Compression = compression;
        ret.Read(br, accessor);
        return ret;
    }
    
    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}