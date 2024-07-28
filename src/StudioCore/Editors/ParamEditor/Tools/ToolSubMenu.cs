using ImGuiNET;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Editors.TextEditor.Tools;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public class ToolSubMenu
{
    private ParamEditorScreen Screen;
    public ActionHandler Handler;
    public MassEditHandler MassEditHandler;

    public ToolSubMenu(ParamEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
        MassEditHandler = new MassEditHandler(screen);
    }

    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }
    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");

            if (ImGui.BeginMenu("Import Row Names"))
            {
                if (ImGui.BeginMenu("Smithbox"))
                {
                    if (ImGui.MenuItem("Selected Rows"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific rows currently selected.");

                    if (ImGui.MenuItem("Selected Param"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific param currently selected.");

                    if (ImGui.MenuItem("All Params"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                ImguiUtils.ShowHoverTooltip("Draw names from the in-built Smithbox name lists.");

                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Selected Rows"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific rows currently selected.");

                    if (ImGui.MenuItem("Selected Param"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific param currently selected.");

                    if (ImGui.MenuItem("All Params"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                ImguiUtils.ShowHoverTooltip("Draw names from your Project-specific name lists.");

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Export Row Names"))
            {
                if (ImGui.MenuItem("Export Selected Rows"))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedRows;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                ImguiUtils.ShowHoverTooltip("Export the row names for the currently selected rows.");

                if (ImGui.MenuItem("Export Selected Param"))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedParam;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                ImguiUtils.ShowHoverTooltip("Export the row names for the currently selected param.");

                if (ImGui.MenuItem("Export All"))
                {
                    Handler.CurrentTargetCategory = TargetType.AllParams;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                ImguiUtils.ShowHoverTooltip("Export all of the row names for all params.");

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Trim Row Names"))
            {
                if (Screen._activeView._selection.ActiveParamExists())
                {
                    Handler.RowNameTrimHandler();
                }
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Sort Rows"))
            {
                if (Screen._activeView._selection.ActiveParamExists())
                {
                    Handler.SortRowsHandler();
                }
            }
            
            /*
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Check Params for Edits", null, false, !ParamBank.PrimaryBank.IsLoadingParams && !ParamBank.VanillaBank.IsLoadingParams))
            {
                ParamBank.RefreshAllParamDiffCaches(true);
            }
            */

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Editor Mode"))
            {
                if (ImGui.MenuItem("Toggle"))
                {
                    ParamEditorScreen.EditorMode = !ParamEditorScreen.EditorMode;
                }
                ImguiUtils.ShowHoverTooltip("Toggle Editor Mode, allowing you to edit the Param Meta within Smithbox.");
                ImguiUtils.ShowActiveStatus(ParamEditorScreen.EditorMode);

                if (ImGui.MenuItem("Save Changes"))
                {
                    ParamMetaData.SaveAll();
                    ParamEditorScreen.EditorMode = false;
                }
                ImguiUtils.ShowHoverTooltip("Save current Param Meta changes.");

                if (ImGui.MenuItem("Discard Changes"))
                {

                    ParamEditorScreen.EditorMode = false;
                }
                ImguiUtils.ShowHoverTooltip("Discard current Param Meta changes.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}
