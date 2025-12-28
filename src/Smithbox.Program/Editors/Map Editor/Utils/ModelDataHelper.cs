using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public static class ModelDataHelper
{
    public static Dictionary<string, ModelDataEntry> Entries { get; set; } = new Dictionary<string, ModelDataEntry>();

    public static ModelDataEntry SelectedDataEntry { get; set; }
    public static FlverDataEntry SelectedFlverEntry { get; set; }

    public static void AddEntry(MapEditorScreen editor, MapContainer map)
    {
        if(!Entries.ContainsKey(map.Name))
        {
            Entries.Add(map.Name, new ModelDataEntry(map.Name, map));
        }
    }

    public static void ClearEntry(MapEditorScreen editor, MapContainer map)
    {
        if (Entries.ContainsKey(map.Name))
        {
            Entries.Remove(map.Name);
        }
    }

    public static void UpdateEntry(string flverVirtPath, string texVirtPath, IFlver flver, MTD mtd, MATBIN matbin, string materialStr)
    {
        var project = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        if (project == null)
            return;

        if (project.MapEditor == null)
            return;

        var flverName = Path.GetFileNameWithoutExtension(flverVirtPath);
        var textureName = Path.GetFileName(texVirtPath);

        if (project.MapEditor.Universe.ModelDataMapID == null)
            return;

        if (Entries.ContainsKey(project.MapEditor.Universe.ModelDataMapID))
        {
            var entry = Entries[project.MapEditor.Universe.ModelDataMapID];

            if(!entry.Models.Any(e => e.Name == flverName))
            {
                var newModel = new FlverDataEntry();
                newModel.Name = flverName;
                newModel.VirtualPath = flverVirtPath.ToLower();
                newModel.FLVER2 = (FLVER2)flver;

                entry.Models.Add(newModel);
            }

            if (entry.Models.Any(e => e.Name == flverName))
            {
                var modelEntry = entry.Models.FirstOrDefault(e => e.Name == flverName);

                if(modelEntry != null)
                {
                    if(!modelEntry.Entries.Any(e => e.Name == textureName))
                    {
                        var newTexture = new TextureDataEntry();
                        newTexture.Name = textureName;
                        newTexture.VirtualPath = texVirtPath.ToLower();
                        newTexture.MTD = mtd;
                        newTexture.MATBIN = matbin;
                        newTexture.MaterialString = materialStr;

                        modelEntry.Entries.Add(newTexture);
                    }
                }
            }
        }
    }

    public static void ExtractFLVER(ProjectEntry project, FlverDataEntry entry, string outputDirectory)
    {
        var successful = false;

        var relativePath = ResourceLocator.GetRelativePath(project, entry.VirtualPath);

        var fileName = Path.GetFileName(relativePath);
        var fileData = project.FS.ReadFile(relativePath);

        var writeDir = outputDirectory;

        if(CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder)
        {
            writeDir = Path.Combine(outputDirectory, entry.Name);

            if(!Directory.Exists(writeDir))
            {
                Directory.CreateDirectory(writeDir);
            }
        }

        if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Contained)
        {
            var writePath = Path.Combine(writeDir, fileName);
            File.WriteAllBytes(writePath, fileData.Value.ToArray());
        }

        if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Loose)
        {
            var containerType = ModelEditorUtils.GetContainerTypeFromVirtualPath(project, entry.VirtualPath);

            if(containerType is ResourceContainerType.None)
            {
                var writePath = Path.Combine(writeDir, fileName);
                File.WriteAllBytes(writePath, fileData.Value.ToArray());
            }

            if (containerType is ResourceContainerType.BND)
            {
                if(project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                {
                    var reader = new BND3Reader(fileData.Value);
                    foreach(var file in reader.Files)
                    {
                        if(file.Name.Contains(entry.Name))
                        {
                            fileData = reader.ReadFile(file);

                            var rawFileName = Path.GetFileName(file.Name);
                            var writePath = Path.Combine(writeDir, rawFileName);
                            File.WriteAllBytes(writePath, fileData.Value.ToArray());
                            successful = true;

                            break;
                        }
                    }
                }
                else
                {
                    var reader = new BND4Reader(fileData.Value);
                    foreach (var file in reader.Files)
                    {
                        if (file.Name.Contains(entry.Name))
                        {
                            fileData = reader.ReadFile(file);

                            var rawFileName = Path.GetFileName(file.Name);
                            var writePath = Path.Combine(writeDir, rawFileName);
                            File.WriteAllBytes(writePath, fileData.Value.ToArray());
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

    public static void ExtractMaterial(ProjectEntry project, FlverDataEntry entry, string outputDirectory)
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

            if(project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
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

    public static void ExtractDDS(ProjectEntry project, FlverDataEntry entry, string outputDirectory)
    {
        var successful = false;

        // So we don't write the same container multiple times
        List<string> writtenFiles = new();

        foreach (var tex in entry.Entries)
        {
            var relativePath = ResourceLocator.GetRelativePath(project, tex.VirtualPath);

            var fileName = Path.GetFileName(relativePath);
            var fileData = project.FS.ReadFile(relativePath);

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
                    if (project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                    {
                        var reader = new BND3Reader(fileData.Value);
                        foreach (var file in reader.Files)
                        {
                            var filename = file.Name.ToLower();

                            if (filename.Contains(".tpf") || filename.Contains(".tpf.dcx"))
                            {
                                fileData = reader.ReadFile(file);

                                TPF tpf = TPF.Read(fileData.Value);

                                foreach(var tpfTex in tpf.Textures)
                                {
                                    if(tpfTex.Name.ToLower() == tex.Name)
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

public class ModelDataEntry
{
    public string MapID { get; set; }
    public MapContainer Map { get; set; }

    public List<FlverDataEntry> Models { get; set; } = new();

    public ModelDataEntry(string mapID, MapContainer map)
    {
        MapID = mapID;
        Map = map;
    }
}

public class FlverDataEntry
{
    public string Name { get; set; }

    public string VirtualPath { get; set; }

    public FLVER2 FLVER2 { get; set; }

    public List<TextureDataEntry> Entries { get; set; } = new();
}

public class TextureDataEntry
{
    public string Name { get; set; }

    public string VirtualPath { get; set; }

    public string MaterialString { get; set; }

    public MTD MTD { get; set; }
    public MATBIN MATBIN { get; set; }
}

