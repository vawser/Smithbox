using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The list of binders for the source type.
/// </summary>
public class MaterialContainerList
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    private string ContainerListFilter = "";
    private bool ExactContainerListFilter = false;

    public MaterialContainerList(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Draw(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("MAT_ContainerList_Header_Containers"),
            LOC.Get("MAT_ContainerList_Header_Containers_TT"));

        EditorFilters.DisplayFramedListFilter("materialEditor_ContainerList",
            ref ContainerListFilter, ref ExactContainerListFilter);

        ImGui.BeginChild("ContainerList", new System.Numerics.Vector2(width, height), ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("sourceTabs");

        // The tab bar applies a requested tab on the next frame
        var requestedType = Parent.Selection.RequestedSourceType;
        Parent.Selection.RequestedSourceType = null;

        var mtdFlags = requestedType is MaterialSourceType.MTD ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
        var matbinFlags = requestedType is MaterialSourceType.MATBIN ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

        // MTD
        if (ImGui.BeginTabItem($"{LOC.Get("MAT_ContainerList_Tab_MTD")}##mtdTab", mtdFlags))
        {
            Parent.Selection.SourceType = MaterialSourceType.MTD;

            ImGui.BeginChild("MtdListSection", ImGuiChildFlags.Borders);

            DisplayMtdList();

            ImGui.EndChild();

            ImGui.EndTabItem();
        }

        if (MaterialUtils.SupportsMATBIN(Project))
        {
            // MATBIN
            if (ImGui.BeginTabItem($"{LOC.Get("MAT_ContainerList_Tab_MATBIN")}##matbinTab", matbinFlags))
            {
                Parent.Selection.SourceType = MaterialSourceType.MATBIN;

                ImGui.BeginChild("MatbinListSection", ImGuiChildFlags.Borders);

                DisplayMatbinList();

                ImGui.EndChild();

                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void DisplayMtdList()
    {
        ImGui.BeginChild("mtdListSection");

        if (Parent.Selection.SourceType is MaterialSourceType.MTD)
        {
            var wrappers = Project.Handler.MaterialData.PrimaryBank.MTDs;

            if (wrappers != null)
            {
                var filteredEntries = new List<FileDictionaryEntry>();
                foreach (var entry in wrappers)
                {
                    var isMatch = EditorFilters.IsMatch(ContainerListFilter, entry.Key.Filename, ExactContainerListFilter);

                    if (isMatch)
                    {
                        filteredEntries.Add(entry.Key);
                    }
                }

                if (Parent.Selection.ScrollToContainer && wrappers.ContainsKey(Parent.Selection.SelectedBinderEntry))
                {
                    Parent.Selection.ScrollToContainer = false;
                    MaterialUtils.ScrollToIndex(filteredEntries.IndexOf(Parent.Selection.SelectedBinderEntry));
                }

                var clipper = new ImGuiListClipper();
                clipper.Begin(filteredEntries.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var key = filteredEntries[i];
                        var curWrapper = Project.Handler.MaterialData.PrimaryBank.MTDs[key];

                        var displayName = $"{key.Filename}";

                        // Mark binders that contain files with unsaved changes
                        if (curWrapper.IsDirty())
                        {
                            displayName = $"{displayName}*";
                        }

                        if (ImGui.Selectable($"{displayName}##mtdEntry_{key.Filename}{i}", key == Parent.Selection.SelectedBinderEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            Parent.Selection.SelectedBinderEntry = key;
                            Parent.Selection.MTDWrapper = curWrapper;

                            Parent.Selection.SelectedFileKey = "";
                            Parent.Selection.SelectedMTD = null;
                            Parent.Selection.SelectedMATBIN = null;
                            Parent.Selection.SelectedFileWrapper = null;
                        }
                    }
                }

                clipper.End();
            }
        }

        ImGui.EndChild();
    }

    public void DisplayMatbinList()
    {
        ImGui.BeginChild("mtdListSection");

        if (MaterialUtils.SupportsMATBIN(Project))
        {
            if (Parent.Selection.SourceType is MaterialSourceType.MATBIN)
            {
                var wrappers = Project.Handler.MaterialData.PrimaryBank.MATBINs;

                if (wrappers != null)
                {
                    var filteredEntries = new List<FileDictionaryEntry>();
                    foreach (var entry in wrappers)
                    {
                        var isMatch = EditorFilters.IsMatch(ContainerListFilter, entry.Key.Filename, ExactContainerListFilter);

                        if (isMatch)
                        {
                            filteredEntries.Add(entry.Key);
                        }
                    }

                    if (Parent.Selection.ScrollToContainer && wrappers.ContainsKey(Parent.Selection.SelectedBinderEntry))
                    {
                        Parent.Selection.ScrollToContainer = false;
                        MaterialUtils.ScrollToIndex(filteredEntries.IndexOf(Parent.Selection.SelectedBinderEntry));
                    }

                    var clipper = new ImGuiListClipper();
                    clipper.Begin(filteredEntries.Count);

                    while (clipper.Step())
                    {
                        for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                        {
                            var key = filteredEntries[i];
                            var curWrapper = Project.Handler.MaterialData.PrimaryBank.MATBINs[key];

                            var displayName = $"{key.Filename}";

                            // Mark binders that contain files with unsaved changes
                            if (curWrapper.IsDirty())
                            {
                                displayName = $"{displayName}*";
                            }

                            if (ImGui.Selectable($"{displayName}##matbinEntry_{key}", key == Parent.Selection.SelectedBinderEntry, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                Parent.Selection.SelectedBinderEntry = key;
                                Parent.Selection.MATBINWrapper = curWrapper;

                                Parent.Selection.SelectedFileKey = "";
                                Parent.Selection.SelectedMTD = null;
                                Parent.Selection.SelectedMATBIN = null;
                                Parent.Selection.SelectedFileWrapper = null;
                            }
                        }
                    }

                    clipper.End();
                }
            }
        }

        ImGui.EndChild();
    }
}
