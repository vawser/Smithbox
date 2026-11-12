using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;


namespace StudioCore.Editors.FileBrowser;

public class FileItemView
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    private string InternalFileListFilter = "";
    private bool ExactInternalFileListFilter = false;

    private string TextureFileListFilter = "";
    private bool ExactTextureFileListFilter = false;

    private readonly FilePreview Preview;

    public FileItemView(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        Preview = new FilePreview(view, project);
    }

    public void Display()
    {
        GUI.TitleHeader(
            LOC.Get("FILE_ItemViewer_Header"),
            LOC.Get("FILE_ItemViewer_Header_TT"));

        DisplayItemViewer();
    }

    private void DisplayItemViewer()
    {
        if(Parent.Selection.SelectedVfsFile != null)
        {
            DisplayVfsItem();
        }
    }

    /// <summary>
    /// The container's entries as a tree. Nested binders open like folders. Click selects one entry, Ctrl-click
    /// adds or removes it from the selection used by Export Selected Internal Files.
    /// </summary>
    private void DrawInternalNodes(List<InternalNode> nodes)
    {
        var selection = Parent.Selection;
        var filtering = !string.IsNullOrWhiteSpace(InternalFileListFilter);

        foreach (var node in nodes)
        {
            var matches = EditorFilters.IsMatch(InternalFileListFilter, node.Name, ExactInternalFileListFilter);
            var childMatches = filtering && node.Children.Count > 0 && AnyChildMatches(node);

            if (!matches && !childMatches)
                continue;

            var flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.OpenOnArrow;

            if (node.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

            if (selection.SelectedInternalKeys.Contains(node.Key))
                flags |= ImGuiTreeNodeFlags.Selected;

            if (childMatches)
                ImGui.SetNextItemOpen(true, ImGuiCond.Always);

            var open = ImGui.TreeNodeEx($"{node.Name}##internal_{node.Key.GetHashCode()}", flags);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && !ImGui.IsItemToggledOpen())
            {
                if (ImGui.GetIO().KeyCtrl)
                {
                    if (!selection.SelectedInternalKeys.Remove(node.Key))
                    {
                        selection.SelectedInternalKeys.Add(node.Key);
                        selection.SelectedInternalFile = node.Key;
                    }
                    else if (selection.SelectedInternalFile == node.Key)
                    {
                        selection.SelectedInternalFile = "";
                    }
                }
                else
                {
                    selection.SelectedInternalKeys.Clear();
                    selection.SelectedInternalKeys.Add(node.Key);
                    selection.SelectedInternalFile = node.Key;
                }

                selection.SelectedInternalTexFile = "";
            }

            if (open && node.Children.Count > 0)
            {
                DrawInternalNodes(node.Children);
                ImGui.TreePop();
            }
        }
    }

    private bool AnyChildMatches(InternalNode node)
    {
        foreach (var child in node.Children)
        {
            if (EditorFilters.IsMatch(InternalFileListFilter, child.Name, ExactInternalFileListFilter) || AnyChildMatches(child))
                return true;
        }

        return false;
    }

    private void DisplayDetails(FileDetails details)
    {
        if (details == null)
            return;

        // Source, coloured as in the File List
        switch (details.Source)
        {
            case FileSource.ProjectOverride:
                ImGui.TextColored(UI.Current.ImGui_PrimaryChanged_Text, LOC.Get("FILE_ItemViewer_Source_Override"));
                break;
            case FileSource.ProjectOnly:
                ImGui.TextColored(UI.Current.ImGui_AuxAdded_Text, LOC.Get("FILE_ItemViewer_Source_Project_Only"));
                break;
            default:
                ImGui.Text(LOC.Get("FILE_ItemViewer_Source_Vanilla"));
                break;
        }

        if (details.StoredIn != "")
            ImGui.Text(LOC.Get("FILE_ItemViewer_Stored_In", details.StoredIn));

        if (details.Size >= 0)
            ImGui.Text(LOC.Get("FILE_ItemViewer_Size", FormatSize(details.Size), details.Size.ToString("N0")));

        if (details.Format != "")
        {
            ImGui.Text(LOC.Get("FILE_ItemViewer_Format", details.Format,
                details.Compression is "" or "None" ? LOC.Get("FILE_ItemViewer_Uncompressed") : details.Compression));
        }

        if (details.EntryCount >= 0)
            ImGui.Text(LOC.Get("FILE_ItemViewer_Entry_Count", details.EntryCount));
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double value = bytes;
        var unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.##} {units[unit]}";
    }

    public void DisplayVfsItem()
    {
        // Window height is already in scaled pixels, so no extra DPI factor here
        var sectionHeight = ImGui.GetWindowHeight() * 0.3f;
        var sectionSize = new Vector2(0, sectionHeight);

        var entry = Parent.Selection.SelectedVfsFile;

        // Main Container
        GUI.SimpleHeader(
            LOC.Get("FILE_ItemViewer_Main_File_Header"),
            LOC.Get("FILE_ItemViewer_Main_File_Header_TT"));

        ImGui.BeginChild("MainFileSection", new Vector2(0, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

        ImGui.Text($"{LOC.Get("FILE_ItemViewer_MainFile_Name", entry.Filename)}");
        ImGui.Text($"{LOC.Get("FILE_ItemViewer_MainFile_Path", entry.Path)}");

        DisplayDetails(Parent.Selection.Details);

        // Open in Editor, with the selected texture when there is one
        var actions = Parent.Editor.Actions;
        if (actions.GetOpenCommand(entry, Parent.Selection.SelectedInternalTexFile, out var editorName) != null)
        {
            if (ImGui.Button($"{LOC.Get("FILE_Actions_Open_In_Editor", editorName)}##itemOpenInEditor"))
            {
                actions.OpenInEditor(entry, Parent.Selection.SelectedInternalTexFile);
            }
            GUI.Tooltip(LOC.Get("FILE_Actions_Open_In_Editor_TT"));
        }

        ImGui.EndChild();

        if (Parent.Selection.IsLoading)
        {
            ImGui.Text(LOC.Get("FILE_ItemViewer_Loading"));
            return;
        }

        // Binder Entries: only for binders, so loose files don't get an empty filter and list
        if (Parent.Selection.HasInternalFiles)
        {
            GUI.SimpleHeader(
                LOC.Get("FILE_ItemViewer_Binder_File_Header"),
                LOC.Get("FILE_ItemViewer_Binder_File_Header_TT"));

            EditorFilters.DisplayFramedListFilter("fileBrowser_InternalFileList",
                ref InternalFileListFilter, ref ExactInternalFileListFilter);

            ImGui.BeginChild("internalFileSection", sectionSize, ImGuiChildFlags.Borders);

            DrawInternalNodes(Parent.Selection.InternalNodes);

            ImGui.EndChild();
        }

        // TPF Entries
        if (Parent.Selection.InternalTextureList.Count > 0)
        {
            GUI.SimpleHeader(
                LOC.Get("FILE_ItemViewer_Texture_File_Header"),
                LOC.Get("FILE_ItemViewer_Texture_File_Header_TT"));

            EditorFilters.DisplayFramedListFilter("fileBrowser_TextureFileList",
                ref TextureFileListFilter, ref ExactTextureFileListFilter);

            ImGui.BeginChild("internalTexFileSection", sectionSize, ImGuiChildFlags.Borders);

            foreach (var texEntry in Parent.Selection.InternalTextureList)
            {
                var containerFile = texEntry.Key;
                var texNames = texEntry.Value;

                if (!LocatorUtils.IsTPF(Parent.Selection.SelectedVfsFile.Path))
                {
                    if (containerFile != Parent.Selection.SelectedInternalFile)
                        continue;
                }

                foreach (var tex in texNames)
                {
                    var isMatch = EditorFilters.IsMatch(TextureFileListFilter, tex, ExactTextureFileListFilter);

                    if (!isMatch)
                        continue;

                    var selected = false;
                    if (tex == Parent.Selection.SelectedInternalTexFile)
                    {
                        selected = true;
                    }

                    if (ImGui.Selectable($"{tex}##fileTexEntry_{tex.GetHashCode()}", selected))
                    {
                        Parent.Selection.SelectedInternalTexFile = tex;
                    }
                }
            }

            ImGui.EndChild();
        }

        // Preview of the texture, inner file or file selected above
        Preview.Display();
    }

}
