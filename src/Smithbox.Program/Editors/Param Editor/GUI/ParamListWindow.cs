using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        DisplayParams(doFocus, scrollTo);

        ImGui.EndChild();
    }

    private void DisplayHeader(bool isActiveView)
    {
        ImGui.Text("Params");

        // Param Version
        if (Editor.Project.Handler.ParamData.PrimaryBank.ParamVersion != 0)
        {
            ImGui.SameLine();
            ImGui.Text($"- Version {ParamUtils.ParseParamVersion(Editor.Project.Handler.ParamData.PrimaryBank.ParamVersion)}");

            if (Editor.Project.Handler.ParamData.PrimaryBank.ParamVersion < Editor.Project.Handler.ParamData.VanillaBank.ParamVersion)
            {
                ImGui.SameLine();

                UIHelper.WrappedText("(out of date)");
            }
        }

        ImGui.Separator();

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
        ImGui.InputText($"##paramSearch", ref currentParamSearchString, 256);

        UIHelper.Tooltip($"Search <{InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)}>");

        if (!currentParamSearchString.Equals(lastParamSearch))
        {
            CacheBank.ClearCaches();
            lastParamSearch = currentParamSearchString;
        }

        // Toggle Table Group Column
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Table}##tableGroupVisToggle"))
        {
            CFG.Current.ParamEditor_Display_Table_List = !CFG.Current.ParamEditor_Display_Table_List;
        }

        var tableGroupWindowVis = "Hidden";
        if (!CFG.Current.ParamEditor_Display_Table_List)
            tableGroupWindowVis = "Visible";

        UIHelper.Tooltip($"Toggle the display of the Table Group window.\nCurrent Mode: {tableGroupWindowVis}");

        ImGui.Separator();
    }

    private void DisplayPinnedParams()
    {
        List<string> pinnedParamKeyList = new(Editor.Project.Descriptor.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = Editor.Project.Handler.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);

                if (Editor.Project.Handler.ParamData.PrimaryBank.Params.ContainsKey(paramKey))
                {
                    Param p = Editor.Project.Handler.ParamData.PrimaryBank.Params[paramKey];
                    if (p != null)
                    {
                        var meta = Editor.Project.Handler.ParamData.GetParamMeta(p.AppliedParamdef);
                        var Wiki = meta?.Wiki;
                        if (Wiki != null)
                        {
                            if (EditorTableUtils.HelpIcon(paramKey + "wiki", ref Wiki, true))
                            {
                                meta.Wiki = Wiki;
                            }
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

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    private void DisplayParams(bool doFocus, float scrollTo)
    {
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
                foreach (var category in categories)
                {
                    if (category.ForceTop)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scrollTo);
                        }
                    }
                }

                // General List
                if (ImGui.CollapsingHeader($"General", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    DisplayParamList(paramKeyList, generalParamList, doFocus, scrollTo);
                }

                // Categories - Default
                foreach (var category in categories)
                {
                    if (!category.ForceTop && !category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scrollTo);
                        }
                    }
                }

                // Categories - Forced Bottom
                foreach (var category in categories)
                {
                    if (category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
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

                var Wiki = meta?.Wiki;
                if (Wiki != null)
                {
                    if (EditorTableUtils.HelpIcon(paramKey + "wiki", ref Wiki, true))
                    {
                        meta.Wiki = Wiki;
                    }
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
            if (ImGui.Selectable("Copy Name"))
            {
                PlatformUtils.Instance.SetClipboardText(paramKey);
            }
            UIHelper.Tooltip($"Copy the name of the current param selection to the clipboard.");

            // Pin
            if (!isPinnedEntry)
            {
                if (ImGui.Selectable($"Pin"))
                {
                    List<string> pinned = Editor.Project.Descriptor.PinnedParams;

                    if (!pinned.Contains(paramKey))
                    {
                        pinned.Add(paramKey);
                    }
                }
                UIHelper.Tooltip($"Pin the current param selection to the top of the param list.");
            }
            // Unpin
            else if (isPinnedEntry)
            {
                if (ImGui.Selectable($"Unpin"))
                {
                    List<string> pinned = Editor.Project.Descriptor.PinnedParams;

                    if (pinned.Contains(paramKey))
                    {
                        pinned.Remove(paramKey);
                    }
                }
                UIHelper.Tooltip($"Unpin the current param selection from the top of the param list.");
            }

            if (CFG.Current.Developer_Enable_Tools)
            {
                if (ImGui.Selectable("Copy Param List"))
                {
                    ParamDebugTools.OutputParamTableInformation(Editor, Project);
                }
                UIHelper.Tooltip($"Export the param list table for the SoulsModding wiki to the clipboard.");

                if (ImGui.Selectable("Copy Param Field List"))
                {
                    ParamDebugTools.OutputParamInformation(Editor, Project, paramKey);
                }
                UIHelper.Tooltip($"Export the param field list table for the SoulsModding wiki for this param to the clipboard.");
            }

            ImGui.EndPopup();
        }
    }

    private void SelectParam(string paramKey)
    {
        EditorCommandQueue.AddCommand($@"param/view/{ParentView.ViewIndex}/{paramKey}");

        ParentView.ParamTableWindow.UpdateTableSelection(paramKey);

        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask(true);

        ParentView.RowDecorators.SetupFmgDecorators(paramKey);

        Smithbox.TextureManager.IconManager.PurgeCache();
    }
}
