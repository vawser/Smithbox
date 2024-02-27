using ImGuiNET;
using StudioCore.BanksMain;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Windows;

public class MapGroupWindow
{
    private bool MenuOpenState;

    public MapGroupWindow() { }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        if (Project.Type == ProjectType.Undefined)
            return;

        if (MapGroupsBank.Bank.IsMapGroupBankLoaded)
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

        if (ImGui.Begin("Map Groups##MapGroupWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            // TODO: add ability to change existing groups
            // (place in mod .smithbox, the bank reload will override the existing ones

            // TODO: add the ability add project-specific groups
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);

        if (MapGroupsBank.Bank.CanReloadMapGroupBank)
        {
            MapGroupsBank.Bank.CanReloadMapGroupBank = false;
            MapGroupsBank.Bank.ReloadMapGroupBank();
        }
    }
}


