using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
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
    public TextFmgEntryGroupManager EntryGroupManager;

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
            if (ImGui.BeginCombo("Input Mode##textInputMode", CFG.Current.TextEditor_TextInputMode.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(TextInputMode)))
                {
                    var type = (TextInputMode)entry;

                    if (ImGui.Selectable(type.GetDisplayName()))
                    {
                        CFG.Current.TextEditor_TextInputMode = (TextInputMode)entry;
                    }
                }
                ImGui.EndCombo();
            }
            UIHelper.ShowWideHoverTooltip("Change the text input mode:" +
                "\nSimple: edit the ID and text for the selected row.\nGroup: see the grouped entries for the selected row, such as Title, Description, etc.\nProgrammatic: configure a regex conditional input, and a regex replacement string to apply to all selected rows.");

            ImGui.BeginChild("FmgEntryContents");

            if (Selection._selectedFmgEntry != null)
            {
                if (CFG.Current.TextEditor_TextInputMode == TextInputMode.Simple)
                {
                    DisplaySimpleEditor();
                }
                if (CFG.Current.TextEditor_TextInputMode == TextInputMode.Group)
                {
                    DisplayGroupEditor();
                }
                if (CFG.Current.TextEditor_TextInputMode == TextInputMode.Programmatic)
                {
                    DisplayProgrammaticEditor();
                }
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    /// <summary>
    /// Simple Editor: ID and Text edit inputs
    /// </summary>
    public void DisplaySimpleEditor()
    {
        DisplayEditor(Selection._selectedFmgEntry);
    }

    /// <summary>
    /// Group Editor: ID and Text edit inputs for all associated entries + the main one
    /// </summary>
    public void DisplayGroupEditor()
    {
        var fmgEntryGroup = EntryGroupManager.GetEntryGroup(Selection._selectedFmgEntry);

        // Fallback to Simple Editor if chosen entry doesn't support grouping
        if(!fmgEntryGroup.SupportsGrouping)
        {
            DisplaySimpleEditor();
            return;
        }

        if(fmgEntryGroup.Title != null)
        {
            DisplayEditor(fmgEntryGroup.Title);
        }
        if (fmgEntryGroup.Summary != null)
        {
            DisplayEditor(fmgEntryGroup.Summary);
        }
        if (fmgEntryGroup.Description != null)
        {
            DisplayEditor(fmgEntryGroup.Description);
        }
        if (fmgEntryGroup.Effect != null)
        {
            DisplayEditor(fmgEntryGroup.Effect);
        }
    }

    /// <summary>
    /// Programmatic Editor: search and replace for selected rows
    /// </summary>
    public void DisplayProgrammaticEditor()
    {
        // Find

        // Replace
    }

    public void DisplayEditor(FMG.Entry entry)
    {
        var textboxHeight = 100;
        var textboxWidth = ImGui.GetWindowWidth();

        var id = entry.ID;
        var contents = entry.Text;

        // Correct contents if the entry.Text is null
        if (contents == null)
            contents = "";

        var height = (textboxHeight + ImGui.CalcTextSize(contents).Y) * DPI.GetUIScale();
        var changed = false;
        var commit = false;

        if (ImGui.BeginTable("fmgEditTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(textboxWidth);
            if(ImGui.InputInt("##fmgEntryIdInput", ref id))
            {
                changed = true;
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Text");

            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline("##fmgTextInput", ref contents, 2000, new Vector2(-1, height)))
            {
                changed = true;
            }
            commit = ImGui.IsItemDeactivatedAfterEdit();

            ImGui.EndTable();
        }

        // Update the entry if it was changed and the text input was exited
        if(changed && commit)
        {

        }
    }
}

