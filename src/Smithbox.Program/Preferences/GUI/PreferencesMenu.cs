using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class PreferencesMenu
{
    public bool IsDisplayed = false;
    private bool _wasDisplayedLastFrame;

    private bool InitialLayout = false;

    private List<PreferenceItem> SystemPrefList = new();
    private List<PreferenceItem> ProjectPrefList = new();
    private List<PreferenceItem> InterfacePrefList = new();
    private List<PreferenceItem> ViewportPrefList = new();
    private List<PreferenceItem> MapEditorPrefList = new();
    private List<PreferenceItem> ModelEditorPrefList = new();
    private List<PreferenceItem> ParamEditorPrefList = new();
    private List<PreferenceItem> TextEditorPrefList = new();
    private List<PreferenceItem> GparamEditorPrefList = new();
    private List<PreferenceItem> MaterialEditorPrefList = new();
    private List<PreferenceItem> TextureViewerPrefList = new();
    private List<PreferenceItem> MapDataEditorPrefList = new();
    private List<PreferenceItem> AnimEditorPrefList = new();

    public PreferencesMenu()
    {
        SystemPrefList = SetupPrefList(SystemPrefs.GetPrefType());
        ProjectPrefList = SetupPrefList(ProjectPrefs.GetPrefType());
        InterfacePrefList = SetupPrefList(InterfacePrefs.GetPrefType());
        ViewportPrefList = SetupPrefList(ViewportPrefs.GetPrefType());
        MapEditorPrefList = SetupPrefList(MapEditorPrefs.GetPrefType());
        ModelEditorPrefList = SetupPrefList(ModelEditorPrefs.GetPrefType());
        ParamEditorPrefList = SetupPrefList(ParamEditorPrefs.GetPrefType());
        TextEditorPrefList = SetupPrefList(TextEditorPrefs.GetPrefType());
        GparamEditorPrefList = SetupPrefList(GparamEditorPrefs.GetPrefType());
        MaterialEditorPrefList = SetupPrefList(MaterialEditorPrefs.GetPrefType());
        TextureViewerPrefList = SetupPrefList(TextureViewerPrefs.GetPrefType());
        MapDataEditorPrefList = SetupPrefList(MapDataEditorPrefs.GetPrefType());
        AnimEditorPrefList = SetupPrefList(AnimEditorPrefs.GetPrefType());
    }

    public List<PreferenceItem> SetupPrefList(Type targetPref)
    {
        var list = new List<PreferenceItem>();

        // Auto-magically add new settings to the window
        var methods = targetPref.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.ReturnType == typeof(PreferenceItem) && m.GetParameters().Length == 0);

        list = methods
            .Select(m => (PreferenceItem)m.Invoke(null, null))
            .OrderBy(s => s.Section)
            .ToList();

        return list;
    }

    public void Draw()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (IsDisplayed)
        {
            if (!InitialLayout)
            {
                UIHelper.SetupPopupWindow();
                InitialLayout = true;
            }

            // Preferences
            if (ImGui.Begin($"{LOC.Get("PREF_Window_Preferences")}###preferencesMenu", ref IsDisplayed, UIHelper.GetFloatingWindowFlags()))
            {
                ImGui.BeginMenuBar();

                // Options
                if (ImGui.BeginMenu($"{LOC.Get("PREF_Menu_Header_Options")}##optionsMenuHeader"))
                {
                    // Save
                    if (ImGui.MenuItem($"{LOC.Get("PREF_Menu_Save_Action")}##saveAction"))
                    {
                        CFG.Save();
                        Smithbox.Log(this, LOC.Get("PREF_Preferences_Saved"), LogLevel.Information);
                    }

                    // Reset to Default
                    if(ImGui.BeginMenu($"{LOC.Get("PREF_Menu_Revert_Action")}##revertAction"))
                    {
                        DisplayRevertOptions();

                        ImGui.EndMenu();
                    }


                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();

                DisplaySettingsSearch();

                ImGui.BeginTabBar("settingsTabs");

                if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_System")}##systemTab"))
                {
                    ImGui.BeginChild("systemPrefSection");

                    DisplaySettings(SystemPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Project")}##projectTab"))
                {
                    ImGui.BeginChild("projectPrefSection");

                    DisplaySettings(ProjectPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Interface")}##interfaceTab"))
                {
                    ImGui.BeginChild("interfacePrefSection");

                    DisplaySettings(InterfacePrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Viewport")}##viewportTab"))
                {
                    ImGui.BeginChild("viewportPrefSection");

                    DisplaySettings(ViewportPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (curProject != null && curProject.Handler != null && 
                    curProject.Handler.MapEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Map_Editor")}##mapEditorTab"))
                    {
                        ImGui.BeginChild("mapEditorPrefSection");

                        if (MapEditorPrefList.Count > 0)
                        {
                            DisplaySettings(MapEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.MapDataEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Map_Data_Editor")}##mapDataEditorTab"))
                    {
                        ImGui.BeginChild("mapDataEditorPrefSection");

                        if (MapDataEditorPrefList.Count > 0)
                        {
                            DisplaySettings(MapDataEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.ModelEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Model_Editor")}##modelEditorTab"))
                    {
                        ImGui.BeginChild("modelEditorPrefSection");

                        if (ModelEditorPrefList.Count > 0)
                        {
                            DisplaySettings(ModelEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.ParamEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Param_Editor")}##paramEditorTab"))
                    {
                        ImGui.BeginChild("paramEditorPrefSection");

                        if (ParamEditorPrefList.Count > 0)
                        {
                            DisplaySettings(ParamEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.TextEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Text_Editor")}##textEditorTab"))
                    {
                        ImGui.BeginChild("textEditorPrefSection");

                        if (TextEditorPrefList.Count > 0)
                        {
                            DisplaySettings(TextEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.GparamEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Graphics_Param_Editor")}##gparamEditorTab"))
                    {
                        ImGui.BeginChild("gparamEditorPrefSection");

                        if (GparamEditorPrefList.Count > 0)
                        {
                            DisplaySettings(GparamEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.MaterialEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Material_Editor")}##materialEditorTab"))
                    {
                        ImGui.BeginChild("materialEditorPrefSection");

                        if (MaterialEditorPrefList.Count > 0)
                        {
                            DisplaySettings(MaterialEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.TextureViewer != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Texture_Viewer")}##textureViewerTab"))
                    {
                        ImGui.BeginChild("textureViewerPrefSection");

                        if (TextureViewerPrefList.Count > 0)
                        {
                            DisplaySettings(TextureViewerPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                if (curProject != null && curProject.Handler != null &&
                    curProject.Handler.AnimEditor != null)
                {
                    if (ImGui.BeginTabItem($"{LOC.Get("PREF_Tab_Anim_Editor")}##animEditorTab"))
                    {
                        ImGui.BeginChild("animEditorPrefSection");

                        if (AnimEditorPrefList.Count > 0)
                        {
                            DisplaySettings(AnimEditorPrefList);
                        }
                        else
                        {
                            ImGui.Text(LOC.Get("PREF_No_Preferences_Yet"));
                        }

                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();


                ImGui.End();
            }
        }

        if (!IsDisplayed && _wasDisplayedLastFrame)
        {
            CFG.Save();
            Startup.Save();
            Smithbox.Log(this, LOC.Get("PREF_Preferences_Saved"), LogLevel.Information);
        }

        _wasDisplayedLastFrame = IsDisplayed;
    }

    private string searchFilter = "";

    private void DisplaySettingsSearch()
    {
        ImGui.InputTextWithHint("##settingsSearch", LOC.Get("PREF_Search_Filter_Hint"), ref searchFilter, 128);
    }

    private void DisplaySettings(List<PreferenceItem> prefs)
    {
        // Group settings by section
        var groupedSettings = prefs
            .GroupBy(s => s.Section);

        // Force General to the top
        foreach (var sectionGroup in groupedSettings)
        {
            if (sectionGroup.Any(e => e.Section is SectionCategory.General))
            {
                DisplayPrefSettings(sectionGroup);
            }
        }

        // Then display the rest
        foreach (var sectionGroup in groupedSettings)
        {
            if (sectionGroup.Any(e => e.Section is not SectionCategory.General))
            {
                DisplayPrefSettings(sectionGroup);
            }
        }
    }

    private void DisplayPrefSettings(IGrouping<SectionCategory, PreferenceItem> sectionGroup)
    {
        // Filter settings in this section based on search
        var filteredSettings = sectionGroup
            .Where(s => string.IsNullOrWhiteSpace(searchFilter) ||
                        LOC.Get(s.Title).Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        LOC.Get(s.Description).Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.OrderID)
            .ToList();

        // Skip sections with no matching settings
        if (filteredSettings.Count == 0)
            return;

        // Auto-expand sections if a search filter is active
        bool sectionOpen = string.IsNullOrWhiteSpace(searchFilter)
            ? ImGui.CollapsingHeader(
                $"{LOC.Get(sectionGroup.Key.GetDisplayName())}##{sectionGroup.Key}", ImGuiTreeNodeFlags.DefaultOpen)
            : ImGui.CollapsingHeader(
                $"{LOC.Get(sectionGroup.Key.GetDisplayName())}##{sectionGroup.Key}", ImGuiTreeNodeFlags.DefaultOpen);

        if (sectionOpen)
        {
            foreach (var setting in filteredSettings)
            {
                // If set to anything, check
                if (!setting.DisplayRestrictions.Any(e => e == ProjectType.Undefined))
                {
                    var projectType = Smithbox.Orchestrator.SelectedProject;

                    if (projectType != null)
                    {
                        var display = false;

                        foreach (var projType in setting.DisplayRestrictions)
                        {
                            if (projType == projectType.Descriptor.ProjectType)
                            {
                                display = true;
                            }
                        }

                        if (!display)
                        {
                            continue;
                        }
                    }
                }

                var displayName = LOC.Get(setting.Title);
                var description = LOC.Get(setting.Description);

                ImGui.PushID(setting.Title);

                setting.PreDraw?.Invoke();

                if (!setting.InlineName)
                {
                    ImGui.Text(displayName);
                }

                setting.Draw?.Invoke();

                if (setting.InlineName)
                {
                    ImGui.SameLine();
                    ImGui.Text(displayName);
                }

                if (!string.IsNullOrEmpty(description))
                {
                    ImGui.TextDisabled(description);
                }

                setting.PostDraw?.Invoke();

                if (setting.Spacer)
                    ImGui.Text("");

                ImGui.PopID();
            }
        }
    }

    public void DisplayRevertOptions()
    {
        if (ImGui.MenuItem($"{LOC.Get("PREF_Action_Revert_All")}##revertAllAction"))
        {
            var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialog is DialogResult.Yes)
            {
                CFGHelpers.ResetCurrentToDefault();
            }
        }
        UIHelper.Tooltip(
            LOC.Get("PREF_Action_Revert_All_TT"));

        // Add these to the user can revert these easily without reverting everything
        if (ImGui.BeginMenu($"{LOC.Get("PREF_Menu_Header_Viewport")}##viewportMenuHeader"))
        {
            if(ImGui.MenuItem($"{LOC.Get("PREF_Action_Viewport_General")}##viewportGeneralAction"))
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    PreferencesUtil.ResetViewportGeneralCFG();
                }
            }
            UIHelper.Tooltip(
                LOC.Get("PREF_Action_Viewport_General_TT"));

            if (ImGui.MenuItem($"{LOC.Get("PREF_Action_Viewport_Rendering")}##viewportRenderingAction"))
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    PreferencesUtil.ResetViewportRenderingCFG();
                }
            }
            UIHelper.Tooltip(LOC.Get("PREF_Action_Viewport_Rendering_TT"));

            if (ImGui.MenuItem($"{LOC.Get("PREF_Action_Viewport_Model_Rendering")}##viewportModelRenderingAction"))
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    PreferencesUtil.ResetViewportModelRenderingCFG();
                }
            }
            UIHelper.Tooltip(LOC.Get("PREF_Action_Viewport_Model_Rendering_TT"));

            if (ImGui.MenuItem($"{LOC.Get("PREF_Action_Viewport_Selection")}##viewportSelectionAction"))
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    PreferencesUtil.ResetViewportSelectionCFG();
                }
            }
            UIHelper.Tooltip(LOC.Get("PREF_Action_Viewport_Selection_TT"));

            if (ImGui.MenuItem($"{LOC.Get("PREF_Action_Viewport_Coloring")}##viewportColoringAction"))
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    PreferencesUtil.ResetViewportColoringCFG();
                }
            }
            UIHelper.Tooltip(LOC.Get("PREF_Action_Viewport_Coloring_TT"));

            if (ImGui.MenuItem($"{LOC.Get("PREF_Action_Viewport_Display_Preset")}##viewportDpAction"))
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PREF_Action_Prompt"),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    PreferencesUtil.ResetViewportDisplayPresetCFG();
                }
            }
            UIHelper.Tooltip(LOC.Get("PREF_Action_Viewport_Display_Preset_TT"));

            ImGui.EndMenu();
        }
    }
}