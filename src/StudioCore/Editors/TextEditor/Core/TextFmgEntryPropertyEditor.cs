using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry editing
/// </summary>
public class TextFmgEntryPropertyEditor
{
    public TextEditorScreen Screen;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;
    public TextEntryGroupManager EntryGroupManager;

    public TextFmgEntryPropertyEditor(TextEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
        EntryGroupManager = screen.EntryGroupManager;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Contents##fmgEntryContents"))
        {
            ImGui.BeginChild("FmgEntryContents");

            if (Selection._selectedFmgEntry != null)
            {
                DisplayEditor();
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    /// <summary>
    /// Group Editor: ID and Text edit inputs for all associated entries + the main one
    /// </summary>
    public void DisplayEditor()
    {
        var fmgEntryGroup = EntryGroupManager.GetEntryGroup();

        // Display normally if entry has no groups, or it has been disabled
        if(!fmgEntryGroup.SupportsGrouping || !CFG.Current.TextEditor_Entry_DisplayGroupedEntries)
        {
            DisplayTextInput(0, Selection._selectedFmgEntry);
            return;
        }

        if(fmgEntryGroup.Title != null)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Title");
            ImGui.Separator();

            DisplayTextInput(1, fmgEntryGroup.Title);
        }
        if (fmgEntryGroup.Summary != null)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Summary");
            ImGui.Separator();

            DisplayTextInput(2, fmgEntryGroup.Summary);
        }
        if (fmgEntryGroup.Description != null)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Description");
            ImGui.Separator();

            DisplayTextInput(3, fmgEntryGroup.Description);
        }
        if (fmgEntryGroup.Effect != null)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Effect");
            ImGui.Separator();

            DisplayTextInput(4, fmgEntryGroup.Effect);
        }
    }

    public void DisplayTextInput(int index, FMG.Entry entry)
    {
        var textboxHeight = 100;
        var textboxWidth = ImGui.GetWindowWidth() * 0.9f;

        var id = entry.ID;
        var contents = entry.Text;

        // Correct contents if the entry.Text is null
        if (contents == null)
            contents = "";

        var height = (textboxHeight + ImGui.CalcTextSize(contents).Y) * DPI.GetUIScale();

        var idChanged = false;
        var idCommit = false;

        var textChanged = false;
        var textCommit = false;

        if (ImGui.BeginTable($"fmgEditTable{index}", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(textboxWidth);
            if(ImGui.InputInt($"##fmgEntryIdInput{index}", ref id))
            {
                idChanged = true;
            }

            idCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (ImGui.IsItemActivated())
            {
                Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Text");

            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline($"##fmgTextInput{index}", ref contents, 2000, new Vector2(-1, height)))
            {
                textChanged = true;
            }
            textCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (ImGui.IsItemActivated())
            {
                Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
            }

            ImGui.EndTable();
        }

        // Update the ID if it was changed and the id input was exited
        if(idChanged && idCommit)
        {
            var action = new ChangeFmgEntryID(entry, id);
            Screen.EditorActionManager.ExecuteAction(action);
        }

        // Update the Text if it was changed and the text input was exited
        if (textChanged && textCommit)
        {
            var action = new ChangeFmgEntryText(entry, contents);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }
}

