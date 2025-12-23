using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Application;

//------------------------------------------
// System
//------------------------------------------
#region System
public class SystemTab
{
    public Smithbox BaseEditor;

    public SystemTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        var width = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Check for new versions of Smithbox during startup",
                ref CFG.Current.System_Check_Program_Update);
            UIHelper.Tooltip("When enabled Smithbox will automatically check for new versions upon program start.");

            ImGui.Separator();

            UIHelper.WrappedText("By default, files are read by Smithbox in a strict manner. Data that is present in locations that it should not be will throw an exception.");

            UIHelper.WrappedText("This option will remove that strictness, and will cause Smithbox to ignore the invalid data when reading a file.");

            ImGui.Checkbox("Ignore Read asserts", ref CFG.Current.System_IgnoreAsserts);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;
            }
            UIHelper.Tooltip("If enabled, when attempting to read files, asserts will be ignored.");

            ImGui.Checkbox("Use DCX Heuristic", ref CFG.Current.System_UseDCXHeuristicOnReadFailure);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                BinaryReaderEx.UseDCXHeuristicOnReadFailure = CFG.Current.System_UseDCXHeuristicOnReadFailure;
            }
            UIHelper.Tooltip("If enabled, if a DCX fails to read its compression type, use a heuristic to guess which it should be instead.");
        }

        if (ImGui.CollapsingHeader("Project"))
        {
            ImGui.Checkbox("Enable Automatic Saves", ref CFG.Current.EnableAutomaticSave);
            UIHelper.Tooltip("If enabled, all enabled editors will automatically save.");

            ImGui.SliderFloat("Automatic Save Interval", ref CFG.Current.AutomaticSaveIntervalTime, 5f, 3600f);
            UIHelper.Tooltip("The rate at which the automatic save occurs. In seconds.");

            ImGui.Separator();

            ImGui.Text("Editors to Automatically Save:");

            ImGui.Separator();

            ImGui.Checkbox("Map Editor##autoSave_mapEditor", ref CFG.Current.AutomaticSave_MapEditor);
            UIHelper.Tooltip("If enabled, the Map Editor is automatically saved.");

            ImGui.Checkbox("Param Editor##autoSave_paramEditor", ref CFG.Current.AutomaticSave_ParamEditor);
            UIHelper.Tooltip("If enabled, the Param Editor is automatically saved.");

            ImGui.Checkbox("Text Editor##autoSave_textEditor", ref CFG.Current.AutomaticSave_TextEditor);
            UIHelper.Tooltip("If enabled, the Text Editor is automatically saved.");

            ImGui.Checkbox("Graphics Param Editor##autoSave_gparamEditor", ref CFG.Current.AutomaticSave_GparamEditor);
            UIHelper.Tooltip("If enabled, the Graphics Param Editor is automatically saved.");

            ImGui.Checkbox("Material Editor##autoSave_materialEditor", ref CFG.Current.AutomaticSave_MaterialEditor);
            UIHelper.Tooltip("If enabled, the Material Editor is automatically saved.");

            ImGui.Separator();

            ImGui.Checkbox("Enable Backup Saves", ref CFG.Current.EnableBackupSaves);
            UIHelper.Tooltip("If enabled, the .prev and .bak files will be produced when saving.");

            if(ImGui.Button("Clear Backup Files", DPI.WholeWidthButton(width, 24)))
            {
                var root = BaseEditor.ProjectManager.SelectedProject.ProjectPath;

                var filesToDelete = GetBackupFiles(root);

                var fileList = "";

                int i = 0;

                foreach(var entry in filesToDelete)
                {
                    fileList = fileList + $"\n{entry}";

                    i++;

                    if (i > 25)
                    {
                        fileList = fileList + $"\n....";
                        break;
                    }
                }

                var dialog = PlatformUtils.Instance.MessageBox($"You will delete the following files:\n{fileList}", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
               
                if(dialog is DialogResult.OK)
                {
                    DeleteFiles(filesToDelete);
                }
            }
            UIHelper.Tooltip("This will clear all files with the .prev and .bak extension from your project.");

            ImGui.Separator();

            // Project Name Prefix
            ImGui.Checkbox("Display Project Type Prefix in Project List", ref CFG.Current.DisplayProjectPrefix);
            UIHelper.Tooltip("If enabled, the prefix for the project type will be displayed in the project list for each project.");

            // Default Project Directory
            if (ImGui.Button("Select##projectDirSelect", DPI.StandardButtonSize))
            {
                var newProjectPath = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath);

                if (result)
                {
                    CFG.Current.DefaultModDirectory = newProjectPath;
                }
            }

            ImGui.SameLine();

            DPI.ApplyInputWidth();
            ImGui.InputText("Default Project Directory", ref CFG.Current.DefaultModDirectory, 255);
            UIHelper.Tooltip("The default directory to use during the project directory selection when creating a new project.");

            // Default Data Directory
            if (ImGui.Button("Select##ProjectDataDirSelect", DPI.StandardButtonSize))
            {
                var newDataPath = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Data Directory", out newDataPath);

                if (result)
                {
                    CFG.Current.DefaultDataDirectory = newDataPath;
                }
            }

            ImGui.SameLine();

            DPI.ApplyInputWidth();
            ImGui.InputText("Default Data Directory", ref CFG.Current.DefaultDataDirectory, 255);
            UIHelper.Tooltip("The default directory to use during the data directory selection when creating a new project.");

            ImGui.Separator();

            // ME3 Setup
            if (ImGui.Button("Select##modEngine3PathSelect", DPI.StandardButtonSize))
            {
                var profilePath = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select ME3 Profile Directory", out profilePath);

                if (result)
                {
                    CFG.Current.ModEngine3ProfileDirectory = profilePath;
                }
            }

            ImGui.SameLine();

            ImGui.InputText("ME3 Profile Directory##me3ProfileDir", ref CFG.Current.ModEngine3ProfileDirectory, 255);
            UIHelper.Tooltip("Select the directory you want the generated ME3 profiles to be placed in.");

            ImGui.Separator();

            // ME2 Setup
            if (ImGui.Button("Select##modEngine2PathSelect", DPI.StandardButtonSize))
            {
                var modEnginePath = "";
                var result = PlatformUtils.Instance.OpenFileDialog("Select ME2 Executable", ["exe"], out modEnginePath);

                if (result)
                {
                    if (modEnginePath.Contains("modengine2_launcher.exe"))
                    {
                        CFG.Current.ModEngine2Install = modEnginePath;
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("Error", "The file you selected was not modengine2_launcher.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            ImGui.SameLine();

            DPI.ApplyInputWidth();
            ImGui.InputText("ME2 Executable Location##modEnginePath", ref CFG.Current.ModEngine2Install, 255);
            UIHelper.Tooltip("Select the modengine2_launcher.exe within your ModEngine2 install folder.");

            // ME2 Dlls
            DPI.ApplyInputWidth();
            ImGui.InputText("ME2 DLL Entries##modEngineDllEntries", ref CFG.Current.ModEngine2Dlls, 255);
            UIHelper.Tooltip("The relative paths of the DLLs to include in the 'Launch Mod' action. Separate them by a space if using multiple.");


            ImGui.Separator();

            if (ImGui.Button("Clear Auto-Load##clearProjectAutoload", DPI.StandardButtonSize))
            {
                foreach (var project in BaseEditor.ProjectManager.Projects)
                {
                    project.AutoSelect = false;

                    BaseEditor.ProjectManager.SaveProject(project);
                }
            }
            UIHelper.Tooltip("Clear the project that has been set as primary, so no project is loaded on Smithbox start.");
        }

        if (ImGui.CollapsingHeader("Loggers"))
        {
            ImGui.Checkbox("Show Action Logger", ref CFG.Current.System_ShowActionLogger);
            UIHelper.Tooltip("If enabled, the action logger will be visible in the menu bar.");

            ImGui.InputInt("Action Log Visibility Duration", ref CFG.Current.System_ActionLogger_FadeTime);
            UIHelper.Tooltip("The number of frames for which the action logger message stays visible in the menu bar.\n-1 means the message never disappears.");

            ImGui.Separator();

            ImGui.Checkbox("Show Warning Logger", ref CFG.Current.System_ShowWarningLogger);
            UIHelper.Tooltip("If enabled, the warning logger will be visible in the menu bar.");

            ImGui.InputInt("Warning Log Visibility Duration", ref CFG.Current.System_WarningLogger_FadeTime);
            UIHelper.Tooltip("The number of frames for which the warning logger message stays visible in the menu bar.\n-1 means the message never disappears.");
        }

        if (ImGui.CollapsingHeader("Developer"))
        {
            ImGui.Checkbox("Enable Developer Tools",
                ref CFG.Current.EnableDeveloperTools);
            UIHelper.Tooltip("Enables various tools meant for Smithbox developers only.");

            ImGui.Separator();

            if (ImGui.Button("Select##smithboxBuildDirSelect", DPI.StandardButtonSize))
            {
                var smithboxBuildDir = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Build directory", out smithboxBuildDir);

                if (result)
                {
                    CFG.Current.SmithboxBuildFolder = smithboxBuildDir;
                }
            }

            ImGui.SameLine();

            ImGui.InputText("Smithbox Build Directory##smithboxBuildDir", ref CFG.Current.SmithboxBuildFolder, 255);
            UIHelper.Tooltip("Select the build directory for Smithbox (where the Smithbox.sln is placed).");
        }
    }
    public static List<string> GetBackupFiles(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"Directory not found: {rootDirectory}");

        var results = new List<string>();

        foreach (var file in Directory.EnumerateFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(file);

            if (ext.Equals(".bak", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".prev", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(file);
            }
        }

        return results;
    }

    public static void DeleteFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                // Log or handle as needed
                Console.WriteLine($"Failed to delete {file}: {ex.Message}");
            }
        }
    }
}
#endregion

//------------------------------------------
// Map Editor
//------------------------------------------
#region Map Editor
public class MapEditorTab
{
    public Smithbox BaseEditor;

    public MapEditorTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        // General
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Allow map unload", ref CFG.Current.MapEditor_EnableMapUnload);
            UIHelper.Tooltip("When enabled, a map's resources will be unloaded and released when a map is unloaded. If disabled, they are kept in memory until Smithbox closes.");

