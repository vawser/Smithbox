using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Diagnostics;
using static Andre.IO.VFS.VirtualFileSystem;

namespace StudioCore.Editors.FileBrowser;

public class FileExporterTool
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    private string ExtractionPath = "";

    public FileExporterTool(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        ExtractionPath = project.Descriptor.ProjectPath;
        if (CFG.Current.ExtractDirectory != "")
            ExtractionPath = CFG.Current.ExtractDirectory;
    }

    public void Display()
    {
        ImGui.BeginChild("FileExtractSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("FILE_Exporter_Hint"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileExporter_Export_Directory_Header"),
            LOC.Get("FILE_FileExporter_Export_Directory_Header_TT"));

        GUI.HintTextInput("##outputDir", ref ExtractionPath, LOC.Get("FILE_FileExporter_Export_Dir_Hint"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileExporter_Actions_Header"),
            LOC.Get("FILE_FileExporter_Actions_Header_TT"));

        GUI.MultiButtonInput("extractActions",
            "setExtractDir", 
            LOC.Get("FILE_FileExporter_Set_Export_Dir_Action"),
            LOC.Get("FILE_FileExporter_Set_Export_Dir_Action_TT"),
            SetExtractionDirectory,

            "openExtractDir",
            LOC.Get("FILE_FileExporter_Open_Export_Dir_Action"),
            LOC.Get("FILE_FileExporter_Open_Export_Dir_Action_TT"), 
            OpenExtractionDirectory,

             "extractFolder",
            LOC.Get("FILE_FileExporter_Export_Folder"),
            LOC.Get("FILE_FileExporter_Export_Folder_TT"),
            ExportSelectedFolder,

            "extractMainFile",
            LOC.Get("FILE_FileExporter_Export_Container_File"),
            LOC.Get("FILE_FileExporter_Export_Container_File_TT"), 
            ExportContainerFile,

            "extractInternalFile",
            LOC.Get("FILE_FileExporter_Export_Internal_File"),
            LOC.Get("FILE_FileExporter_Export_Internal_File_TT"), 
            ExportInternalFile,

            "extractTextureFile",
            LOC.Get("FILE_FileExporter_Export_Texture_File"),
            LOC.Get("FILE_FileExporter_Export_Texture_File_TT"), 
            ExportTextureFile);

        ImGui.EndChild();
    }

    public void OpenExtractionDirectory()
    {
        Process.Start("explorer.exe", ExtractionPath);
    }

    public void SetExtractionDirectory()
    {
        var newOutputDir = "";
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("FILE_FileExporter_Select_Output_Dir"), out newOutputDir, ExtractionPath);

        if (result)
        {
            ExtractionPath = newOutputDir;
            CFG.Current.ExtractDirectory = newOutputDir;
        }
    }

    public void ExportContainerFile()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExporterTool>("Output directory has not been set.");
            return;
        }

        var fileEntry = Parent.Selection.SelectedVfsFile;

        if (fileEntry == null)
        {
            Smithbox.LogError<FileExporterTool>("No main file has been selected.");
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

    public void ExportInternalFile()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Output_Dir"));
            return;
        }

        if (Parent.Selection.SelectedVfsFile == null)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Container_File_Selected"));
            return;
        }

        if (Parent.Selection.SelectedInternalFile == "")
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Internal_File_Selected"));
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
                    Smithbox.LogError(this,  LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
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
                    Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
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
                Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
            }

            try
            {
                bdt = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
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

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Exported_File", filename));
        }
    }

    public void ExportTextureFile()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Output_Dir"));
            return;
        }

        if (Parent.Selection.SelectedVfsFile == null)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Container_File_Selected"));
            return;
        }

        if (Parent.Selection.SelectedInternalTexFile == null)
        {
            if (Parent.Selection.SelectedInternalFile == "" || Parent.Selection.SelectedInternalTexFile == "")
            {
                Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Internal_File_Selected"));
                return;
            }
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

        if (internalFile != "" && binderType is ResourceContainerType.BND)
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
                    Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
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
                    Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
                }
            }
        }

        if (internalFile != "" && binderType is ResourceContainerType.BXF)
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
                Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
            }

            try
            {
                bdt = (Memory<byte>)Project.VFS.VanillaFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
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
            var filename = "test";

            if(internalFile != "")
                filename = Path.GetFileName(internalFile);

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

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Exported_File", filename));
        }
    }

    private bool IsFolderExporting = false;

    private enum FileExportResult
    {
        Exported,
        Skipped,
        Failed
    }

    public void ExportSelectedFolder()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Output_Dir"));
            return;
        }

        var folder = Parent.Selection.SelectedVfsFolder;

        if (folder == null)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Folder_Selected"));
            return;
        }

        if (IsFolderExporting)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Folder_Export_In_Progress"));
            return;
        }

        var files = new List<FileDictionaryEntry>();
        CollectFolderFiles(folder, files);

        if (files.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Folder_Export_No_Files", folder.Name));

            return;
        }

        IsFolderExporting = true;

        var unpackPath = ExtractionPath;
        var folderName = folder.Name;

        Task.Run(() =>
        {
            var exported = 0;
            var skipped = 0;
            var failed = 0;

            try
            {
                foreach (var fileEntry in files)
                {
                    switch (ExtractVfsFile(fileEntry, unpackPath))
                    {
                        case FileExportResult.Exported: exported++; break;
                        case FileExportResult.Skipped: skipped++; break;
                        case FileExportResult.Failed: failed++; break;
                    }
                }
            }
            finally
            {
                IsFolderExporting = false;
            }

            Smithbox.Log(this,
                LOC.Get("FILE_FileExporter_Folder_Export_Complete",
                folderName, exported, skipped, failed));
        });
    }

    private void CollectFolderFiles(FolderNode node, List<FileDictionaryEntry> results)
    {
        results.AddRange(node.Files);

        foreach (var child in node.Children)
        {
            CollectFolderFiles(child, results);
        }
    }

    private FileExportResult ExtractVfsFile(FileDictionaryEntry fileEntry, string unpackPath)
    {
        try
        {
            var data = Project.VFS.VanillaFS.ReadFile(fileEntry.Path);

            if (data == null)
            {
                Smithbox.LogError<FileExporterTool>(
                    LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Read_FAIL",
                    fileEntry.Path));

                return FileExportResult.Failed;
            }

            var absFolder = $@"{unpackPath}/{fileEntry.Folder}";
            var absPath = $@"{unpackPath}/{fileEntry.Path}";

            // Matches ExportContainerFile: never overwrite an existing file
            if (File.Exists(absPath))
                return FileExportResult.Skipped;

            Directory.CreateDirectory(absFolder);
            File.WriteAllBytes(absPath, data.Value.ToArray());

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Extracted", absPath));

            return FileExportResult.Exported;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Write_FAIL", fileEntry.Path), LogPriority.High, e);

            return FileExportResult.Failed;
        }
    }
}
