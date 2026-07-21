using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamListWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamEditorView ParentView;

    private string lastParamSearch = "";
    private string currentParamSearchString = "";

    public ParamListWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;

        ParentView = curView;
    }

    public void Display(bool doFocus, bool isActiveView, float scrollTo)
    {
        DisplayTitle();
        DisplayHeader(isActiveView);

        if (CFG.Current.ParamEditor_Param_List_Pinned_Stay_Visible)
        {
            DisplayPinnedParams();
        }

        ImGui.BeginChild("paramTypes");
        FocusManager.SetFocus(EditorFocusContext.ParamEditor_ParamList);

        if (!CFG.Current.ParamEditor_Param_List_Pinned_Stay_Visible)
        {
            DisplayPinnedParams();
        }

        // Stay Params
        if(Project.Descriptor.ProjectType is ProjectType.DS3)
        {
            if (Project.Handler.ParamData.PrimaryBank.StayParams.Count > 0)
            {
                DisplayStayParams(doFocus, scrollTo);
            }
        }

        DisplayParams(doFocus, scrollTo);

        ImGui.EndChild();
    }

    private void DisplayTitle()
    {
        var paramListTitle = LOC.Get("PARAM_ParamWindow_Title");

        // Param Version
        if (Editor.Project.Handler.ParamData.PrimaryBank.ParamVersion != 0)
        {
            var version = ParamUtils.ParseParamVersion(Editor.Project.Handler.ParamData.PrimaryBank.ParamVersion);

            paramListTitle = LOC.Get("PARAM_ParamWindow_Title_Version", version);

            if (Editor.Project.Handler.ParamData.PrimaryBank.ParamVersion < Editor.Project.Handler.ParamData.VanillaBank.ParamVersion)
            {
                paramListTitle = LOC.Get("PARAM_ParamWindow_Title_Version_Out_of_Date", version);
            }
        }

        GUI.SimpleHeader($"{paramListTitle}", "");
    }

    private void DisplayHeader(bool isActiveView)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("ParamFileHeaderSection", searchHeight, ImGuiChildFlags.Borders);

        // Autofill
        if (ParentView.MassEdit.AutoFill != null)
        {
            ImGui.AlignTextToFramePadding();
            var resAutoParam = ParentView.MassEdit.AutoFill.ParamSearchBarAutoFill();

            if (resAutoParam != null &&
                Editor.ViewHandler.ActiveView.Selection.ActiveParamExists())
            {
                currentParamSearchString = resAutoParam;
            }
        }

        ImGui.SameLine();

        // Search param
        if (isActiveView && InputManager.IsPressed(KeybindID.ParamEditor_Focus_Searchbar))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputTextWithHint($"##paramSearch", LOC.Get("PARAM_ParamWindow_Search_Hint"), ref currentParamSearchString, 256);
        GUI.Tooltip(LOC.Get("PARAM_ParamWindow_Search_Hint_TT", InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)));

        if (!currentParamSearchString.Equals(lastParamSearch))
        {
            CacheBank.ClearCaches();
            lastParamSearch = currentParamSearchString;
        }

        // Toggle Table Group Column
        if (AllowTableGroupToggle())
        {
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Table}##tableGroupVisToggle"))
            {
                CFG.Current.ParamEditor_Display_Table_List = !CFG.Current.ParamEditor_Display_Table_List;
            }

            var tableGroupWindowVis = LOC.Get("PARAM_ParamWindow_ToggleTableGroup_Hidden");
            if (!CFG.Current.ParamEditor_Display_Table_List)
                tableGroupWindowVis = LOC.Get("PARAM_ParamWindow_ToggleTableGroup_Visible");

            GUI.Tooltip(LOC.Get("PARAM_ParamWindow_ToggleTableGroup_Hint", tableGroupWindowVis));
        }

        // Toggle Param Community Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}##paramCommunityNamesToggle"))
        {
            CFG.Current.ParamEditor_Param_List_Display_Community_Names = !CFG.Current.ParamEditor_Param_List_Display_Community_Names;
        }

        var paramCommunityNamesVis = LOC.Get("PARAM_ParamWindow_ToggleCommunityNames_Source");
        if (CFG.Current.ParamEditor_Param_List_Display_Community_Names)
            paramCommunityNamesVis = LOC.Get("PARAM_ParamWindow_ToggleCommunityNames_Community");

        GUI.Tooltip(LOC.Get("PARAM_ParamWindow_ToggleCommunityNames_Hint", paramCommunityNamesVis));

        // Toggle Param Categories
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Cubes}##paramCategoriesToggle"))
        {
            CFG.Current.ParamEditor_Param_List_Display_Categories = !CFG.Current.ParamEditor_Param_List_Display_Categories;
        }

        var paramCategoriesVis = LOC.Get("PARAM_ParamWindow_ToggleCategories_Hidden");
        if (CFG.Current.ParamEditor_Param_List_Display_Categories)
            paramCategoriesVis = LOC.Get("PARAM_ParamWindow_ToggleCategories_Visible");

        GUI.Tooltip(LOC.Get("PARAM_ParamWindow_ToggleCategories_Hint", paramCategoriesVis));

        ImGui.EndChild();
    }

    private bool AllowTableGroupToggle()
    {
        var activeParam = ParentView.Selection.GetActiveParam();

        if (Project.Handler.ParamData.TableParamList == null)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Count == 0)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Contains(activeParam))
        {
            return true;
        }

        return false;
    }

    private void DisplayPinnedParams()
    {
        List<string> pinnedParamKeyList = new(Editor.Project.Descriptor.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            var height = 10 + (20 * pinnedParamKeyList.Count);

            var searchHeight = new Vector2(0, height) * DPI.UIScale();
            ImGui.BeginChild("ParamFilePinnedSection", searchHeight, ImGuiChildFlags.Borders);

            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = Editor.Project.Handler.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);

                if (Editor.Project.Handler.ParamData.PrimaryBank.Params.ContainsKey(paramKey))
                {
                    Param p = Editor.Project.Handler.ParamData.PrimaryBank.Params[paramKey];
                    if (p != null)
                    {
                        var meta = Editor.Project.Handler.ParamData.GetParamMeta(p.AppliedParamdef);
                        var annotations = Editor.Project.Handler.ParamData.GetParamAnnotations(p.AppliedParamdef.ParamType);

                        var Wiki = annotations?.Description;
                        if (Wiki != null)
                        {
                            EditorTableUtils.HelpIcon(paramKey + "wiki", ref Wiki, true);
                        }
                    }

                    ImGui.Indent(15.0f);
                    if (ImGui.Selectable($"{paramKey}##pin{paramKey}", paramKey == ParentView.Selection.GetActiveParam()))
                    {
                        SelectParam(paramKey);
                    }

                    DisplayContextMenu(p, paramKey, true);

                    ImGui.Unindent(15.0f);
                }
            }

            ImGui.EndChild();
        }
    }

    private void DisplayParams(bool doFocus, float scrollTo)
    {
        ImGui.BeginChild("ParamFileParamSection", ImGuiChildFlags.Borders);

        List<string> paramKeyList = CacheBank.GetCached(Editor, ParentView.ViewIndex, () =>
        {
            var primaryBank = Editor.Project.Handler.ParamData.PrimaryBank;

            List<(ParamBank, Param)> list = null;

            if (ParentView.MassEdit.PSE != null)
            {
                list = ParentView.MassEdit.PSE.Search(true, currentParamSearchString, true, true);
            }

            if (list != null)
            {
                var keyList = list.Where(param => param.Item1 == primaryBank)
                    .Select(param => primaryBank.GetKeyForParam(param.Item2)).ToList();

                if (CFG.Current.ParamEditor_Param_List_Sort_Alphabetically)
                {
                    keyList.Sort();
                }

                return keyList;
            }
            else
            {
                var keyList = primaryBank.Params.Select(e => e.Key).ToList();

                return keyList;
            }
        });

        // Param Categories
        var categoryObj = Editor.Project.Handler.ParamData.ParamCategories;
        var categories = Editor.Project.Handler.ParamData.ParamCategories.Categories;

        if (categories != null && CFG.Current.ParamEditor_Param_List_Display_Categories)
        {
            var generalParamList = new List<string>();

            // Build a general list from all unassigned params
            foreach (var paramKey in paramKeyList)
            {
                bool add = true;

                foreach (var category in categories)
                {
                    foreach (var entry in category.Params)
                    {
                        if (entry == paramKey)
                            add = false;
                    }
                }

                if (add)
                    generalParamList.Add(paramKey);
            }

            // TODO: perhaps add an actual ordering system to the ParamCategories (using SortID) instead of this crude boolean method
            if (categories.Count > 0)
            {
                // Categories - Forced Top
                for(int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];

                    if (category.ForceTop)
                    {
                        if (ImGui.CollapsingHeader($"{category.GetDisplayName()}##forceTopCategory{i}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scrollTo);
                        }
                    }
                }

                // General List
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("PARAM_ParamWindow_Category_General")}##generalCategory", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    DisplayParamList(paramKeyList, generalParamList, doFocus, scrollTo);
                }

                int index = 0;

                // Categories - Default
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];

                    if (!category.ForceTop && !category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.GetDisplayName()}##category{i}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scrollTo);
                        }
                    }
                    index++;
                }

                // Categories - Forced Bottom
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];

                    if (category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.GetDisplayName()}##forceBottomCategory{i}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scrollTo);
                        }

                    }
                }
            }
            else
            {
                // Fallback to full view
                DisplayParamList(paramKeyList, paramKeyList, doFocus, scrollTo);
            }
        }
        else
        {
            // Fallback to full view
            DisplayParamList(paramKeyList, paramKeyList, doFocus, scrollTo);
        }

        ImGui.EndChild();
    }

    public void DisplayParamList(List<string> paramKeyList, List<string> visibleParams, bool doFocus, float scrollTo)
    {
        foreach (var paramKey in paramKeyList)
        {
            HashSet<int> primary = Editor.Project.Handler.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
            Param p = Editor.Project.Handler.ParamData.PrimaryBank.Params[paramKey];

            if (!visibleParams.Contains(paramKey))
                continue;

            if (p != null)
            {
                var meta = Editor.Project.Handler.ParamData.GetParamMeta(p.AppliedParamdef);
                var annotations = Editor.Project.Handler.ParamData.GetParamAnnotations(p.AppliedParamdef.ParamType);

                var Wiki = annotations?.Description;
                if (Wiki != null)
                {
                    EditorTableUtils.HelpIcon(paramKey + "wiki", ref Wiki, true);
                }
            }

            ImGui.Indent(15.0f);

            var popColor = false;

            if (primary != null ? primary.Any() : false)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
                popColor = true;
            }

            var displayedName = paramKey;

            if (CFG.Current.ParamEditor_Param_List_Display_Community_Names)
            {
                var meta = Editor.Project.Handler.ParamData.GetParamMeta(p.AppliedParamdef);
                var names = meta?.DisplayNames;

                if (names != null)
                {
                    var paramDisplayName = names.Where(e => e.Param == paramKey).FirstOrDefault();

                    if (paramDisplayName != null)
                    {
                        displayedName = paramDisplayName.Name;
                    }
                }
            }

            if (ImGui.Selectable($"{displayedName}##{paramKey}", paramKey == ParentView.Selection.GetActiveParam()))
            {
                SelectParam(paramKey);
            }

            if (popColor)
            {
                ImGui.PopStyleColor(1);
            }

            if (doFocus && paramKey == ParentView.Selection.GetActiveParam())
            {
                scrollTo = ImGui.GetCursorPosY();
            }

            // Context Menu
            DisplayContextMenu(p, paramKey);

            ImGui.Unindent(15.0f);
        }

        if (doFocus)
        {
            ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
        }
    }

    /// <summary>
    /// The context menu for a Param entry.
    /// </summary>
    /// <param name="param"></param>
    /// <param name="paramKey"></param>
    /// <param name="isPinnedEntry"></param>
    public void DisplayContextMenu(Param param, string paramKey, bool isPinnedEntry = false)
    {
        if (ImGui.BeginPopupContextItem($"{paramKey}"))
        {
            // Information
            if(ImGui.BeginMenu($"{LOC.Get("PARAM_ParamWindow_Context_Info_Header")}##infoMenuHeader"))
            {
                ImGui.Text(LOC.Get("PARAM_ParamWindow_Context_ParamType", param.ParamType));

                // Copy Name
                if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Context_Action_Copy_Name")}##copyNameAction"))
                {
                    PlatformUtils.Instance.SetClipboardText(paramKey);
                }

                // Copy Type
                if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Context_Action_Copy_Type")}##copyTypeAction"))
                {
                    PlatformUtils.Instance.SetClipboardText(param.ParamType);
                }

                ImGui.EndMenu();
            }

            // Pinning
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_ParamWindow_Context_Pin_Header")}##pinMenuHeader"))
            {
                if (!isPinnedEntry)
                {
                    // Pin
                    if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Context_Action_Pin")}##pinAction"))
                    {
                        List<string> pinned = Editor.Project.Descriptor.PinnedParams;

                        if (!pinned.Contains(paramKey))
                        {
                            pinned.Add(paramKey);
                        }
                    }
                    GUI.Tooltip(LOC.Get("PARAM_ParamWindow_Context_Action_Pin_TT"));
                }
                else if (isPinnedEntry)
                {
                    // Unpin
                    if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Context_Action_Unpin")}##unpinAction"))
                    {
                        List<string> pinned = Editor.Project.Descriptor.PinnedParams;

                        if (pinned.Contains(paramKey))
                        {
                            pinned.Remove(paramKey);
                        }
                    }
                    GUI.Tooltip(LOC.Get("PARAM_ParamWindow_Context_Action_Unpin_TT"));
                }

                ImGui.EndMenu();
            }

            // Export
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_ParamWindow_Context_Export_Header")}##exportMenuHeader"))
            {
                // Export Loose Param as File
                if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Context_Action_Export_Loose_Param")}##exportLooseParamAction"))
                {
                    ExportParam(paramKey);
                }
                GUI.Tooltip(LOC.Get("PARAM_ParamWindow_Context_Action_Export_Loose_Param_TT"));

                ImGui.EndMenu();
            }

            // Wiki
            if (CFG.Current.Developer_Enable_Tools)
            {
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_ParamWindow_Context_Wiki_Header")}##wikiMenuHeader"))
                {
                    // Export Param List
                    if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Action_Copy_Param_List")}##copyParamListAction"))
                    {
                        ParamDebugTools.OutputParamTableInformation(Editor, Project);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_ParamWindow_Action_Copy_Param_TT"));

                    // Export Field List
                    if (ImGui.Selectable($"{LOC.Get("PARAM_ParamWindow_Action_Copy_Field_List")}##copyFieldListAction"))
                    {
                        ParamDebugTools.OutputParamInformation(Editor, Project, paramKey);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_ParamWindow_Action_Copy_Field_TT2"));

                    ImGui.EndMenu();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void SelectParam(string paramKey)
    {
        ParentView.Selection.ActiveStayParam = null; // Clear this so we know to hide the StayParam Field window

        EditorCommandQueue.AddCommand($@"param/view/{ParentView.ViewIndex}/{paramKey}");

        ParentView.ParamTableWindow.UpdateTableSelection(paramKey);

        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask(true);

        ParentView.RowDecorators.SetupFmgDecorators(paramKey);

        Smithbox.TextureManager.IconManager.PurgeCache();
    }

    private void SelectStayParam(string paramKey)
    {
        ParentView.Selection.CleanAllSelectionState();
        ParentView.Selection.ActiveStayParam = paramKey;

        Smithbox.TextureManager.IconManager.PurgeCache();
    }

    private void ExportParam(string paramKey)
    {
        var paramData = Editor.Project.Handler.ParamData.PrimaryBank.Params.GetValueOrDefault(paramKey);

        if(paramData == null)
        {
            Smithbox.LogError<ParamListWindow>(
                LOC.Get("PARAM_ParamWindow_ExportParam_Error_Invalid_ParamData", paramKey));

            return;
        }

        var exportDir = Path.Combine(Project.Descriptor.ProjectPath, "param");
        var savePath = ProjectUtils.NormalizePath($"{exportDir}\\{paramKey}.param");

        if(!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        var saveData = paramData.Write();

        File.WriteAllBytes(savePath, saveData);

        Smithbox.Log<ParamListWindow>(LOC.Get("PARAM_ParamWindow_ExportParam_Log", savePath));
    }

    #region Stay Params

    private void DisplayStayParams(bool doFocus, float scrollTo)
    {
        ImGui.BeginChild("StayParamFileParamSection", new Vector2(0, 110) * DPI.UIScale(), ImGuiChildFlags.Borders);

        foreach(var param in Project.Handler.ParamData.PrimaryBank.StayParams)
        {
            var paramKey = param.Key;
            var meta = Editor.Project.Handler.ParamData.GetParamMeta(param.Value.Def);
            var annotations = Editor.Project.Handler.ParamData.GetParamAnnotations(param.Value.Def.ParamType);

            var Wiki = annotations?.Description;
            if (Wiki != null)
            {
                EditorTableUtils.HelpIcon(paramKey + "wiki", ref Wiki, true);
            }

            ImGui.Indent(15.0f);
            if (ImGui.Selectable($"{paramKey}##selectStayParam{paramKey}", paramKey == ParentView.Selection.ActiveStayParam))
            {
                SelectStayParam(paramKey);
            }

            ImGui.Unindent(15.0f);
        }

        ImGui.EndChild();
    }

    #endregion
}
