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
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public MapModelInsightView(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader("Model Insight"))
        {
            var curEntity = Editor.ViewportSelection.GetSelection().FirstOrDefault();

            if (curEntity == null)
                return;

            var mapEntity = (Entity)curEntity;

            var curModelData = MapModelInsightHelper.Entries.FirstOrDefault(e => e.Key == Editor.Selection.SelectedMapID);

            if (curModelData.Value != null && curModelData.Value != MapModelInsightHelper.SelectedDataEntry)
            {
                MapModelInsightHelper.SelectedDataEntry = curModelData.Value;
            }

            if (MapModelInsightHelper.SelectedDataEntry == null)
                return;

            var propValue = mapEntity.GetPropertyValue("ModelName");

            if (propValue == null)
                return;

            var modelName = propValue.ToString();

            var flverEntry = MapModelInsightHelper.SelectedDataEntry.Models.FirstOrDefault(
            e => e.Name == modelName);

            if (flverEntry != null && flverEntry != MapModelInsightHelper.SelectedFlverEntry)
            {
                MapModelInsightHelper.SelectedFlverEntry = flverEntry;
            }

            Display();
        }
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (MapModelInsightHelper.SelectedFlverEntry != null)
        {
            var entry = MapModelInsightHelper.SelectedFlverEntry;

            UIHelper.SimpleHeader("actHeader", "Actions", "", UI.Current.ImGui_AliasName_Text);

            var outputDirectory = Path.Combine(Project.Descriptor.ProjectPath, CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (ImGui.Button($"{Icons.EnvelopeOpen}##openOutputDir", DPI.IconButtonSize))
            {
                StudioCore.Common.FileExplorer.Start(outputDirectory);
            }

            ImGui.SameLine();

            ImGui.Text($"Output Directory: {outputDirectory}");

            ImGui.Separator();

            if (ImGui.BeginCombo("Extraction Type##extractTypeSelect", CFG.Current.MapEditor_ModelDataExtraction_Type.GetDisplayName()))
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
                UIHelper.Tooltip("Files when extracted will be extracted loose and outside of the container files they normally reside in.");
            }
            if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Contained)
            {
                UIHelper.Tooltip("Files when extracted will be contained with the container files they belong to.");
            }

            ImGui.Checkbox("Include Folder", ref CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder);
            UIHelper.Tooltip("If enabled, a folder will be created to contain the files, titled with the name of the FLVER model.");

            ImGui.Separator();

            if (ImGui.Button("Extract FLVER", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                ExtractFLVER(Project, entry, outputDirectory);
            }
            UIHelper.Tooltip("Extract the FLVER model for the current selection.");

            ImGui.SameLine();

            if (ImGui.Button("Extract DDS", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                ExtractDDS(Project, entry, outputDirectory);
            }
            UIHelper.Tooltip("Extract the DDS texture files for the current selection.");

            ImGui.SameLine();

            if (ImGui.Button("Extract Materials", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                ExtractMaterial(Project, entry, outputDirectory);
            }
            UIHelper.Tooltip("Extract the MTD or MATBIN that the textures are linked to for this current selection.");

            UIHelper.SimpleHeader("flverHeader", "FLVER", "", UI.Current.ImGui_AliasName_Text);

            ImGui.Text($"Name: {entry.Name}");
            ImGui.Text($"Virtual Path: {entry.VirtualPath}");

            if (ResourceManager.ResourceDatabase.ContainsKey(entry.VirtualPath))
            {
                var listener = ResourceManager.ResourceDatabase[entry.VirtualPath];

                if (listener.IsLoaded())
                {
                    UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
                }
                else
                {
                    UIHelper.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, "UNLOADED");
                }
            }

            UIHelper.SimpleHeader("texHeader", "Textures", "", UI.Current.ImGui_AliasName_Text);

            foreach (var tex in entry.Entries)
            {
                ImGui.Text($"Name: {tex.Name}");
                ImGui.Text($"Virtual Path: {tex.VirtualPath}");

                if (ResourceManager.ResourceDatabase.ContainsKey(tex.VirtualPath))
                {
                    var listener = ResourceManager.ResourceDatabase[tex.VirtualPath];

                    if (listener.IsLoaded())
                    {
                        UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
                    }
                    else
                    {
                        UIHelper.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, "UNLOADED");
                    }
                }

                ImGui.Separator();
            }
        }
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
            TaskLogs.AddLog($"Failed to read {relativePath}");
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
                            File.WriteAllBytes(writePath, virtFile.GetData().ToArray());
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
                            File.WriteAllBytes(writePath, virtFile.GetData().ToArray());
                            successful = true;

                            break;
                        }
                    }
                }
            }
        }

        if (successful)
        {
            TaskLogs.AddLog("Model extraction complete.");
        }
        else
        {
            TaskLogs.AddLog("Could not complete model extraction.");
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
            TaskLogs.AddLog("Material extraction complete.");
        }
        else
        {
            TaskLogs.AddLog("Could not complete material extraction.");
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
            TaskLogs.AddLog("Texture extraction complete.");
        }
        else
        {
            TaskLogs.AddLog("Could not complete texture extraction.");
        }
    }
}
