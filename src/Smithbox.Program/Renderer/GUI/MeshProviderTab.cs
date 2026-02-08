using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

public class MeshProviderTab
{
    public string SelectedProviderEntry = "";

    private ResourceListWindow ListWindow;

    public MeshProviderTab(ResourceListWindow listWindow)
    {
        ListWindow = listWindow;
    }

    public void Display()
    {
        var resDatabase = ResourceManager.GetResourceDatabase();

        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

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

            ImGui.Text("Name");
            UIHelper.Tooltip("Name of this resource.");

            // Contents
            foreach (var item in MeshProviderInspector.Providers.OrderBy(e => e.Value.VirtualPath))
            {
                var hash = item.Key;
                var context = item.Value;

                if (ListWindow.SearchFilter != "" && !context.VirtualPath.Contains(ListWindow.SearchFilter))
                {
                    continue;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if (ImGui.Button($"{Icons.Bars}##{imguiId}_select", DPI.IconButtonSize))
                {
                    SelectedProviderEntry = context.VirtualPath;
                }
                UIHelper.Tooltip("Select this resource.");

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
