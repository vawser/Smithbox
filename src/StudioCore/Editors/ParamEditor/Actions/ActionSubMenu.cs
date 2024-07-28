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
        ImguiUtils.ShowMenuIcon($"{ForkAwesome.FilesO}");
        if (ImGui.MenuItem("Copy", KeyBindings.Current.Param_Copy.HintText, false, Screen._activeView._selection.RowSelectionExists()))
        {
            Screen.CopySelectionToClipboard();
        }

        ImguiUtils.ShowMenuIcon($"{ForkAwesome.Clipboard}");
        if (ImGui.MenuItem("Paste", KeyBindings.Current.Param_Paste.HintText, false, ParamBank.ClipboardRows.Any()))
        {
            EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
        }

        ImguiUtils.ShowMenuIcon($"{ForkAwesome.Scissors}");
        if (ImGui.MenuItem("Remove", KeyBindings.Current.Core_Delete.HintText, false, Screen._activeView._selection.RowSelectionExists()))
        {
            Screen.DeleteSelection();
        }

        ImguiUtils.ShowMenuIcon($"{ForkAwesome.FilesO}");
        if (ImGui.MenuItem("Duplicate", KeyBindings.Current.Core_Duplicate.HintText, false, Screen._activeView._selection.RowSelectionExists()))
        {
            Handler.DuplicateHandler();
        }

        ImguiUtils.ShowMenuIcon($"{ForkAwesome.ArrowRight}");
        if (ImGui.MenuItem("Goto selected row", KeyBindings.Current.Param_GotoSelectedRow.HintText, false, Screen._activeView._selection.RowSelectionExists()))
        {
            Screen.GotoSelectedRow = true;
        }

        ImGui.EndMenu();
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate Row", KeyBindings.Current.Core_Duplicate.HintText))
            {
                Handler.DuplicateHandler();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Remove Row", KeyBindings.Current.Core_Delete.HintText))
            {
                Screen.DeleteSelection();
            }
            ImguiUtils.ShowHoverTooltip("Deletes current selection.");

            ImGui.EndMenu();
        }
    }
}
