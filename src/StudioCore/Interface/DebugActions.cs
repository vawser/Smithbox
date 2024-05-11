using ImGuiNET;
using SoulsFormats;
using StudioCore.Formats;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;

public static class DebugActions
{
    public static void ForceCrash()
    {
        var badArray = new int[2];
        var crash = badArray[5];
    }

    public static void DumpFlverLayouts()
    {
        if (PlatformUtils.Instance.SaveFileDialog("Save Flver layout dump", new[] { FilterStrings.TxtFilter },
                 out var path))
        {
            using (StreamWriter file = new(path))
            {
                foreach (KeyValuePair<string, FLVER2.BufferLayout> mat in FlverResource.MaterialLayouts)
                {
                    file.WriteLine(mat.Key + ":");
                    foreach (FLVER.LayoutMember member in mat.Value)
                    {
                        file.WriteLine($@"{member.Index}: {member.Type.ToString()}: {member.Semantic.ToString()}");
                    }

                    file.WriteLine();
                }
            }
        }
    }

    public static void ReadShoeboxFile()
    {
        string sourcePath = $@"F:\\SteamLibrary\\steamapps\\common\\ELDEN RING\\Game\\menu\\hi\01_common.sblytbnd.dcx";

        ShoeboxLayoutContainer container = new ShoeboxLayoutContainer(sourcePath);
        foreach (var layout in container.Layouts)
        {
            foreach (var texAtlas in layout.Value.TextureAtlases)
            {
                foreach (var subText in texAtlas.SubTextures)
                {
                    TaskLogs.AddLog($"{subText.Name}");
                    TaskLogs.AddLog($"{subText.X}");
                    TaskLogs.AddLog($"{subText.Y}");
                    TaskLogs.AddLog($"{subText.Width}");
                    TaskLogs.AddLog($"{subText.Height}");
                    TaskLogs.AddLog($"{subText.Half}");
                }
            }
        }
    }

    public static void DumpUncompressedFiles()
    {
        string sourcePath = "F:\\SteamLibrary\\steamapps\\common\\ARMORED CORE VI FIRES OF RUBICON\\Game\\map\\msld";
        string destPath = "C:\\Users\\benja\\Programming\\C#\\Smithbox\\Dump";
        string ext = $"*.msld.dcx";

        foreach (string path in Directory.GetFiles(sourcePath, ext))
        {
            TaskLogs.AddLog($"{path}");
            string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));

            var bnd = BND4.Read(path);
            bnd.Files.Where(f => f.Name.EndsWith(".msld")).ToList().ForEach(f => File.WriteAllBytes($@"{destPath}\\msld\\{name}", f.Bytes.ToArray()));
        }
    }

    private static string sourceMap = "";
    private static string sourcePath = "";
    private static string destPath = "";

    public static void CollectTextures()
    {
        ImGui.Text("Collect Textures");

        ImGui.InputText("Source Map:", ref sourceMap, 1024);

        ImGui.InputText("Destination:", ref destPath, 1024);
        ImGui.SameLine();
        if (ImGui.Button("Select##destSelect"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Choose destination directory", out var path))
            {
                destPath = path;
            }
        }

        if (ImGui.Button("Collect"))
        {
            List<string> sourcePaths = new List<string>
            {
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0000-tpfbhd",
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0001-tpfbhd",
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0002-tpfbhd",
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0003-tpfbhd"
            };

            List<string> witchyEntries = new List<string>();

            foreach (var srcPath in sourcePaths)
            {
                List<string> newEntries = MoveTextures(srcPath, destPath);
                foreach (var entry in newEntries)
                {
                    witchyEntries.Add(entry);
                }
            }

            File.WriteAllLines(Path.Combine(destPath, "_entries.txt"), witchyEntries);
        }
    }

    private static List<string> MoveTextures(string pSrcPath, string pDstPath)
    {
        List<string> entries = new List<string>();

        if (Directory.Exists(pSrcPath))
        {
            foreach (var entry in Directory.GetDirectories(pSrcPath))
            {
                TaskLogs.AddLog($"{entry}");

                foreach (var fEntry in Directory.GetFiles(entry))
                {
                    var srcPath = fEntry;
                    var filename = Path.GetFileName(fEntry);
                    var dstPath = Path.Combine(pDstPath, filename);

                    if (fEntry.Contains(".dds"))
                    {
                        TaskLogs.AddLog($"{fEntry}");

                        var format = 0;
                        // Color
                        if (fEntry.Contains("_a.dds"))
                        {
                            TaskLogs.AddLog($"Color");
                            format = 0;
                        }
                        // Metallic
                        if (fEntry.Contains("_m.dds"))
                        {
                            TaskLogs.AddLog($"Metallic");
                            format = 103;
                        }
                        // Reflectance
                        if (fEntry.Contains("_r.dds"))
                        {
                            TaskLogs.AddLog($"Reflectance");
                            format = 0;
                        }
                        // Normal
                        if (fEntry.Contains("_n.dds"))
                        {
                            TaskLogs.AddLog($"Normal");
                            format = 106;
                        }
                        // Normal
                        if (fEntry.Contains("_v.dds"))
                        {
                            TaskLogs.AddLog($"Volume");
                            format = 104;
                        }

                        if (File.Exists(srcPath))
                        {
                            entries.Add($"<texture>\r\n      <name>{filename}</name>\r\n      <format>{format}</format>\r\n      <flags1>0x00</flags1>\r\n    </texture>");

                            File.Copy(srcPath, dstPath, true);
                        }
                    }
                }
            }
        }

        return entries;
    }
}
