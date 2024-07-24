using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.ModelEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class ActionSubMenu
{
    private TextEditorScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(TextEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate))
        {
            Handler.DuplicateHandler();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete))
        {
            Handler.DeleteHandler();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.TextFMG_Sync))
        {
            Handler.SyncDescriptionHandler();
        }
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("动作 Actions"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("复刻 Duplicate Entries", KeyBindings.Current.Core_Duplicate.HintText))
            {
                Handler.DuplicateHandler();
            }
            ImguiUtils.ShowHoverTooltip("复刻选中值 Duplicates current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("删除 Delete Entries", KeyBindings.Current.Core_Delete.HintText))
            {
                Handler.DeleteHandler();
            }
            ImguiUtils.ShowHoverTooltip("删除选中值 Deletes current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("同步描述条目 Sync Description Entries", KeyBindings.Current.TextFMG_Sync.HintText))
            {
                Handler.SyncDescriptionHandler();
            }
            ImguiUtils.ShowHoverTooltip("同步所有选中条目的描述到选中项的第一个成员的描述 Sync the description of all selected entries to the description of the first member of the selection.");


            ImGui.EndMenu();
        }
    }
}
