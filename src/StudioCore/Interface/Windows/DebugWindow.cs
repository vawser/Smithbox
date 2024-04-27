using ImGuiNET;
using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Tests;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Drawing;
using Veldrid;
using DirectXTexNet;
using Vortice.Vulkan;
using static SoulsFormats.MSB_AC6;
using StudioCore.AssetLocator;
using System.Threading.Tasks;
using HKX2;

namespace StudioCore.Interface.Windows;

public class DebugWindow : IResourceEventListener
{
    private bool MenuOpenState;

    public bool _showImGuiDemoWindow = false;
    public bool _showImGuiMetricsWindow = false;
    public bool _showImGuiDebugLogWindow = false;
    public bool _showImGuiStackToolWindow = false;

    public DebugWindow()
    {
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private Task _loadingTask;

    private unsafe void ImageTest(GraphicsDevice gDevice)
    {
        if (_loadingTask != null && _loadingTask.IsCompleted)
        {
            _loadingTask = null;
        }

        if (ImGui.Button("Load Menu Textures"))
        {
            ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading texture");

            AssetDescription ad = TextureAssetLocator.GetMenuTextures("01_common");

            if (!ResourceManager.IsResourceLoadedOrInFlight(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
            {
                if (ad.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly, true);
                }

                _loadingTask = job.Complete();
            }

            ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);
        }

        if (ImGui.Button("Purge Menu Textures"))
        {
            ResourceManager.UnloadMenuTextures();
        }

        var resources = ResourceManager.GetResourceDatabase();

        foreach (var (key, val) in ResourceManager.GetResourceDatabase())
        {
            ResourceHandle<TextureResource> resHandle = (ResourceHandle<TextureResource>)val;
            TextureResource texRes = resHandle.Get();

            if (texRes != null)
            {
                IntPtr handle = (nint)texRes.GPUTexture.TexHandle;
                Vector2 size = new Vector2(100, 100);

                ImGui.Text($"{key}");
                ImGui.Image(handle, size);

                break;
            }
        }

        // Match with AssetVirtualPath (selection with generate this, ResourceHandle will have it)
    }

    public void Display(GraphicsDevice gDevice)
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        
        if (ImGui.Begin("Tests##TestWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImageTest(gDevice);

            ImGui.Columns(4);

            // Actions 
            if (ImGui.Button("Dump Uncompressed Files"))
            {
                string sourcePath = "F:\\SteamLibrary\\steamapps\\common\\ARMORED CORE VI FIRES OF RUBICON\\Game\\map\\msld";
                string destPath = "C:\\Users\\benja\\Programming\\C#\\Smithbox\\Dump";
                string ext = $"*.msld.dcx";

                foreach(string path in Directory.GetFiles(sourcePath, ext) )
                {
                    TaskLogs.AddLog($"{path}");
                    string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));

                    var bnd = BND4.Read(path);
                    bnd.Files.Where(f => f.Name.EndsWith(".msld")).ToList().ForEach(f => File.WriteAllBytes($@"{destPath}\\msld\\{name}", f.Bytes.ToArray()));
                }
            }

            if (ImGui.Button("Force Crash"))
            {
                var badArray = new int[2];
                var crash = badArray[5];
            }

            if (ImGui.Button("Reset CFG.Current.Debug_FireOnce"))
            {
                CFG.Current.Debug_FireOnce = false;
            }

            if (ImGui.Button("Dump FLVER Layouts"))
            {
                DumpFlverLayouts();
            }

            // Imgui
            ImGui.NextColumn();

            if (ImGui.Button("Demo"))
            {
                _showImGuiDemoWindow = !_showImGuiDemoWindow;
            }

            if (ImGui.Button("Metrics"))
            {
                _showImGuiMetricsWindow = !_showImGuiMetricsWindow;
            }

            if (ImGui.Button("Debug Log"))
            {
                _showImGuiDebugLogWindow = !_showImGuiDebugLogWindow;
            }

            if (ImGui.Button("Stack Tool"))
            {
                _showImGuiStackToolWindow = !_showImGuiStackToolWindow;
            }

            // Tests
            ImGui.NextColumn();

            if (ImGui.Button("MSBE read/write test"))
            {
                MSBReadWrite.Run();
            }

            if (ImGui.Button("MSB_AC6 Read/Write Test"))
            {
                MSB_AC6_Read_Write.Run();
            }

            if (ImGui.Button("BTL read/write test"))
            {
                BTLReadWrite.Run();
            }

            if (ImGui.Button("Insert unique rows IDs into params"))
            {
                ParamUniqueRowFinder.Run();
            }

            // Live Tasks
            ImGui.NextColumn();

            if (TaskManager.GetLiveThreads().Count > 0)
            {
                foreach (var task in TaskManager.GetLiveThreads())
                {
                    ImGui.Text(task);
                }
            }
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private string sourceMap = "";
    private string sourcePath = "";
    private string destPath = "";

    private void DumpFlverLayouts()
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

    private void AssignEntityGroupsForAllCharacters()
    {
        IOrderedEnumerable<KeyValuePair<string, ObjectContainer>> orderedMaps = MapEditorState.Universe.LoadedObjectContainers.OrderBy(k => k.Key);

        int printId = 400005300;

        foreach (var entry in ModelAliasBank.Bank.AliasNames.GetEntries("Characters"))
        {
            TaskLogs.AddLog($"{entry.id} - {entry.name} - {printId}");
            printId = printId + 1;
        }

        foreach (KeyValuePair<string, ObjectContainer> lm in orderedMaps)
        {
            var rootPath = $"{Project.GameRootDirectory}\\map\\MapStudio\\{lm.Key}.msb.dcx";
            var filepath = $"{Project.GameModDirectory}\\map\\MapStudio\\{lm.Key}.msb.dcx";

            if(!File.Exists(filepath)) 
            { 
                File.Copy(rootPath, filepath);
            }

            MSBE map = MSBE.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSBE.Part.Enemy enemy = part;

                int currentChrEntityID = 400005300;

                foreach(var entry in ModelAliasBank.Bank.AliasNames.GetEntries("Characters"))
                {
                    if(entry.id == enemy.ModelName)
                    {
                        // Break out and then use the currentChrEntityID
                        break;
                    }

                    currentChrEntityID = currentChrEntityID + 1;
                }

                if (!enemy.EntityGroupIDs.Any(x => x == currentChrEntityID))
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = (uint)currentChrEntityID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }
    }

    private void CollectTextures()
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

            foreach(var srcPath in sourcePaths)
            {
                List<string> newEntries = MoveTextures(srcPath, destPath);
                foreach(var entry in newEntries)
                {
                    witchyEntries.Add(entry);
                }
            }

            File.WriteAllLines(Path.Combine(destPath, "_entries.txt"), witchyEntries);
        }
    }

    private List<string> MoveTextures(string pSrcPath, string pDstPath)
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

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        throw new NotImplementedException();
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        throw new NotImplementedException();
    }
}
