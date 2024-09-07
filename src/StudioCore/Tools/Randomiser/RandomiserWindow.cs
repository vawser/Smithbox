using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Numerics;
using System.ComponentModel.DataAnnotations;
using static StudioCore.Tools.Development.DebugWindow;
using StudioCore.Utilities;

namespace StudioCore.Tools.Randomiser;

public class RandomiserWindow
{
    private bool MenuOpenState;
    private Task _loadingTask;

    private TreasureRandomiser TreasureRandomiser;
    private MobRandomiser MobRandomiser;

    public RandomiserWindow() 
    {
        TreasureRandomiser = new TreasureRandomiser();
        MobRandomiser = new MobRandomiser();
    }

    private SelectedRandomiserTab SelectedTab = SelectedRandomiserTab.TreasureRandomiser;

    public enum SelectedRandomiserTab
    {
        // Randomisers
        [Display(Name = "Treasure")] TreasureRandomiser,
        [Display(Name = "Mobs")] MobRandomiser,
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;

        RandomiserCFG.Save();
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
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Randomisers##RandomiserWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("randomiserList");

            var arr = Enum.GetValues(typeof(SelectedRandomiserTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedRandomiserTab)arr.GetValue(i);

                if (tab == SelectedRandomiserTab.TreasureRandomiser)
                {
                    ImGui.Separator();
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Randomisers");
                    ImGui.Separator();
                }

                if (ImGui.Selectable(tab.GetDisplayName(), tab == SelectedTab))
                {
                    SelectedTab = tab;
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("randomiserTab");
            switch (SelectedTab)
            {
                case SelectedRandomiserTab.TreasureRandomiser:
                    TreasureRandomiser.Display();
                    break;
                case SelectedRandomiserTab.MobRandomiser:
                    MobRandomiser.Display();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }
}
