using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Renderer;

public class MeshProviderTab
{
    public string SelectedProviderEntry = "";

    private ResourceListTool ListWindow;

    public MeshProviderTab(ResourceListTool listWindow)
    {
        ListWindow = listWindow;
    }

    public void Display()
    {
        var resDatabase = ResourceManager.GetResourceDatabase();

        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        var imguiId = 0;

        ImGui.BeginChild("providerTableSection");

        if (ImGui.BeginTable($"providerTable", 2, tableFlags))
        {
            ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

            // Header
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.TableSetColumnIndex(1);

            ImGui.Text(LOC.Get("REND_Mesh_Provider_Tab_Name_Column"));
            GUI.Tooltip(LOC.Get("REND_Mesh_Provider_Tab_Name_Column_TT"));

            // Contents
            foreach (var item in MeshProviderInspector.Providers.OrderBy(e => e.Value.VirtualPath))
            {
                var hash = item.Key;
                var context = item.Value;

                var isMatch = EditorFilters.IsMatch(ListWindow.ResourceListFilter, context.VirtualPath, ListWindow.ExactResourceListFilter);

                if (!isMatch)
                    continue;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if (ImGui.Button($"{Icons.Bars}##{imguiId}_select", DPI.IconButtonSize))
                {
                    SelectedProviderEntry = context.VirtualPath;
                }
                GUI.Tooltip(LOC.Get("REND_Mesh_Provider_Action_Select_TT"));

                ImGui.TableSetColumnIndex(1);

                // Name
                ImGui.AlignTextToFramePadding();
                if (SelectedProviderEntry == context.VirtualPath)
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{context.VirtualPath}");
                }
                else
                {
                    ImGui.Text(context.VirtualPath);
                }

                imguiId++;
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
}
