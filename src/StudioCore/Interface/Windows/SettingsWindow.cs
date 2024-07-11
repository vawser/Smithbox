using ImGuiNET;
using SoapstoneLib;
using StudioCore.Editor;
using StudioCore.Scene;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Veldrid;
using StudioCore.Editors;
using StudioCore.Settings;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.TextureViewer;
using System.IO;
using StudioCore.Platform;
using StudioCore.Core;
using StudioCore.Interface.Settings;

namespace StudioCore.Interface.Windows;

public class SettingsWindow
{
    public bool MenuOpenState;

    private SystemTab SystemSettings;
    private ViewportTab ViewportSettings;
    private MapEditorTab MapEditorSettings;
    private ModelEditorTab ModelEditorSettings;
    private ParamEditorTab ParamEditorSettings;
    private TextEditorTab TextEditorSettings;
    private GparamEditorTab GparamEditorSettings;
    private TextureViewerTab TextureViewerSettings;
    private InterfaceTab InterfaceSettings;

    public SettingsWindow()
    {
        SystemSettings = new SystemTab();
        ViewportSettings = new ViewportTab();
        MapEditorSettings = new MapEditorTab();
        ModelEditorSettings = new ModelEditorTab();
        ParamEditorSettings = new ParamEditorTab();
        TextEditorSettings = new TextEditorTab();
        GparamEditorSettings = new GparamEditorTab();
        TextureViewerSettings = new TextureViewerTab();
        InterfaceSettings = new InterfaceTab();
    }

    public void SaveSettings()
    {
        CFG.Save();
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

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Settings##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#SettingsMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
            ImGui.PushItemWidth(300f);

            SystemSettings.Display();
            ViewportSettings.Display();
            MapEditorSettings.Display();
            ModelEditorSettings.Display();
            ParamEditorSettings.Display();
            TextEditorSettings.Display();
            GparamEditorSettings.Display();
            TextureViewerSettings.Display();
            InterfaceSettings.Display();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }
}