            ImGui.Checkbox("Enable map load on double-click", ref CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click);
            UIHelper.Tooltip("This option will cause double-clicking on a map in the map object list to load it.");

            ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.MapEditor_Always_List_Loaded_Maps);
            UIHelper.Tooltip("This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");

            ImGui.Checkbox("Enable global property search", ref CFG.Current.MapEditor_LoadMapQueryData);
            UIHelper.Tooltip("This option will allow the global property search to be used. Note, this will load all map files into memory.\nYou need to restart Smithbox after enabling this.");

            ImGui.Checkbox("Wrap alias text", ref CFG.Current.Interface_MapEditor_WrapAliasDisplay);
            UIHelper.Tooltip("Makes the alias text display wrap instead of being cut off within the Map Editor.");

        }

        // Map Collision
        if (BaseEditor.ProjectManager.SelectedProject.ProjectType is ProjectType.DS1R)
        {
            if (ImGui.CollapsingHeader("Map Collision", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var width = ImGui.GetWindowWidth();

                UIHelper.WrappedText("Select the install directory for your Dark Souls: Prepare to Die Edition. This will allow collision to be visible within a Dark Souls: Remastered project.");

                ImGui.InputText("PTDE Game Directory##ptdeGameDirectory", ref CFG.Current.PTDE_Collision_Root, 255);
                UIHelper.Tooltip("Select the directory of the Dark Souls: Prepare to Die Edition install.");

                ImGui.Separator();

                if (ImGui.Button("Select##ptdeGameDirectorySelect", DPI.StandardButtonSize))
                {
                    var ptdeDir = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select PTDE directory", out ptdeDir);

                    if (result)
                    {
                        CFG.Current.PTDE_Collision_Root = ptdeDir;
                    }
                }
            }
        }

        // Scene View
        if (ImGui.CollapsingHeader("Map Object List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display list sorting type", ref CFG.Current.MapEditor_MapObjectList_ShowListSortingType);
            UIHelper.Tooltip("Display the list sorting type combo box.");

            ImGui.Checkbox("Display map id search", ref CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch);
            UIHelper.Tooltip("Display the map id search text box.");

            ImGui.Checkbox("Display map content search", ref CFG.Current.MapEditor_MapObjectList_ShowMapContentSearch);
            UIHelper.Tooltip("Display the map object list search text box.");

            ImGui.Checkbox("Display map groups interface", ref CFG.Current.MapEditor_ShowMapGroups);
            UIHelper.Tooltip("Display the map group drop-downs.");

            ImGui.Checkbox("Display world map interface", ref CFG.Current.MapEditor_ShowWorldMapButtons);
            UIHelper.Tooltip("Display the world map buttons.");

            ImGui.Checkbox("Display map categories", ref CFG.Current.MapEditor_DisplayMapCategories);
            UIHelper.Tooltip("If defined, display maps in their assigned map category groupings.");

            ImGui.Separator();

            ImGui.Checkbox("Display map names", ref CFG.Current.MapEditor_MapObjectList_ShowMapNames);
            UIHelper.Tooltip("Map names will be displayed within the scene view list.");

            ImGui.Checkbox("Display character names", ref CFG.Current.MapEditor_MapObjectList_ShowCharacterNames);
            UIHelper.Tooltip("Characters names will be displayed within the scene view list.");

            ImGui.Checkbox("Display asset names", ref CFG.Current.MapEditor_MapObjectList_ShowAssetNames);
            UIHelper.Tooltip("Asset/object names will be displayed within the scene view list.");

            ImGui.Checkbox("Display map piece names", ref CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames);
            UIHelper.Tooltip("Map piece names will be displayed within the scene view list.");

            ImGui.Checkbox("Display treasure names", ref CFG.Current.MapEditor_MapObjectList_ShowTreasureNames);
            UIHelper.Tooltip("Treasure itemlot names will be displayed within the scene view list.");
        }

        // Property View
        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display community names", ref CFG.Current.MapEditor_Enable_Commmunity_Names);
            UIHelper.Tooltip("The MSB property fields will be given crowd-sourced names instead of the canonical name.");

            ImGui.Checkbox("Display unknown fields", ref CFG.Current.MapEditor_DisplayUnknownFields);
            UIHelper.Tooltip("The MSB property fields that are considered unknown.");

            ImGui.Checkbox("Display obsolete fields", ref CFG.Current.MapEditor_Enable_Obsolete_Fields);
            UIHelper.Tooltip("The MSB property fields that are considered obsolete.");

            ImGui.Checkbox("Enable complex rename", ref CFG.Current.MapEditor_Enable_Referenced_Rename);
            UIHelper.Tooltip("This option will allow renaming an object to also rename every reference to it, but will require a confirmation to apply a rename");

            ImGui.Checkbox("Display property info", ref CFG.Current.MapEditor_Enable_Property_Info);
            UIHelper.Tooltip("The MSB property fields show the property info, such as minimum and maximum values, when right-clicked.");

            ImGui.Checkbox("Display property filter", ref CFG.Current.MapEditor_Enable_Property_Filter);
            UIHelper.Tooltip("The MSB property filter combo-box will be visible.");
        }

        // Asset Browser
        if (ImGui.CollapsingHeader("Asset Browser", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display aliases in list", ref CFG.Current.MapEditor_AssetBrowser_ShowAliases);
            UIHelper.Tooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

            ImGui.Checkbox("Display tags in list", ref CFG.Current.MapEditor_AssetBrowser_ShowTags);
            UIHelper.Tooltip("Show the tags for each entry within the browser list as part of their displayed name.");

            ImGui.Checkbox("Display low detail Parts in list", ref CFG.Current.MapEditor_AssetBrowser_ShowLowDetailParts);
            UIHelper.Tooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
        }

        // Additional Property Information
        if (ImGui.CollapsingHeader("Additional Property Information", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display additional property information at the top", ref CFG.Current.MapEditor_Enable_Property_Property_TopDecoration);
            UIHelper.Tooltip("The additional property information will be displayed at the top of the properties window. By default they will appear at the bottom.");

            ImGui.Checkbox("Display information about Map Object class", ref CFG.Current.MapEditor_Enable_Property_Property_Class_Info);
            UIHelper.Tooltip("The MSB property view will display additional information relating to the map object's class.");

            ImGui.Checkbox("Display information about specific properties", ref CFG.Current.MapEditor_Enable_Property_Property_SpecialProperty_Info);
            UIHelper.Tooltip("The MSB property view will display additional information relating to specific properties, such as the alias for a Map ID a property or set of properties represents.");

            ImGui.Checkbox("Display references by for the Map Object", ref CFG.Current.MapEditor_Enable_Property_Property_ReferencesBy);
            UIHelper.Tooltip("The MSB property view will display references by the selected map object.");

            ImGui.Checkbox("Display references to the Map Object", ref CFG.Current.MapEditor_Enable_Property_Property_ReferencesTo);
            UIHelper.Tooltip("The MSB property view will display references to the selected map object.");

            ImGui.Checkbox("Display quick-links to params used by the Map Object", ref CFG.Current.MapEditor_Enable_Param_Quick_Links);
            UIHelper.Tooltip("The MSB property view will display quick-links to related params pointed to within the properties for the selected map object.");
        }

        // References
        if (ImGui.CollapsingHeader("References", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Name", ref CFG.Current.MsbReference_DisplayName);
            UIHelper.Tooltip("If enabled, map object references will display the name of the reference.");

            ImGui.Checkbox("Display Entity ID", ref CFG.Current.MsbReference_DisplayEntityID);
            UIHelper.Tooltip("If enabled, map object references will display the entity of the reference.");

            ImGui.Checkbox("Display Alias", ref CFG.Current.MsbReference_DisplayAlias);
            UIHelper.Tooltip("If enabled, map object references will display the alias of the reference.");
        }

        // Substitutions
        if (ImGui.CollapsingHeader("Substitutions", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Substitute c0000 entity", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_Model);
            UIHelper.Tooltip("The c0000 enemy that represents the player-like enemies will be given a visual model substitution so it is visible.");

            ImGui.InputText("##modelString", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID, 255);
            UIHelper.Tooltip("The Chr ID of the model you want to use as the replacement.");
        }

        // Selection Groups
        if (ImGui.CollapsingHeader("Selection Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Enable shortcuts", ref CFG.Current.Shortcuts_MapEditor_EnableSelectionGroupShortcuts);
            UIHelper.Tooltip("If enabled, selection group shortcuts will be detected.");

            ImGui.Checkbox("Frame selection group on select", ref CFG.Current.MapEditor_SelectionGroup_FrameSelection);
            UIHelper.Tooltip("Frame the selection group entities automatically in the viewport when selecting a group.");

            ImGui.Checkbox("Enable group auto-creation", ref CFG.Current.MapEditor_SelectionGroup_AutoCreation);
            UIHelper.Tooltip("The selection group will be given the name of the first entity within the selection as the group name and no tags, bypassing the creation prompt.");

            ImGui.Checkbox("Enable group deletion prompt", ref CFG.Current.MapEditor_SelectionGroup_ConfirmDelete);
            UIHelper.Tooltip("Display the confirmation dialog when deleting a group.");

            ImGui.Checkbox("Show keybind in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowKeybind);
            UIHelper.Tooltip("Append the keybind hint to the selection group name.");

            ImGui.Checkbox("Show tags in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowTags);
            UIHelper.Tooltip("Append the tags to the selection group name.");
        }
    }
}

#endregion

//------------------------------------------
// Model Editor
//------------------------------------------
#region Model Editor
public class ModelEditorTab
{
    public Smithbox BaseEditor;

    public ModelEditorTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        // General
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Wrap alias text", ref CFG.Current.Interface_ModelEditor_WrapAliasDisplay);
            UIHelper.Tooltip("Makes the alias text display wrap instead of being cut off within the Model Editor.");
        }

        // Actions
        if (ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
        {
            UIHelper.SimpleHeader("frameActionHeader", "Frame", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.DragFloat3("Offset##frameOffset", ref CFG.Current.ModelEditor_FrameInViewport_Offset);
            UIHelper.Tooltip("Determine the offset applied to the camera when framing a selection in the viewport.");

            ImGui.DragFloat("Distance##frameDist", ref CFG.Current.ModelEditor_FrameInViewport_Distance);
            UIHelper.Tooltip("Determine the distance the camera is placed at when framing a selection in the viewport.");

            UIHelper.SimpleHeader("pullCameraHeader", "Pull to Camera", "", UI.Current.ImGui_Default_Text_Color);

            if (ImGui.SliderFloat("Distance##pullToCameraDist", ref CFG.Current.ModelEditor_PullToCamera_Offset, 0, 100))
            {
                if (CFG.Current.ModelEditor_PullToCamera_Offset < 0)
                    CFG.Current.ModelEditor_PullToCamera_Offset = 0;

                if (CFG.Current.ModelEditor_PullToCamera_Offset > 100)
                    CFG.Current.ModelEditor_PullToCamera_Offset = 100;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the distance at which the current selection is offset from the camera when this action is used.");
        }

        // Property View
        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display community names", ref CFG.Current.ModelEditor_Enable_Commmunity_Names);
            UIHelper.Tooltip("The FLVER property fields will be given crowd-sourced names instead of the canonical name.");

            ImGui.Checkbox("Display community descriptions", ref CFG.Current.ModelEditor_Enable_Commmunity_Hints);
            UIHelper.Tooltip("The FLVER property fields will be given crowd-sourced descriptions.");

        }
    }
}

#endregion

//------------------------------------------
// Text Editor
//------------------------------------------
#region Text Editor
public class TextEditorTab
{
    public Smithbox BaseEditor;

    public TextEditorTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        // Data
        if (ImGui.CollapsingHeader("Data", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include Non-Primary Containers", ref CFG.Current.TextEditor_IncludeNonPrimaryContainers);
            UIHelper.Tooltip("If enabled, non-primary FMG containers are loaded.");

            ImGui.Checkbox("Include Vanilla Cache", ref CFG.Current.TextEditor_IncludeVanillaCache);
            UIHelper.Tooltip("If enabled, the vanilla cache is loaded, which enables the modified and unique difference features.");

            ImGui.Checkbox("Enable Background Difference Update", ref CFG.Current.TextEditor_EnableAutomaticDifferenceCheck);
            UIHelper.Tooltip("If enabled, the difference cache will be updated in the background every 5 minutes.");

            ImGui.Checkbox("Enable Obsolete Container Loading", ref CFG.Current.TextEditor_EnableObsoleteContainerLoad);
            UIHelper.Tooltip("If enabled, obsolete containers will be loaded. Otherwise, they are ignored.");
        }

        // Primary Category
        if (ImGui.CollapsingHeader("Primary Category", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (BaseEditor.ProjectManager.SelectedProject != null)
            {
                var curProject = BaseEditor.ProjectManager.SelectedProject;

                if (ImGui.BeginCombo("Primary Category##primaryCategoryCombo", CFG.Current.TextEditor_PrimaryCategory.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(TextContainerCategory)))
                    {
                        var type = (TextContainerCategory)entry;

                        if (TextUtils.IsSupportedLanguage(curProject, (TextContainerCategory)entry))
                        {
                            if (ImGui.Selectable(type.GetDisplayName()))
                            {
                                CFG.Current.TextEditor_PrimaryCategory = (TextContainerCategory)entry;

                                // Refresh the param editor FMG decorators when the category changes.
                                if (curProject.ParamEditor != null)
                                {
                                    curProject.ParamEditor.DecoratorHandler.ClearFmgDecorators();
                                }
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
                UIHelper.Tooltip("Change the primary category, this determines which text files are used for FMG references and other stuff.");
            }

            ImGui.Checkbox("Hide non-primary categories in list", ref CFG.Current.TextEditor_DisplayPrimaryCategoryOnly);
            UIHelper.Tooltip("Hide the non-primary categories in the File List.");

        }

        // File List
        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Simple File List", ref CFG.Current.TextEditor_SimpleFileList);
            UIHelper.Tooltip("Display the file list in a simple form: this means unused containers are hidden.");

            ImGui.Checkbox("Display Community File Name", ref CFG.Current.TextEditor_DisplayCommunityContainerName);
            UIHelper.Tooltip("If enabled, the names in the File List will be given a community name.");

            ImGui.Checkbox("Display Source Path", ref CFG.Current.TextEditor_DisplaySourcePath);
            UIHelper.Tooltip("If enabled, the path of the source file will be displayed in the hover tooltip.");

            ImGui.Checkbox("Display File Precedence Hint", ref CFG.Current.TextEditor_DisplayContainerPrecedenceHint);
            UIHelper.Tooltip("Display the File precedence hint on hover.");
        }

        // Text File List
        if (ImGui.CollapsingHeader("Text File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Simple Text File List", ref CFG.Current.TextEditor_SimpleFmgList);
            UIHelper.Tooltip("Display the text file list in a simple form: this means non-title or standalone files are hidden.");

            ImGui.Checkbox("Display FMG ID", ref CFG.Current.TextEditor_DisplayFmgID);
            UIHelper.Tooltip("Display the FMG ID in the Text File List by the name.");

            ImGui.Checkbox("Display Community FMG Name", ref CFG.Current.TextEditor_DisplayFmgPrettyName);
            UIHelper.Tooltip("Display the FMG community name instead of the internal form.");

            ImGui.Checkbox("Display FMG Precedence Hint", ref CFG.Current.TextEditor_DisplayFmgPrecedenceHint);
            UIHelper.Tooltip("Display the FMG precedence hint on hover.");
        }

        // Text Entries List
        if (ImGui.CollapsingHeader("Text Entries List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Empty Text Placeholder", ref CFG.Current.TextEditor_DisplayNullPlaceholder);
            UIHelper.Tooltip("Display placeholder text for rows that have no text.");

            ImGui.Checkbox("Display Empty Rows", ref CFG.Current.TextEditor_DisplayNullEntries);
            UIHelper.Tooltip("Display FMG entries with empty text.");

            ImGui.Checkbox("Trucate Displayed Text", ref CFG.Current.TextEditor_TruncateTextDisplay);
            UIHelper.Tooltip("Truncate the displayed text so it is always one line (does not affect the contents of the entry).");

            ImGui.Checkbox("Ignore ID on Duplication", ref CFG.Current.TextEditor_IgnoreIdOnDuplicate);
            UIHelper.Tooltip("Keep the Entry ID the same on duplication. Useful if you want to manually edit the IDs afterwards.");
        }

        // Entry Properties
        if (ImGui.CollapsingHeader("Text Entry Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Grouped Entries", ref CFG.Current.TextEditor_Entry_DisplayGroupedEntries);
            UIHelper.Tooltip("Include related entries in the Contents window, e.g. Title, Summary, Description, Effect entries that share the same ID.");

            ImGui.Checkbox("Allow Duplicate IDs", ref CFG.Current.TextEditor_Entry_AllowDuplicateIds);
            UIHelper.Tooltip("Allow Entry ID input to apply change even if the ID is a duplicate of an existing entry row.");
        }

        // Text Export
        if (ImGui.CollapsingHeader("Text Export", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include Grouped Entries", ref CFG.Current.TextEditor_TextExport_IncludeGroupedEntries);
            UIHelper.Tooltip("When exporting Text Entries, if they are associated with a group, include the associated entries as well whilst exporting.");

            ImGui.Checkbox("Use Quick Export", ref CFG.Current.TextEditor_TextExport_UseQuickExport);
            UIHelper.Tooltip("Automatically name the export file instead of display the Export Text prompt. Will overwrite the existing quick export file each time.");
        }

        // Language Sync
        if (ImGui.CollapsingHeader("Language Sync", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Primary Category only", ref CFG.Current.TextEditor_LanguageSync_PrimaryOnly);
            UIHelper.Tooltip("Only show your primary category (language) in the selection dropdown.");

            ImGui.Checkbox("Apply Prefix", ref CFG.Current.TextEditor_LanguageSync_AddPrefix);
            UIHelper.Tooltip("Add a prefix to synced text in the target language container for all new entries.");

            ImGui.InputText("##prefixText", ref CFG.Current.TextEditor_LanguageSync_Prefix, 255);
            UIHelper.Tooltip("The prefix to apply.");
        }

        // Text Entry Copy
        if (ImGui.CollapsingHeader("Clipboard Action", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include ID", ref CFG.Current.TextEditor_TextCopy_IncludeID);
            UIHelper.Tooltip("Include the row ID when copying a Text Entry to the clipboard.");

            ImGui.Checkbox("Escape New Lines", ref CFG.Current.TextEditor_TextCopy_EscapeNewLines);
            UIHelper.Tooltip("Escape the new lines characters when copying a Text Entry to the clipboard.");
        }
    }
}

#endregion

//------------------------------------------
// Param Editor
//------------------------------------------
#region Param Editor
public class ParamEditorTab
{
    public Smithbox BaseEditor;

    public ParamEditorTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        if (BaseEditor.ProjectManager.SelectedProject != null)
        {
            var curProject = BaseEditor.ProjectManager.SelectedProject;

            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Use loose params", ref CFG.Current.UseLooseParams);
                UIHelper.Tooltip("If true, then loose params will be loaded over the packed versions.");

                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);
                UIHelper.Tooltip("Reduces the line height within the the Param Editor screen.");

                ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
                UIHelper.Tooltip("Show additional options for advanced users within the massedit popup.");

                ImGui.Checkbox("Pinned params stay visible", ref CFG.Current.Param_PinnedParamsStayVisible);
                UIHelper.Tooltip("Pinned params will stay visible when you scroll instead of only being pinned to the top of the list.");

                ImGui.Checkbox("Pinned rows stay visible", ref CFG.Current.Param_PinnedRowsStayVisible);
                UIHelper.Tooltip("Pinned rows will stay visible when you scroll instead of only being pinned to the top of the list.");

                ImGui.Checkbox("Pinned fields stay visible", ref CFG.Current.Param_PinnedFieldsStayVisible);
                UIHelper.Tooltip("Pinned fields will stay visible when you scroll instead of only being pinned to the top of the list.");
            }

            if (ImGui.CollapsingHeader("Metadata", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("Configure whether the current project draws the param editor metadata from project files rather than base files.");

                if (ImGui.Button("Create Project Metadata##createProjectMetaData", DPI.StandardButtonSize))
                {
                    var dialog = PlatformUtils.Instance.MessageBox("This will overwrite any existing project-specific metadata. Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (dialog is DialogResult.Yes)
                    {
                        curProject.ParamData.CreateProjectMetadata();
                    }
                }

                if (ImGui.Checkbox("Use project metadata", ref CFG.Current.Param_UseProjectMeta))
                {
                    curProject.ParamData.ParamMeta.Clear();
                    curProject.ParamData.ReloadMeta();
                }
                UIHelper.Tooltip("Use project-specific metadata instead of Smithbox's base versions.");
            }

            if (ImGui.CollapsingHeader("Regulation Data", ImGuiTreeNodeFlags.DefaultOpen))
            {
                switch (curProject.ProjectType)
                {
                    case ProjectType.DES:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DES);
                        break;

                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DS1);
                        break;

                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DS2);
                        break;

                    case ProjectType.BB:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_BB);
                        break;

                    case ProjectType.DS3:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DS3);
                        break;

                    case ProjectType.SDT:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_SDT);
                        break;

                    case ProjectType.ER:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_ER);
                        break;

                    case ProjectType.AC6:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_AC6);
                        break;

                    case ProjectType.NR:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_NR);
                        break;
                }
                UIHelper.Tooltip("If enabled, row names are stripped upon save, meaning no row names will be stored in the regulation.\n\nThe row names are saved in the /.smithbox/Workflow/Stripped Row Names/ folder within your project folder.");


                switch (curProject.ProjectType)
                {
                    case ProjectType.DES:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DES);
                        break;

                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS1);
                        break;

                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS2);
                        break;

                    case ProjectType.BB:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_BB);
                        break;

                    case ProjectType.DS3:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS3);
                        break;

                    case ProjectType.SDT:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_SDT);
                        break;

                    case ProjectType.ER:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_ER);
                        break;

                    case ProjectType.AC6:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_AC6);
                        break;

                    case ProjectType.NR:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_NR);
                        break;
                }
                UIHelper.Tooltip("If enabled, stripped row names that have been stored will be applied to the row names during param loading.\n\nThe row names are saved in the /.smithbox/Workflow/Stripped Row Names/ folder within your project folder.");

                if (curProject.ProjectType is ProjectType.ER && curProject.ParamData.PrimaryBank.ParamVersion >= 11210015L)
                {
                    ImGui.Checkbox("Save regulation.bin as DCX.DFLT", ref CFG.Current.Param_SaveERAsDFLT);
                    UIHelper.Tooltip("If enabled, the regulation will be saved with the DCX.DFLT compression instead of the ZSTD compression that Elden Ring uses post patch 1.12.1.\n\nEnable if you want to load the regulation in an older tool that doesn't support ZSTD compression.");
                }
            }

            // Params
            if (ImGui.CollapsingHeader("Params", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
                UIHelper.Tooltip("Sort the Param View list alphabetically.");

                if (ImGui.Checkbox("Show community param names", ref CFG.Current.Param_ShowParamCommunityName))
                    UICache.ClearCaches();
                UIHelper.Tooltip("Show the community name for a param instead of its raw filename in the list.");

                if (ImGui.Checkbox("Display param categories", ref CFG.Current.Param_DisplayParamCategories))
                    UICache.ClearCaches();
                UIHelper.Tooltip("If defined, display params in their assigned param category groupings.");
            }

            // Rows
            if (ImGui.CollapsingHeader("Rows", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
                UIHelper.Tooltip("Disable the row names from wrapping within the Row View list.");

                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
                UIHelper.Tooltip("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
            }

            // Fields
            if (ImGui.CollapsingHeader("Field Layout", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
                UIHelper.Tooltip("Allow the field order to be changed by an alternative order as defined within the PARAM META file.");

                ImGui.Separator();

                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
                UIHelper.Tooltip("Crowd-sourced names will appear before the canonical name in the Field View list.");

                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
                UIHelper.Tooltip("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
                UIHelper.Tooltip("The field offset within the .PARAM file will be shown to the left in the Field View List.");

                ImGui.Checkbox("Show color preview", ref CFG.Current.Param_ShowColorPreview);
                UIHelper.Tooltip("Show color preview in field column if applicable.");

                ImGui.Checkbox("Show graph visualisation", ref CFG.Current.Param_ShowGraphVisualisation);
                UIHelper.Tooltip("Show graph visualisation in field column if applicable.");

                ImGui.Checkbox("Show view in map button", ref CFG.Current.Param_ViewInMapOption);
                UIHelper.Tooltip("Show the view in map if applicable.");

                ImGui.Checkbox("Show view model button", ref CFG.Current.Param_ViewModelOption);
                UIHelper.Tooltip("Show the view model if applicable.");

                ImGui.Separator();

                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
                UIHelper.Tooltip("Hide the generated param references for fields that link to other params.");

                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
                UIHelper.Tooltip("Hide the crowd-sourced namelist for index-based enum fields.");

                ImGui.Checkbox("Hide padding fields", ref CFG.Current.Param_HidePaddingFields);
                UIHelper.Tooltip("Hides fields that are considered 'padding' in the property editor view.");

                ImGui.Checkbox("Hide obsolete fields", ref CFG.Current.Param_HideObsoleteFields);
                UIHelper.Tooltip("Hides fields that are obsolete in the property editor view.");

                ImGui.Separator();

                ImGui.Checkbox("Show field param labels", ref CFG.Current.Param_ShowFieldParamLabels);
                UIHelper.Tooltip("The field param labels will be shown below the field name.");

                ImGui.Checkbox("Show field enum labels", ref CFG.Current.Param_ShowFieldEnumLabels);
                UIHelper.Tooltip("The field enum labels will be shown below the field name.");

                ImGui.Checkbox("Show field text labels", ref CFG.Current.Param_ShowFieldFmgLabels);
                UIHelper.Tooltip("The field fmg reference labels will be shown below the field name.");

                ImGui.Checkbox("Show field icon labels", ref CFG.Current.Param_ShowFieldTextureLabels);
                UIHelper.Tooltip("The field texture reference labels will be shown below the field name.");
            }

            // Field Information
            if (ImGui.CollapsingHeader("Field Information", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Help Icon: Show field description", ref CFG.Current.Param_ShowFieldDescription_onIcon);
                UIHelper.Tooltip("Display the description for the field when hovering over the help icon.");

                ImGui.Checkbox("Help Icon: Show field limits", ref CFG.Current.Param_ShowFieldLimits_onIcon);
                UIHelper.Tooltip("Display the minimum and maximum limits for the field when hovering over the help icon.");

                ImGui.Checkbox("Name: Show field description", ref CFG.Current.Param_ShowFieldDescription_onName);
                UIHelper.Tooltip("Display the description for the field when hovering over the name.");

                ImGui.Checkbox("Name: Show field limits", ref CFG.Current.Param_ShowFieldLimits_onName);
                UIHelper.Tooltip("Display the minimum and maximum limits for the field when hovering over the name.");

            }

            // Values
            if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
                UIHelper.Tooltip("Displays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
            }

            // Param Context Menu
            if (ImGui.CollapsingHeader("Param Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat("Context Menu Width##paramContextMenuWidth", ref CFG.Current.Param_ParamContextMenu_Width);
            }

            // Table Group Context Menu
            if (ImGui.CollapsingHeader("Table Group Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat("Context Menu Width##tableGroupContextMenuWidth", ref CFG.Current.Param_TableGroupContextMenu_Width);
            }

            // Row Context Menu
            if (ImGui.CollapsingHeader("Row Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display row name input", ref CFG.Current.Param_RowContextMenu_NameInput);
                UIHelper.Tooltip("Display a row name input within the right-click context menu.");

                ImGui.Checkbox("Display row shortcut tools", ref CFG.Current.Param_RowContextMenu_ShortcutTools);
                UIHelper.Tooltip("Show the shortcut tools in the right-click row context menu.");

                ImGui.Checkbox("Display row pin options", ref CFG.Current.Param_RowContextMenu_PinOptions);
                UIHelper.Tooltip("Show the pin options in the right-click row context menu.");

                ImGui.Checkbox("Display row compare options", ref CFG.Current.Param_RowContextMenu_CompareOptions);
                UIHelper.Tooltip("Show the compare options in the right-click row context menu.");

                ImGui.Checkbox("Display row reverse lookup option", ref CFG.Current.Param_RowContextMenu_ReverseLoopup);
                UIHelper.Tooltip("Show the reverse lookup option in the right-click row context menu.");

                ImGui.Checkbox("Display proliferate name option", ref CFG.Current.Param_RowContextMenu_ProliferateName);
                UIHelper.Tooltip("Show the proliferate name option in the right-click row context menu.");

                ImGui.Checkbox("Display inherit name option", ref CFG.Current.Param_RowContextMenu_InheritName);
                UIHelper.Tooltip("Show the inherit name option in the right-click row context menu.");

                ImGui.Checkbox("Display row name adjustment options", ref CFG.Current.Param_RowContextMenu_RowNameAdjustments);
                UIHelper.Tooltip("Show the row name adjustment options in the right-click row context menu.");

                ImGui.DragFloat("Context Menu Width##rowContextMenuWidth", ref CFG.Current.Param_RowContextMenu_Width);
            }

            // Field Context Menu
            if (ImGui.CollapsingHeader("Field Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Split context menu", ref CFG.Current.Param_FieldContextMenu_Split);
                UIHelper.Tooltip("Split the field context menu into separate menus for separate right-click locations.");

                ImGui.Checkbox("Display field name", ref CFG.Current.Param_FieldContextMenu_Name);
                UIHelper.Tooltip("Display the field name in the context menu.");

                ImGui.Checkbox("Display field description", ref CFG.Current.Param_FieldContextMenu_Description);
                UIHelper.Tooltip("Display the field description in the context menu.");

                ImGui.Checkbox("Display field property info", ref CFG.Current.Param_FieldContextMenu_PropertyInfo);
                UIHelper.Tooltip("Display the field property info in the context menu.");

                ImGui.Checkbox("Display field pin options", ref CFG.Current.Param_FieldContextMenu_PinOptions);
                UIHelper.Tooltip("Display the field pin options in the context menu.");

                ImGui.Checkbox("Display field compare options", ref CFG.Current.Param_FieldContextMenu_CompareOptions);
                UIHelper.Tooltip("Display the field compare options in the context menu.");

                ImGui.Checkbox("Display field value distribution option", ref CFG.Current.Param_FieldContextMenu_ValueDistribution);
                UIHelper.Tooltip("Display the field value distribution option in the context menu.");

                ImGui.Checkbox("Display field add options", ref CFG.Current.Param_FieldContextMenu_AddOptions);
                UIHelper.Tooltip("Display the field add to searchbar and mass edit options in the context menu.");

                ImGui.Checkbox("Display field references", ref CFG.Current.Param_FieldContextMenu_References);
                UIHelper.Tooltip("Display the field references in the context menu.");

                ImGui.Checkbox("Display field reference search", ref CFG.Current.Param_FieldContextMenu_ReferenceSearch);
                UIHelper.Tooltip("Display the field reference search in the context menu.");

                ImGui.Checkbox("Display field mass edit options", ref CFG.Current.Param_FieldContextMenu_MassEdit);
                UIHelper.Tooltip("Display the field mass edit options in the context menu.");

                ImGui.Checkbox("Display full mass edit submenu", ref CFG.Current.Param_FieldContextMenu_FullMassEdit);
                UIHelper.Tooltip("If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");

                ImGui.DragFloat("Context Menu Width##fieldContextMenuWidth", ref CFG.Current.Param_FieldContextMenu_Width);
                UIHelper.Tooltip("Controls the width of the field context menu when enum or aliases lists are present.");

                ImGui.DragFloat("List Height Multiplier##fieldContextListHeightMultiplier", ref CFG.Current.Param_FieldContextMenu_ListHeightMultiplier);
                UIHelper.Tooltip("Controls the height of the field context menu when enum or aliases lists are present.");
            }

            // Icon Preview
            if (ImGui.CollapsingHeader("Icon Preview", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display icon preview", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);

                ImGui.Text("Icon Preview Scale:");
                ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
                UIHelper.Tooltip("Scale of the previewed image.");
            }

            // Ignore if no game offsets exist for the project type
            if (curProject.ParamData.ParamMemoryOffsets != null && curProject.ParamData.ParamMemoryOffsets.list != null)
            {
                // Auto-set to the latest version
                if (ImGui.CollapsingHeader("Param Reloader", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Checkbox("Set latest version on program start", ref CFG.Current.UseLatestGameOffset);
                    UIHelper.Tooltip("If enabled, the param reloader version will be set to the latest executable version whenever Smithbox is started.");

                    ImGui.Text("Param Reloader Version");
                    UIHelper.Tooltip("This should match the executable version you wish to target, otherwise the memory offsets will be incorrect.");

                    var index = CFG.Current.SelectedGameOffsetData;
                    string[] options = curProject.ParamData.ParamMemoryOffsets.list.Select(entry => entry.exeVersion).ToArray();

                    if (ImGui.Combo("##GameOffsetVersion", ref index, options, options.Length))
                    {
                        CFG.Current.SelectedGameOffsetData = index;
                    }
                }
            }
        }
    }
}

#endregion

//------------------------------------------
// Graphics Param Editor
//------------------------------------------
#region Graphics Param Editor
public class GparamEditorTab
{
    public Smithbox BaseEditor;

    public GparamEditorTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display aliases in file list", ref CFG.Current.Interface_Display_Alias_for_Gparam);
            UIHelper.Tooltip("Toggle the display of the aliases in the file list.");
        }

        if (ImGui.CollapsingHeader("Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display param group aliases", ref CFG.Current.Gparam_DisplayParamGroupAlias);
            UIHelper.Tooltip("Display the aliased name for param groups, instead of the internal key.");

            ImGui.Checkbox("Show add button for missing groups", ref CFG.Current.Gparam_DisplayAddGroups);
            UIHelper.Tooltip("Show the Add button for groups that are not present.");

            ImGui.Checkbox("Show empty groups", ref CFG.Current.Gparam_DisplayEmptyGroups);
            UIHelper.Tooltip("Display empty groups in the group list.");
        }

        if (ImGui.CollapsingHeader("Fields", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display param field aliases", ref CFG.Current.Gparam_DisplayParamFieldAlias);
            UIHelper.Tooltip("Display the aliased name for param fields, instead of the internal key.");

            ImGui.Checkbox("Show add button for missing fields", ref CFG.Current.Gparam_DisplayAddFields);
            UIHelper.Tooltip("Show the Add button for fields that are not present.");
        }

        if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show color edit for 4 digit properties", ref CFG.Current.Gparam_DisplayColorEditForVector4Fields);
            UIHelper.Tooltip("Show the color edit tool for 4 digit properties.");
        }

        if (ImGui.CollapsingHeader("Color Edit", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Checkbox("Show color as Integer RGB", ref CFG.Current.Gparam_ColorEdit_RGB))
            {
                CFG.Current.Gparam_ColorEdit_Decimal = false;
                CFG.Current.Gparam_ColorEdit_HSV = false;
            }
            UIHelper.Tooltip("Show the color data as Integer RGB color (0 to 255)");

            if (ImGui.Checkbox("Show color as Decimal RGB", ref CFG.Current.Gparam_ColorEdit_Decimal))
            {
                CFG.Current.Gparam_ColorEdit_RGB = false;
                CFG.Current.Gparam_ColorEdit_HSV = false;
            }
            UIHelper.Tooltip("Show the color data as Decimal RGB color (0.0 to 1.0)");

            if (ImGui.Checkbox("Show color as HSV", ref CFG.Current.Gparam_ColorEdit_HSV))
            {
                CFG.Current.Gparam_ColorEdit_RGB = false;
                CFG.Current.Gparam_ColorEdit_Decimal = false;
            }
            UIHelper.Tooltip("Show the color data as Hue, Saturation, Value color (0.0 to 1.0)");
        }

        if (ImGui.CollapsingHeader("Quick Edit - General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Delimiter", ref CFG.Current.Gparam_QuickEdit_Chain, 255);
            UIHelper.Tooltip("The text string to split filter and commands.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Chain = "+";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - File Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("File Filter: Match File", ref CFG.Current.Gparam_QuickEdit_File, 255);
            UIHelper.Tooltip("The text string to detect for the 'File' filter argument.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_File = "file";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Group Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Group Filter: Match Group", ref CFG.Current.Gparam_QuickEdit_Group, 255);
            UIHelper.Tooltip("The text string to detect for the 'Group' filter argument.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Group = "group";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Field Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Field Filter: Match Field", ref CFG.Current.Gparam_QuickEdit_Field, 255);
            UIHelper.Tooltip("The text string to detect for the 'Field' filter argument.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Field = "field";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Value Filters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Value Filter: Match ID", ref CFG.Current.Gparam_QuickEdit_ID, 255);
            UIHelper.Tooltip("The text string to detect for the 'ID' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Filter: Match Time of Day", ref CFG.Current.Gparam_QuickEdit_TimeOfDay, 255);
            UIHelper.Tooltip("The text string to detect for the 'Time of Day' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Filter: Match Value", ref CFG.Current.Gparam_QuickEdit_Value, 255);
            UIHelper.Tooltip("The text string to detect for the 'Value' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Filter: Match Index", ref CFG.Current.Gparam_QuickEdit_Index, 255);
            UIHelper.Tooltip("The text string to detect for the 'Index' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_ID = "id";
                CFG.Current.Gparam_QuickEdit_TimeOfDay = "tod";
                CFG.Current.Gparam_QuickEdit_Value = "value";
                CFG.Current.Gparam_QuickEdit_Index = "index";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Value Commands", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Value Command: Set", ref CFG.Current.Gparam_QuickEdit_Set, 255);
            UIHelper.Tooltip("The text string to detect for the 'Set' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Addition", ref CFG.Current.Gparam_QuickEdit_Add, 255);
            UIHelper.Tooltip("The text string to detect for the 'Addition' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Subtract", ref CFG.Current.Gparam_QuickEdit_Subtract, 255);
            UIHelper.Tooltip("The text string to detect for the 'Subtract' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Multiply", ref CFG.Current.Gparam_QuickEdit_Multiply, 255);
            UIHelper.Tooltip("The text string to detect for the 'Multiply' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Set by Row", ref CFG.Current.Gparam_QuickEdit_SetByRow, 255);
            UIHelper.Tooltip("The text string to detect for the 'Set By Row' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Restore", ref CFG.Current.Gparam_QuickEdit_Restore, 255);
            UIHelper.Tooltip("The text string to detect for the 'Restore' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Random", ref CFG.Current.Gparam_QuickEdit_Random, 255);
            UIHelper.Tooltip("The text string to detect for the 'Random' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Set = "set";
                CFG.Current.Gparam_QuickEdit_Add = "add";
                CFG.Current.Gparam_QuickEdit_Subtract = "sub";
                CFG.Current.Gparam_QuickEdit_Multiply = "mult";
                CFG.Current.Gparam_QuickEdit_SetByRow = "setbyrow";
                CFG.Current.Gparam_QuickEdit_Restore = "restore";
                CFG.Current.Gparam_QuickEdit_Random = "random";
            }
        }
    }

}
#endregion


//------------------------------------------
// Texture Viewer
//------------------------------------------
#region Texture Viewer
public class TextureViewerTab
{
    public Smithbox BaseEditor;

    public TextureViewerTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show character names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Characters);
            UIHelper.Tooltip("Show matching character aliases within the file list.");

            ImGui.Checkbox("Show asset names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Assets);
            UIHelper.Tooltip("Show matching asset/object aliases within the file list.");

            ImGui.Checkbox("Show part names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Parts);
            UIHelper.Tooltip("Show matching part aliases within the file list.");

            ImGui.Checkbox("Show low detail entries", ref CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries);
            UIHelper.Tooltip("Show the low-detail texture containers.");
        }

        if (ImGui.CollapsingHeader("Texture List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show particle names", ref CFG.Current.TextureViewer_TextureList_ShowAliasName_Particles);
            UIHelper.Tooltip("Show matching particle aliases within the texture list.");
        }
    }
}

#endregion


//------------------------------------------
// Interface
//------------------------------------------
#region Interface
public class InterfaceTab
{
    public Smithbox BaseEditor;

    private float _tempScale;
    private string newThemeName = "";
    private string currentThemeName = "";

    public InterfaceTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
        _tempScale = CFG.Current.System_UI_Scale;

        currentThemeName = CFG.Current.SelectedTheme;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Wrap alias text", ref CFG.Current.System_WrapAliasDisplay);
            UIHelper.Tooltip("Makes the alias text display wrap instead of being cut off.");

            DPI.ApplyInputWidth();
            ImGui.SliderFloat("UI scale", ref _tempScale, 0.5f, 4.0f);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                // Round to 0.05
                CFG.Current.System_UI_Scale = (float)Math.Round(_tempScale * 20) / 20;
                _tempScale = CFG.Current.System_UI_Scale;
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }
            UIHelper.Tooltip("Adjusts the scale of the user interface throughout all of Smithbox.");

            ImGui.SameLine();

            if (ImGui.Button("Reset", DPI.StandardButtonSize))
            {
                CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                _tempScale = CFG.Current.System_UI_Scale;
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }

            ImGui.Checkbox($"Multiply UI scale by DPI ({(DPI.Dpi / 96).ToString("P0", new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 })})", ref CFG.Current.System_ScaleByDPI);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }
            UIHelper.Tooltip("Multiplies the user interface scale by your monitor's DPI setting.");
        }

        // Fonts
        if (ImGui.CollapsingHeader("Fonts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DPI.ApplyInputWidth();
            ImGui.SliderFloat("Font size", ref CFG.Current.Interface_FontSize, 8.0f, 32.0f);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.Interface_FontSize = (float)Math.Round(CFG.Current.Interface_FontSize);
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }
            UIHelper.Tooltip("Adjusts the size of the font in Smithbox.");

            ImGui.Text("Current English Font:");
            ImGui.SameLine();
            ImGui.Text(Path.GetFileName(CFG.Current.System_English_Font));

            if (ImGui.Button("Set English font", DPI.StandardButtonSize))
            {
                PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                if (File.Exists(path))
                {
                    CFG.Current.System_English_Font = path;
                    Smithbox.FontRebuildRequest = true;
                }
            }
            UIHelper.Tooltip("Use the following font for English characters. .ttf and .otf expected.");

            ImGui.Text("Current Non-English Font:");
            ImGui.SameLine();
            ImGui.Text(Path.GetFileName(CFG.Current.System_Other_Font));

            if (ImGui.Button("Set Non-English font", DPI.StandardButtonSize))
            {
                PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                if (File.Exists(path))
                {
                    CFG.Current.System_Other_Font = path;
                    Smithbox.FontRebuildRequest = true;
                }
            }
            UIHelper.Tooltip("Use the following font for Non-English characters. .ttf and .otf expected.");

            if (ImGui.Button("Restore Default Fonts", DPI.StandardButtonSize))
            {
                CFG.Current.System_English_Font = Path.Join("Assets","Fonts","RobotoMono-Light.ttf");
                CFG.Current.System_Other_Font = Path.Join("Assets","Fonts","NotoSansCJKtc-Light.otf");
                Smithbox.FontRebuildRequest = true;
            }
        }

        // Additional Language Fonts
        if (ImGui.CollapsingHeader("Additional Language Fonts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Checkbox("Chinese", ref CFG.Current.System_Font_Chinese))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Korean", ref CFG.Current.System_Font_Korean))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Thai", ref CFG.Current.System_Font_Thai))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Vietnamese", ref CFG.Current.System_Font_Vietnamese))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Cyrillic", ref CFG.Current.System_Font_Cyrillic))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
        }

        // ImGui
        if (ImGui.CollapsingHeader("Interface Layout", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var storedDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Smithbox");
            var storedPath = Path.Join(storedDir,"imgui.ini");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Store the current imgui.ini in the AppData folder for future usage.");

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Store##storeImguiIni", DPI.StandardButtonSize))
            {
                var curImgui = Path.Join(AppContext.BaseDirectory,"imgui.ini");

                if (Directory.Exists(storedDir))
                {
                    if (File.Exists(storedPath))
                    {
                        File.Copy(curImgui, storedPath, true);
                    }
                    else
                    {
                        File.Copy(curImgui, storedPath, true);
                    }
                }

                PlatformUtils.Instance.MessageBox($"Stored at {storedPath}.", "Information", MessageBoxButtons.OK);
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Folder##openImguiIniFolder", DPI.StandardButtonSize))
            {
                Process.Start("explorer.exe", storedDir);
            }

            if (File.Exists(storedPath))
            {
                ImGui.Separator();

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Set the current imgui.ini to the version you stored within the AppData folder.");

                ImGui.AlignTextToFramePadding();

                if (ImGui.Button("Set##setImguiIni", DPI.StandardButtonSize))
                {
                    var curImgui = Path.Join(AppContext.BaseDirectory,"imgui.ini");

                    if (File.Exists(storedPath))
                    {
                        File.Copy(storedPath, curImgui, true);
                    }

                    PlatformUtils.Instance.MessageBox("Applied imgui.ini change. Restart Smithbox to apply changes.", "Information", MessageBoxButtons.OK);
                }
            }

            ImGui.Separator();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Reset the imgui.ini to the default version.");

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Reset##resetImguiIni", DPI.StandardButtonSize))
            {
                var curImgui = Path.Join(AppContext.BaseDirectory,"imgui.ini");
                var defaultImgui = Path.Join(AppContext.BaseDirectory,"imgui.default");

                if (Directory.Exists(storedDir))
                {
                    File.Copy(defaultImgui, curImgui, true);
                }

                PlatformUtils.Instance.MessageBox("Applied imgui.ini change. Restart Smithbox to apply changes.", "Information", MessageBoxButtons.OK);
            }
        }

        if (ImGui.CollapsingHeader("Theme", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Current Theme");

            var folder = ProjectUtils.GetThemeFolder();

            var files = Directory.EnumerateFiles(folder);

            var themeFiles = new List<string>();
            foreach(var file in files)
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                themeFiles.Add(filename);
            }

            if(ImGui.BeginCombo("Theme Selection", currentThemeName))
            {
                foreach(var entry in themeFiles)
                {
                    if(ImGui.Selectable(entry, entry == currentThemeName))
                    {
                        currentThemeName = entry;
                        CFG.Current.SelectedTheme = currentThemeName;
                        UI.LoadTheme(entry);
                    }
                }

                ImGui.EndCombo();
            }

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                UI.LoadDefault();
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Theme Folder", DPI.StandardButtonSize))
            {
                Process.Start("explorer.exe", Path.Join(AppContext.BaseDirectory,"Assets","Themes")); //! FIXME explorer does not exist
            }
            ImGui.SameLine();

            if (ImGui.Button("Export Theme", DPI.StandardButtonSize))
            {
                UI.ExportTheme(newThemeName);
            }
            ImGui.SameLine();

            DPI.ApplyInputWidth();
            ImGui.InputText("##themeName", ref newThemeName, 255);

            var flags = ImGuiColorEditFlags.AlphaOpaque;

            if (ImGui.CollapsingHeader("Editor Window", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Main Background##ImGui_MainBg", ref UI.Current.ImGui_MainBg, flags);
                ImGui.ColorEdit4("Child Background##ImGui_ChildBg", ref UI.Current.ImGui_ChildBg, flags);
                ImGui.ColorEdit4("Popup Background##ImGui_PopupBg", ref UI.Current.ImGui_PopupBg, flags);
                ImGui.ColorEdit4("Border##ImGui_Border", ref UI.Current.ImGui_Border, flags);
                ImGui.ColorEdit4("Title Bar Background##ImGui_TitleBarBg", ref UI.Current.ImGui_TitleBarBg, flags);
                ImGui.ColorEdit4("Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref UI.Current.ImGui_TitleBarBg_Active, flags);
                ImGui.ColorEdit4("Menu Bar Background##ImGui_MenuBarBg", ref UI.Current.ImGui_MenuBarBg, flags);
            }

            if (ImGui.CollapsingHeader("Moveable Window", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Main Background##Imgui_Moveable_MainBg", ref UI.Current.Imgui_Moveable_MainBg, flags);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBg", ref UI.Current.Imgui_Moveable_ChildBg, flags);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBgSecondary", ref UI.Current.Imgui_Moveable_ChildBgSecondary, flags);
                ImGui.ColorEdit4("Header##Imgui_Moveable_Header", ref UI.Current.Imgui_Moveable_Header, flags);
                ImGui.ColorEdit4("Title Bar Background##Imgui_Moveable_TitleBg", ref UI.Current.Imgui_Moveable_TitleBg, flags);
                ImGui.ColorEdit4("Title Bar Background (Active)##Imgui_Moveable_TitleBg_Active", ref UI.Current.Imgui_Moveable_TitleBg_Active, flags);
            }

            if (ImGui.CollapsingHeader("Scrollbar", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Scrollbar Background", ref UI.Current.ImGui_ScrollbarBg, flags);
                ImGui.ColorEdit4("Scrollbar Grab", ref UI.Current.ImGui_ScrollbarGrab, flags);
                ImGui.ColorEdit4("Scrollbar Grab (Hover)", ref UI.Current.ImGui_ScrollbarGrab_Hover, flags);
                ImGui.ColorEdit4("Scrollbar Grab (Active)", ref UI.Current.ImGui_ScrollbarGrab_Active, flags);
                ImGui.ColorEdit4("Slider Grab", ref UI.Current.ImGui_SliderGrab, flags);
                ImGui.ColorEdit4("Slider Grab (Active)", ref UI.Current.ImGui_SliderGrab_Active, flags);
            }

            if (ImGui.CollapsingHeader("Tab", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Tab", ref UI.Current.ImGui_Tab, flags);
                ImGui.ColorEdit4("Tab (Hover)", ref UI.Current.ImGui_Tab_Hover, flags);
                ImGui.ColorEdit4("Tab (Active)", ref UI.Current.ImGui_Tab_Active, flags);
                ImGui.ColorEdit4("Unfocused Tab", ref UI.Current.ImGui_UnfocusedTab, flags);
                ImGui.ColorEdit4("Unfocused Tab (Active)", ref UI.Current.ImGui_UnfocusedTab_Active, flags);
            }

            if (ImGui.CollapsingHeader("Button", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Button", ref UI.Current.ImGui_Button, flags);
                ImGui.ColorEdit4("Button (Hover)", ref UI.Current.ImGui_Button_Hovered, flags);
                ImGui.ColorEdit4("Button (Active)", ref UI.Current.ImGui_ButtonActive, flags);
            }

            if (ImGui.CollapsingHeader("Selection", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Selection", ref UI.Current.ImGui_Selection, flags);
                ImGui.ColorEdit4("Selection (Hover)", ref UI.Current.ImGui_Selection_Hover, flags);
                ImGui.ColorEdit4("Selection (Active)", ref UI.Current.ImGui_Selection_Active, flags);
            }

            if (ImGui.CollapsingHeader("Inputs", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Input Background", ref UI.Current.ImGui_Input_Background, flags);
                ImGui.ColorEdit4("Input Background (Hover)", ref UI.Current.ImGui_Input_Background_Hover, flags);
                ImGui.ColorEdit4("Input Background (Active)", ref UI.Current.ImGui_Input_Background_Active, flags);
                ImGui.ColorEdit4("Input Checkmark", ref UI.Current.ImGui_Input_CheckMark, flags);
                ImGui.ColorEdit4("Input Conflict Background", ref UI.Current.ImGui_Input_Conflict_Background, flags);
                ImGui.ColorEdit4("Input Vanilla Background", ref UI.Current.ImGui_Input_Vanilla_Background, flags);
                ImGui.ColorEdit4("Input Default Background", ref UI.Current.ImGui_Input_Default_Background, flags);
                ImGui.ColorEdit4("Input Auxillary Vanilla Background", ref UI.Current.ImGui_Input_AuxVanilla_Background, flags);
                ImGui.ColorEdit4("Input Difference Comparison Background", ref UI.Current.ImGui_Input_DiffCompare_Background, flags);
            }

            if (ImGui.CollapsingHeader("Text", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Default Text", ref UI.Current.ImGui_Default_Text_Color, flags);
                ImGui.ColorEdit4("Warning Text", ref UI.Current.ImGui_Warning_Text_Color, flags);
                ImGui.ColorEdit4("Beneficial Text", ref UI.Current.ImGui_Benefit_Text_Color, flags);
                ImGui.ColorEdit4("Invalid Text", ref UI.Current.ImGui_Invalid_Text_Color, flags);

                ImGui.ColorEdit4("Param Reference Text", ref UI.Current.ImGui_ParamRef_Text, flags);
                ImGui.ColorEdit4("Param Reference Missing Text", ref UI.Current.ImGui_ParamRefMissing_Text, flags);
                ImGui.ColorEdit4("Param Reference Inactive Text", ref UI.Current.ImGui_ParamRefInactive_Text, flags);
                ImGui.ColorEdit4("Enum Name Text", ref UI.Current.ImGui_EnumName_Text, flags);
                ImGui.ColorEdit4("Enum Value Text", ref UI.Current.ImGui_EnumValue_Text, flags);
                ImGui.ColorEdit4("FMG Link Text", ref UI.Current.ImGui_FmgLink_Text, flags);
                ImGui.ColorEdit4("FMG Reference Text", ref UI.Current.ImGui_FmgRef_Text, flags);
                ImGui.ColorEdit4("FMG Reference Inactive Text", ref UI.Current.ImGui_FmgRefInactive_Text, flags);
                ImGui.ColorEdit4("Is Reference Text", ref UI.Current.ImGui_IsRef_Text, flags);
                ImGui.ColorEdit4("Virtual Reference Text", ref UI.Current.ImGui_VirtualRef_Text, flags);
                ImGui.ColorEdit4("Reference Text", ref UI.Current.ImGui_Ref_Text, flags);
                ImGui.ColorEdit4("Auxiliary Conflict Text", ref UI.Current.ImGui_AuxConflict_Text, flags);
                ImGui.ColorEdit4("Auxiliary Added Text", ref UI.Current.ImGui_AuxAdded_Text, flags);
                ImGui.ColorEdit4("Primary Changed Text", ref UI.Current.ImGui_PrimaryChanged_Text, flags);
                ImGui.ColorEdit4("Param Row Text", ref UI.Current.ImGui_ParamRow_Text, flags);
                ImGui.ColorEdit4("Aliased Name Text", ref UI.Current.ImGui_AliasName_Text, flags);

                ImGui.ColorEdit4("Text Editor: Modified Row", ref UI.Current.ImGui_TextEditor_ModifiedTextEntry_Text, flags);
                ImGui.ColorEdit4("Text Editor: Unique Row", ref UI.Current.ImGui_TextEditor_UniqueTextEntry_Text, flags);

                ImGui.ColorEdit4("Logger: Information", ref UI.Current.ImGui_Logger_Information_Color, flags);
                ImGui.ColorEdit4("Logger: Warning", ref UI.Current.ImGui_Logger_Warning_Color, flags);
                ImGui.ColorEdit4("Logger: Error", ref UI.Current.ImGui_Logger_Error_Color, flags);
            }

            if (ImGui.CollapsingHeader("Miscellaneous", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Display Group: Border Highlight", ref UI.Current.DisplayGroupEditor_Border_Highlight, flags);
                ImGui.ColorEdit4("Display Group: Active Input Background", ref UI.Current.DisplayGroupEditor_DisplayActive_Frame, flags);
                ImGui.ColorEdit4("Display Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_DisplayActive_Checkbox, flags);
                ImGui.ColorEdit4("Draw Group: Active Input Background", ref UI.Current.DisplayGroupEditor_DrawActive_Frame, flags);
                ImGui.ColorEdit4("Draw Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_DrawActive_Checkbox, flags);
                ImGui.ColorEdit4("Combined Group: Active Input Background", ref UI.Current.DisplayGroupEditor_CombinedActive_Frame, flags);
                ImGui.ColorEdit4("Combined Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_CombinedActive_Checkbox, flags);
            }
        }
    }
}
#endregion

//------------------------------------------
// Viewport
//------------------------------------------
#region Viewport
public class ViewportTab
{
    public Smithbox BaseEditor;

    public ViewportTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        var defaultButtonSize = new Vector2(ImGui.GetWindowWidth(), 24);

        //---------------------------------------
        // Rendering
        //---------------------------------------
        if (ImGui.CollapsingHeader("Rendering", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Frame Rate
            if (ImGui.SliderFloat("Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f))
            {
                CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);
            }
            UIHelper.Tooltip("Adjusts the frame rate of the viewport.");

            ImGui.Separator();

            // Toggle Rendering
            ImGui.Checkbox("Enable rendering", ref CFG.Current.Viewport_Enable_Rendering);
            UIHelper.Tooltip("Enabling this option will allow Smithbox to render entities in the viewport.");

            // Toggle Texturing
            ImGui.Checkbox("Enable texturing", ref CFG.Current.Viewport_Enable_Texturing);
            UIHelper.Tooltip("Enabling this option will allow Smithbox to render the textures of models within the viewport.");

            // Toggle culling
            ImGui.Checkbox("Enable frustum culling", ref CFG.Current.Viewport_Enable_Culling);
            UIHelper.Tooltip("Enabling this option will cause entities outside of the camera frustum to be culled.");

            ImGui.Separator();

            if (ImGui.InputInt("Renderables", ref CFG.Current.Viewport_Limit_Renderables, 0, 0))
                if (CFG.Current.Viewport_Limit_Renderables < CFG.Default.Viewport_Limit_Renderables)
                    CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
            UIHelper.Tooltip("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");

            Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
            UIHelper.Tooltip("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");

            Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.Viewport_Limit_Buffer_Flver_Bone);
            UIHelper.Tooltip("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");

            ImGui.Separator();

            ImGui.InputFloat("Default Model Render: Brightness", ref CFG.Current.Viewport_DefaultRender_Brightness);
            UIHelper.Tooltip("Change the brightness modifier for the Default Model Rendering shader.");
            ImGui.InputFloat("Default Model Render: Saturation", ref CFG.Current.Viewport_DefaultRender_Saturation);
            UIHelper.Tooltip("Change the saturation modifier for the Default Model Rendering shader.");

            ImGui.Checkbox("Enable enemy model masks", ref CFG.Current.Viewport_Enable_Model_Masks);
            UIHelper.Tooltip("Attempt to display the correct model masks for enemies based on NpcParam.");

            ImGui.Checkbox("Draw LOD facesets", ref CFG.Current.Viewport_Enable_LOD_Facesets);
            UIHelper.Tooltip("Render all facesets for all FLVER meshes, including LOD ones.");

            if (ImGui.Button("Reset##ResetRenderProperties", defaultButtonSize))
            {
                ResetRenderingCFG();
            }
            UIHelper.Tooltip("Resets all of the values within this section to their default values.");
        }

        //---------------------------------------
        // Visualization
        //---------------------------------------
        if (ImGui.CollapsingHeader("Visualization", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.ColorEdit3("Viewport Background Color", ref CFG.Current.Viewport_Background_Color);

            ImGui.ColorEdit3("Selection Color", ref CFG.Current.Viewport_DefaultRender_SelectColor);

            ImGui.Checkbox("Enable selection outline", ref CFG.Current.Viewport_Enable_Selection_Outline);
            UIHelper.Tooltip("Enabling this option will cause a selection outline to appear on selected objects.");

            ImGui.Checkbox("Enable box selection", ref CFG.Current.Viewport_Enable_BoxSelection);
            UIHelper.Tooltip("Click and drag the mouse to select multiple objects. (Ctrl: Subtract, Shift: Add)");

            ImGui.SliderFloat("Box selection - distance threshold factor", ref CFG.Current.Viewport_BS_DistThresFactor, 1.0f, 2.0f);
            UIHelper.Tooltip("Lower = select objects closer to each other, higher = select objects farther from each other.");

            ImGui.Separator();

            ImGui.ColorEdit3("Box region - base color", ref CFG.Current.GFX_Renderable_Box_BaseColor);
            ImGui.ColorEdit3("Box region - highlight color", ref CFG.Current.GFX_Renderable_Box_HighlightColor);
            ImGui.DragFloat("Box region - transparency when solid", ref CFG.Current.GFX_Renderable_Box_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Cylinder region - base color", ref CFG.Current.GFX_Renderable_Cylinder_BaseColor);
            ImGui.ColorEdit3("Cylinder region - highlight color", ref CFG.Current.GFX_Renderable_Cylinder_HighlightColor);
            ImGui.DragFloat("Cylinder region - transparency when solid", ref CFG.Current.GFX_Renderable_Cylinder_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Sphere region - base color", ref CFG.Current.GFX_Renderable_Sphere_BaseColor);
            ImGui.ColorEdit3("Sphere region - highlight color", ref CFG.Current.GFX_Renderable_Sphere_HighlightColor);
            ImGui.DragFloat("Sphere region - transparency when solid", ref CFG.Current.GFX_Renderable_Sphere_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Point region - base color", ref CFG.Current.GFX_Renderable_Point_BaseColor);
            ImGui.ColorEdit3("Point region - highlight color", ref CFG.Current.GFX_Renderable_Point_HighlightColor);
            ImGui.DragFloat("Point region - transparency when solid", ref CFG.Current.GFX_Renderable_Point_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Dummy poly - base color", ref CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
            ImGui.ColorEdit3("Dummy poly - highlight color", ref CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);
            ImGui.DragFloat("Dummy poly - transparency when solid", ref CFG.Current.GFX_Renderable_DummyPoly_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Bone point - base color", ref CFG.Current.GFX_Renderable_BonePoint_BaseColor);
            ImGui.ColorEdit3("Bone point - highlight color", ref CFG.Current.GFX_Renderable_BonePoint_HighlightColor);
            ImGui.DragFloat("Bone point - transparency when solid", ref CFG.Current.GFX_Renderable_BonePoint_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Chr marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
            ImGui.ColorEdit3("Chr marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);
            ImGui.DragFloat("Chr marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Object marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
            ImGui.ColorEdit3("Object marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);
            ImGui.DragFloat("Object marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Object_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Player marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
            ImGui.ColorEdit3("Player marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);
            ImGui.DragFloat("Player marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Player_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Other marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
            ImGui.ColorEdit3("Other marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);
            ImGui.DragFloat("Other marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Other_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Point light - base color", ref CFG.Current.GFX_Renderable_PointLight_BaseColor);
            ImGui.ColorEdit3("Point light - highlight color", ref CFG.Current.GFX_Renderable_PointLight_HighlightColor);
            ImGui.DragFloat("Point light - transparency when solid", ref CFG.Current.GFX_Renderable_PointLight_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Spot light - base color", ref CFG.Current.GFX_Renderable_SpotLight_BaseColor);
            ImGui.ColorEdit3("Spot light - highlight color", ref CFG.Current.GFX_Renderable_SpotLight_HighlightColor);
            ImGui.DragFloat("Spot light - transparency when solid", ref CFG.Current.GFX_Renderable_SpotLight_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Directional light - base color", ref CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
            ImGui.ColorEdit3("Directional light - highlight color", ref CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);
            ImGui.DragFloat("Directional light - transparency when solid", ref CFG.Current.GFX_Renderable_DirectionalLight_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Auto Invade Sphere - base color", ref CFG.Current.GFX_Renderable_AutoInvadeSphere_BaseColor);
            ImGui.ColorEdit3("Auto Invade Sphere - highlight color", ref CFG.Current.GFX_Renderable_AutoInvadeSphere_HighlightColor);

            ImGui.ColorEdit3("Level Connector Sphere - base color", ref CFG.Current.GFX_Renderable_LevelConnectorSphere_BaseColor);
            ImGui.ColorEdit3("Level Connector - highlight color", ref CFG.Current.GFX_Renderable_LevelConnectorSphere_HighlightColor);


            ImGui.ColorEdit3("Gizmo: X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
            ImGui.ColorEdit3("Gizmo: X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

            ImGui.ColorEdit3("Gizmo: Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
            ImGui.ColorEdit3("Gizmo: Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

            ImGui.ColorEdit3("Gizmo: Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
            ImGui.ColorEdit3("Gizmo: Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);

            ImGui.SliderFloat("Wireframe color variance", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);

            if (ImGui.Button("Reset", DPI.StandardButtonSize))
            {
                ResetVisualisationCFG();
            }
            UIHelper.Tooltip("Resets all of the values within this section to their default values.");

        }

        //---------------------------------------
        // Display Presets
        //---------------------------------------
        if (ImGui.CollapsingHeader("Display Presets"))
        {
            ImGui.Text("Configure each of the six display presets available.");

            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);

            if (ImGui.Button("Reset##DisplayPresets", defaultButtonSize))
            {
                ResetSceneFilterPresetCFG();
            }
            UIHelper.Tooltip("Reset the values within this section to their default values.");

        }
    }

    private void SettingsRenderFilterPresetEditor(RenderFilterPreset preset)
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

    private void ResetRenderingCFG()
    {
        CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;

        CFG.Current.Viewport_DefaultRender_Brightness = CFG.Default.Viewport_DefaultRender_Brightness;
        CFG.Current.Viewport_DefaultRender_Saturation = CFG.Default.Viewport_DefaultRender_Saturation;
        CFG.Current.Viewport_Enable_Model_Masks = CFG.Default.Viewport_Enable_Model_Masks;
        CFG.Current.Viewport_Enable_LOD_Facesets = CFG.Default.Viewport_Enable_LOD_Facesets;

        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
        CFG.Current.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport_Limit_Buffer_Indirect_Draw;
        CFG.Current.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport_Limit_Buffer_Flver_Bone;
    }

    private void ResetSceneFilterPresetCFG()
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

    private void ResetVisualisationCFG()
    {
        CFG.Current.Viewport_Enable_Selection_Outline = CFG.Default.Viewport_Enable_Selection_Outline;
        CFG.Current.Viewport_DefaultRender_SelectColor = CFG.Default.Viewport_DefaultRender_SelectColor;
        CFG.Current.GFX_Renderable_Default_Wireframe_Alpha = CFG.Default.GFX_Renderable_Default_Wireframe_Alpha;

        CFG.Current.GFX_Renderable_Box_BaseColor = CFG.Default.GFX_Renderable_Box_BaseColor;
        CFG.Current.GFX_Renderable_Box_HighlightColor = CFG.Default.GFX_Renderable_Box_HighlightColor;
        CFG.Current.GFX_Renderable_Box_Alpha = CFG.Default.GFX_Renderable_Box_Alpha;

        CFG.Current.GFX_Renderable_Cylinder_BaseColor = CFG.Default.GFX_Renderable_Cylinder_BaseColor;
        CFG.Current.GFX_Renderable_Cylinder_HighlightColor = CFG.Default.GFX_Renderable_Cylinder_HighlightColor;
        CFG.Current.GFX_Renderable_Cylinder_Alpha = CFG.Default.GFX_Renderable_Cylinder_Alpha;

        CFG.Current.GFX_Renderable_Sphere_BaseColor = CFG.Default.GFX_Renderable_Sphere_BaseColor;
        CFG.Current.GFX_Renderable_Sphere_HighlightColor = CFG.Default.GFX_Renderable_Sphere_HighlightColor;
        CFG.Current.GFX_Renderable_Sphere_Alpha = CFG.Default.GFX_Renderable_Sphere_Alpha;

        CFG.Current.GFX_Renderable_Point_BaseColor = CFG.Default.GFX_Renderable_Point_BaseColor;
        CFG.Current.GFX_Renderable_Point_HighlightColor = CFG.Default.GFX_Renderable_Point_HighlightColor;
        CFG.Current.GFX_Renderable_Point_Alpha = CFG.Default.GFX_Renderable_Point_Alpha;

        CFG.Current.GFX_Renderable_DummyPoly_BaseColor = CFG.Default.GFX_Renderable_DummyPoly_BaseColor;
        CFG.Current.GFX_Renderable_DummyPoly_HighlightColor = CFG.Default.GFX_Renderable_DummyPoly_HighlightColor;
        CFG.Current.GFX_Renderable_DummyPoly_Alpha = CFG.Default.GFX_Renderable_DummyPoly_Alpha;

        CFG.Current.GFX_Renderable_BonePoint_BaseColor = CFG.Default.GFX_Renderable_BonePoint_BaseColor;
        CFG.Current.GFX_Renderable_BonePoint_HighlightColor = CFG.Default.GFX_Renderable_BonePoint_HighlightColor;
        CFG.Current.GFX_Renderable_BonePoint_Alpha = CFG.Default.GFX_Renderable_BonePoint_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Chr_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Chr_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Chr_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Chr_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Object_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Object_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Object_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Object_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Player_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Player_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Player_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Player_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Other_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Other_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Other_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Other_Alpha;

        CFG.Current.GFX_Renderable_PointLight_BaseColor = CFG.Default.GFX_Renderable_PointLight_BaseColor;
        CFG.Current.GFX_Renderable_PointLight_HighlightColor = CFG.Default.GFX_Renderable_PointLight_HighlightColor;
        CFG.Current.GFX_Renderable_PointLight_Alpha = CFG.Default.GFX_Renderable_PointLight_Alpha;

        CFG.Current.GFX_Renderable_SpotLight_BaseColor = CFG.Default.GFX_Renderable_SpotLight_BaseColor;
        CFG.Current.GFX_Renderable_SpotLight_HighlightColor = CFG.Default.GFX_Renderable_SpotLight_HighlightColor;
        CFG.Current.GFX_Renderable_SpotLight_Alpha = CFG.Default.GFX_Renderable_SpotLight_Alpha;

        CFG.Current.GFX_Renderable_DirectionalLight_BaseColor = CFG.Default.GFX_Renderable_DirectionalLight_BaseColor;
        CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor = CFG.Default.GFX_Renderable_DirectionalLight_HighlightColor;
        CFG.Current.GFX_Renderable_DirectionalLight_Alpha = CFG.Default.GFX_Renderable_DirectionalLight_Alpha;

        CFG.Current.GFX_Renderable_AutoInvadeSphere_BaseColor = CFG.Default.GFX_Renderable_AutoInvadeSphere_BaseColor;
        CFG.Current.GFX_Renderable_AutoInvadeSphere_HighlightColor = CFG.Default.GFX_Renderable_AutoInvadeSphere_HighlightColor;

        CFG.Current.GFX_Renderable_LevelConnectorSphere_BaseColor = CFG.Default.GFX_Renderable_LevelConnectorSphere_BaseColor;
        CFG.Current.GFX_Renderable_LevelConnectorSphere_HighlightColor = CFG.Default.GFX_Renderable_LevelConnectorSphere_HighlightColor;

        CFG.Current.GFX_Gizmo_X_BaseColor = CFG.Default.GFX_Gizmo_X_BaseColor;
        CFG.Current.GFX_Gizmo_X_HighlightColor = CFG.Default.GFX_Gizmo_X_HighlightColor;

        CFG.Current.GFX_Gizmo_Y_BaseColor = CFG.Default.GFX_Gizmo_Y_BaseColor;
        CFG.Current.GFX_Gizmo_Y_HighlightColor = CFG.Default.GFX_Gizmo_Y_HighlightColor;

        CFG.Current.GFX_Gizmo_Z_BaseColor = CFG.Default.GFX_Gizmo_Z_BaseColor;
        CFG.Current.GFX_Gizmo_Z_HighlightColor = CFG.Default.GFX_Gizmo_Z_HighlightColor;

        CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
    }
}

#endregion