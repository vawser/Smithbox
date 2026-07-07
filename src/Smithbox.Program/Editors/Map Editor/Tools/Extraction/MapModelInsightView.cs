using Andre.IO.VFS;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapModelInsightView
{
    public MapEditorView View;
    public ProjectEntry Project;

    public MapModelInsightView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        var selection = View.ViewportSelection.GetSelection();

        if (selection == null)
        {
            ImGui.TextWrapped("No model selected 1.");
            return;
        }

        if (selection.Count == 0)
        {
            ImGui.TextWrapped("No model selected 2.");
            return;
        }

        var curEntity = View.ViewportSelection.GetSelection().FirstOrDefault();

        if (curEntity == null)
        {
            ImGui.TextWrapped("No model selected 3.");
            return;
        }

        var mapEntity = (Entity)curEntity;

        var curModelData = View.ModelInsightTool.Entries.FirstOrDefault(e => e.Key == View.Selection.SelectedMapID);

        if (curModelData.Value != null && curModelData.Value != View.ModelInsightTool.SelectedDataEntry)
        {
            View.ModelInsightTool.SelectedDataEntry = curModelData.Value;
        }

        if (View.ModelInsightTool.SelectedDataEntry == null)
        {
            ImGui.TextWrapped("No model selected 4.");
            return;
        }

        var propValue = mapEntity.GetPropertyValue("ModelName");

        if (propValue == null)
        {
            ImGui.TextWrapped("No model selected 5.");
            return;
        }

        var modelName = propValue.ToString();

        var flverEntry = View.ModelInsightTool.SelectedDataEntry.Models.FirstOrDefault(
        e => e.Name == modelName);

        if (flverEntry != null && flverEntry != View.ModelInsightTool.SelectedFlverEntry)
        {
            View.ModelInsightTool.SelectedFlverEntry = flverEntry;
        }

        Display();
    }

    public void Display()
    {
        if (View.ModelInsightTool.SelectedFlverEntry == null)
            return;

        var entry = View.ModelInsightTool.SelectedFlverEntry;

        GUI.WrappedText("Use this to extract the currently selected model and/or textures.");

        GUI.Spacer();
        GUI.SimpleHeader("Export Directory", "The directory to export the model/texture data to.");
        GUI.SinglelineTextInput("extractDir", ref CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder);

        GUI.MultiButtonInput("extractDirActions",
            "setDirectory", "Set Export Directory", "", SetExportDirectory,
            "openDirectory", "Open Export Directory", "", OpenExportDirectory);

        GUI.Spacer();
        GUI.SimpleHeader("Extraction Type", "");

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##extractTypeSelect", CFG.Current.MapEditor_ModelDataExtraction_Type.GetDisplayName()))
        {
            foreach (var typ in Enum.GetValues(typeof(ResourceExtractionType)))
            {
                var curEntry = (ResourceExtractionType)typ;

                if (ImGui.Selectable(curEntry.GetDisplayName()))
                {
                    CFG.Current.MapEditor_ModelDataExtraction_Type = (ResourceExtractionType)typ;
                }
            }

            ImGui.EndCombo();
        }
        if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Loose)
        {
            GUI.Tooltip("Files when extracted will be extracted loose and outside of the container files they normally reside in.");
        }
        if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Contained)
        {
            GUI.Tooltip("Files when extracted will be contained with the container files they belong to.");
        }

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        ImGui.Checkbox("Include Folder", ref CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder);
        GUI.Tooltip("If enabled, a folder will be created to contain the files, titled with the name of the FLVER model.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("extractActions",
            "extractModel", "Extract Model", "Extract the FLVER model for the current selection.", ExtractFlverAction,
            "extractTexture", "Extract Textures", "Extract the DDS texture files for the current selection.", ExtractTextureAction,
            "extractMaterial", "Extract Materials", "Extract the MTD or MATBIN that the textures are linked to for this current selection.", ExtractMaterialAction);

        DisplaySelectionInfo();
    }

    public void DisplaySelectionInfo()
    {
        var entry = View.ModelInsightTool.SelectedFlverEntry;

        GUI.Spacer();
        GUI.SimpleHeader("flverHeader", "Model", "", UI.Current.ImGui_AliasName_Text);

        ImGui.Text($"Name: {entry.Name}");
        ImGui.Text($"Virtual Path: {entry.VirtualPath}");

        if (ResourceManager.ResourceDatabase.ContainsKey(entry.VirtualPath))
        {
            var listener = ResourceManager.ResourceDatabase[entry.VirtualPath];

            if (listener.IsLoaded())
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
            }
            else
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, "UNLOADED");
            }
        }

        GUI.Spacer();
        GUI.SimpleHeader("texHeader", "Textures", "", UI.Current.ImGui_AliasName_Text);

        foreach (var tex in entry.Entries)
        {
            ImGui.Text($"Name: {tex.Name}");
            ImGui.Text($"Virtual Path: {tex.VirtualPath}");

            if (ResourceManager.ResourceDatabase.ContainsKey(tex.VirtualPath))
            {
                var listener = ResourceManager.ResourceDatabase[tex.VirtualPath];

                if (listener.IsLoaded())
                {
                    GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
                }
                else
                {
                    GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, "UNLOADED");
                }
            }

            ImGui.Separator();
        }
    }

    public void ExtractFlverAction()
    {
        var entry = View.ModelInsightTool.SelectedFlverEntry;
        var outputFolder = CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder;

        if(outputFolder == "")
        {
            Smithbox.LogError<MapModelInsightView>("Export directory is empty.");
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Smithbox.LogError<MapModelInsightView>("Export directory is invalid.");
            return;
        }

        ExtractFLVER(Project, entry, outputFolder);
    }

    public void ExtractTextureAction()
    {
        var entry = View.ModelInsightTool.SelectedFlverEntry;
        var outputFolder = CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder;

        if (outputFolder == "")
        {
            Smithbox.LogError<MapModelInsightView>("Export directory is empty.");
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Smithbox.LogError<MapModelInsightView>("Export directory is invalid.");
            return;
        }

        ExtractDDS(Project, entry, outputFolder);
    }

    public void ExtractMaterialAction()
    {
        var entry = View.ModelInsightTool.SelectedFlverEntry;
        var outputFolder = CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder;

        if (outputFolder == "")
        {
            Smithbox.LogError<MapModelInsightView>("Export directory is empty.");
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Smithbox.LogError<MapModelInsightView>("Export directory is invalid.");
            return;
        }

        ExtractMaterial(Project, entry, outputFolder);
    }

    public void SetExportDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
        if (result)
        {
            CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder = path;
        }
    }

    public void OpenExportDirectory()
    {
        Process.Start("explorer.exe", CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder);
    }

    public void ExtractFLVER(ProjectEntry project, MapFlverInsightEntry entry, string outputDirectory)
    {
        var successful = false;

        var relativePath = PathBuilder.GetRelativePath(project, entry.VirtualPath);

        var fileName = Path.GetFileName(relativePath);

        VirtualFile virtFile;
        var readFile = project.VFS.FS.TryGetFile(relativePath, out virtFile);

        if (!readFile)
        {
            Smithbox.Log(this, $"Failed to read {relativePath}");
            return;
        }

        var writeDir = outputDirectory;

        if (CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder)
        {
            writeDir = Path.Combine(outputDirectory, entry.Name);

            if (!Directory.Exists(writeDir))
            {
                Directory.CreateDirectory(writeDir);
            }
        }

        if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Contained)
        {
            var writePath = Path.Combine(writeDir, fileName);
            File.WriteAllBytes(writePath, virtFile.GetData().ToArray());
        }

        if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Loose)
        {
            var containerType = ModelEditorUtils.GetContainerTypeFromVirtualPath(project, entry.VirtualPath);

            if (containerType is ResourceContainerType.None)
            {
                var writePath = Path.Combine(writeDir, fileName);
                File.WriteAllBytes(writePath, virtFile.GetData().ToArray());
            }

            if (containerType is ResourceContainerType.BND)
            {
                if (project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                {
                    var reader = new BND3Reader(virtFile.GetData());
                    foreach (var file in reader.Files)
                    {
                        if (file.Name.Contains(entry.Name))
                        {
                            var fileData = reader.ReadFile(file);

                            var rawFileName = Path.GetFileName(file.Name);
                            var writePath = Path.Combine(writeDir, rawFileName);
                            File.WriteAllBytes(writePath, fileData.ToArray());
                            successful = true;

                            break;
                        }
                    }
                }
                else
                {
                    var reader = new BND4Reader(virtFile.GetData());
                    foreach (var file in reader.Files)
                    {
                        if (file.Name.Contains(entry.Name))
                        {
                            var fileData = reader.ReadFile(file);

                            var rawFileName = Path.GetFileName(file.Name);
                            var writePath = Path.Combine(writeDir, rawFileName);
                            File.WriteAllBytes(writePath, fileData.ToArray());
                            successful = true;

                            break;
                        }
                    }
                }
            }
        }

        if (successful)
        {
            Smithbox.Log(this, "Model extraction complete.");
        }
        else
        {
            Smithbox.Log(this, "Could not complete model extraction.");
        }
    }

    public void ExtractMaterial(ProjectEntry project, MapFlverInsightEntry entry, string outputDirectory)
    {
        var successful = false;

        foreach (var tex in entry.Entries)
        {
            var mtd = tex.MTD;
            var matbin = tex.MATBIN;

            var filename = Path.GetFileNameWithoutExtension(tex.MaterialString);

            var writeDir = outputDirectory;

            if (CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder)
            {
                writeDir = Path.Combine(outputDirectory, entry.Name);

                if (!Directory.Exists(writeDir))
                {
                    Directory.CreateDirectory(writeDir);
                }
            }

            if (project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                if (matbin != null)
                {
                    var data = matbin.Write();

                    var writePath = Path.Combine(writeDir, $"{filename}.matbin");
                    File.WriteAllBytes(writePath, data);
                    successful = true;
                }
            }
            else
            {
                if (mtd != null)
                {
                    var data = mtd.Write();

                    var writePath = Path.Combine(writeDir, $"{filename}.mtd");
                    File.WriteAllBytes(writePath, data);
                    successful = true;
                }
            }
        }

        if (successful)
        {
            Smithbox.Log(this, "Material extraction complete.");
        }
        else
        {
            Smithbox.Log(this, "Could not complete material extraction.");
        }
    }

    public void ExtractDDS(ProjectEntry project, MapFlverInsightEntry entry, string outputDirectory)
    {
        var successful = false;

        // So we don't write the same container multiple times
        List<string> writtenFiles = new();

        foreach (var tex in entry.Entries)
        {
            var relativePath = PathBuilder.GetRelativePath(project, tex.VirtualPath);

            var fileName = Path.GetFileName(relativePath);
            var fileData = project.VFS.FS.ReadFile(relativePath);

            if (fileData == null)
                continue;

            var writeDir = outputDirectory;

            if (CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder)
            {
                writeDir = Path.Combine(outputDirectory, entry.Name);

                if (!Directory.Exists(writeDir))
                {
                    Directory.CreateDirectory(writeDir);
                }
            }

            if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Contained)
            {
                if (!writtenFiles.Contains(fileName))
                {
                    writtenFiles.Add(fileName);

                    var writePath = Path.Combine(writeDir, fileName);
                    File.WriteAllBytes(writePath, fileData.Value.ToArray());
                    successful = true;
                }
            }

            if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Loose)
            {
                var containerType = ModelEditorUtils.GetContainerTypeFromVirtualPath(project, tex.VirtualPath);

                // TPF
                if (containerType is ResourceContainerType.None)
                {
                    TPF tpf = TPF.Read(fileData.Value);

                    foreach (var tpfTex in tpf.Textures)
                    {
                        if (tpfTex.Name.ToLower() == tex.Name)
                        {
                            var data = tpfTex.Bytes;

                            var writePath = Path.Combine(writeDir, $"{tpfTex.Name}.dds");
                            File.WriteAllBytes(writePath, data.ToArray());
                            successful = true;

                            break;
                        }
                    }
                }

                // TPFBND
                if (containerType is ResourceContainerType.BND)
                {
                    if (project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                    {
                        var reader = new BND3Reader(fileData.Value);
                        foreach (var file in reader.Files)
                        {
                            var filename = file.Name.ToLower();

                            if (filename.Contains(".tpf") || filename.Contains(".tpf.dcx"))
                            {
                                fileData = reader.ReadFile(file);

                                TPF tpf = TPF.Read(fileData.Value);

                                foreach (var tpfTex in tpf.Textures)
                                {
                                    if (tpfTex.Name.ToLower() == tex.Name)
                                    {
                                        var data = tpfTex.Bytes;

                                        var writePath = Path.Combine(writeDir, $"{tpfTex.Name}.dds");
                                        File.WriteAllBytes(writePath, data.ToArray());
                                        successful = true;

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        var reader = new BND4Reader(fileData.Value);
                        foreach (var file in reader.Files)
                        {
                            var filename = file.Name.ToLower();

                            if (filename.Contains(".tpf") || filename.Contains(".tpf.dcx"))
                            {
                                fileData = reader.ReadFile(file);

                                TPF tpf = TPF.Read(fileData.Value);

                                foreach (var tpfTex in tpf.Textures)
                                {
                                    if (tpfTex.Name.ToLower() == tex.Name)
                                    {
                                        var data = tpfTex.Bytes;

                                        var writePath = Path.Combine(writeDir, $"{tpfTex.Name}.dds");
                                        File.WriteAllBytes(writePath, data.ToArray());
                                        successful = true;

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        if (successful)
        {
            Smithbox.Log(this, "Texture extraction complete.");
        }
        else
        {
            Smithbox.Log(this, "Could not complete texture extraction.");
        }
    }
}
