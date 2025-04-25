using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;

using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core.Project;

public static class ProjectConfigurationWindow
{
    public static bool DisplayProjectConfigurationWindow = false;
    public static bool DisplayClosePopup = false;

    private static bool renameProject = false;
    private static string newProjectName = "";

    public static void ToggleWindow()
    {
        DisplayProjectConfigurationWindow = !DisplayProjectConfigurationWindow;
    }

    public static void Display()
    {
        if (!DisplayProjectConfigurationWindow)
            return;

        var scale = DPI.GetUIScale();

        ImGui.SetNextWindowSize(new Vector2(1200.0f, 1000.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin($"Project Configuration", ref DisplayProjectConfigurationWindow, ImGuiWindowFlags.NoDocking))
        {
            var width = ImGui.GetWindowWidth();

            if (Smithbox.ProjectHandler.CurrentProject == null)
            {
                ImGui.Text("No project loaded");
                UIHelper.ShowHoverTooltip("No project has been loaded yet.");
            }
            else if (TaskManager.AnyActiveTasks())
            {
                ImGui.Text("Waiting for program tasks to finish...");
                UIHelper.ShowHoverTooltip("Smithbox must finished all program tasks before it can load a project.");
            }
            else
            {
                var itemWidth = 120;

                if (ImGui.BeginTable($"projectDetailsTable", 3, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Name");
                    UIHelper.ShowHoverTooltip("The name of this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();

                    if (renameProject)
                    {
                        ImGui.InputText("##newProjectName", ref newProjectName, 255);

                        if(ImGui.IsItemDeactivatedAfterEdit())
                        {
                            renameProject = false;

                            Smithbox.ProjectHandler.CurrentProject.Config.ProjectName = newProjectName;
                            Smithbox.ProjectHandler.SaveCurrentProject();
                        }
                    }
                    else
                    {
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text,
                            $"{Smithbox.ProjectHandler.CurrentProject.Config.ProjectName}");
                    }

                    ImGui.TableSetColumnIndex(2);

                    if (renameProject)
                    {
                        ImGui.BeginDisabled();

                        if (ImGui.Button("Rename", new Vector2(itemWidth, 20)))
                        {
                        }
                        UIHelper.ShowHoverTooltip("Opens the project.json file for this project.");

                        ImGui.EndDisabled();
                    }
                    else
                    {
                        if (ImGui.Button("Rename", new Vector2(itemWidth, 20)))
                        {
                            renameProject = true;
                        }
                        UIHelper.ShowHoverTooltip("Opens the project.json file for this project.");
                    }

                    // Project Directory
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Project Directory");
                    UIHelper.ShowHoverTooltip("The directory that contains the project files. This is always the location of the project.json file.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text,
                        $"{Smithbox.ProjectRoot}");

                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Open", new Vector2(itemWidth, 20)))
                    {
                        var projectPath = CFG.Current.LastProjectFile;
                        Process.Start("explorer.exe", projectPath);
                    }
                    UIHelper.ShowHoverTooltip("Opens the project.json file for this project.");

                    // Type
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Type");
                    UIHelper.ShowHoverTooltip("The internal type for this project. Determines how the project is handled within the editors.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text,
                        $"{Smithbox.ProjectType.GetDisplayName()}");

                    ImGui.TableSetColumnIndex(2);

                    ImGui.AlignTextToFramePadding();
                    ImGui.SetNextItemWidth(itemWidth);
                    if (ImGui.BeginCombo("##projectTypePicker", Smithbox.ProjectType.GetDisplayName()))
                    {
                        foreach (var entry in Enum.GetValues(typeof(ProjectType)))
                        {
                            var type = (ProjectType)entry;

                            if (ImGui.Selectable(type.GetDisplayName()))
                            {
                                Smithbox.ProjectType = type;

                                Smithbox.ProjectHandler.CurrentProject.Config.GameType = type;
                                Smithbox.ProjectHandler.SaveCurrentProject();
                            }
                        }
                        ImGui.EndCombo();
                    }

                    UIHelper.ShowHoverTooltip("Set the game directory to use for this project.");

                    // Root Directory
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Game Directory");
                    UIHelper.ShowHoverTooltip("The directory that contains the game files. Typically the directory that the game executable is located in.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text,
                        $"{Smithbox.GameRoot}");

                    ImGui.TableSetColumnIndex(2);

                    ImGui.AlignTextToFramePadding();
                    if (ImGui.Button($@"Change##rootDirectoryPicker", new Vector2(itemWidth, 20)))
                    {
                        var gameDir = WindowsUtils.GetFileSelection();

                        var newGameRoot = Path.GetDirectoryName(gameDir);

                        Smithbox.GameRoot = newGameRoot;

                        Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = newGameRoot;
                        Smithbox.ProjectHandler.SaveCurrentProject();
                    }
                    UIHelper.ShowHoverTooltip("Set the game directory to use for this project.");

                    // Collision Directory
                    if (Smithbox.ProjectType is ProjectType.DS1R)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);

                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("Collision Directory");
                        UIHelper.ShowHoverTooltip("The directory that contains the DS1 game files. This is used to load the original DS1 collisions.");

                        ImGui.TableSetColumnIndex(1);

                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text,
                            $"{CFG.Current.PTDE_Collision_Root}");

                        ImGui.TableSetColumnIndex(2);

                        ImGui.AlignTextToFramePadding();
                        if (ImGui.Button($@"Change##collisionDirPicker", new Vector2(itemWidth, 20)))
                        {
                            var ptdeRoot = WindowsUtils.GetFolderSelection();

                            CFG.Current.PTDE_Collision_Root = ptdeRoot;
                            CFG.Save();
                        }
                        UIHelper.ShowHoverTooltip("When set this will allow collisions to be visible whilst editing Dark Souls: Remastered maps, assuming you have unpacked Dark Souls: Prepare the Die Edition.");
                    }

                    ImGui.EndTable();
                }

                // Options
                if (Smithbox.ProjectHandler.CurrentProject.Config.GameType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3)
                {
                    ImGui.Separator();

                    var useLoose = Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams;

                    if (ImGui.Checkbox("Use loose params", ref useLoose))
                    {
                        Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams = useLoose;
                    }
                    UIHelper.ShowHoverTooltip("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");

                    if (Smithbox.ProjectType is ProjectType.DS1R)
                    {
                        ImGui.Checkbox("Warning for Missing Collision Directory", ref CFG.Current.PTDE_Collision_Root_Warning);
                        UIHelper.ShowHoverTooltip("When set to true, a warning will be displayed when loading Dark Souls: Remastered projects if the DS1 collision directory is not set.");
                    }
                }
            }
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }
}
