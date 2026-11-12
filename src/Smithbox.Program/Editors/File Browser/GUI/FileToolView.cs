using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileToolView
{
    public FileEditorView View;
    public ProjectEntry Project;

    // Shared by all views (owned by FileBrowserScreen)
    public FileUnpackerTool UnpackTool => View.Editor.UnpackTool;

    public FileExporterTool ExtractTool;
    public FileBinderSearchTool BinderSearchTool;

    private ToolSections Sections = new();

    public FileToolView(FileEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        ExtractTool = new(view, project);
        BinderSearchTool = new(view, project);
    }

    public void Display()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Files"));

        if (CFG.Current.Interface_FileBrowser_Tool_GameUnpacker)
        {
            if (ToolSections.Header($"{LOC.Get("FILE_Tools_File_Unpacker_Header")}##fileUnpacker"))
            {
                UnpackTool.Display();
            }
        }

        if (CFG.Current.Interface_FileBrowser_Tool_FileExtract)
        {
            if (ToolSections.Header($"{LOC.Get("FILE_Tools_File_Exporter_Header")}##fileExporter"))
            {
                ExtractTool.Display();
            }
        }

        if (CFG.Current.Interface_FileBrowser_Tool_BinderSearch)
        {
            if (ToolSections.Header($"{LOC.Get("FILE_Tools_Binder_Search_Header")}##binderSearch"))
            {
                BinderSearchTool.Display();
            }
        }

        Sections.End();
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("FILE_Tools_ViewToggle_File_Unpacker")}##toggleFileUnpacker"))
            {
                CFG.Current.Interface_FileBrowser_Tool_GameUnpacker = !CFG.Current.Interface_FileBrowser_Tool_GameUnpacker;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_FileBrowser_Tool_GameUnpacker);

            if (ImGui.MenuItem($"{LOC.Get("FILE_Tools_ViewToggle_File_Exporter")}##toggleFileExporter"))
            {
                CFG.Current.Interface_FileBrowser_Tool_FileExtract = !CFG.Current.Interface_FileBrowser_Tool_FileExtract;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_FileBrowser_Tool_FileExtract);

            if (ImGui.MenuItem($"{LOC.Get("FILE_Tools_ViewToggle_Binder_Search")}##toggleBinderSearch"))
            {
                CFG.Current.Interface_FileBrowser_Tool_BinderSearch = !CFG.Current.Interface_FileBrowser_Tool_BinderSearch;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_FileBrowser_Tool_BinderSearch);

            ImGui.EndMenu();
        }
    }
}
