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
        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Files"));

        if (ToolSections.Header($"{LOC.Get("FILE_Tools_File_Unpacker_Header")}##fileUnpacker"))
        {
            UnpackTool.Display();
        }

        if (ToolSections.Header($"{LOC.Get("FILE_Tools_File_Exporter_Header")}##fileExporter"))
        {
            ExtractTool.Display();
        }

        if (ToolSections.Header($"{LOC.Get("FILE_Tools_Binder_Search_Header")}##binderSearch"))
        {
            BinderSearchTool.Display();
        }

        Sections.End();
    }
}
