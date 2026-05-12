using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileExtractTool
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    private string ExtractionPath = "";

    public FileExtractTool(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        ExtractionPath = project.Descriptor.ProjectPath;
    }

    public void Display()
    {
        ImGui.BeginChild("FileExtractSection", ImGuiChildFlags.Borders);

        UIHelper.SimpleHeader("Output Directory", "");

        UIHelper.HintTextInput("##outputDir", ref ExtractionPath, "Set the output directory...");

        UIHelper.MultiButtonInput("extractActions",
            "setExtractDir", "Set Output Directory", "", SetExtractionDirectory,
            "openExtractDir", "Open Output Directory", "", OpenExtractionDirectory,
            "extractMainFile", "Extract Main File", "", ExtractMainFile,
            "extractInternalFile", "Extract Internal File", "", ExtractInternalFile,
            "extractTextureFile", "Extract Texture File", "", ExtractTextureFile);

        ImGui.EndChild();
    }

    public void OpenExtractionDirectory()
    {
        Process.Start("explorer.exe", ExtractionPath);
    }

    public void SetExtractionDirectory()
    {
        var newOutputDir = "";
        var result = PlatformUtils.Instance.OpenFolderDialog("Select Output Directory", out newOutputDir, ExtractionPath);

        if (result)
        {
            ExtractionPath = newOutputDir;
        }
    }

    public void ExtractMainFile()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExtractTool>("Output directory has not been set.");
            return;
        }

        var fileEntry = Parent.Selection.SelectedVfsFile;

        if (fileEntry == null)
        {
            Smithbox.LogError<FileExtractTool>("No main file has been selected.");
            return;
        }

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

                Smithbox.Log(this, $"[File Browser] Extracted {absPath}");

                data = null;
                rawData = null;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[File Browser] Failed to write file: {fileEntry.Path}", LogPriority.High, e);
        }
    }

    public void ExtractInternalFile()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExtractTool>("Output directory has not been set.");
            return;
        }

        if (Parent.Selection.SelectedVfsFile == null)
        {
            Smithbox.LogError<FileExtractTool>("No main file has been selected.");
            return;
        }

        if (Parent.Selection.SelectedInternalFile == "")
        {
            Smithbox.LogError<FileExtractTool>("No internal file has been selected.");
            return;
        }

        var targetFile = Parent.Selection.SelectedVfsFile;
        var internalFile = Parent.Selection.SelectedInternalFile;

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
                        if (Parent.Selection.SelectedInternalTexFile == entry.Name)
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
                            if (file.Name.ToLower() == internalFile)
                            {
                                extractData = binder.ReadFile(file).ToArray();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
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
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
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
                Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
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
                        }
                    }
                }
            }
        }

        if (extractData.Length > 0)
        {
            var extension = "";
            var filename = Path.GetFileName(internalFile);

            var writePath = Path.Combine(ExtractionPath, filename);

            if (extension != "")
            {
                writePath = Path.Combine(ExtractionPath, $"{filename}{extension}");
            }

            File.WriteAllBytes(writePath, extractData);

            Smithbox.Log(this, $"[Smithbox:File Browser] Extracted {filename}");
        }
    }


    public void ExtractTextureFile()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExtractTool>("Output directory has not been set.");
            return;
        }

        if (Parent.Selection.SelectedVfsFile == null)
        {
            Smithbox.LogError<FileExtractTool>("No main file has been selected.");
            return;
        }

        if (Parent.Selection.SelectedInternalFile == "" || Parent.Selection.SelectedInternalTexFile == "")
        {
            Smithbox.LogError<FileExtractTool>("No internal file has been selected.");
            return;
        }

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
                        if (Parent.Selection.SelectedInternalTexFile == entry.Name)
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
                    Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
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
                    Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
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
                Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[File Browser] Failed to read {targetFile.Path}.", LogPriority.High, e);
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

        if (extractData.Length > 0)
        {
            var extension = "";
            var filename = Path.GetFileName(internalFile);
            if (internalTexFile != "")
            {
                filename = Path.GetFileName(internalTexFile);
                extension = ".dds";
            }

            var writePath = Path.Combine(ExtractionPath, filename);

            if (extension != "")
            {
                writePath = Path.Combine(ExtractionPath, $"{filename}{extension}");
            }

            File.WriteAllBytes(writePath, extractData);

            Smithbox.Log(this, $"[Smithbox:File Browser] Extracted {filename}");
        }
    }
}
