using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

public class ListenerTab
{
    public string SelectedListenerEntry = "";

    public bool DisplayEmptyListeners = false;

    private ResourceListTool ListWindow;

    public ListenerTab(ResourceListTool listWindow)
    {
        ListWindow = listWindow;
    }

    public void Display()
    {
        var resDatabase = ResourceManager.GetResourceDatabase();

        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        var imguiId = 0;

        ImGui.BeginChild("listenerTableSection");

        if (ImGui.BeginTable($"listenerTable", 6, tableFlags))
        {
            ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Load State", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Access Level", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Reference Count", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Unload", ImGuiTableColumnFlags.WidthStretch);

            // Header
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.TableSetColumnIndex(1);

            ImGui.Text(LOC.Get("REND_Listener_Tab_Name_Column"));
            UIHelper.Tooltip(LOC.Get("REND_Listener_Tab_Name_Column_TT"));

            ImGui.TableSetColumnIndex(2);

            ImGui.Text(LOC.Get("REND_Listener_Tab_Load_State_Column"));
            UIHelper.Tooltip(LOC.Get("REND_Listener_Tab_Load_State_Column_TT"));

            ImGui.TableSetColumnIndex(3);

            // Access Level
            ImGui.Text(LOC.Get("REND_Listener_Tab_Access_Level_Column"));
            UIHelper.Tooltip(LOC.Get("REND_Listener_Tab_Access_Level_Column_TT"));

            ImGui.TableSetColumnIndex(4);

            // Reference Count
            ImGui.Text(LOC.Get("REND_Listener_Tab_Reference_Count_Column"));
            UIHelper.Tooltip(LOC.Get("REND_Listener_Tab_Reference_Count_Column_TT"));

            ImGui.TableSetColumnIndex(5);

            // Unload

            // Contents
            foreach (var item in resDatabase)
            {
                var resName = item.Key;
                var resHandle = item.Value;

                var isMatch = EditorFilters.IsMatch(ListWindow.ResourceListFilter, resName, ListWindow.ExactResourceListFilter);

                if (!isMatch)
                    continue;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if (ImGui.Button($"{Icons.Bars}##{imguiId}_select", DPI.IconButtonSize))
                {
                    SelectedListenerEntry = resName;
                }
                UIHelper.Tooltip(LOC.Get("REND_Listener_Tab_Action_Select_TT"));

                ImGui.TableSetColumnIndex(1);

                // Name
                ImGui.AlignTextToFramePadding();
                if (SelectedListenerEntry == resName)
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{resName}");
                }
                else
                {
                    ImGui.Text(resName);
                }

                ImGui.TableSetColumnIndex(2);

                // Load State
                if (resHandle.IsLoaded())
                {
                    ImGui.Text(LOC.Get("REND_Listener_Tab_Loaded_State"));
                }
                else
                {
                    ImGui.Text(LOC.Get("REND_Listener_Tab_Unloaded_State"));
                }

                ImGui.TableSetColumnIndex(3);

                // Access Level
                ImGui.Text($"{resHandle.AccessLevel}");
                ImGui.TableSetColumnIndex(4);

                // Reference Count
                ImGui.Text($"{resHandle.GetReferenceCounts()}");
                ImGui.TableSetColumnIndex(5);

                // Unload
                if (ImGui.Button($"{Icons.Times}##{imguiId}_unload", DPI.IconButtonSize))
                {
                    resHandle.Release(true);
                }
                UIHelper.Tooltip(LOC.Get("REND_Listener_Tab_Action_Unload_TT"));

                imguiId++;
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
}
