using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MapEditor;

// WIP: image preview asset browser
public class AssetBrowserTool
{
    private MapEditorView View;
    public ProjectEntry Project;

    private string AssetFiler = "";
    private bool ExactAssetFiler = false;

    private bool DisplayAlias = false;

    public AssetBrowserTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        DisplayHeader();
        DisplayBrowser();
    }

    private void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_assetBrowser", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("assetBrowser_Search", ref AssetFiler, ref ExactAssetFiler);

        ImGui.SameLine();

        if(ImGui.Button($"{Icons.Bars}##aliasToggle"))
        {
            DisplayAlias = !DisplayAlias;
        }
        UIHelper.Tooltip("Toggle whether the asset alias is displayed.");

        ImGui.EndChild();
    }

    private int ItemWidth = 128;
    private Dictionary<string, string> CachedAliases = new();

    private void DisplayBrowser()
    {
        var width = ImGui.GetWindowWidth();
        var assets = Project.Locator.AssetFiles.Entries;
        var itemsPerRow = (int)(width / ItemWidth);
        var curItemCount = 0;

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        ImGui.BeginChild($"assetGridSection", ImGuiChildFlags.Borders);

        if (ImGui.BeginTable($"assetGridTable", itemsPerRow, tblFlags))
        {
            for(int i = 0; i < itemsPerRow; i++)
            {
                ImGui.TableSetupColumn($"col_{i}", ImGuiTableColumnFlags.WidthFixed, ItemWidth);
            }

            ImGui.TableNextRow();

            foreach (var entry in assets)
            {
                if (curItemCount == itemsPerRow)
                {
                    ImGui.TableNextRow();
                    curItemCount = 0;
                }

                if (!IsMatchingFilter(entry.Filename))
                    continue;

                ImGui.TableSetColumnIndex(curItemCount);

                ImGui.Dummy(new Vector2(ItemWidth, ItemWidth));

                ImGui.AlignTextToFramePadding();

                var displayName = entry.Filename;
                var aliasName = "";

                if(CachedAliases.ContainsKey(displayName))
                {
                    aliasName = CachedAliases[displayName];
                }
                else
                {
                    aliasName = AliasHelper.GetAssetAlias(Project, entry.Filename);
                    CachedAliases.Add(displayName, aliasName);
                }

                if(DisplayAlias)
                {
                    BrowserButton(displayName, aliasName, $"{displayName}\n{aliasName}");
                }
                else
                {
                    BrowserButton(displayName, displayName, $"{displayName}\n{aliasName}");
                }

                curItemCount++;
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void BrowserButton(string id, string title, string tooltip)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{title.GetHashCode()}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if(ImGui.Button($"{Icons.Plus}##add{title}"))
            {

            }

            ImGui.SameLine();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{title}");

            if (tooltip != "")
            {
                UIHelper.Tooltip(tooltip);
            }

            ImGui.EndTable();
        }
    }

    private bool IsMatchingFilter(string filename)
    {
        if (ExactAssetFiler)
        {
            return filename.Equals(AssetFiler, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            return filename.Contains(AssetFiler, StringComparison.OrdinalIgnoreCase);
        }
    }
}