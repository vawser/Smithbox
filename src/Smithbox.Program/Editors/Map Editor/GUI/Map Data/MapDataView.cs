using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The table presentation of the map selected in the Map List: its MSB categories and entries.
/// Entries select the map's entities, so their properties are edited in the Properties window.
/// </summary>
public class MapDataView
{
    public MapEditorView View;
    public ProjectEntry Project;

    public MapDataSelection Selection;

    public MsbCategoryView CategoryView;
    public MsbEntryView EntryView;

    public MapDataView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Selection = new(view);

        CategoryView = new(view, project);
        EntryView = new(view, project);
    }

    public void Display()
    {
        var map = Selection.SelectedMap;

        GUI.TitleHeader(map != null ? $"Current Map: {map.Name}" : "Current Map", "");

        if (map == null)
        {
            ImGui.TextDisabled("No loaded map has been selected in the Map List.");
            return;
        }

        var columnCount = 2;

        if (ImGui.BeginTable($"mapDataTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("CategoriesCol", ImGuiTableColumnFlags.WidthStretch, 0.3f);
            ImGui.TableSetupColumn("EntryCol", ImGuiTableColumnFlags.WidthStretch, 0.7f);

            // Categories
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            CategoryView.Display();

            // Entries
            ImGui.TableSetColumnIndex(1);

            EntryView.Display();

            ImGui.EndTable();
        }
    }
}
