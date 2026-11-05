using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The Data Transfer section of the Tools window: a staging panel. It holds one payload (loaded from the clipboard
/// or a file), shows what is in it and what applying it to the current selection will do, and applies it, as many
/// times and to as many targets as wanted.
/// </summary>
/// <remarks>
/// It also owns the transfer service and the commands every other entry point uses (the list context menus, the
/// File and Edit menus, the keybinds).
/// </remarks>
public class GparamDataTransferTool
{
    private GparamEditorView View;
    private ProjectEntry Project;

    public GparamTransferService Transfer;
    public GparamTransferCommands Commands;

    // The staged payload, where it came from, and a label per item for the preview
    private GparamTransferPayload Payload;
    private string PayloadOrigin = "";
    private List<string> ItemLabels = new();

    private bool ShowItems = true;

    // The last check, and what it was made for: checking materialises the payload, too much to do every frame
    private GparamTransferCheck Check;
    private object CheckKey;
    private bool CheckDirty = true;

    public GparamDataTransferTool(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Transfer = new GparamTransferService(view, project);
        Commands = new GparamTransferCommands(view, project, Transfer);

        // An edit or undo can change what the payload would do (e.g. a group it would add now exists)
        View.ActionManager.ActionPerformed += () => CheckDirty = true;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("GPARAM_Tools_Data_Transfer")}##dataTransferHeader"))
            return;

        ImGui.BeginChild("GparamDataTransferSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("GPARAM_Transfer_Panel_Hint"));

        // Data
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Panel_Data_Header"), LOC.Get("GPARAM_Transfer_Panel_Data_Header_TT"));

        if (Payload == null)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Transfer_Panel_No_Data"));
        }
        else
        {
            GUI.WrappedText(GparamTransferCommands.DescribePayload(Payload, PayloadOrigin));
        }

        GUI.ConditionalMultiButtonInput("dataTransferLoadActions",
            "dataTransferLoadClipboard", LOC.Get("GPARAM_Transfer_Panel_Load_Clipboard"), LOC.Get("GPARAM_Transfer_Panel_Load_Clipboard_TT"),
            LoadFromClipboard, true,

            "dataTransferLoadFile", LOC.Get("GPARAM_Transfer_Panel_Load_File"), LOC.Get("GPARAM_Transfer_Panel_Load_File_TT"),
            LoadFromFile, true,

            "dataTransferClear", LOC.Get("GPARAM_Transfer_Panel_Clear"), LOC.Get("GPARAM_Transfer_Panel_Clear_TT"),
            Clear, Payload != null);

        if (Payload == null)
            return;

        DisplayItems();

        // Mode: the same setting Paste uses
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Panel_Mode_Header"), LOC.Get("GPARAM_Transfer_Panel_Mode_Header_TT"));

        var modes = GparamPasteModes.FromConfig();
        var newModes = GparamTransferCommands.ModeCombo(Payload.Kind, modes);

        if (newModes != modes)
        {
            GparamTransferCommands.SaveDefault(Payload.Kind, newModes);
            modes = newModes;
        }

        // Result
        var target = GparamTransferTarget.FromSelection(View.Selection);
        var check = GetCheck(target, modes);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Panel_Result_Header"), LOC.Get("GPARAM_Transfer_Panel_Result_Header_TT"));

        if (check.CanApply)
        {
            GUI.WrappedText(check.Description);
        }
        else
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, check.Error);
        }

        if (check.Warning != null)
        {
            GUI.WrappedText(check.Warning);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Panel_Actions_Header"), "");

        GUI.ConditionalMultiButtonInput("dataTransferPanelActions",
            "dataTransferApply", LOC.Get("GPARAM_Transfer_Panel_Apply"), LOC.Get("GPARAM_Transfer_Panel_Apply_TT"),
            () => Commands.Apply(Payload, target, modes), check.CanApply,

            "dataTransferSave", LOC.Get("GPARAM_Transfer_Panel_Save"), LOC.Get("GPARAM_Transfer_Panel_Save_TT"),
            () => Commands.SaveToFile(Payload), true,

            "dataTransferCopyJson", LOC.Get("GPARAM_Transfer_Panel_Copy_Json"), LOC.Get("GPARAM_Transfer_Panel_Copy_Json_TT"),
            () => PlatformUtils.Instance.SetClipboardText(Payload.ToJson()), true);
    }

    private void DisplayItems()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("GPARAM_Transfer_Panel_Items_Header", ItemLabels.Count),
            LOC.Get("GPARAM_Transfer_Panel_Items_Header_TT"), ref ShowItems);

        if (!ShowItems)
            return;

        // Up to 8 rows tall, scrolling past that
        var rowHeight = ImGui.GetTextLineHeightWithSpacing();
        var height = Math.Min(ItemLabels.Count, 8) * rowHeight + ImGui.GetStyle().WindowPadding.Y * 2;

        ImGui.BeginChild("dataTransferItems", new Vector2(0, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(ItemLabels.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                ImGui.TextUnformatted(ItemLabels[i]);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    #region Load

    private void LoadFromClipboard()
    {
        var payload = Commands.ReadClipboardPayload(out var origin);

        if (payload != null)
            Stage(payload, origin);
    }

    private void LoadFromFile()
    {
        var payload = Commands.ReadFile(out var origin);

        if (payload != null)
            Stage(payload, origin);
    }

    private void Stage(GparamTransferPayload payload, string origin)
    {
        Payload = payload;
        PayloadOrigin = origin;
        ItemLabels = BuildItemLabels(payload);
        CheckDirty = true;
    }

    private void Clear()
    {
        Payload = null;
        PayloadOrigin = "";
        ItemLabels = new();
        Check = null;
        CheckKey = null;
    }

    /// <summary>
    /// One line per item, made once when the payload is staged.
    /// </summary>
    private List<string> BuildItemLabels(GparamTransferPayload payload)
    {
        switch (payload.Kind)
        {
            case GparamTransferKind.Gparam:
                return (payload.GetGparam()?.Params ?? new List<GPARAM.Param>())
                    .Where(e => e != null)
                    .Select(GroupLabel)
                    .ToList();

            case GparamTransferKind.Group:
                return payload.GetGroups().Select(GroupLabel).ToList();

            case GparamTransferKind.Field:
                return payload.GetFields().Select(e => FieldLabel(payload.Source?.Group, e)).ToList();

            case GparamTransferKind.Values:
                return payload.GetValues().Select(ValueLabel).ToList();
        }

        return new List<string>();
    }

    private string GroupLabel(GPARAM.Param group)
    {
        var name = GparamMetaUtils.GetGroupName(Project, group.Key) ?? group.Key;

        return LOC.Get("GPARAM_Transfer_Panel_Item_Group", name, group.Key, group.Fields?.Count ?? 0);
    }

    private string FieldLabel(string groupKey, GPARAM.IField field)
    {
        // The name needs the group, which legacy exports don't record
        var name = groupKey != null ? GparamMetaUtils.GetFieldName(Project, groupKey, field.Key) ?? field.Key : field.Key;

        return LOC.Get("GPARAM_Transfer_Panel_Item_Field", name, field.Key,
            GparamUtils.GetReadableObjectTypeName(field), field.Values?.Count ?? 0);
    }

    private static string ValueLabel(GPARAM.IFieldValue value)
    {
        var text = Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? "";

        return LOC.Get("GPARAM_Transfer_Panel_Item_Value", value.ID, text);
    }

    #endregion

    private GparamTransferCheck GetCheck(GparamTransferTarget target, GparamPasteModes modes)
    {
        // The check depends on the mode, on what is selected, and on the data (see CheckDirty)
        var key = (Payload, modes, target.FileEntry, target.Group, target.Field,
            target.Values.Count, target.Values.FirstOrDefault(), target.Values.LastOrDefault());

        if (Check == null || CheckDirty || !Equals(CheckKey, key))
        {
            Check = Transfer.Check(Payload, target, modes);
            CheckKey = key;
            CheckDirty = false;
        }

        return Check;
    }
}
