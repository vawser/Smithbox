using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// A material binder (mtdbnd / matbinbnd) without its file type, for the actions and lists that work on either
/// </summary>
public interface IMaterialBinder
{
    string Name { get; }
    string FilePath { get; }
    MaterialSourceType SourceType { get; }

    /// <summary>
    /// Every file name in the binder, in binder order, including files that failed to read
    /// </summary>
    IReadOnlyList<string> FileOrder { get; }

    bool IsDirty();
    bool ContainsFile(string key);
    MaterialFileWrapper GetFile(string key);
    IEnumerable<MaterialFileWrapper> GetFiles();

    /// <summary>
    /// A new, unattached copy of a file in this binder, under another name
    /// </summary>
    MaterialFileWrapper CreateCopy(MaterialFileWrapper source, string newKey);

    void InsertFile(int orderIndex, MaterialFileWrapper file);

    /// <summary>
    /// Takes a file out, returning where it was in the binder order
    /// </summary>
    int RemoveFile(string key);

    void RenameFile(string oldKey, string newKey);

    /// <summary>
    /// Called by an action that adds, removes or renames a file, when done / undone
    /// </summary>
    void MarkStructureChanged();
    void MarkStructureUndone();

    bool SaveDirtyFiles();
}

/// <summary>
/// A material binder and its files. Files are added, removed and renamed in memory; saving rebuilds the binder
/// from the project's copy, so files the editor doesn't hold (unreadable, other types) keep their bytes.
/// </summary>
public abstract class MaterialBinderWrapper<TFile> : IMaterialBinder where TFile : MaterialFileWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string FilePath { get; set; }
    public Dictionary<string, TFile> Entries { get; set; } = new();

    /// <summary>
    /// The files in this binder that failed to read, shown but not editable
    /// </summary>
    public List<string> UnreadableFiles { get; } = new();

    private readonly List<string> Order = new();
    public IReadOnlyList<string> FileOrder => Order;

    // Structure edits done minus undone since load or save, as MaterialFileWrapper counts edits
    private int StructureBalance;

    public abstract MaterialSourceType SourceType { get; }

    protected abstract string Extension { get; }
    protected abstract bool IsBnd3 { get; }
    protected abstract TFile Read(string key, Memory<byte> bytes);
    protected abstract byte[] Write(TFile file);

    protected MaterialBinderWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        FilePath = dictEntry.Path;
    }

    /// <summary>
    /// Whether any file within this binder has been modified, added, removed or renamed
    /// </summary>
    public bool IsDirty()
    {
        return StructureBalance != 0 || Entries.Values.Any(e => e.IsDirty);
    }

    public async Task<bool> Load()
    {
        await Task.Yield();

        if (!TargetFS.FileExists(FilePath))
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Find_File", FilePath));
            return false;
        }

        try
        {
            var binderData = TargetFS.ReadFile(FilePath);

            IBinder binder = IsBnd3 ? BND3.Read(binderData.Value) : BND4.Read(binderData.Value);

            foreach (var entry in binder.Files)
            {
                Order.Add(entry.Name);

                if (!entry.Name.Contains(Extension))
                    continue;

                // One unreadable file must not hide the rest of the binder. It is left out of the
                // editor, and saving the binder keeps its bytes as they are.
                try
                {
                    Entries[entry.Name] = Read(entry.Name, entry.Bytes);
                }
                catch (Exception e)
                {
                    UnreadableFiles.Add(entry.Name);
                    Smithbox.LogError(this, LOC.Get(SourceType is MaterialSourceType.MTD
                        ? "MAT_Data_Log_Failed_to_Read_MTD"
                        : "MAT_Data_Log_Failed_to_Read_MATBIN", entry.Name), e);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_File", FilePath), e);
            return false;
        }
    }

    #region Structure
    public bool ContainsFile(string key) => Order.Contains(key);

    public MaterialFileWrapper GetFile(string key) => Entries.TryGetValue(key, out var file) ? file : null;

    public IEnumerable<MaterialFileWrapper> GetFiles() => Entries.Values;

    public MaterialFileWrapper CreateCopy(MaterialFileWrapper source, string newKey)
    {
        // Through the writer and reader, so the copy shares nothing with the original
        return Read(newKey, Write((TFile)source));
    }

    public void InsertFile(int orderIndex, MaterialFileWrapper file)
    {
        Entries[file.Key] = (TFile)file;
        Order.Insert(Math.Clamp(orderIndex, 0, Order.Count), file.Key);
    }

    public int RemoveFile(string key)
    {
        var index = Order.IndexOf(key);

        if (index >= 0)
            Order.RemoveAt(index);

        Entries.Remove(key);

        return index;
    }

    public void RenameFile(string oldKey, string newKey)
    {
        if (!Entries.Remove(oldKey, out var file))
            return;

        file.Key = newKey;
        Entries[newKey] = file;

        var index = Order.IndexOf(oldKey);
        if (index >= 0)
            Order[index] = newKey;
    }

    public void MarkStructureChanged() => StructureBalance++;

    public void MarkStructureUndone() => StructureBalance--;
    #endregion

    #region Save
    /// <summary>
    /// Rebuilds the binder in the editor's file order and saves it to the project, then clears the dirty marks.
    /// Unchanged files keep their bytes from the project's binder; files are renumbered 0..n-1 in order, as
    /// every vanilla material binder is, and take that binder's file flags and compression.
    /// Doesn't need a view, so the unsaved changes prompt can save every binder.
    /// </summary>
    public bool SaveDirtyFiles()
    {
        if (!IsDirty())
            return true;

        var containerFileData = TargetFS.ReadFile(FilePath);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        try
        {
            IBinder binder = IsBnd3 ? BND3.Read(containerFileData.Value) : BND4.Read(containerFileData.Value);

            var onDisk = new Dictionary<string, BinderFile>();
            foreach (var file in binder.Files)
                onDisk.TryAdd(file.Name, file);

            var template = binder.Files.FirstOrDefault();
            var files = new List<BinderFile>();

            foreach (var key in Order)
            {
                onDisk.TryGetValue(key, out var source);

                Memory<byte> bytes;

                if (Entries.TryGetValue(key, out var entry))
                {
                    bytes = entry.IsDirty || source == null ? Write(entry) : source.Bytes;
                }
                else if (source != null)
                {
                    bytes = source.Bytes;
                }
                else
                {
                    continue;
                }

                var flags = source?.Flags ?? template?.Flags ?? Binder.FileFlags.Flag1;

                files.Add(new BinderFile(flags, files.Count, key, bytes)
                {
                    CompressionType = source?.CompressionType ?? template?.CompressionType ?? DCX.Type.Zlib
                });
            }

            binder.Files = files;

            var writeFile = binder is BND3 bnd3 ? bnd3.Write() : ((BND4)binder).Write();

            Project.VFS.ProjectFS.WriteFile(FilePath, writeFile);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", FilePath), ex);
            return false;
        }

        foreach (var entry in Entries.Values)
        {
            entry.IsDirty = false;
        }

        StructureBalance = 0;

        Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", FilePath));

        return true;
    }
    #endregion
}

public class MTDWrapper : MaterialBinderWrapper<MTDFileWrapper>
{
    public MTDWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
        : base(project, dictEntry, targetFS) { }

    public override MaterialSourceType SourceType => MaterialSourceType.MTD;
    protected override string Extension => ".mtd";
    protected override bool IsBnd3 => Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    protected override MTDFileWrapper Read(string key, Memory<byte> bytes) => new(key, MTD.Read(bytes));
    protected override byte[] Write(MTDFileWrapper file) => file.MTD.Write();
}

public class MATBINWrapper : MaterialBinderWrapper<MATBINFileWrapper>
{
    public MATBINWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
        : base(project, dictEntry, targetFS) { }

    public override MaterialSourceType SourceType => MaterialSourceType.MATBIN;
    protected override string Extension => ".matbin";
    protected override bool IsBnd3 => false;
    protected override MATBINFileWrapper Read(string key, Memory<byte> bytes) => new(key, MATBIN.Read(bytes));
    protected override byte[] Write(MATBINFileWrapper file) => file.MATBIN.Write();
}
