using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Debug.Generators;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The param column within a Param Editor view
/// </summary>
public class ParamView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView View;

    private string lastParamSearch = "";

    public ParamView(ParamEditorScreen editor, ProjectEntry project, ParamEditorView view)
    {
        Editor = editor;
        Project = project;
        View = view;
    }

    /// <summary>
    /// Entry point for the Params column.
    /// </summary>
    /// <param name="doFocus"></param>
    /// <param name="isActiveView"></param>
    /// <param name="scale"></param>
    /// <param name="scrollTo"></param>
    public void Display(bool doFocus, bool isActiveView, float scale, float scrollTo)
    {
        DisplayHeader(isActiveView);

        if (CFG.Current.Param_PinnedRowsStayVisible)
        {
            DisplayPinnedParams(scale);
        }

        ImGui.BeginChild("paramTypes");

        if (!CFG.Current.Param_PinnedRowsStayVisible)
        {
            DisplayPinnedParams(scale);
        }

        if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedParams)
        {
            DisplayParams(doFocus, scale, scrollTo);
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// The header section in the Params column.
    /// </summary>
    /// <param name="isActiveView"></param>
    private void DisplayHeader(bool isActiveView)
    {
        ImGui.Text("Params");

        // Param Version
        if (Editor.Project.ParamData.PrimaryBank.ParamVersion != 0)
        {
            ImGui.SameLine();
            ImGui.Text($"- Version {Utils.ParseParamVersion(Editor.Project.ParamData.PrimaryBank.ParamVersion)}");

            if (Editor.Project.ParamData.PrimaryBank.ParamVersion < Editor.Project.ParamData.VanillaBank.ParamVersion)
            {
                ImGui.SameLine();
                UIHelper.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, "(out of date)");
            }
        }

        ImGui.Separator();

        // Autofill
        if (Editor.MassEditHandler.AutoFill != null)
        {
            ImGui.AlignTextToFramePadding();
            var resAutoParam = Editor.MassEditHandler.AutoFill.ParamSearchBarAutoFill();

            if (resAutoParam != null)
            {
                View.Selection.SetCurrentParamSearchString(resAutoParam);
            }
        }

        ImGui.SameLine();

        // Search param
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchParam))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##paramSearch", ref View.Selection.currentParamSearchString, 256);
        UIHelper.Tooltip($"Search <{KeyBindings.Current.PARAM_SearchParam.HintText}>");

        if (!View.Selection.currentParamSearchString.Equals(lastParamSearch))
        {
            UICache.ClearCaches();
            lastParamSearch = View.Selection.currentParamSearchString;
        }

        ImGui.Separator();
    }

    /// <summary>
    /// The pinned params section in the Param column.
    /// </summary>
    /// <param name="scale"></param>
    private void DisplayPinnedParams(float scale)
    {
        List<string> pinnedParamKeyList = new(Editor.Project.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = Editor.Project.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);

                if (Editor.Project.ParamData.PrimaryBank.Params.ContainsKey(paramKey))
                {
                    Param p = Editor.Project.ParamData.PrimaryBank.Params[paramKey];
                    if (p != null)
                    {
                        var meta = Editor.Project.ParamData.GetParamMeta(p.AppliedParamdef);
                        var Wiki = meta?.Wiki;
                        if (Wiki != null)
                        {
                            if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                            {
                                meta.Wiki = Wiki;
                            }
                        }
                    }

                    ImGui.Indent(15.0f * scale);
                    if (ImGui.Selectable($"{paramKey}##pin{paramKey}", paramKey == View.Selection.GetActiveParam()))
                    {
                        EditorCommandQueue.AddCommand($@"param/view/{View.ViewIndex}/{paramKey}");
                    }

                    DisplayContextMenu(p, paramKey, true);

                    ImGui.Unindent(15.0f * scale);
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    /// <summary>
    /// The main params section in the Param column.
    /// </summary>
    /// <param name="doFocus"></param>
    /// <param name="scale"></param>
    /// <param name="scrollTo"></param>
    private void DisplayParams(bool doFocus, float scale, float scrollTo)
    {
        List<string> paramKeyList = UICache.GetCached(Editor, View.ViewIndex, () =>
        {
            List<(ParamBank, Param)> list = null;

            if (Editor.MassEditHandler.pse != null)
            {
                list = Editor.MassEditHandler.pse.Search(true, View.Selection.currentParamSearchString, true, true);
            }

            if (list != null)
            {
                var keyList = list.Where(param => param.Item1 == Editor.Project.ParamData.PrimaryBank)
                    .Select(param => Editor.Project.ParamData.PrimaryBank.GetKeyForParam(param.Item2)).ToList();

                if (CFG.Current.Param_AlphabeticalParams)
                {
                    keyList.Sort();
                }

                return keyList;
            }
            else
            {
                var keyList = Editor.Project.ParamData.PrimaryBank.Params.Select(e => e.Key).ToList();
            }

            return new List<string>();
        });


        var categoryObj = Editor.Project.ParamCategories;
        var categories = Editor.Project.ParamCategories.Categories;

        if (categories != null && CFG.Current.Param_DisplayParamCategories)
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
                            DisplayParamList(paramKeyList, category.Params, doFocus, scale, scrollTo);
                        }
                    }
                }

                // General List
                if (ImGui.CollapsingHeader($"General", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    DisplayParamList(paramKeyList, generalParamList, doFocus, scale, scrollTo);
                }

                // Categories - Default
                foreach (var category in categories)
                {
                    if (!category.ForceTop && !category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scale, scrollTo);
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
                            DisplayParamList(paramKeyList, category.Params, doFocus, scale, scrollTo);
                        }
                    }
                }
            }
            else
            {
                // Fallback to full view
                DisplayParamList(paramKeyList, paramKeyList, doFocus, scale, scrollTo);
            }
        }
        else
        {
            // Fallback to full view
            DisplayParamList(paramKeyList, paramKeyList, doFocus, scale, scrollTo);
        }
    }

    /// <summary>
    /// The actual list of params for the main section.
    /// </summary>
    /// <param name="paramKeyList"></param>
    /// <param name="visibleParams"></param>
    /// <param name="doFocus"></param>
    /// <param name="scale"></param>
    /// <param name="scrollTo"></param>
    public void DisplayParamList(List<string> paramKeyList, List<string> visibleParams, bool doFocus, float scale, float scrollTo)
    {
        foreach (var paramKey in paramKeyList)
        {
            HashSet<int> primary = Editor.Project.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
            Param p = Editor.Project.ParamData.PrimaryBank.Params[paramKey];

            if (!visibleParams.Contains(paramKey))
                continue;

            if (p != null)
            {
                var meta = Editor.Project.ParamData.GetParamMeta(p.AppliedParamdef);

                var Wiki = meta?.Wiki;
                if (Wiki != null)
                {
                    if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                    {
                        meta.Wiki = Wiki;
                    }
                }
            }

            ImGui.Indent(15.0f * scale);

            if (primary != null ? primary.Any() : false)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            }

            var displayedName = paramKey;

            if (CFG.Current.Param_ShowParamCommunityName)
            {
                var meta = Editor.Project.ParamData.GetParamMeta(p.AppliedParamdef);
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

            if (ImGui.Selectable($"{displayedName}##{paramKey}", paramKey == View.Selection.GetActiveParam()))
            {
                //_selection.setActiveParam(param.Key);
                EditorCommandQueue.AddCommand($@"param/view/{View.ViewIndex}/{paramKey}");
            }

            ImGui.PopStyleColor();

            if (doFocus && paramKey == View.Selection.GetActiveParam())
            {
                scrollTo = ImGui.GetCursorPosY();
            }

            // Context Menu
            DisplayContextMenu(p, paramKey);

            ImGui.Unindent(15.0f * scale);
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
            var width = CFG.Current.Param_ParamContextMenu_Width;

            ImGui.SetNextItemWidth(width);

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
                    List<string> pinned = Editor.Project.PinnedParams;

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
                    List<string> pinned = Editor.Project.PinnedParams;

                    if (pinned.Contains(paramKey))
                    {
                        pinned.Remove(paramKey);
                    }
                }
                UIHelper.Tooltip($"Unpin the current param selection from the top of the param list.");
            }

            if (CFG.Current.EnableWikiTools)
            {
                if (ImGui.Selectable("Copy Param List"))
                {
                    DokuWikiGenerator.OutputParamTableInformation(Editor.BaseEditor, Editor.Project);
                }
                UIHelper.Tooltip($"Export the param list table for the SoulsModding wiki to the clipboard.");

                if (ImGui.Selectable("Copy Param Field List"))
                {
                    DokuWikiGenerator.OutputParamInformation(Editor.BaseEditor, Editor.Project, paramKey);
                }
                UIHelper.Tooltip($"Export the param field list table for the SoulsModding wiki for this param to the clipboard.");
            }

            ImGui.EndPopup();
        }
    }
}
