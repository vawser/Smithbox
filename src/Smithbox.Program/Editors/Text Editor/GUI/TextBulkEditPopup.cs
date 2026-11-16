using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Edit Selection: change the text of every selected entry at once (set, prefix, suffix,
/// or find and replace). Applied with the Apply button only, as the dialog has several inputs.
/// </summary>
public class TextBulkEditPopup
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    private enum BulkEditMode
    {
        SetText,
        AddPrefix,
        AddSuffix,
        FindReplace
    }

    private BulkEditMode Mode = BulkEditMode.FindReplace;

    private string ValueText = "";
    private string ReplaceText = "";
    private bool IgnoreCase = false;

    /// <summary>
    /// Set by the menus; the popup is opened from Display(), inside the view window,
    /// as popup IDs depend on the window (see TextDuplicatePopup).
    /// </summary>
    public bool OpenRequested = false;

    private const string PopupId = "textBulkEditPopup";

    public TextBulkEditPopup(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        if (OpenRequested)
        {
            OpenRequested = false;
            ImGui.OpenPopup(PopupId);
        }

        if (!ImGui.BeginPopup(PopupId))
            return;

        var entries = Parent.ActionHandler.GetSelectedListedEntries();

        ImGui.Text(LOC.Get("TEXT_BulkEdit_Header", entries.Count));
        ImGui.Separator();

        // Mode
        DPI.ApplyInputWidth(300f);
        if (ImGui.BeginCombo("##bulkEditMode", LOC.Get(ModeKey(Mode))))
        {
            foreach (BulkEditMode mode in Enum.GetValues(typeof(BulkEditMode)))
            {
                if (ImGui.Selectable(LOC.Get(ModeKey(mode)), mode == Mode))
                {
                    Mode = mode;
                }
            }

            ImGui.EndCombo();
        }

        // Inputs
        switch (Mode)
        {
            case BulkEditMode.SetText:
                ImGui.Text(LOC.Get("TEXT_BulkEdit_Label_Text"));
                ImGui.InputTextMultiline("##bulkEditValue", ref ValueText, TextContents.GetEditBufferSize(ValueText),
                    new Vector2(400f * DPI.UIScale(), 100f * DPI.UIScale()));
                break;

            case BulkEditMode.AddPrefix:
            case BulkEditMode.AddSuffix:
                ImGui.Text(LOC.Get(Mode == BulkEditMode.AddPrefix ? "TEXT_BulkEdit_Label_Prefix" : "TEXT_BulkEdit_Label_Suffix"));
                DPI.ApplyInputWidth(400f);
                ImGui.InputText("##bulkEditValue", ref ValueText, TextContents.GetEditBufferSize(ValueText));
                break;

            case BulkEditMode.FindReplace:
                ImGui.Text(LOC.Get("TEXT_BulkEdit_Label_Find"));
                DPI.ApplyInputWidth(400f);
                ImGui.InputText("##bulkEditValue", ref ValueText, TextContents.GetEditBufferSize(ValueText));

                ImGui.Text(LOC.Get("TEXT_BulkEdit_Label_Replace"));
                DPI.ApplyInputWidth(400f);
                ImGui.InputText("##bulkEditReplace", ref ReplaceText, TextContents.GetEditBufferSize(ReplaceText));

                ImGui.Checkbox($"{LOC.Get("TEXT_BulkEdit_Ignore_Case")}##bulkEditIgnoreCase", ref IgnoreCase);
                break;
        }

        ImGui.Separator();

        if (entries.Count == 0)
            ImGui.BeginDisabled();

        if (ImGui.Button($"{LOC.Get("TEXT_BulkEdit_Apply")}##bulkEditApply", DPI.StandardButtonSize))
        {
            Apply(entries);
            ImGui.CloseCurrentPopup();
        }

        if (entries.Count == 0)
            ImGui.EndDisabled();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("TEXT_BulkEdit_Cancel")}##bulkEditCancel", DPI.StandardButtonSize))
        {
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private static string ModeKey(BulkEditMode mode) => mode switch
    {
        BulkEditMode.SetText => "TEXT_BulkEdit_Mode_Set",
        BulkEditMode.AddPrefix => "TEXT_BulkEdit_Mode_Prefix",
        BulkEditMode.AddSuffix => "TEXT_BulkEdit_Mode_Suffix",
        _ => "TEXT_BulkEdit_Mode_Replace"
    };

    private void Apply(List<SoulsFormats.FMG.Entry> entries)
    {
        // Find & Replace with nothing to find would change nothing
        if (Mode == BulkEditMode.FindReplace && string.IsNullOrEmpty(ValueText))
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_BulkEdit_Find_Empty"));
            return;
        }

        var container = Parent.Selection.SelectedContainerWrapper;
        var actions = new List<EditorAction>();

        foreach (var entry in entries)
        {
            var oldText = entry.Text ?? "";
            var newText = Mode switch
            {
                BulkEditMode.SetText => ValueText,
                BulkEditMode.AddPrefix => ValueText + oldText,
                BulkEditMode.AddSuffix => oldText + ValueText,
                _ => Replace(oldText)
            };

            if (newText == oldText)
                continue;

            actions.Add(new ChangeFmgEntryText(Parent, container, entry, newText));
        }

        if (actions.Count > 0)
        {
            Parent.ActionManager.ExecuteAction(new FmgGroupedAction(actions));
        }

        Smithbox.Log(this, LOC.Get("TEXT_BulkEdit_Result", actions.Count, entries.Count));
    }

    private string Replace(string text)
    {
        if (!IgnoreCase)
            return text.Replace(ValueText, ReplaceText, StringComparison.Ordinal);

        // Plain text, not a pattern: escape the search, and keep '$' in the replacement literal
        return Regex.Replace(text, Regex.Escape(ValueText), ReplaceText.Replace("$", "$$"),
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
