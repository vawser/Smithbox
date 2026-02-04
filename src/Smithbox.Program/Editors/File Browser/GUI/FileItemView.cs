using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
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

    private string ExtractionPath = "";

    public FileItemView(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        ExtractionPath = project.Descriptor.ProjectPath;
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
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = ImGui.GetWindowHeight() * 0.3f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        var entry = Parent.Selection.SelectedVfsFile;

        // Main Container
        UIHelper.SimpleHeader("containerFile", "Main File", "", UI.Current.ImGui_Default_Text_Color);

        ImGui.Text($"Name: {entry.Filename}");
        ImGui.Text($"Path: {entry.Path}");

        ImGui.Text($"");

        // Binder Entries
        if (Parent.Selection.InternalFileList.Count > 0)
        {
            UIHelper.SimpleHeader("internalFiles", "Internal Files", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.InputTextWithHint(
                "##InternalFileSearch",
                "Search files...",
                ref _internalFileSearch,
                256
            );
            
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
                }
            }

            ImGui.EndChild();
        }

        // TPF Entries
        if (Parent.Selection.InternalTextureList.Count > 0)
        {
            UIHelper.SimpleHeader("internalTexFiles", "Texture Files", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.InputTextWithHint(
                "##textureFileSearch",
                "Search files...",
                ref _internalTexFileSearch,
                256
            );

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

        // Actions
        UIHelper.SimpleHeader("actions", "Actions", "", UI.Current.ImGui_Default_Text_Color);

        ImGui.Text($"Output Directory: {ExtractionPath}");

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##setOutputDir", DPI.IconButtonSize))
        {
            var newOutputDir = "";
            var result = PlatformUtils.Instance.OpenFolderDialog("Select Output Directory", out newOutputDir, ExtractionPath);

            if (result)
            {
                ExtractionPath = newOutputDir;
            }
        }

        ImGui.Text($"");

        if (ImGui.Button("Extract Main File", DPI.ThirdWidthButton(sectionWidth, 24)))
        {
            ExtractMainFile();
        }
        UIHelper.Tooltip("Extract the main file. Creates the folder structure it should reside in if missing.");

        if (Parent.Selection.SelectedInternalFile != "" || Parent.Selection.SelectedInternalTexFile != "")
        {
            ImGui.SameLine();

            if (ImGui.Button("Extract Selected Internal File", DPI.ThirdWidthButton(sectionWidth, 24)))
            {
                ExtractInternalFile();
            }
        }
        UIHelper.Tooltip("Extract selected internal file.");
    }

    public void ExtractMainFile()
    {
        var fileEntry = Parent.Selection.SelectedVfsFile;

        try
        {
            var data = Project.VFS.VanillaFS.ReadFile(fileEntry.Path);
            var rawData = (Memory<byte>)data;

            var unpackPath = ExtractionPath;

            var absFolder = $@"{unpackPath}/{fileEntry.Folder}";
            var absPath = $@"{unpackPath}/{fileEntry.Path}";

            if (!Directory.Exists(absFolder))
            {
                Directory.CreateDirectory(absFolder);
            }

            if (!File.Exists(absPath))
            {
                File.WriteAllBytes(absPath, rawData.ToArray());

                TaskLogs.AddLog($"[Smithbox:File Browser] Extracted {absPath}");

                data = null;
                rawData = null;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[Smithbox:File Browser] Failed to write file: {fileEntry.Path}", LogLevel.Error, LogPriority.High, e);
        }
    }

    public void ExtractInternalFile()
    {
        var targetFile = Parent.Selection.SelectedVfsFile;
        var internalFile = Parent.Selection.SelectedInternalFile;
        var internalTexFile = Parent.Selection.SelectedInternalTexFile;

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, targetFile.Path);

        var extractData = new byte[0];

        if (binderType is ResourceContainerType.None)
        {
            var fileData = Project.VFS.VanillaFS.ReadFile(targetFile.Path);
            if (fileData != null)
            {
                if (LocatorUtils.IsTPF(targetFile.Path))
                {
                    var tpfData = TPF.Read(fileData.Value);
                    foreach (var entry in tpfData.Textures)
                    {
                        if(Parent.Selection.SelectedInternalTexFile == entry.Name)
                        {
                            extractData = entry.Bytes;
                        }
                    }
                }
            }
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var fileData = Project.VFS.VanillaFS.ReadFile(targetFile.Path);
                    if (fileData != null)
                    {
                        var binder = new BND3Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            if(file.Name.ToLower() == internalFile)
                            {
                                extractData = binder.ReadFile(file).ToArray();

                                if (LocatorUtils.IsTPF(file.Name))
                                {
                                    var containerData = binder.ReadFile(file).ToArray();
                                    var tpfData = TPF.Read(containerData);
                                    foreach (var entry in tpfData.Textures)
                                    {
                                        if (Parent.Selection.SelectedInternalTexFile == entry.Name)
                                        {
                                            extractData = entry.Bytes;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogLevel.Error, LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var fileData = Project.VFS.VanillaFS.ReadFile(targetFile.Path);
                    if (fileData != null)
                    {
                        var binder = new BND4Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            if (file.Name.ToLower() == internalFile)
                            {
                                extractData = binder.ReadFile(file).ToArray();

                                if (LocatorUtils.IsTPF(file.Name))
                                {
                                    var containerData = binder.ReadFile(file).ToArray();
                                    var tpfData = TPF.Read(containerData);
                                    foreach (var entry in tpfData.Textures)
                                    {
                                        if (Parent.Selection.SelectedInternalTexFile == entry.Name)
                                        {
                                            extractData = entry.Bytes;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogLevel.Error, LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = targetFile.Path;
            var targetBdtPath = targetFile.Path.Replace("bhd", "bdt");

            try
            {
                bhd = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogLevel.Error, LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:File Browser] Failed to read {targetFile.Path}.", LogLevel.Error, LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (Project.Descriptor.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = new BXF3Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        if (file.Name.ToLower() == internalFile)
                        {
                            extractData = binder.ReadFile(file).ToArray();

                            if (LocatorUtils.IsTPF(file.Name))
                            {
                                var containerData = binder.ReadFile(file).ToArray();
                                var tpfData = TPF.Read(containerData);
                                foreach (var entry in tpfData.Textures)
                                {
                                    if (Parent.Selection.SelectedInternalTexFile == entry.Name)
                                    {
                                        extractData = entry.Bytes;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    var binder = new BXF4Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        if (file.Name.ToLower() == internalFile)
                        {
                            extractData = binder.ReadFile(file).ToArray();

                            if (LocatorUtils.IsTPF(file.Name))
                            {
                                var containerData = binder.ReadFile(file).ToArray();
                                var tpfData = TPF.Read(containerData);
                                foreach (var entry in tpfData.Textures)
                                {
                                    if (Parent.Selection.SelectedInternalTexFile == entry.Name)
                                    {
                                        extractData = entry.Bytes;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if(extractData.Length > 0)
        {
            var extension = "";
            var filename = Path.GetFileName(internalFile);
            if (internalTexFile != "")
            {
                filename = Path.GetFileName(internalTexFile);
                extension = ".dds";
            }

            var writePath = Path.Combine(ExtractionPath, filename);

            if(extension != "")
            {
                writePath = Path.Combine(ExtractionPath, $"{filename}{extension}");
            }

            File.WriteAllBytes(writePath, extractData);

            TaskLogs.AddLog($"[Smithbox:File Browser] Extracted {filename}");
        }
    }
}
