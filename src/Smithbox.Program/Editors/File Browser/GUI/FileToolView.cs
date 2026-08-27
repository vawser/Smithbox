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

    public FileUnpackerTool UnpackTool;
    public FileExporterTool ExtractTool;

    public FileToolView(FileEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        UnpackTool = new(view, project);
        ExtractTool = new(view, project);
    }

    public void Display()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_FileBrowser_Tool_GameUnpacker)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("FILE_Tools_File_Unpacker_Header")}##fileUnpacker"))
            {
                UnpackTool.Display();
            }
        }

        if (CFG.Current.Interface_FileBrowser_Tool_FileExtract)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("FILE_Tools_File_Exporter_Header")}##fileExporter"))
            {
                ExtractTool.Display();
            }
        }
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

            ImGui.EndMenu();
        }
    }
}
