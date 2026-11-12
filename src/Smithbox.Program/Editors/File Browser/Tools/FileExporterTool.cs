using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.TextureViewer;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Diagnostics;

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
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            CFG.Current.ExtractDirectory = ExtractionPath;
        }

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileExporter_Options_Header"),
            LOC.Get("FILE_FileExporter_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("FILE_FileExporter_Overwrite")}##exportOverwrite", ref CFG.Current.FileBrowser_Export_Overwrite);
        GUI.Tooltip(LOC.Get("FILE_FileExporter_Overwrite_TT"));

        ImGui.Checkbox($"{LOC.Get("FILE_FileExporter_Keep_Container_Path")}##exportKeepPath", ref CFG.Current.FileBrowser_Export_Keep_Container_Path);
        GUI.Tooltip(LOC.Get("FILE_FileExporter_Keep_Container_Path_TT"));

        ImGui.Checkbox($"{LOC.Get("FILE_FileExporter_Decompress")}##exportDecompress", ref CFG.Current.FileBrowser_Export_Decompress);
        GUI.Tooltip(LOC.Get("FILE_FileExporter_Decompress_TT"));

        // Texture format, as in the Texture Viewer's export
        var textureType = CFG.Current.FileBrowser_Export_Texture_Type;
        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo($"{LOC.Get("FILE_FileExporter_Texture_Format")}##exportTextureType", LOC.Get(textureType.GetDisplayName())))
        {
            foreach (TextureExportType type in Enum.GetValues(typeof(TextureExportType)))
            {
                if (ImGui.Selectable(LOC.Get(type.GetDisplayName()), type == textureType))
                    CFG.Current.FileBrowser_Export_Texture_Type = type;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("FILE_FileExporter_Texture_Format_TT"));

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

        GUI.MultiButtonInput("batchExtractActions",
            "extractSelectedFiles",
            LOC.Get("FILE_FileExporter_Export_Selected_Files", Parent.Selection.SelectedPaths.Count),
            LOC.Get("FILE_FileExporter_Export_Selected_Files_TT"),
            ExportSelectedFiles,

            "extractSelectedInternal",
            LOC.Get("FILE_FileExporter_Export_Selected_Internal", Parent.Selection.SelectedInternalKeys.Count),
            LOC.Get("FILE_FileExporter_Export_Selected_Internal_TT"),
            ExportSelectedInternalFiles,

            "extractUnpacked",
            LOC.Get("FILE_FileExporter_Export_Unpacked"),
            LOC.Get("FILE_FileExporter_Export_Unpacked_TT"),
            ExportContainerUnpacked);

        ImGui.EndChild();
    }

    public void OpenExtractionDirectory()
    {
        FileBrowserScreen.OpenInExplorer(ExtractionPath);
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

        var paths = new List<string> { fileEntry.Path };

        // A .bhd is useless without its .bdt, so a split binder is exported as the pair
        var bdtPath = IsBhd(fileEntry.Path) ? FileContainerReader.GetBdtPath(fileEntry.Path) : null;
        if (bdtPath != null)
            paths.Add(bdtPath);

        foreach (var path in paths)
        {
            var result = ExtractVfsPath(path, ExtractionPath, CFG.Current.FileBrowser_Export_Overwrite, logEach: false);

            var message = result switch
            {
                FileExportResult.Exported => LOC.Get("FILE_FileExporter_Exported_File", path),
                FileExportResult.Skipped => LOC.Get("FILE_FileExporter_Skipped_Existing", path),
                _ => null
            };

            if (message != null)
                Smithbox.Log(this, message);
        }
    }

    private static bool IsBhd(string path)
    {
        var name = Path.GetFileName(path).ToLowerInvariant();
        if (name.EndsWith(".dcx"))
            name = name[..^4];

        return name.EndsWith("bhd");
    }

    /// <summary>
    /// Opens the selected file as a container, logging why when it can't be
    /// </summary>
    private FileContainer OpenSelectedContainer()
    {
        var targetFile = Parent.Selection.SelectedVfsFile;

        try
        {
            var container = FileContainerReader.Open(Project.VFS.FS, targetFile.Path);

            if (container == null)
                Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Not_A_Container", targetFile.Path));

            return container;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", targetFile.Path), e);
            return null;
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

        var container = OpenSelectedContainer();
        if (container == null)
            return;

        ExportInternalByKey(container, Parent.Selection.SelectedInternalFile);
    }

    /// <summary>
    /// Exports every inner entry selected in the binder list (Ctrl-click)
    /// </summary>
    public void ExportSelectedInternalFiles()
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

        var keys = Parent.Selection.SelectedInternalKeys.ToList();
        if (keys.Count == 0)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Internal_File_Selected"));
            return;
        }

        var container = OpenSelectedContainer();
        if (container == null)
            return;

        foreach (var key in keys)
            ExportInternalByKey(container, key);
    }

    /// <summary>
    /// Exports one inner entry, found by its key (nested entries are read through their parent binders)
    /// </summary>
    private void ExportInternalByKey(FileContainer container, string key)
    {
        try
        {
            var data = FileContainerReader.ReadByKey(container, key);
            if (data == null)
            {
                Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Internal_File_Not_Found", key));
                return;
            }

            var folder = GetInnerFolder(Parent.Selection.SelectedVfsFile.Path, key);

            WriteToFolder(folder, Path.GetFileName(LastKeyPart(key)), data.Value);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", key), e);
        }
    }

    /// <summary>
    /// Writes every entry of the selected container (or every texture of a TPF) into a folder named after it, at
    /// the container's path: "/chr/c0000.chrbnd.dcx" -> "chr/c0000-chrbnd-dcx/". Nested binders are written as files.
    /// </summary>
    public void ExportContainerUnpacked()
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

        var container = OpenSelectedContainer();
        if (container == null)
            return;

        var folder = GetContainerFolder(Parent.Selection.SelectedVfsFile.Path);

        try
        {
            if (container.Kind is FileContainerKind.Tpf)
            {
                foreach (var texture in container.Tpf.Textures)
                    WriteTexture(folder, container.Tpf, texture);
            }
            else
            {
                foreach (var file in container.Files)
                    WriteToFolder(folder, Path.GetFileName(file.Name), file.Read());
            }

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Unpacked_Done", Parent.Selection.SelectedVfsFile.Path, folder));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", Parent.Selection.SelectedVfsFile.Path), e);
        }
    }

    private static string LastKeyPart(string key)
    {
        var index = key.LastIndexOf(FileContainerReader.KeySeparator);
        return index < 0 ? key : key[(index + 1)..];
    }

    /// <summary>
    /// The folder an exported container's contents go in: its path with the file name's dots as dashes
    /// </summary>
    private string GetContainerFolder(string containerPath)
    {
        var rel = containerPath.Replace('\\', '/').TrimStart('/');
        var relFolder = Path.GetDirectoryName(rel) ?? "";

        return Path.Combine(ExtractionPath, relFolder, Path.GetFileName(rel).Replace('.', '-'));
    }

    /// <summary>
    /// Where an inner entry or texture is written: the export directory, or with Keep Container Path the container's
    /// folder plus one folder per nested binder on the way ("c0000-chrtpfbhd/")
    /// </summary>
    private string GetInnerFolder(string containerPath, string key)
    {
        if (!CFG.Current.FileBrowser_Export_Keep_Container_Path)
            return ExtractionPath;

        var folder = GetContainerFolder(containerPath);

        if (!string.IsNullOrEmpty(key))
        {
            var parts = key.Split(FileContainerReader.KeySeparator);

            foreach (var parent in parts[..^1])
                folder = Path.Combine(folder, Path.GetFileName(parent).Replace('.', '-'));
        }

        return folder;
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

        // A texture must be selected: without one this used to export the whole inner file under its own name
        if (Parent.Selection.SelectedInternalTexFile == "")
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Texture_Selected"));
            return;
        }

        var internalFile = Parent.Selection.SelectedInternalFile;
        var internalTexFile = Parent.Selection.SelectedInternalTexFile;

        var container = OpenSelectedContainer();
        if (container == null)
            return;

        try
        {
            TPF tpf;

            if (container.Kind is FileContainerKind.Tpf)
            {
                tpf = container.Tpf;
            }
            else
            {
                var data = FileContainerReader.ReadByKey(container, internalFile);
                if (data == null || !LocatorUtils.IsTPF(LastKeyPart(internalFile)))
                {
                    Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Internal_File_Not_Found", internalFile));
                    return;
                }

                tpf = FileContainerReader.ReadTpf(data.Value);
            }

            var texture = tpf.Textures.FirstOrDefault(t => t.Name == internalTexFile);
            if (texture == null)
            {
                Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Texture_Not_Found", internalTexFile));
                return;
            }

            WriteTexture(GetInnerFolder(Parent.Selection.SelectedVfsFile.Path, internalFile), tpf, texture);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_Data_Failed_Read_File", internalTexFile), e);
        }
    }

    /// <summary>
    /// Writes one texture in the chosen format. Console textures are given a DDS header first (TexUtils), so the
    /// file is readable; other formats are decoded through DirectXTex.
    /// </summary>
    private void WriteTexture(string folder, TPF tpf, TPF.Texture texture)
    {
        var type = CFG.Current.FileBrowser_Export_Texture_Type;
        var name = Path.GetFileName(texture.Name);
        var basePath = Path.Combine(folder, name);
        var writePath = basePath + TexUtils.GetExportExtension(type);

        try
        {
            if (File.Exists(writePath) && !CFG.Current.FileBrowser_Export_Overwrite)
            {
                Smithbox.Log(this, LOC.Get("FILE_FileExporter_Skipped_Existing", writePath));
                return;
            }

            var dds = TexUtils.GetDdsBytes(tpf, texture);

            if (dds == null || dds.Length == 0)
            {
                Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Empty_File", name));
                return;
            }

            Directory.CreateDirectory(folder);

            // ExportImage appends the extension itself
            TexUtils.ExportImage(basePath, dds, type);

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Exported_File", writePath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Write_FAIL", writePath), LogPriority.High, e);
        }
    }

    /// <summary>
    /// Writes a file into folder. With Decompress DCX on, DCX data is written decompressed and loses its ".dcx".
    /// </summary>
    private void WriteToFolder(string folder, string filename, Memory<byte> data)
    {
        if (data.Length == 0)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Empty_File", filename));
            return;
        }

        if (CFG.Current.FileBrowser_Export_Decompress && DCX.Is(data))
        {
            data = DCX.Decompress(data);

            if (filename.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
                filename = filename[..^4];
        }

        var writePath = Path.Combine(folder, filename);

        try
        {
            if (File.Exists(writePath) && !CFG.Current.FileBrowser_Export_Overwrite)
            {
                Smithbox.Log(this, LOC.Get("FILE_FileExporter_Skipped_Existing", writePath));
                return;
            }

            Directory.CreateDirectory(folder);
            File.WriteAllBytes(writePath, data.ToArray());

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Exported_File", writePath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Write_FAIL", writePath), LogPriority.High, e);
        }
    }

    // Shared across views (FileBrowserScreen), so two views can't run folder exports at once
    private bool IsFolderExporting
    {
        get => Parent.Editor.IsFolderExporting;
        set => Parent.Editor.IsFolderExporting = value;
    }

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
        var overwrite = CFG.Current.FileBrowser_Export_Overwrite;

        Task.Run(() =>
        {
            var exported = 0;
            var skipped = 0;
            var failed = 0;

            try
            {
                foreach (var fileEntry in files)
                {
                    // Summary only: a large folder would otherwise log a line per file
                    switch (ExtractVfsPath(fileEntry.Path, unpackPath, overwrite, logEach: false))
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

    /// <summary>
    /// Exports every file selected in the File List (Ctrl/Shift-click) to its path under the export directory, in the
    /// background. Split binders bring their .bdt along, as with Export Container File.
    /// </summary>
    public void ExportSelectedFiles()
    {
        if (ExtractionPath == "")
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Output_Dir"));
            return;
        }

        if (IsFolderExporting)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_Folder_Export_In_Progress"));
            return;
        }

        var paths = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in Parent.Selection.SelectedPaths)
        {
            if (seen.Add(path))
                paths.Add(path);

            var bdt = IsBhd(path) ? FileContainerReader.GetBdtPath(path) : null;
            if (bdt != null && seen.Add(bdt))
                paths.Add(bdt);
        }

        if (paths.Count == 0)
        {
            Smithbox.LogError<FileExporterTool>(LOC.Get("FILE_FileExporter_No_Container_File_Selected"));
            return;
        }

        IsFolderExporting = true;

        var exportRoot = ExtractionPath;
        var overwrite = CFG.Current.FileBrowser_Export_Overwrite;

        Task.Run(() =>
        {
            var exported = 0;
            var skipped = 0;
            var failed = 0;

            try
            {
                foreach (var path in paths)
                {
                    switch (ExtractVfsPath(path, exportRoot, overwrite, logEach: false))
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

            Smithbox.Log(this, LOC.Get("FILE_FileExporter_Selected_Export_Complete", exported, skipped, failed));
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

    /// <summary>
    /// Copies one VFS file to the same relative path under exportRoot. Reads the merged VFS (project first).
    /// An existing file is skipped unless overwrite is set.
    /// </summary>
    private FileExportResult ExtractVfsPath(string relPath, string exportRoot, bool overwrite, bool logEach)
    {
        try
        {
            var outRel = relPath.Replace('\\', '/').TrimStart('/');

            // Decompress DCX: the written file drops ".dcx", so check for that name
            var decompress = CFG.Current.FileBrowser_Export_Decompress;
            if (decompress && outRel.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
                outRel = outRel[..^4];

            var absPath = Path.Combine(exportRoot, outRel);

            if (File.Exists(absPath) && !overwrite)
                return FileExportResult.Skipped;

            var data = Project.VFS.FS.ReadFile(relPath);

            if (data == null)
            {
                Smithbox.LogError<FileExporterTool>(
                    LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Read_FAIL", relPath));

                return FileExportResult.Failed;
            }

            var bytes = data.Value;
            if (decompress && DCX.Is(bytes))
                bytes = DCX.Decompress(bytes);

            Directory.CreateDirectory(Path.GetDirectoryName(absPath));
            File.WriteAllBytes(absPath, bytes.ToArray());

            if (logEach)
                Smithbox.Log(this, LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Extracted", absPath));

            return FileExportResult.Exported;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileExporter_Folder_Export_VFS_File_Write_FAIL", relPath), LogPriority.High, e);

            return FileExportResult.Failed;
        }
    }
}
