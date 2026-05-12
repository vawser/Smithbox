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

    public FileItemView(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        UIHelper.SimpleHeader("Item", "");

        DisplayItemViewer();
    }

    private void DisplayItemViewer()
    {
        if(Parent.Selection.SelectedVfsFile != null)
        {
            DisplayVfsItem();
        }
    }

    private string _internalFileSearch = "";
    private string _internalTexFileSearch = "";

    public void DisplayVfsItem()
    {
        var sectionHeight = ImGui.GetWindowHeight() * 0.3f;
        var sectionSize = new Vector2(0, sectionHeight * DPI.UIScale());

        var entry = Parent.Selection.SelectedVfsFile;

        // Main Container
        UIHelper.SimpleHeader("containerFile", "Main File", "", UI.Current.ImGui_Default_Text_Color);

        ImGui.BeginChild("MainFileSection", new Vector2(0, 70) * DPI.UIScale(), ImGuiChildFlags.Borders);

        ImGui.Text($"Name: {entry.Filename}");
        ImGui.Text($"Path: {entry.Path}");
        ImGui.Text($"");

        ImGui.EndChild();

        // Binder Entries
        if (Parent.Selection.InternalFileList.Count > 0)
        {
            UIHelper.SimpleHeader("internalFiles", "Internal Files", "", UI.Current.ImGui_Default_Text_Color);

            var searchHeight = new Vector2(0, 36) * DPI.UIScale();
            ImGui.BeginChild("InternalFileSearchSection", searchHeight, ImGuiChildFlags.Borders);

            ImGui.InputTextWithHint($"##InternalFileSearch", "Search...", ref _internalFileSearch, 255);

            ImGui.EndChild();

            ImGui.BeginChild("internalFileSection", sectionSize, ImGuiChildFlags.Borders);

            foreach (var file in Parent.Selection.InternalFileList)
            {
                var filename = file;

                if (_internalFileSearch != "")
                {
                    if (!filename.Contains(_internalFileSearch))
                    {
                        continue;
                    }
                }

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
            UIHelper.SimpleHeader("internalTexFiles", "Texture Files", "", UI.Current.ImGui_Default_Text_Color);

            var searchHeight = new Vector2(0, 36) * DPI.UIScale();
            ImGui.BeginChild("InternalTexFileSearchSection", searchHeight, ImGuiChildFlags.Borders);

            ImGui.InputTextWithHint($"##InternalTextFileSearch", "Search...", ref _internalTexFileSearch, 255);

            ImGui.EndChild();

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
                    if (_internalTexFileSearch != "")
                    {
                        if (!tex.Contains(_internalTexFileSearch))
                        {
                            continue;
                        }
                    }

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
