using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        var id = Selection._selectedFmgEntry.ID;
        var text = Selection._selectedFmgEntry.Text;

        ImGui.Text($"{id}");
        ImGui.Text($"{text}");
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
            ImGui.Text($"{fmgEntryGroup.Title.ID}");
            ImGui.Text($"{fmgEntryGroup.Title.Text}");
        }
        if (fmgEntryGroup.Summary != null)
        {
            ImGui.Text($"{fmgEntryGroup.Summary.ID}");
            ImGui.Text($"{fmgEntryGroup.Summary.Text}");
        }
        if (fmgEntryGroup.Description != null)
        {
            ImGui.Text($"{fmgEntryGroup.Description.ID}");
            ImGui.Text($"{fmgEntryGroup.Description.Text}");
        }
        if (fmgEntryGroup.Effect != null)
        {
            ImGui.Text($"{fmgEntryGroup.Effect.ID}");
            ImGui.Text($"{fmgEntryGroup.Effect.Text}");
        }
    }

    /// <summary>
    /// Programmatic Editor: search and replace for selected rows
    /// </summary>
    public void DisplayProgrammaticEditor()
    {

    }
}

