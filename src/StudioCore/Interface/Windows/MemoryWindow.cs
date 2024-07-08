using ImGuiNET;
using StudioCore.Banks.GameOffsetBank;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Memory;
using System.Linq;
using System.Numerics;

namespace StudioCore.Interface.Windows;

public class MemoryWindow
{
    private bool MenuOpenState;

    public ParamEditorView _activeView;

    private bool ShowParamList = false;

    private GameOffsetResource SelectedGameOffsetData { get; set; }

    public MemoryWindow()
    {
        SelectedGameOffsetData = null;
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

        if (ImGui.Begin("Memory##MemoryWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##memoryTabs");

            DisplayParamHotReloader();
            DisplayParamItemGib();
            DisplayMemorySettings();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayParamHotReloader()
    {
        if (ImGui.BeginTabItem("Param Reloader"))
        {
            if (!ParamReloader.GameIsSupported(Smithbox.ProjectType))
            {
                ImGui.Text("This project type does not support this feature.");
                ImGui.EndTabItem();
                return;
            }

            ImGui.Text("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");

            ImGui.Separator();

            var canHotReload = ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank);

            // Currently Selected Param
            if (ImGui.MenuItem("Current Param", KeyBindings.Current.Param_HotReload.HintText, false, canHotReload && _activeView._selection.GetActiveParam() != null))
            {
                ParamReloader.ReloadMemoryParam(ParamBank.PrimaryBank, _activeView._selection.GetActiveParam());
            }

            // All Params
            if (ImGui.MenuItem("All Params", KeyBindings.Current.Param_HotReloadAll.HintText, false, canHotReload))
            {
                ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, ParamBank.PrimaryBank.Params.Keys.ToArray());
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Toggle Individual Param List"))
            {
                ShowParamList = !ShowParamList;
            }

            ImGui.Separator();

            if (ShowParamList)
            {
                // List each param
                foreach (var param in ParamReloader.GetReloadableParams())
                {
                    if (ImGui.MenuItem(param, "", false, canHotReload))
                    {
                        ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, new[] { param });
                    }
                }
            }

            ImGui.EndTabItem();
        }
    }

    public void DisplayParamItemGib()
    {
        if (ImGui.BeginTabItem("Item Gib"))
        {
            if (Smithbox.ProjectType != ProjectType.DS3)
            {
                ImGui.Text("This project type does not support this feature.");
                ImGui.EndTabItem();
                return;
            }

            ImGui.Text("Select an item param in the Param Editor.");

            ImGui.Separator();

            var activeParam = _activeView._selection.GetActiveParam();
            if (activeParam != null && Smithbox.ProjectType == ProjectType.DS3)
            {
                ParamReloader.GiveItemMenu(_activeView._selection.GetSelectedRows(), _activeView._selection.GetActiveParam());
            }

            ImGui.EndTabItem();
        }
    }

    public void DisplayMemorySettings()
    {
        if (SelectedGameOffsetData != null)
        {
            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.Text("Game Offset Version");
                ImguiUtils.ShowHoverTooltip("This should match the executable version you wish to target, otherwise the memory offsets will be incorrect.");

                var index = CFG.Current.SelectedGameOffsetData;
                string[] options = Smithbox.BankHandler.GameOffsets.GetList().Select(entry => entry.exeVersion).ToArray();

                if (ImGui.Combo("##GameOffsetVersion", ref index, options, options.Length))
                {
                    CFG.Current.SelectedGameOffsetData = index;
                }

                ImGui.EndTabItem();
            }
        }
        else
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {
                SelectedGameOffsetData = Smithbox.BankHandler.GameOffsets.Offsets;
            }
        }
    }
}
