using Hexa.NET.ImGui;
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
    }

    public List<PreferenceItem> SetupPrefList(Type targetPref)
    {
        var list = new List<PreferenceItem>();

        // Auto-magically add new settings to the window
        var methods = targetPref.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.ReturnType == typeof(PreferenceItem) && m.GetParameters().Length == 0);

        list = methods
            .Select(m => (PreferenceItem)m.Invoke(null, null))
            .OrderBy(s => s.Section)   // sort by Section
            .ThenBy(s => s.Title)      // then by Title
            .ToList();

        return list;
    }

    public void Draw()
    {
        if (IsDisplayed)
        {
            if (!InitialLayout)
            {
                UIHelper.SetupPopupWindow();
                InitialLayout = true;
            }

            if (ImGui.Begin("Preferences##appSettingsMenu", ref IsDisplayed, UIHelper.GetEditorPopupWindowFlags()))
            {
                ImGui.BeginMenuBar();

                if (ImGui.BeginMenu("Options"))
                {
                    if (ImGui.MenuItem("Save"))
                    {
                        CFG.Save();
                        TaskLogs.AddLog("Preferences saved.");
                    }

                    if(ImGui.BeginMenu("Reset to Default"))
                    {
                        DisplayRevertOptions();

                        ImGui.EndMenu();
                    }


                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();

                DisplaySettingsSearch();

                ImGui.BeginTabBar("settingsTabs");

                if (ImGui.BeginTabItem("System"))
                {
                    ImGui.BeginChild("systemPrefSection");

                    DisplaySettings(SystemPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Project"))
                {
                    ImGui.BeginChild("projectPrefSection");

                    DisplaySettings(ProjectPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Interface"))
                {
                    ImGui.BeginChild("interfacePrefSection");

                    DisplaySettings(InterfacePrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Viewport"))
                {
                    ImGui.BeginChild("viewportPrefSection");

                    DisplaySettings(ViewportPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Map Editor"))
                {
                    ImGui.BeginChild("mapEditorPrefSection");

                    DisplaySettings(MapEditorPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Model Editor"))
                {
                    ImGui.BeginChild("modelEditorPrefSection");

                    DisplaySettings(ModelEditorPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Param Editor"))
                {
                    ImGui.BeginChild("paramEditorPrefSection");

                    DisplaySettings(ParamEditorPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Text Editor"))
                {
                    ImGui.BeginChild("textEditorPrefSection");

                    DisplaySettings(TextEditorPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Graphics Param Editor"))
                {
                    ImGui.BeginChild("gparamEditorPrefSection");

                    DisplaySettings(GparamEditorPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Material Editor"))
                {
                    ImGui.BeginChild("materialEditorPrefSection");

                    DisplaySettings(MaterialEditorPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Texture Viewer"))
                {
                    ImGui.BeginChild("textureViewerPrefSection");

                    DisplaySettings(TextureViewerPrefList);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();


                ImGui.End();
            }
        }
    }

    private string searchFilter = "";

    private void DisplaySettingsSearch()
    {
        ImGui.InputText("##settingsSearch", ref searchFilter, 255);
    }

    private void DisplaySettings(List<PreferenceItem> prefs)
    {
        // Group settings by section
        var groupedSettings = prefs
            .GroupBy(s => s.Section);

        // Force General to the top
        foreach (var sectionGroup in groupedSettings)
        {
            if (sectionGroup.Any(e => e.Section == "General"))
            {
                DisplayPrefSettings(sectionGroup);
            }
        }

        // Then display the rest
        foreach (var sectionGroup in groupedSettings)
        {
            if (sectionGroup.Any(e => e.Section != "General"))
            {
                DisplayPrefSettings(sectionGroup);
            }
        }
    }

    private void DisplayPrefSettings(IGrouping<string, PreferenceItem> sectionGroup)
    {
        // Filter settings in this section based on search
        var filteredSettings = sectionGroup
            .Where(s => string.IsNullOrWhiteSpace(searchFilter) ||
                        s.Title.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        s.Description.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Skip sections with no matching settings
        if (filteredSettings.Count == 0)
            return;

        // Auto-expand sections if a search filter is active
        bool sectionOpen = string.IsNullOrWhiteSpace(searchFilter)
            ? ImGui.CollapsingHeader(sectionGroup.Key, ImGuiTreeNodeFlags.DefaultOpen)
            : ImGui.CollapsingHeader(sectionGroup.Key, ImGuiTreeNodeFlags.DefaultOpen);

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

                ImGui.PushID(setting.Title);

                setting.PreDraw?.Invoke();

                if (!setting.InlineName)
                {
                    ImGui.Text($"{setting.Title}");
                }

                setting.Draw?.Invoke();

                if (setting.InlineName)
                {
                    ImGui.SameLine();
                    ImGui.Text(setting.Title);
                }

                if (!string.IsNullOrEmpty(setting.Description))
                {
                    ImGui.TextDisabled(setting.Description);
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
        if (ImGui.MenuItem("All"))
        {
            var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialog is DialogResult.Yes)
            {
                CFGHelpers.ResetCurrentToDefault();
            }
        }
        UIHelper.Tooltip("Reverts all preferences to their default value.");

        // Add these to the user can revert these easily without reverting everything
        if (ImGui.BeginMenu("Viewport"))
        {
            if(ImGui.MenuItem("General"))
            {
                var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    CFGHelpers.ResetViewportGeneralCFG();
                }
            }
            UIHelper.Tooltip("Reverts all preferences in the Viewport General section to their default value.");

            if (ImGui.MenuItem("Rendering"))
            {
                var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    CFGHelpers.ResetViewportRenderingCFG();
                }
            }
            UIHelper.Tooltip("Reverts all preferences in the Viewport Rendering section to their default value.");

            if (ImGui.MenuItem("Model Rendering"))
            {
                var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    CFGHelpers.ResetViewportModelRenderingCFG();
                }
            }
            UIHelper.Tooltip("Reverts all preferences in the Viewport Model Rendering section to their default value.");

            if (ImGui.MenuItem("Coloring"))
            {
                var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    CFGHelpers.ResetViewportColoringCFG();
                }
            }
            UIHelper.Tooltip("Reverts all preferences in the Viewport Coloring section to their default value.");

            if (ImGui.MenuItem("Display Preset"))
            {
                var dialog = PlatformUtils.Instance.MessageBox("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialog is DialogResult.Yes)
                {
                    CFGHelpers.ResetViewportDisplayPresetCFG();
                }
            }
            UIHelper.Tooltip("Reverts all preferences in the Viewport Display Preset section to their default value.");

            ImGui.EndMenu();
        }
    }
}