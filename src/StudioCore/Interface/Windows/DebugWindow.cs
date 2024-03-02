using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Help;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Tests;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Windows;

public class DebugWindow
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

    public void Display()
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
            ImGui.Columns(4);

            // DISABLE this when not using it for custom code stuff
            if (ImGui.Button("Vawser Custom Action"))
            {
                AssignEntityGroupsForAllCharacters();
            }

            // Actions
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
}
