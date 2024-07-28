using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TextEditor.Actions;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Tools;

public class ToolWindow
{
    private TextEditorScreen Screen;
    public ActionHandler Handler;

    public ToolWindow(TextEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            // Generate Entries
            if (ImGui.CollapsingHeader("Generate Entries"))
            {
                ImguiUtils.WrappedText("Generate entries based on the selected entry (or first member of a multi-selection), with a template applied.");
                ImguiUtils.WrappedText("");

                FmgEntryGenerator.SetupTemplates();
                FmgEntryGenerator.DisplayConfiguration();
            }
            // Duplicate Entries
            if (ImGui.CollapsingHeader("Duplicate Entries"))
            {
                ImguiUtils.WrappedText("Duplicate the selected entries in the Text Entries list.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Amount:");
                ImGui.SetNextItemWidth(defaultButtonSize.X);
                ImGui.InputInt("##dupeamount", ref CFG.Current.FMG_DuplicateAmount);
                ImguiUtils.ShowHoverTooltip("The number of times to duplicate this entry.");
                ImguiUtils.WrappedText("");

                if (CFG.Current.FMG_DuplicateAmount < 1)
                    CFG.Current.FMG_DuplicateAmount = 1;

                ImguiUtils.WrappedText("Increment:");
                ImGui.SetNextItemWidth(defaultButtonSize.X);
                ImGui.InputInt("##dupeIncrement", ref CFG.Current.FMG_DuplicateIncrement);
                ImguiUtils.ShowHoverTooltip("The increment to apply to the text id when duplicating.");
                ImguiUtils.WrappedText("");

                if (CFG.Current.FMG_DuplicateIncrement < 1)
                    CFG.Current.FMG_DuplicateIncrement = 1;

                if (ImGui.Button("Apply##action_Duplicate", defaultButtonSize))
                {
                    Handler.DuplicateHandler();
                }
            }
            // Delete Entries
            if (ImGui.CollapsingHeader("Delete Entries"))
            {
                ImguiUtils.WrappedText("Delete the selected entries in the Text Entries list.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Apply##action_Delete", defaultButtonSize))
                {
                    Handler.DeleteHandler();
                }
            }
            // Sync Entries
            if (ImGui.CollapsingHeader("Sync Description"))
            {
                ImguiUtils.WrappedText("Sync the descriptions of the selected entries in the Text Entries list with the first member's description.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Apply##action_Sync", defaultButtonSize))
                {
                    Handler.SyncDescriptionHandler();
                }
            }
            // Search and Replace
            if (ImGui.CollapsingHeader("Search and Replace"))
            {
                SearchAndReplace.DisplayConfiguration(defaultButtonSize);
            }
            // Upgrade FMG Files
            if (ImGui.CollapsingHeader("Upgrade Text Files"))
            {
                ImguiUtils.WrappedText("Ports all unique entries from item.msgbnd.dcx and menu.msgbnd.dcx to the DLC version: item_dlc02.msgbnd.dcx and menu_dlc02.msgbnd.dcx");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Apply##action_UpdateFmgFiles", defaultButtonSize))
                {
                    FmgUpdater.UpdateFMGs();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }


}
