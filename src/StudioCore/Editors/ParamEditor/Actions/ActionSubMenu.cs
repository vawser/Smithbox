using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Actions;

public class ActionSubMenu
{
    private ParamEditorScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(ParamEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate Row", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Handler.DuplicateHandler();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Remove Row", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Screen.DeleteSelection();
            }
            ImguiUtils.ShowHoverTooltip("Deletes current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.FilesO}");
            if (ImGui.MenuItem("Copy", KeyBindings.Current.PARAM_CopyToClipboard.HintText, false, Screen._activeView._selection.RowSelectionExists()))
            {
                Screen.CopySelectionToClipboard();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Clipboard}");
            if (ImGui.MenuItem("Paste", KeyBindings.Current.PARAM_PasteClipboard.HintText, false, ParamBank.ClipboardRows.Any()))
            {
                EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.ArrowRight}");
            if (ImGui.MenuItem("Go to selected row", KeyBindings.Current.PARAM_GoToSelectedRow.HintText, false, Screen._activeView._selection.RowSelectionExists()))
            {
                Screen.GotoSelectedRow = true;
            }

            ImGui.EndMenu();
        }
    }
}
