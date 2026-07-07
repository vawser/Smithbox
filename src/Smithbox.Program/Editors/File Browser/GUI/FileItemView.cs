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

    public FileItemView(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        GUI.SimpleHeader("Item", "");

        DisplayItemViewer();
    }

    private void DisplayItemViewer()
    {
        if(Parent.Selection.SelectedVfsFile != null)
        {
            DisplayVfsItem();
        }
    }

    public void DisplayVfsItem()
    {
        var sectionHeight = ImGui.GetWindowHeight() * 0.3f;
        var sectionSize = new Vector2(0, sectionHeight * DPI.UIScale());

        var entry = Parent.Selection.SelectedVfsFile;

        // Main Container
        GUI.SimpleHeader("containerFile", "Main File", "", UI.Current.ImGui_Default_Text_Color);

        ImGui.BeginChild("MainFileSection", new Vector2(0, 70) * DPI.UIScale(), ImGuiChildFlags.Borders);

        ImGui.Text($"Name: {entry.Filename}");
        ImGui.Text($"Path: {entry.Path}");
        ImGui.Text($"");

        ImGui.EndChild();

        // Binder Entries
        if (Parent.Selection.InternalFileList.Count > 0)
        {
            GUI.SimpleHeader("internalFiles", "Internal Files", "", UI.Current.ImGui_Default_Text_Color);

            EditorFilters.DisplayFramedListFilter("fileBrowser_InternalFileList",
                ref InternalFileListFilter, ref ExactInternalFileListFilter);

            ImGui.BeginChild("internalFileSection", sectionSize, ImGuiChildFlags.Borders);

            foreach (var file in Parent.Selection.InternalFileList)
            {
                var filename = file;

                var isMatch = EditorFilters.IsMatch(InternalFileListFilter, filename, ExactInternalFileListFilter);

                if (!isMatch)
                    continue;

                var selected = false;
                if (file == Parent.Selection.SelectedInternalFile)
                {
                    selected = true;
                }

                if (ImGui.Selectable($"{filename}##fileEntry_{file.GetHashCode()}", selected))
                {
                    Parent.Selection.SelectedInternalFile = file;
                    Parent.Selection.SelectedInternalTexFile = "";
                }
            }

            ImGui.EndChild();
        }

        // TPF Entries
        if (Parent.Selection.InternalTextureList.Count > 0)
        {
            GUI.SimpleHeader("internalTexFiles", "Texture Files", "", UI.Current.ImGui_Default_Text_Color);

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
    }

}
