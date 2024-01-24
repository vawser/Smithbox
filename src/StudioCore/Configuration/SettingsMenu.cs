using ImGuiNET;
using SoapstoneLib;
using StudioCore.Aliases;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using StudioCore.ParamEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;

namespace StudioCore.Settings;

public class SettingsMenu
{
    private string _id;
    private AssetLocator _locator;

    private KeyBind _currentKeyBind;
    public bool MenuOpenState;
    public ModelEditorScreen ModelEditor;
    public MsbEditorScreen MsbEditor;
    public ParamEditorScreen ParamEditor;
    public ProjectSettings ProjSettings = null;
    public TextEditorScreen TextEditor;

    private AliasBank _mapAliasBank;

    private string _searchInput = "";
    private string _searchInputCache = "";

    private string _refUpdateId = "";
    private string _refUpdateName = "";
    private string _refUpdateTags = "";

    private string _newRefId = "";
    private string _newRefName = "";
    private string _newRefTags = "";

    private string _selectedName;

    public SettingsMenu(string id, AssetLocator assetLocator, AliasBank mapAliasBank)
    {
        _id = id;
        _locator = assetLocator;
        _mapAliasBank = mapAliasBank;
    }

    public void SaveSettings()
    {
        CFG.Save();
    }

    private void DisplaySettings_System()
    {
        if (ImGui.BeginTabItem("System"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("When enabled Smithbox will automatically check for new versions upon program start.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Check for new versions of Smithbox during startup",
                    ref CFG.Current.System_Check_Program_Update);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("This is a tooltip.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show UI tooltips", ref CFG.Current.System_Show_UI_Tooltips);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Adjusts the scale of the user interface throughout all of Smithbox.");
                    ImGui.SameLine();
                }
                ImGui.SliderFloat("UI scale", ref CFG.Current.System_UI_Scale, 0.5f, 4.0f);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // Round to 0.05
                    CFG.Current.System_UI_Scale = (float)Math.Round(CFG.Current.System_UI_Scale * 20) / 20;
                    Smithbox.FontRebuildRequest = true;
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset"))
                {
                    CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                    Smithbox.FontRebuildRequest = true;
                }
            }

            if (ImGui.CollapsingHeader("Soapstone Server"))
            {
                var running = SoapstoneServer.GetRunningPort() is int port
                    ? $"running on port {port}"
                    : "not running";
                ImGui.Text(
                    $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
                ImGui.Checkbox("Enable cross-editor features", ref CFG.Current.System_Enable_Soapstone_Server);
            }

            // Additional Language Fonts
            if (ImGui.CollapsingHeader("Additional Language Fonts"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Chinese", ref CFG.Current.System_Font_Chinese))
                    Smithbox.FontRebuildRequest = true;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Korean", ref CFG.Current.System_Font_Korean))
                    Smithbox.FontRebuildRequest = true;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Thai", ref CFG.Current.System_Font_Thai))
                    Smithbox.FontRebuildRequest = true;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Vietnamese", ref CFG.Current.System_Font_Vietnamese))
                    Smithbox.FontRebuildRequest = true;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Cyrillic", ref CFG.Current.System_Font_Cyrillic))
                    Smithbox.FontRebuildRequest = true;
            }

            if (ImGui.CollapsingHeader("Project", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ProjSettings == null || ProjSettings.ProjectName == null)
                {
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ShowHelpMarker("No project has been loaded yet.");
                        ImGui.SameLine();
                    }
                    ImGui.Text("No project loaded");
                }
                else
                    if (TaskManager.AnyActiveTasks())
                {
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ShowHelpMarker("DSMS must finished all program tasks before it can load a project.");
                        ImGui.SameLine();
                    }
                    ImGui.Text("Waiting for program tasks to finish...");
                }
                else
                {
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ShowHelpMarker("This is the currently loaded project.");
                        ImGui.SameLine();
                    }
                    ImGui.Text($@"Project: {ProjSettings.ProjectName}");

                    ImGui.SameLine();
                    if (ImGui.Button("Open project settings file"))
                    {
                        var projectPath = CFG.Current.LastProjectFile;
                        Process.Start("explorer.exe", projectPath);
                    }

                    var useLoose = ProjSettings.UseLooseParams;
                    if (ProjSettings.GameType is GameType.DarkSoulsIISOTFS or GameType.DarkSoulsIII)
                    {
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ShowHelpMarker("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                            ImGui.SameLine();
                        }

                        if (ImGui.Checkbox("Use loose params", ref useLoose))
                            ProjSettings.UseLooseParams = useLoose;
                    }

                    var usepartial = ProjSettings.PartialParams;
                    if (FeatureFlags.EnablePartialParam || usepartial)
                    {
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ShowHelpMarker("Partial params.");
                            ImGui.SameLine();
                        }

                        if (ProjSettings.GameType == GameType.EldenRing &&
                        ImGui.Checkbox("Partial params", ref usepartial))
                            ProjSettings.PartialParams = usepartial;
                    }

                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ShowHelpMarker("Enabling this option will allow unused or debug map names to appear in the scene tree view.");
                        ImGui.SameLine();
                    }
                    ImGui.Checkbox("Show unused map names", ref CFG.Current.MapAliases_ShowUnusedNames);

                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ShowHelpMarker("Toggle the map name list.");
                        ImGui.SameLine();
                    }
                    if (ImGui.Button("Edit map names"))
                    {
                        CFG.Current.MapAliases_ShowMapAliasEditList = !CFG.Current.MapAliases_ShowMapAliasEditList;
                    }

                    if (CFG.Current.MapAliases_ShowMapAliasEditList)
                    {
                        ImGui.Separator();

                        if (ImGui.Button("Toggle Alias Addition"))
                        {
                            CFG.Current.MapAliases_ShowAliasAddition = !CFG.Current.MapAliases_ShowAliasAddition;
                        }

                        ImGui.SameLine();
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            Utils.ShowHelpMarker("When enabled the list will display the tags next to the name.");
                            ImGui.SameLine();
                        }
                        ImGui.Checkbox("Show Tags", ref CFG.Current.MapAliases_ShowTagsInBrowser);

                        ImGui.Separator();

                        if (CFG.Current.MapAliases_ShowAliasAddition)
                        {
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                Utils.ShowHelpMarker("The map ID of the alias to add.");
                                ImGui.SameLine();
                            }
                            ImGui.InputText($"ID", ref _newRefId, 255);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                Utils.ShowHelpMarker("The name of the alias to add.");
                                ImGui.SameLine();
                            }
                            ImGui.InputText($"Name", ref _newRefName, 255);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                Utils.ShowHelpMarker("The tags of the alias to add.\nEach tag should be separated by the ',' character.");
                                ImGui.SameLine();
                            }
                            ImGui.InputText($"Tags", ref _newRefTags, 255);

                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                Utils.ShowHelpMarker("Adds a new alias to the project-specific alias bank.");
                                ImGui.SameLine();
                            }
                            if (ImGui.Button("Add New Alias"))
                            {
                                // Make sure the ref ID is a MSB name
                                if (Regex.IsMatch(_newRefId, @"m\d{2}_\d{2}_\d{2}_\d{2}"))
                                {
                                    bool isValid = true;

                                    var entries = _mapAliasBank.AliasNames.GetEntries("Maps");

                                    foreach (var entry in entries)
                                    {
                                        if (_newRefId == entry.id)
                                            isValid = false;
                                    }

                                    if (isValid)
                                    {
                                        _mapAliasBank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                                        ImGui.CloseCurrentPopup();
                                        _mapAliasBank.mayReloadAliasBank = true;
                                    }
                                    else
                                    {
                                        PlatformUtils.Instance.MessageBox($"Map Alias with {_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }

                            ImGui.Separator();
                        }

                        DisplayMapAliasSelectionList(_mapAliasBank.AliasNames.GetEntries("Maps"));
                    }
                }
            }

            ImGui.EndTabItem();
        }

        if (_mapAliasBank.mayReloadAliasBank)
        {
            _mapAliasBank.mayReloadAliasBank = false;
            _mapAliasBank.ReloadAliasBank();
        }
    }

    private void DisplayMapAliasSelectionList(List<AliasReference> referenceList)
    {
        Dictionary<string, AliasReference> referenceDict = new Dictionary<string, AliasReference>();

        foreach (AliasReference v in referenceList)
        {
            if (!referenceDict.ContainsKey(v.id))
                referenceDict.Add(v.id, v);
        }

        if (_searchInput != _searchInputCache)
        {
            _searchInputCache = _searchInput;
        }

        var entries = _mapAliasBank.AliasNames.GetEntries("Maps");

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Skip the unused names if this is disabled
            if (!CFG.Current.MapAliases_ShowUnusedNames)
            {
                if (refTagList[0] == "unused")
                    continue;
            }

            // Append tags to to displayed name
            if (CFG.Current.MapAliases_ShowTagsInBrowser)
            {
                var tagString = string.Join(" ", refTagList);
                displayedName = $"{displayedName} {{ {tagString} }}";
            }

            if (Utils.IsMapSearchFilterMatch(_searchInput, refID, refName, refTagList))
            {
                if (ImGui.Selectable(displayedName))
                {
                    _selectedName = refID;
                    _refUpdateId = refID;
                    _refUpdateName = refName;

                    if (refTagList.Count > 0)
                    {
                        string tagStr = refTagList[0];
                        foreach (string tEntry in refTagList.Skip(1))
                        {
                            tagStr = $"{tagStr},{tEntry}";
                        }
                        _refUpdateTags = tagStr;
                    }
                    else
                    {
                        _refUpdateTags = "";
                    }
                }

                if (_selectedName == refID)
                {
                    if (ImGui.BeginPopupContextItem($"{refID}##context"))
                    {
                        ImGui.InputText($"Name", ref _refUpdateName, 255);
                        ImGui.InputText($"Tags", ref _refUpdateTags, 255);

                        if (ImGui.Button("Update"))
                        {
                            _mapAliasBank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                            ImGui.CloseCurrentPopup();
                            _mapAliasBank.mayReloadAliasBank = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Restore Default"))
                        {
                            _mapAliasBank.RemoveFromLocalAliasBank("", _refUpdateId);
                            ImGui.CloseCurrentPopup();
                            _mapAliasBank.mayReloadAliasBank = true;
                        }

                        ImGui.EndPopup();
                    }
                }

                if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                {
                }
            }
        }
    }

    private void DisplaySettings_MapEditor()
    {
        if (ImGui.BeginTabItem("Map Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Enabling this option will cause entities outside of the camera frustrum to be culled.\n\nDisable this if working with the grid.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable frustrum culling", ref CFG.Current.Map_Enable_Frustum_Culling);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Enabling this option will cause a selection outline to appear on selected objects.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable selection outline", ref CFG.Current.Map_Enable_Selection_Outline);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Enabling this option will allow DSMS to render the textures of models within the viewport.\n\nNote, this feature is in an alpha state.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable texturing", ref CFG.Current.Map_Enable_Texturing);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.Map_Always_List_Loaded_Maps);

                if (ProjSettings != null)
                    if (ProjSettings.GameType is GameType.EldenRing)
                    {
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ShowHelpMarker("");
                            ImGui.SameLine();
                        }
                        ImGui.Checkbox("Enable Elden Ring auto map offset", ref CFG.Current.Map_Enable_ER_Auto_Map_Offset);
                    }
            }

            if(ImGui.CollapsingHeader("Scene View"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Characters names will be displayed within the scene view list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Display character names", ref CFG.Current.Map_Show_Character_Names_in_Scene_Tree);
            }

            if (ImGui.CollapsingHeader("Camera"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Resets all of the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset##ViewportCamera"))
                {
                    CFG.Current.GFX_Camera_FOV = CFG.Default.GFX_Camera_FOV;

                    CFG.Current.GFX_RenderDistance_Max = CFG.Default.GFX_RenderDistance_Max;

                    MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow = CFG.Default.GFX_Camera_MoveSpeed_Slow;
                    CFG.Current.GFX_Camera_MoveSpeed_Slow = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                    CFG.Current.GFX_Camera_Sensitivity = CFG.Default.GFX_Camera_Sensitivity;

                    MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal = CFG.Default.GFX_Camera_MoveSpeed_Normal;
                    CFG.Current.GFX_Camera_MoveSpeed_Normal = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                    MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast = CFG.Default.GFX_Camera_MoveSpeed_Fast;
                    CFG.Current.GFX_Camera_MoveSpeed_Fast = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                }

                var cam_fov = CFG.Current.GFX_Camera_FOV;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Set the field of view used by the camera within DSMS.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                    CFG.Current.GFX_Camera_FOV = cam_fov;

                var cam_sensitivity = CFG.Current.GFX_Camera_Sensitivity;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Mouse sensitivty for turning the camera.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Camera sensitivity", ref cam_sensitivity, 0.0f, 0.1f))
                {
                    CFG.Current.GFX_Camera_Sensitivity = cam_sensitivity;
                }

                var farClip = CFG.Current.GFX_RenderDistance_Max;
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Set the maximum distance at which entities will be rendered within the DSMS viewport.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map max render distance", ref farClip, 10.0f, 500000.0f))
                    CFG.Current.GFX_RenderDistance_Max = farClip;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map camera speed (slow)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow, 0.1f, 999.0f))
                    CFG.Current.GFX_Camera_MoveSpeed_Slow = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Set the speed at which the camera will move whilst moving normally.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map camera speed (normal)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal, 0.1f, 999.0f))
                    CFG.Current.GFX_Camera_MoveSpeed_Normal = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map camera speed (fast)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast, 0.1f, 999.0f))
                    CFG.Current.GFX_Camera_MoveSpeed_Fast = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
            }

            if (ImGui.CollapsingHeader("Limits"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Reset the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset##MapLimits"))
                {
                    CFG.Current.GFX_Limit_Renderables = CFG.Default.GFX_Limit_Renderables;
                    CFG.Current.GFX_Limit_Buffer_Indirect_Draw = CFG.Default.GFX_Limit_Buffer_Indirect_Draw;
                    CFG.Current.GFX_Limit_Buffer_Flver_Bone = CFG.Default.GFX_Limit_Buffer_Flver_Bone;
                }

                ImGui.Text("Please restart the program for changes to take effect.");

                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    @"Try smaller increments (+25%%) at first, as high values will cause issues.");

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");
                    ImGui.SameLine();
                }
                if (ImGui.InputInt("Renderables", ref CFG.Current.GFX_Limit_Renderables, 0, 0))
                    if (CFG.Current.GFX_Limit_Renderables < CFG.Default.GFX_Limit_Renderables)
                        CFG.Current.GFX_Limit_Renderables = CFG.Default.GFX_Limit_Renderables;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");
                    ImGui.SameLine();
                }
                Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.GFX_Limit_Buffer_Indirect_Draw);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");
                    ImGui.SameLine();
                }
                Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.GFX_Limit_Buffer_Flver_Bone);
            }

            if (ImGui.CollapsingHeader("Grid"))
            {
                if(ImGui.Button("Regenerate"))
                {
                    CFG.Current.Viewport_RegenerateMapGrid = true;
                }

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Enable the viewport grid when in the Map Editor.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable viewport grid", ref CFG.Current.Viewport_EnableGrid);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");
                    ImGui.SameLine();
                }
                ImGui.SliderInt("Grid size", ref CFG.Current.Viewport_Grid_Size, 100, 1000);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("The increment size of the grid.");
                    ImGui.SameLine();
                }
                ImGui.SliderInt("Grid increment", ref CFG.Current.Viewport_Grid_Square_Size, 1, 100);

                var height = CFG.Current.Viewport_Grid_Height;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("The height at which the horizontal grid sits.");
                    ImGui.SameLine();
                }
                ImGui.InputFloat("Grid height", ref height);

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.Viewport_Grid_Height = height;

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("The amount to lower or raise the viewport grid height via the shortcuts.");
                    ImGui.SameLine();
                }
                ImGui.SliderFloat("Grid height increment", ref CFG.Current.Viewport_Grid_Height_Increment, 0.1f, 100);

                ImGui.ColorEdit3("Grid color", ref CFG.Current.GFX_Viewport_Grid_Color);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Resets all of the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset"))
                {
                    CFG.Current.GFX_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.Viewport_Grid_Size = 1000;
                    CFG.Current.Viewport_Grid_Square_Size = 10;
                    CFG.Current.Viewport_Grid_Height = 0;
                }
            }

            if (ImGui.CollapsingHeader("Wireframes"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Resets all of the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset"))
                {
                    // Proxies
                    CFG.Current.GFX_Renderable_Box_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Box_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Cylinder_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Cylinder_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Sphere_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Sphere_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Point_BaseColor = Utils.GetDecimalColor(Color.Yellow);
                    CFG.Current.GFX_Renderable_Point_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_DummyPoly_BaseColor = Utils.GetDecimalColor(Color.Yellow);
                    CFG.Current.GFX_Renderable_DummyPoly_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_BonePoint_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_BonePoint_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor = Utils.GetDecimalColor(Color.Firebrick);
                    CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor = Utils.GetDecimalColor(Color.Tomato);

                    CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor = Utils.GetDecimalColor(Color.MediumVioletRed);
                    CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor = Utils.GetDecimalColor(Color.DeepPink);

                    CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor = Utils.GetDecimalColor(Color.DarkOliveGreen);
                    CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor = Utils.GetDecimalColor(Color.OliveDrab);

                    CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor = Utils.GetDecimalColor(Color.Wheat);
                    CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor = Utils.GetDecimalColor(Color.AntiqueWhite);

                    CFG.Current.GFX_Renderable_PointLight_BaseColor = Utils.GetDecimalColor(Color.YellowGreen);
                    CFG.Current.GFX_Renderable_PointLight_HighlightColor = Utils.GetDecimalColor(Color.Yellow);

                    CFG.Current.GFX_Renderable_SpotLight_BaseColor = Utils.GetDecimalColor(Color.Goldenrod);
                    CFG.Current.GFX_Renderable_SpotLight_HighlightColor = Utils.GetDecimalColor(Color.Violet);

                    CFG.Current.GFX_Renderable_DirectionalLight_BaseColor = Utils.GetDecimalColor(Color.Cyan);
                    CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor = Utils.GetDecimalColor(Color.AliceBlue);

                    // Gizmos
                    CFG.Current.GFX_Gizmo_X_BaseColor = new Vector3(0.952f, 0.211f, 0.325f);
                    CFG.Current.GFX_Gizmo_X_HighlightColor = new Vector3(1.0f, 0.4f, 0.513f);

                    CFG.Current.GFX_Gizmo_Y_BaseColor = new Vector3(0.525f, 0.784f, 0.082f);
                    CFG.Current.GFX_Gizmo_Y_HighlightColor = new Vector3(0.713f, 0.972f, 0.270f);

                    CFG.Current.GFX_Gizmo_Z_BaseColor = new Vector3(0.219f, 0.564f, 0.929f);
                    CFG.Current.GFX_Gizmo_Z_HighlightColor = new Vector3(0.407f, 0.690f, 1.0f);

                    // Color Variance
                    CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
                }

                ImGui.SliderFloat("Wireframe color variance", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);

                // Proxies
                ImGui.ColorEdit3("Box region - base color", ref CFG.Current.GFX_Renderable_Box_BaseColor);
                ImGui.ColorEdit3("Box region - highlight color", ref CFG.Current.GFX_Renderable_Box_HighlightColor);

                ImGui.ColorEdit3("Cylinder region - base color", ref CFG.Current.GFX_Renderable_Cylinder_BaseColor);
                ImGui.ColorEdit3("Cylinder region - highlight color", ref CFG.Current.GFX_Renderable_Cylinder_HighlightColor);

                ImGui.ColorEdit3("Sphere region - base color", ref CFG.Current.GFX_Renderable_Sphere_BaseColor);
                ImGui.ColorEdit3("Sphere region - highlight color", ref CFG.Current.GFX_Renderable_Sphere_HighlightColor);

                ImGui.ColorEdit3("Point region - base color", ref CFG.Current.GFX_Renderable_Point_BaseColor);
                ImGui.ColorEdit3("Point region - highlight color", ref CFG.Current.GFX_Renderable_Point_HighlightColor);

                ImGui.ColorEdit3("Dummy poly - base color", ref CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
                ImGui.ColorEdit3("Dummy poly - highlight color", ref CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);

                ImGui.ColorEdit3("Bone point - base color", ref CFG.Current.GFX_Renderable_BonePoint_BaseColor);
                ImGui.ColorEdit3("Bone point - highlight color", ref CFG.Current.GFX_Renderable_BonePoint_HighlightColor);

                ImGui.ColorEdit3("Chr marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
                ImGui.ColorEdit3("Chr marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);

                ImGui.ColorEdit3("Object marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
                ImGui.ColorEdit3("Object marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);

                ImGui.ColorEdit3("Player marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
                ImGui.ColorEdit3("Player marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);

                ImGui.ColorEdit3("Other marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
                ImGui.ColorEdit3("Other marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);

                ImGui.ColorEdit3("Point light - base color", ref CFG.Current.GFX_Renderable_PointLight_BaseColor);
                ImGui.ColorEdit3("Point light - highlight color", ref CFG.Current.GFX_Renderable_PointLight_HighlightColor);

                ImGui.ColorEdit3("Spot light - base color", ref CFG.Current.GFX_Renderable_SpotLight_BaseColor);
                ImGui.ColorEdit3("Spot light - highlight color", ref CFG.Current.GFX_Renderable_SpotLight_HighlightColor);

                ImGui.ColorEdit3("Directional light - base color", ref CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
                ImGui.ColorEdit3("Directional light - highlight color", ref CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);

                // Gizmos
                ImGui.ColorEdit3("Gizmo - X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
                ImGui.ColorEdit3("Gizmo - X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

                ImGui.ColorEdit3("Gizmo - Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
                ImGui.ColorEdit3("Gizmo - Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

                ImGui.ColorEdit3("Gizmo - Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
                ImGui.ColorEdit3("Gizmo - Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);
            }

            if (ImGui.CollapsingHeader("Map Object Display Presets"))
            {
                ImGui.Text("Configure each of the six display presets available.");

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Reset the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset##DisplayPresets"))
                {
                    CFG.Current.SceneFilter_Preset_01.Name = CFG.Default.SceneFilter_Preset_01.Name;
                    CFG.Current.SceneFilter_Preset_01.Filters = CFG.Default.SceneFilter_Preset_01.Filters;
                    CFG.Current.SceneFilter_Preset_02.Name = CFG.Default.SceneFilter_Preset_02.Name;
                    CFG.Current.SceneFilter_Preset_02.Filters = CFG.Default.SceneFilter_Preset_02.Filters;
                    CFG.Current.SceneFilter_Preset_03.Name = CFG.Default.SceneFilter_Preset_03.Name;
                    CFG.Current.SceneFilter_Preset_03.Filters = CFG.Default.SceneFilter_Preset_03.Filters;
                    CFG.Current.SceneFilter_Preset_04.Name = CFG.Default.SceneFilter_Preset_04.Name;
                    CFG.Current.SceneFilter_Preset_04.Filters = CFG.Default.SceneFilter_Preset_04.Filters;
                    CFG.Current.SceneFilter_Preset_05.Name = CFG.Default.SceneFilter_Preset_05.Name;
                    CFG.Current.SceneFilter_Preset_05.Filters = CFG.Default.SceneFilter_Preset_05.Filters;
                    CFG.Current.SceneFilter_Preset_06.Name = CFG.Default.SceneFilter_Preset_06.Name;
                    CFG.Current.SceneFilter_Preset_06.Filters = CFG.Default.SceneFilter_Preset_06.Filters;
                }

                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_ModelEditor()
    {
        if (ImGui.BeginTabItem("Model Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {

            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_ParamEditor()
    {
        if (ImGui.BeginTabItem("Param Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Reduces the line height within the the Param Editor screen.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Show additional options for advanced users within the massedit popup.");
                    ImGui.SameLine();
                }

                ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Show the shortcut tools in the right-click context menu.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show shortcut tools in context menus", ref CFG.Current.Param_ShowHotkeysInContextMenu);
            }

            if (ImGui.CollapsingHeader("Params"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Sort the Param View list alphabetically.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
            }

            if (ImGui.CollapsingHeader("Rows"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Disable the row names from wrapping within the Row View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
            }

            if (ImGui.CollapsingHeader("Fields"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Crowd-sourced names will appear before the canonical name in the Field View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("The field offset within the .PARAM file will be show to the left in the Field View List.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Hide the generated param references for fields that link to other params.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Hide the crowd-sourced namelist for index-based enum fields.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Allow the field order to be changed by an alternative order as defined within the Paramdex META file.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Repeat the field name in the context menu.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Field name in context menu", ref CFG.Current.Param_FieldNameInContextMenu);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Repeat the field description in the context menu.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Field description in context menu", ref CFG.Current.Param_FieldDescriptionInContextMenu);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker(@"If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.
If disabled, simply shows a shortcut to the manual massedit entry element.
(The full menu is still available from the manual popup)");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Full massedit popup in context menu", ref CFG.Current.Param_MasseditPopupInContextMenu);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Split the field context menu into separate menus for separate right-click locations.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Split context menu", ref CFG.Current.Param_SplitContextMenu);
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_TextEditor()
    {
        if (ImGui.BeginTabItem("Text Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Show the original FMG file names within the Text Editor file list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show original FMG names", ref CFG.Current.FMG_ShowOriginalNames);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("If enabled then FMG entries will not be grouped automatically.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
                    TextEditor.OnProjectChanged(ProjSettings);

                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("If enabled then FMG files added from DLCs will not be grouped with vanilla FMG files.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
                    TextEditor.OnProjectChanged(ProjSettings);
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Browsers()
    {
        if (ImGui.BeginTabItem("Browsers"))
        {
            if (ImGui.CollapsingHeader("Asset Browser", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show tags", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
            }

            if (ImGui.CollapsingHeader("Flag ID Browser"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show tags", ref CFG.Current.EventFlagBrowser_ShowTagsInBrowser);
            }

            if (ImGui.CollapsingHeader("Particle ID Browser"))
            {
                if (CFG.Current.System_Show_UI_Tooltips)
                {
                    ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show tags", ref CFG.Current.ParticleBrowser_ShowTagsInBrowser);
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Keybinds()
    {
        if (ImGui.BeginTabItem("Keybinds"))
        {
            if (ImGui.IsAnyItemActive())
                _currentKeyBind = null;

            FieldInfo[] binds = KeyBindings.Current.GetType().GetFields();
            foreach (FieldInfo bind in binds)
            {
                var bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);
                ImGui.Text(bind.Name);

                ImGui.SameLine();
                ImGui.Indent(250f);

                var keyText = bindVal.HintText;
                if (keyText == "")
                    keyText = "[None]";

                if (_currentKeyBind == bindVal)
                {
                    ImGui.Button("Press Key <Esc - Clear>");
                    if (InputTracker.GetKeyDown(Key.Escape))
                    {
                        bind.SetValue(KeyBindings.Current, new KeyBind());
                        _currentKeyBind = null;
                    }
                    else
                    {
                        KeyBind newkey = InputTracker.GetNewKeyBind();
                        if (newkey != null)
                        {
                            bind.SetValue(KeyBindings.Current, newkey);
                            _currentKeyBind = null;
                        }
                    }
                }
                else if (ImGui.Button($"{keyText}##{bind.Name}"))
                    _currentKeyBind = bindVal;

                ImGui.Indent(-250f);
            }

            ImGui.Separator();

            if (ImGui.Button("Restore defaults"))
                KeyBindings.ResetKeyBinds();

            ImGui.EndTabItem();
        }
    }
    public void Display()
    {
        var scale = Smithbox.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Settings Menu##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#SettingsMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.6f, 0.4f));
            ImGui.PushItemWidth(300f);

            // Settings Order
            DisplaySettings_System();
            DisplaySettings_MapEditor();
            //DisplaySettings_ModelEditor();
            DisplaySettings_ParamEditor();
            DisplaySettings_TextEditor();
            DisplaySettings_Browsers();
            DisplaySettings_Keybinds();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);
    }

    public void ShowHelpMarker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(450.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    private void SettingsRenderFilterPresetEditor(CFG.RenderFilterPreset preset)
    {
        ImGui.PushID($"{preset.Name}##PresetEdit");
        if (ImGui.CollapsingHeader($"{preset.Name}##Header"))
        {
            ImGui.Indent();
            var nameInput = preset.Name;
            ImGui.InputText("Preset Name", ref nameInput, 32);
            if (ImGui.IsItemDeactivatedAfterEdit())
                preset.Name = nameInput;

            foreach (RenderFilter e in Enum.GetValues(typeof(RenderFilter)))
            {
                var ticked = false;
                if (preset.Filters.HasFlag(e))
                    ticked = true;

                if (ImGui.Checkbox(e.ToString(), ref ticked))
                    if (ticked)
                        preset.Filters |= e;
                    else
                        preset.Filters &= ~e;
            }

            ImGui.Unindent();
        }

        ImGui.PopID();
    }
}
