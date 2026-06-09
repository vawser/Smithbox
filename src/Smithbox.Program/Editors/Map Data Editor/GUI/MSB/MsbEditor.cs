using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.MapDataEditor;

public class MsbEditor
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    public MsbCategoryView CategoryView;
    public MsbEntryView EntryView;
    public MsbPropertyView PropertyView;

    public MsbEditor(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CategoryView = new(view, project);
        EntryView = new(view, project);
        PropertyView = new(view, project);
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Maps", "");

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_msbEditor_FileList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("msbEditor_FileList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.SameLine();

        if(ImGui.Button($"{Icons.Database}"))
        {
            CFG.Current.MapDataEditor_CacheLoadedMaps = !CFG.Current.MapDataEditor_CacheLoadedMaps;
        }

        var curCacheMode = "Loaded maps are loaded once and then retrieved from a cache.";
        if (!CFG.Current.MapDataEditor_CacheLoadedMaps)
            curCacheMode = "Loaded maps are re-loaded each time.";

        UIHelper.Tooltip($"Determines whether to re-load a map when a entry is selected, or to cache a load and use that on subsequent selections.\n\nCache Mode:\n{curCacheMode}");

        ImGui.EndChild();
    }

    public void DisplaySourceList()
    {
        var primaryBank = Project.Handler.MapDataHandler.PrimaryBank_MSB;

        foreach (var entry in primaryBank.Maps)
        {
            var mapKey = entry.Key.Filename;
            var alias = AliasHelper.GetMapNameAlias(Project, mapKey);

            var isMatch = EditorFilters.IsMatch(FileListFilter, mapKey, ExactFileListFilter, alias);

            if (!isMatch)
                continue;

            var isSelected = entry.Key == View.Selection.SelectedMapDescriptor;

            if(ImGui.Selectable($"{mapKey}", isSelected))
            {
                View.Selection.SelectedMapDescriptor = entry.Key;

                var loadTask = primaryBank.LoadMap(entry.Key);
                if(loadTask)
                {
                    View.Selection.SelectedMap = primaryBank.Maps[entry.Key];
                    View.Selection.ResetMsbSelection();
                    EntryView.RebuildMapEntryCountCache();
                    Smithbox.Log<MsbEditor>($"Loaded map: {entry.Key.Filename}");
                }
            }

            if(alias != "")
            {
                UIHelper.DisplayAlias(alias);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && View.Selection.SelectMapEntry)
            {
                View.Selection.SelectMapEntry = false;
                View.Selection.SelectedMapDescriptor = entry.Key;

                var loadTask = primaryBank.LoadMap(entry.Key);
                if (loadTask)
                {
                    View.Selection.SelectedMap = primaryBank.Maps[entry.Key];
                    View.Selection.ResetMsbSelection();
                    EntryView.RebuildMapEntryCountCache();
                    Smithbox.Log<MsbEditor>($"Loaded map: {entry.Key.Filename}");
                }
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    View.Selection.SelectMapEntry = true;
                }
            }
        }
    }

    public void Draw()
    {
        UIHelper.SimpleHeader("Current Map", "");

        var columnCount = 3;

        if (ImGui.BeginTable($"mapDataEditorTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("CategoriesCol", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("EntryCol", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("PropertiesCol", ImGuiTableColumnFlags.WidthStretch, 0.4f);

            // Categories
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            CategoryView.Display();

            // Entries
            ImGui.TableSetColumnIndex(1);

            EntryView.Display();

            // Properties
            ImGui.TableSetColumnIndex(2);

            PropertyView.Display();

            ImGui.EndTable();
        }
    }

    public void Shortcuts()
    {
        if (FocusManager.IsFocus(EditorFocusContext.MapDataEditor_MsbEditor))
        {
            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                var action = new MsbEntryDuplicate(View, Project);
                View.ActionManager.ExecuteAction(action);
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                var action = new MsbEntryDelete(View, Project);
                View.ActionManager.ExecuteAction(action);
            }

            // Focus Selected Entry
            if (InputManager.IsPressed(KeybindID.Jump))
            {
                // TODO
            }
        }
    }
}
