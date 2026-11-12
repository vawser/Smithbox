using StudioCore.Application;
using StudioCore.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

/// <summary>
/// The folder tree of every file the project can see, shared by all File Browser views. Built in the background from
/// the locator's file dictionary, rebuilt when the locator replaces or resizes it (a project reload or rescan), and on
/// Refresh, which also picks up files saved into the project after it loaded.
/// </summary>
public class FileTree
{
    private readonly ProjectEntry Project;

    private volatile FolderNode _root;

    /// <summary>
    /// The current tree, or null while the first build is running
    /// </summary>
    public FolderNode Root => _root;

    /// <summary>
    /// Goes up by one each time a new tree is published, so views can drop anything they cached for the old one
    /// </summary>
    public int Version => Volatile.Read(ref _version);
    private int _version;

    public bool IsBuilding => _building;
    private volatile bool _building;

    private FileDictionary _source;
    private int _sourceCount;
    private bool _refreshRequested;

    public FileTree(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Call once a frame. Starts a rebuild when the locator's dictionary changed or a refresh was asked for.
    /// </summary>
    public void Update()
    {
        if (_building)
            return;

        var dict = Project.Locator?.FileDictionary;
        if (dict == null)
            return;

        var changed = _root == null
            || !ReferenceEquals(dict, _source)
            || dict.Entries.Count != _sourceCount;

        if (!changed && !_refreshRequested)
            return;

        var scanProject = _refreshRequested;
        _refreshRequested = false;

        _source = dict;
        _sourceCount = dict.Entries.Count;

        // Snapshot on this thread; the build itself runs in the background
        var entries = dict.Entries.ToList();

        _building = true;

        Task.Run(() =>
        {
            try
            {
                if (scanProject)
                {
                    try
                    {
                        entries.AddRange(Project.Locator.FindNewProjectFiles());
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("FILE_FileList_Refresh_Scan_Failed"), e);
                    }
                }

                var projectFiles = IndexProjectFiles();
                var root = Build(entries);

                lock (ProjectFilesLock)
                    _projectFiles = projectFiles;

                _root = root;
                Interlocked.Increment(ref _version);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_FileList_Build_Failed"), e);
            }
            finally
            {
                _building = false;
            }
        });
    }

    // Every file present in the project folder, as dictionary paths ("/chr/c0000.chrbnd.dcx"). Rebuilt with the tree.
    private HashSet<string> _projectFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly object ProjectFilesLock = new();

    /// <summary>
    /// Where the project gets a file from: the game, the project replacing a game file, or the project alone.
    /// Based on the project folder as it was at the last tree build (or Copy to Project since).
    /// </summary>
    public FileSource GetSource(FileDictionaryEntry entry)
    {
        bool inProject;

        lock (ProjectFilesLock)
            inProject = _projectFiles.Contains(entry.Path);

        if (!inProject)
            return FileSource.Vanilla;

        // The locator merges the project scan in only for paths the game doesn't have; those entries are tagged
        // with the project folder's name as their archive
        var projectArchive = System.IO.Path.GetFileName(
            System.IO.Path.TrimEndingDirectorySeparator(Project.Descriptor.ProjectPath ?? ""));

        return string.Equals(entry.Archive, projectArchive, StringComparison.OrdinalIgnoreCase)
            ? FileSource.ProjectOnly
            : FileSource.ProjectOverride;
    }

    /// <summary>
    /// Record a file just written into the project (Copy to Project), so its tag updates without a rebuild
    /// </summary>
    public void MarkProjectFile(string path)
    {
        lock (ProjectFilesLock)
            _projectFiles.Add(path);

        // Views cache per-folder filter results, which can depend on a file's source
        Interlocked.Increment(ref _version);
    }

    private HashSet<string> IndexProjectFiles()
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var projectPath = Project.Descriptor.ProjectPath;

        if (string.IsNullOrEmpty(projectPath) || !System.IO.Directory.Exists(projectPath))
            return files;

        try
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(projectPath, "*", System.IO.SearchOption.AllDirectories))
            {
                files.Add("/" + System.IO.Path.GetRelativePath(projectPath, file).Replace('\\', '/'));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileList_Refresh_Scan_Failed"), e);
        }

        return files;
    }

    /// <summary>
    /// Rebuild on the next Update, including files saved into the project since it loaded
    /// </summary>
    public void Refresh()
    {
        _refreshRequested = true;
    }

    /// <summary>
    /// The folder at a path ("/chr", "/" for the root) in the current tree, or null
    /// </summary>
    public FolderNode FindFolder(string path)
    {
        var node = _root;
        if (node == null || path == null)
            return null;

        foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            node = node.Children.FirstOrDefault(c => string.Equals(c.Name, part, StringComparison.OrdinalIgnoreCase));
            if (node == null)
                return null;
        }

        return node;
    }

    /// <summary>
    /// The file at a dictionary path in the current tree, or null
    /// </summary>
    public FileDictionaryEntry FindFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var slash = path.LastIndexOf('/');
        var folder = FindFolder(slash <= 0 ? "/" : path[..slash]);

        return folder?.Files.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));
    }

    private static FolderNode Build(List<FileDictionaryEntry> entries)
    {
        var root = new FolderNode { Name = "/", Path = "/" };

        // Child lookup by name while building; the sorted lists are what the views draw
        var lookup = new Dictionary<FolderNode, Dictionary<string, FolderNode>>();

        foreach (var entry in entries)
        {
            var current = root;

            foreach (var part in (entry.Folder ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!lookup.TryGetValue(current, out var children))
                {
                    children = new Dictionary<string, FolderNode>(StringComparer.Ordinal);
                    lookup[current] = children;
                }

                if (!children.TryGetValue(part, out var child))
                {
                    child = new FolderNode
                    {
                        Name = part,
                        Path = current.Path == "/" ? $"/{part}" : $"{current.Path}/{part}"
                    };

                    children[part] = child;
                    current.Children.Add(child);
                }

                current = child;
            }

            current.Files.Add(entry);
        }

        Sort(root);

        return root;
    }

    private static void Sort(FolderNode node)
    {
        node.Children.Sort((a, b) =>
            string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        node.Files.Sort((a, b) =>
            string.Compare(
                $"{a.Filename}.{a.Extension}",
                $"{b.Filename}.{b.Extension}",
                StringComparison.OrdinalIgnoreCase));

        foreach (var child in node.Children)
            Sort(child);
    }
}

public enum FileSource
{
    Vanilla,
    ProjectOverride,
    ProjectOnly
}

public class FolderNode
{
    public string Name;

    /// <summary>
    /// Full folder path, as in FileDictionaryEntry.Folder ("/chr", "/" for the root)
    /// </summary>
    public string Path;

    public List<FolderNode> Children = new();
    public List<FileDictionaryEntry> Files = new();
}
