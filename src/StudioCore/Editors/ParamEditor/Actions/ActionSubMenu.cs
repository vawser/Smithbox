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
            if (ImGui.Button("Duplicate Row", UI.MenuButtonSize))
            {
                Handler.DuplicateHandler();
            }
            UIHelper.ShowHoverTooltip($"Duplicates current selection.\n{KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            if (ImGui.Button("Remove Row", UI.MenuButtonSize))
            {
                Screen.DeleteSelection();
            }
            UIHelper.ShowHoverTooltip($"Deletes current selection.\n{KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");

            if (ImGui.Button("Copy", UI.MenuButtonSize))
            {
                if (Screen._activeView._selection.RowSelectionExists())
                {
                    Screen.CopySelectionToClipboard();
                }
            }
            UIHelper.ShowHoverTooltip($"Copy current selection to clipboard.\n{KeyBindings.Current.PARAM_CopyToClipboard.HintText}");

            if (ImGui.Button("Paste", UI.MenuButtonSize))
            {
                if (ParamBank.ClipboardRows.Any())
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }
            }
            UIHelper.ShowHoverTooltip($"Paste current selection into current param.\n{KeyBindings.Current.PARAM_PasteClipboard.HintText}");

            if (ImGui.Button("Go to selected row", UI.MenuButtonSize))
            {
                if (Screen._activeView._selection.RowSelectionExists())
                {
                    Screen.GotoSelectedRow = true;
                }
            }
            UIHelper.ShowHoverTooltip($"Go to currently selected row.\n{KeyBindings.Current.PARAM_GoToSelectedRow.HintText}");

            ImGui.EndMenu();
        }
    }
}
