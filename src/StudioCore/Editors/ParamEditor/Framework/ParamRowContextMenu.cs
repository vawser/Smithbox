using Andre.Formats;
using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Framework;

public static class ParamRowContextMenu
{
    /// <summary>
    /// Display the context menu for a Row entry in the Param Editor
    /// </summary>
    public static void Display(Param.Row r, int selectionCacheIndex, 
        bool isPinned, string activeParam,
        IParamDecorator decorator, ParamEditorSelectionState _selection, ParamEditorScreen _paramEditor)
    {
        // Context Menu
        if (ImGui.BeginPopupContextItem($"{r.ID}_{selectionCacheIndex}"))
        {
            var width = CFG.Current.Param_RowContextMenu_Width;

            ImGui.SetNextItemWidth(width);

            // Name Input
            if (CFG.Current.Param_RowContextMenu_NameInput)
            {
                if (_selection.RowSelectionExists())
                {
                    var name = _selection.GetActiveRow().Name;
                    if (name != null)
                    {
                        ImGui.InputText("##nameMassEdit", ref name, 255);

                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            var editor = Smithbox.EditorHandler.ParamEditor;
                            var editCommand = $"selection: Name := {name}";
                            editor._activeView._selection.SortSelection();

                            (MassEditResult res, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                                editCommand, Smithbox.EditorHandler.ParamEditor._activeView._selection);

                            if (child != null)
                            {
                                editor.EditorActionManager.PushSubManager(child);
                            }

                            if (res.Type == MassEditResultType.SUCCESS)
                            {
                                ParamBank.RefreshParamDifferenceCacheTask();
                            }
                        }
                    }
                }
            }

            // General Options
            if (CFG.Current.Param_RowContextMenu_ShortcutTools)
            {
                // Copy
                if (ImGui.Selectable(@$"Copy", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.CopySelectionToClipboard(_selection);
                }
                UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.PARAM_CopyToClipboard.HintText}\n\n" +
                    "Copy the current row selection to the clipboard.");

                // Paste
                if (ImGui.Selectable(@$"Paste", false,
                        ParamBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }
                UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.PARAM_PasteClipboard.HintText}\n\n" +
                    "Paste the current row clipboard into the current param.");

                // Delete
                if (ImGui.Selectable(@$"Delete", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.DeleteSelection(_selection);
                }
                UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}\n\n" +
                    "Delete the current row selection from the param.");

                // Duplicate
                if (ImGui.Selectable(@$"Duplicate", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.Handler.DuplicateHandler();
                }
                UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}\n\n" +
                    "Duplicate the current row selection, automatically incrementing the row ID.");

                // Duplicate To
                if (ImGui.BeginMenu("Duplicate To", Smithbox.EditorHandler.ParamEditor.Handler.IsCommutativeParam()))
                {
                    Smithbox.EditorHandler.ParamEditor.Handler.DisplayCommutativeDuplicateMenu();

                    ImGui.EndMenu();
                }
                UIHelper.ShowHoverTooltip($"Duplicate the current row selection into the chosen target param.");

                // Copy ID
                if (ImGui.Selectable(@$"Copy ID", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.Handler.CopyRowDetailHandler(false);
                }
                UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.PARAM_CopyId.HintText}\n\n" +
                    "Copy the current row selection ID to the clipboard (multiple rows will produce a list of IDs).");

                // Copy ID and Name
                if (ImGui.Selectable(@$"Copy ID and Name", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.Handler.CopyRowDetailHandler(true);
                }
                UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.PARAM_CopyIdAndName.HintText}\n\n" +
                    "Copy the current row selection ID and Name to the clipboard (multiple rows will produce a list of IDs and Names).");

                // Revert to Default
                if (ImGui.Selectable(@$"Revert to Default", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.Handler.RevertRowToDefault();
                }
                UIHelper.ShowHoverTooltip($"Revert the current row selection field values to the vanilla field values.");

                ImGui.Separator();
            }

            // Pinning Options
            if (CFG.Current.Param_RowContextMenu_PinOptions)
            {
                // Pin
                if (!isPinned)
                {
                    if (ImGui.Selectable($"Pin"))
                    {
                        if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.ContainsKey(activeParam))
                        {
                            Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Add(activeParam, new List<int>());
                        }

                        List<int> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam];

                        foreach (var entry in _selection.GetSelectedRows())
                        {
                            if (!pinned.Contains(entry.ID))
                            {
                                pinned.Add(entry.ID);
                            }
                        }
                    }
                    UIHelper.ShowHoverTooltip($"Pin the current row selection to the top of the row list.");
                }
                // Unpin
                else if(isPinned)
                {
                    if (ImGui.Selectable($"Unpin"))
                    {
                        if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.ContainsKey(activeParam))
                        {
                            Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Add(activeParam, new List<int>());
                        }

                        List<int> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam];

                        foreach (var entry in _selection.GetSelectedRows())
                        {
                            if (pinned.Contains(entry.ID))
                            {
                                pinned.Remove(entry.ID);
                            }
                        }
                    }
                    UIHelper.ShowHoverTooltip($"Unpin the current row selection from top of the row list.");
                }

                ImGui.Separator();
            }

            // Decorator Options (e.g. Go to Text)
            if (decorator != null)
            {
                decorator.DecorateContextMenuItems(r);
            }


            // Comparison Options
            if (CFG.Current.Param_RowContextMenu_CompareOptions)
            {
                if (ImGui.Selectable("Compare"))
                {
                    _selection.SetCompareRow(r);
                }
                UIHelper.ShowHoverTooltip($"Set this row as the row comparison target within the field window.");
            }

            // Reverse Lookup Options
            if (CFG.Current.Param_RowContextMenu_ReverseLoopup)
            {
                EditorDecorations.ParamRefReverseLookupSelectables(_paramEditor, ParamBank.PrimaryBank, activeParam, r.ID);
            }

            ImGui.EndPopup();
        }
    }
}
