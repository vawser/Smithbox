using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

/// <summary>
/// Finds which containers under a folder hold an entry or texture with a given name ("which binder has
/// c2010.flver?"). Opens every container below the selected folder in the background, including nested binders,
/// and lists the matches; clicking one selects the container and its entry.
/// </summary>
public class FileBinderSearchTool
{
    private readonly FileEditorView Parent;
    private readonly ProjectEntry Project;

    private string Query = "";
    private bool ExactQuery = false;
    private bool IncludeTextures = true;

    private const int MaxResults = 2000;
    private const int MaxDepth = 3;

    private sealed record SearchResult(string Path, string Key, string Texture);

    private readonly List<SearchResult> Results = new();
    private string SearchedFolder = "";

    private volatile bool IsSearching = false;
    private int Total = 0;
    private int Done = 0;
    private CancellationTokenSource Cancel;

    public FileBinderSearchTool(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("BinderSearchSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("FILE_BinderSearch_Hint"));

        GUI.Spacer();

        EditorFilters.DisplayFramedListFilter("fileBrowser_BinderSearch", ref Query, ref ExactQuery);

        ImGui.Checkbox($"{LOC.Get("FILE_BinderSearch_Include_Textures")}##binderSearchTextures", ref IncludeTextures);
        GUI.Tooltip(LOC.Get("FILE_BinderSearch_Include_Textures_TT"));

        var folder = Parent.Selection.SelectedVfsFolder;
        ImGui.TextDisabled(LOC.Get("FILE_BinderSearch_Scope", folder?.Path ?? LOC.Get("FILE_BinderSearch_No_Folder")));

        if (IsSearching)
        {
            var progress = Total > 0 ? (float)Done / Total : 0f;
            ImGui.ProgressBar(progress, new Vector2(-1, 0), LOC.Get("FILE_BinderSearch_Progress", Done, Total));

            GUI.MultiButtonInput("binderSearchCancel",
                "cancelBinderSearch",
                LOC.Get("FILE_BinderSearch_Cancel"),
                LOC.Get("FILE_BinderSearch_Cancel_TT"),
                CancelSearch);
        }
        else
        {
            GUI.MultiButtonInput("binderSearchActions",
                "runBinderSearch",
                LOC.Get("FILE_BinderSearch_Run"),
                LOC.Get("FILE_BinderSearch_Run_TT"),
                StartSearch);
        }

        DisplayResults();

        ImGui.EndChild();
    }

    private void DisplayResults()
    {
        List<SearchResult> results;
        lock (Results)
            results = Results.ToList();

        if (results.Count == 0)
        {
            if (!IsSearching && SearchedFolder != "")
                ImGui.TextDisabled(LOC.Get("FILE_BinderSearch_No_Results"));
            return;
        }

        ImGui.Text(LOC.Get("FILE_BinderSearch_Result_Count", results.Count, SearchedFolder));

        if (results.Count >= MaxResults)
            ImGui.TextDisabled(LOC.Get("FILE_BinderSearch_Result_Limit", MaxResults));

        ImGui.BeginChild("binderSearchResults", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(results.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var result = results[i];
                var label = result.Texture != null
                    ? $"{result.Path}  >  {result.Key.Replace(FileContainerReader.KeySeparator.ToString(), "  >  ")}  >  {result.Texture}"
                    : $"{result.Path}  >  {result.Key.Replace(FileContainerReader.KeySeparator.ToString(), "  >  ")}";

                if (ImGui.Selectable($"{label}##binderResult_{i}"))
                    OpenResult(result);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void OpenResult(SearchResult result)
    {
        var file = Parent.Editor.Tree.FindFile(result.Path);
        if (file == null)
            return;

        // A texture's own key is the TPF it is in (empty for a loose TPF)
        Parent.Selection.SelectAndReveal(file, result.Key);
    }

    private void CancelSearch()
    {
        try { Cancel?.Cancel(); } catch (ObjectDisposedException) { }
    }

    private void StartSearch()
    {
        if (IsSearching)
            return;

        if (string.IsNullOrWhiteSpace(Query))
        {
            Smithbox.Log(this, LOC.Get("FILE_BinderSearch_No_Query"));
            return;
        }

        var folder = Parent.Selection.SelectedVfsFolder;
        if (folder == null)
        {
            Smithbox.Log(this, LOC.Get("FILE_BinderSearch_No_Folder_Selected"));
            return;
        }

        var files = new List<string>();
        Collect(folder, files);

        lock (Results)
            Results.Clear();

        SearchedFolder = folder.Path;
        Total = files.Count;
        Done = 0;
        IsSearching = true;

        var cts = new CancellationTokenSource();
        Cancel = cts;

        var fs = Project.VFS.FS;
        var query = Query;
        var exact = ExactQuery;
        var includeTextures = IncludeTextures;

        Task.Run(() =>
        {
            try
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token };

                Parallel.ForEach(files, options, (path, state) =>
                {
                    try
                    {
                        var container = FileContainerReader.Open(fs, path);
                        if (container != null)
                            SearchContainer(container, path, "", 1, query, exact, includeTextures);
                    }
                    catch
                    {
                        // Unreadable containers are skipped; the Item Viewer reports why when selected
                    }
                    finally
                    {
                        Interlocked.Increment(ref Done);
                    }

                    lock (Results)
                    {
                        if (Results.Count >= MaxResults)
                            state.Stop();
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_BinderSearch_Failed"), e);
            }
            finally
            {
                // Workers finish in any order; list the results by container, then entry
                lock (Results)
                {
                    Results.Sort((a, b) =>
                    {
                        var c = string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase);
                        return c != 0 ? c : string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
                    });
                }

                IsSearching = false;
                Cancel = null;
                cts.Dispose();
            }
        });
    }

    private void SearchContainer(FileContainer container, string path, string keyPrefix, int depth,
        string query, bool exact, bool includeTextures)
    {
        if (container.Kind is FileContainerKind.Tpf)
        {
            if (includeTextures)
            {
                foreach (var tex in container.Tpf.Textures)
                {
                    if (EditorFilters.IsMatch(query, tex.Name, exact))
                        Add(new SearchResult(path, keyPrefix, tex.Name));
                }
            }

            return;
        }

        foreach (var file in container.Files)
        {
            var name = file.Name.ToLower();
            var key = keyPrefix == "" ? name : $"{keyPrefix}{FileContainerReader.KeySeparator}{name}";

            if (EditorFilters.IsMatch(query, Path.GetFileName(name), exact))
                Add(new SearchResult(path, key, null));

            var isTpf = file.IsTpf;
            var mayNest = !isTpf && depth < MaxDepth && FileContainerReader.MayBeContainer(name);

            if (!(isTpf && includeTextures) && !mayNest)
                continue;

            try
            {
                var inner = FileContainerReader.OpenBytes(file.Read(), file.Name, FileContainerReader.SiblingResolver(container));
                if (inner != null)
                    SearchContainer(inner, path, key, depth + 1, query, exact, includeTextures);
            }
            catch
            {
            }
        }
    }

    private void Add(SearchResult result)
    {
        lock (Results)
        {
            if (Results.Count < MaxResults)
                Results.Add(result);
        }
    }

    private static void Collect(FolderNode node, List<string> files)
    {
        foreach (var file in node.Files)
        {
            if (FileContainerReader.MayBeContainer(file.Path))
                files.Add(file.Path);
        }

        foreach (var child in node.Children)
            Collect(child, files);
    }
}
