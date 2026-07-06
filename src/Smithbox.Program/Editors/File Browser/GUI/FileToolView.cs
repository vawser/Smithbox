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

    public FileUnpackTool UnpackTool;
    public FileExtractTool ExtractTool;

    public FileToolView(FileEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        UnpackTool = new(view, project);
        ExtractTool = new(view, project);
    }

    public void Display()
    {
        if (!CFG.Current.Interface_FileBrowser_ToolView)
            return;

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_FileBrowser_Tool_GameUnpacker)
        {
            if (ImGui.CollapsingHeader("File Unpacker"))
            {
                UnpackTool.Display();
            }
        }

        if (CFG.Current.Interface_FileBrowser_Tool_FileExtract)
        {
            if (ImGui.CollapsingHeader("File Export"))
            {
                ExtractTool.Display();
            }
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Unpacker"))
            {
                CFG.Current.Interface_FileBrowser_Tool_GameUnpacker = !CFG.Current.Interface_FileBrowser_Tool_GameUnpacker;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_FileBrowser_Tool_GameUnpacker);

            if (ImGui.MenuItem("File Extract"))
            {
                CFG.Current.Interface_FileBrowser_Tool_FileExtract = !CFG.Current.Interface_FileBrowser_Tool_FileExtract;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_FileBrowser_Tool_FileExtract);

            ImGui.EndMenu();
        }
    }
}
