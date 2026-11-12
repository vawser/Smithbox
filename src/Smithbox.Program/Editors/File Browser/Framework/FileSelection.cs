using Andre.IO.VFS;
using StudioCore.Application;
using StudioCore.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileSelection
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    public FolderNode SelectedVfsFolder = null;
    public FileDictionaryEntry SelectedVfsFile = null;

    /// <summary>
    /// The entries of the selected container as a tree: binders inside it are opened and listed under their entry
    /// </summary>
    public List<InternalNode> InternalNodes = new();

    // Texture names per TPF, by entry key (the file's path for a loose TPF)
    public Dictionary<string, List<string>> InternalTextureList = new();

    public bool HasInternalFiles => InternalNodes.Count > 0;

    // Key of the focused inner entry (FileContainerReader.ReadByKey), and the texture selected in it
    public string SelectedInternalFile = "";
    public string SelectedInternalTexFile = "";

    /// <summary>
    /// All selected files in the File List (Ctrl/Shift-click), by path. Always includes SelectedVfsFile.
    /// </summary>
    public HashSet<string> SelectedPaths = new(StringComparer.OrdinalIgnoreCase);

    // Where a Shift-click range starts
    public string SelectionAnchor = null;

    /// <summary>
    /// All selected inner entries (Ctrl-click in the binder list), by key. Always includes SelectedInternalFile.
    /// </summary>
    public HashSet<string> SelectedInternalKeys = new(StringComparer.OrdinalIgnoreCase);

    // Nested binders deeper than this are listed but not opened
    private const int MaxNestingDepth = 4;

    /// <summary>
    /// A file or folder path the File List should open its way to and scroll to on the next frames
    /// </summary>
    public string RevealPath = null;
    public int RevealFramesLeft = 0;

    // An inner entry to select once the background load of the selected file has finished
    private string PendingInternalKey = null;

    /// <summary>
    /// Selects one file, opens the File List down to it and scrolls to it. internalKey, if given, is selected in the
    /// binder list once the file's contents have loaded. Used by bookmarks, Find in Binders and the file/select
    /// command.
    /// </summary>
    public void SelectAndReveal(FileDictionaryEntry file, string internalKey = null)
    {
        SelectedPaths.Clear();
        SelectedPaths.Add(file.Path);
        SelectionAnchor = file.Path;

        // The key is handed in before the load starts, so even a fast load can't finish without it
        UpdateVfsFileSelection(file, internalKey);

        Reveal(file.Path);
    }

    public void Reveal(string path)
    {
        RevealPath = path;
        RevealFramesLeft = 5;
    }

    /// <summary>
    /// Facts about the selected file. Where it comes from and its size are set at once; the format, compression and
    /// entry count arrive with the background load.
    /// </summary>
    public FileDetails Details = null;

    public FileSelection(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// True while the selected file's contents are being read in the background
    /// </summary>
    public volatile bool IsLoading = false;

    // Each selection gets a number; a background load only publishes if its number is still the latest, so a slow
    // load can never overwrite a newer selection
    private int SelectionRequest = 0;
    private readonly object SelectionLock = new();

    /// <summary>
    /// Selects a file and lists what is inside it. The binder and its TPFs are read on a worker thread, so a large
    /// map tpfbhd or chrbnd doesn't freeze the UI. Reads go through the merged VFS, so a file the project overrides
    /// shows the project's version, and files that only exist in the project can be opened.
    /// </summary>
    public void UpdateVfsFileSelection(FileDictionaryEntry targetFile, string pendingInternalKey = null)
    {
        int request;

        lock (SelectionLock)
        {
            SelectedVfsFile = targetFile;
            InternalNodes = new();
            InternalTextureList = new();

            SelectedInternalFile = "";
            SelectedInternalTexFile = "";
            SelectedInternalKeys.Clear();
            PendingInternalKey = string.IsNullOrEmpty(pendingInternalKey) ? null : pendingInternalKey;

            request = ++SelectionRequest;
            IsLoading = FileContainerReader.MayBeContainer(targetFile.Path);
            Details = GetDetails(targetFile);
        }

        if (!IsLoading)
            return;

        var fs = Project.VFS.FS;
        var path = targetFile.Path;
        var baseDetails = Details;

        Task.Run(() =>
        {
            var files = new List<InternalNode>();
            var textures = new Dictionary<string, List<string>>();
            FileDetails details = baseDetails;

            try
            {
                var container = LoadContents(fs, path, files, textures);

                if (container != null)
                {
                    details = baseDetails.With(
                        container.Format,
                        container.Compression.ToString(),
                        container.Kind is FileContainerKind.Tpf ? container.Tpf.Textures.Count : container.Files.Count);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", path), e);
            }

            lock (SelectionLock)
            {
                if (request != SelectionRequest)
                    return;

                InternalNodes = files;
                InternalTextureList = textures;
                Details = details;
                IsLoading = false;

                if (PendingInternalKey != null)
                {
                    SelectedInternalFile = PendingInternalKey;
                    SelectedInternalKeys.Clear();
                    SelectedInternalKeys.Add(PendingInternalKey);
                    PendingInternalKey = null;
                }
            }
        });
    }

    /// <summary>
    /// Where the file is read from and how big it is there. Only stats files and looks up archive headers, so it is
    /// cheap enough to run on the click itself.
    /// </summary>
    private FileDetails GetDetails(FileDictionaryEntry entry)
    {
        var details = new FileDetails
        {
            Source = Parent.Editor.Tree.GetSource(entry),
            Archive = entry.Archive ?? ""
        };

        try
        {
            var rel = entry.Path.Replace('\\', '/').TrimStart('/');
            var projectPath = System.IO.Path.Combine(Project.Descriptor.ProjectPath, rel);
            var gamePath = System.IO.Path.Combine(Project.Descriptor.DataPath, rel);

            if (details.Source is not FileSource.Vanilla && System.IO.File.Exists(projectPath))
            {
                details.StoredIn = LOC.Get("FILE_ItemViewer_Stored_Project");
                details.Size = new System.IO.FileInfo(projectPath).Length;
            }
            else if (FileContainerReader.FindArchiveFile(Project, entry.Path) is { } archived)
            {
                var header = archived.FileHeader;

                details.StoredIn = LOC.Get("FILE_ItemViewer_Stored_Archive",
                    System.IO.Path.GetFileName(archived.Bdt?.Name ?? entry.Archive ?? ""));
                details.Size = header.UnpaddedFileSize > 0 ? header.UnpaddedFileSize : header.PaddedFileSize;
            }
            else if (System.IO.File.Exists(gamePath))
            {
                details.StoredIn = LOC.Get("FILE_ItemViewer_Stored_Loose");
                details.Size = new System.IO.FileInfo(gamePath).Length;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", entry.Path), e);
        }

        return details;
    }

    private FileContainer LoadContents(VirtualFileSystem fs, string path, List<InternalNode> files, Dictionary<string, List<string>> textures)
    {
        var container = FileContainerReader.Open(fs, path);
        if (container == null)
            return null;

        if (container.Kind is FileContainerKind.Tpf)
        {
            textures[path] = GetTextureNames(container.Tpf);
            return container;
        }

        LoadBinder(container, "", 1, path, files, textures);

        return container;
    }

    /// <summary>
    /// Lists a binder's entries. TPFs get their texture names; entries that are binders themselves are opened and
    /// listed as children, so e.g. a chrbnd's chrtpfbhd shows its TPFs.
    /// </summary>
    private void LoadBinder(FileContainer container, string keyPrefix, int depth, string rootPath,
        List<InternalNode> nodes, Dictionary<string, List<string>> textures)
    {
        foreach (var file in container.Files)
        {
            var name = file.Name.ToLower();
            var key = keyPrefix == "" ? name : $"{keyPrefix}{FileContainerReader.KeySeparator}{name}";

            var node = new InternalNode { Name = name, Key = key };
            nodes.Add(node);

            // A bad entry only loses its own contents, not the rest of the binder
            try
            {
                if (file.IsTpf)
                {
                    textures[key] = GetTextureNames(FileContainerReader.ReadTpf(file.Read()));
                    node.IsTpf = true;
                }
                else if (depth < MaxNestingDepth && FileContainerReader.MayBeContainer(name))
                {
                    var inner = FileContainerReader.OpenBytes(file.Read(), file.Name,
                        FileContainerReader.SiblingResolver(container));

                    if (inner?.Kind is FileContainerKind.Binder)
                        LoadBinder(inner, key, depth + 1, rootPath, node.Children, textures);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", $"{rootPath} > {key}"), e);
            }
        }
    }

    private static List<string> GetTextureNames(SoulsFormats.TPF tpf)
    {
        var names = new List<string>();

        foreach (var entry in tpf.Textures)
            names.Add(entry.Name);

        return names;
    }
}

/// <summary>
/// One entry in the selected container. Children are filled when the entry is itself a binder.
/// </summary>
public sealed class InternalNode
{
    public string Name;
    public string Key;
    public bool IsTpf;
    public List<InternalNode> Children = new();
}

/// <summary>
/// What the Item Viewer shows about the selected file. Treated as immutable once published: the background load
/// publishes a new instance (With) rather than changing the one the UI is drawing.
/// </summary>
public sealed class FileDetails
{
    public FileSource Source;
    public string Archive = "";
    public string StoredIn = "";

    // Bytes as stored (project file, archive entry without padding, or loose file); -1 when unknown
    public long Size = -1;

    // Only for containers, once loaded
    public string Format = "";
    public string Compression = "";
    public int EntryCount = -1;

    public FileDetails With(string format, string compression, int entryCount)
    {
        var copy = (FileDetails)MemberwiseClone();
        copy.Format = format;
        copy.Compression = compression;
        copy.EntryCount = entryCount;
        return copy;
    }
}
