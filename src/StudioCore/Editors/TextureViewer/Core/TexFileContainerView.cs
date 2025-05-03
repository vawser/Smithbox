using HKLib.hk2018.hkaiCollisionAvoidance;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;
using StudioCore.Core;

namespace StudioCore.Editors.TextureViewer;

public class TexFileContainerView
{
    private TextureViewerScreen Editor;
    private TexViewSelection Selection;
    private TexFilters Filters;

    public TexFileContainerView(TextureViewerScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        Filters = screen.Filters;
    }

    // <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Files##TextureContainerList");
        Selection.SwitchWindowContext(TextureViewerContext.FileList);

        Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("TextureFileCategories");
        Selection.SwitchWindowContext(TextureViewerContext.FileList);

        if (Editor.Project.ProjectType is ProjectType.AC6 or ProjectType.ER)
        {
            DisplayFileSection("Asset", TextureViewCategory.Asset);
        }
        else
        {
            DisplayFileSection("Object", TextureViewCategory.Object);
        }

        DisplayFileSection("Characters", TextureViewCategory.Character);

        // AC6 needs some adjustments to support its parts properly
        if (Editor.Project.ProjectType != ProjectType.AC6)
        {
            DisplayFileSection("Parts", TextureViewCategory.Part);
        }

        DisplayFileSection("Particles", TextureViewCategory.Particle);

        DisplayFileSection("Menu", TextureViewCategory.Menu);

        // DS2S doesn't have an other folder
        if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
        {
            DisplayFileSection("Other", TextureViewCategory.Other);
        }

        ImGui.EndChild();

        ImGui.End();
    }

    /// <summary>
    /// The UI for each container category type
    /// </summary>
    private void DisplayFileSection(string title, TextureViewCategory displayCategory)
    {
        if (ImGui.CollapsingHeader($"{title}"))
        {
            foreach (var (name, info) in Editor.Project.TextureData.PrimaryBank.Entries)
            {
                // Skip if info is null
                if (info == null)
                    continue;

                if (Selection.InvalidateCachedName)
                {
                    info.CachedName = null;
                }

                if (info.Category == displayCategory)
                {
                    var rawName = info.Name.ToLower();
                    var aliasName = "";

                    switch (displayCategory)
                    {
                        case TextureViewCategory.Character:
                            aliasName = AliasUtils.GetCharacterAlias(Editor.Project, rawName);
                            break;
                        case TextureViewCategory.Asset:
                            aliasName = AliasUtils.GetAssetAlias(Editor.Project, rawName);
                            break;
                        case TextureViewCategory.Part:
                            aliasName = AliasUtils.GetPartAlias(Editor.Project, rawName);
                            break;
                    }

                    //TaskLogs.AddLog(aliasName);

                    if (Filters.IsFileFilterMatch(info.Name, aliasName))
                    {
                        if (!CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries)
                        {
                            if (info.Name.Contains("_l"))
                            {
                                continue;
                            }
                        }

                        ImGui.BeginGroup();

                        var displayName = info.Name;

                        // File row
                        if (ImGui.Selectable($@" {displayName}", info.Name == Selection._selectedTextureContainerKey))
                        {
                            Selection.SelectTextureContainer(info);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Selection.SelectFile)
                        {
                            Selection.SelectFile = false;
                            Selection.SelectTextureContainer(info);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Selection.SelectFile = true;
                        }

                        if (ImGui.IsItemVisible())
                        {
                            var alias = AliasUtils.GetTextureContainerAliasName(Editor.Project, info);
                            UIHelper.DisplayAlias(alias);
                        }

                        ImGui.EndGroup();
                    }
                }
            }
        }
    }
}
