using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Copy, Paste and Paste Special, through the system clipboard. Used by the list context menus, the Edit menu and the
/// keybinds. Pasting always goes by what the clipboard holds, not by the list it is done from: a GPARAM or groups go
/// into the selected file, fields into the selected group, values into the selected field.
/// </summary>
public class GparamTransferCommands
{
    private GparamEditorView View;
    private ProjectEntry Project;
    private GparamTransferService Service;

    public GparamTransferCommands(GparamEditorView view, ProjectEntry project, GparamTransferService service)
    {
        View = view;
        Project = project;
        Service = service;
    }

    private GparamTransferTarget CurrentTarget => GparamTransferTarget.FromSelection(View.Selection);

    #region Copy

    /// <summary>
    /// Whether the selection has something of the kind to copy.
    /// </summary>
    public bool CanCopy(GparamTransferKind kind)
    {
        var target = CurrentTarget;

        if (target.Gparam == null || target.FileEntry == null)
            return false;

        return kind switch
        {
            GparamTransferKind.Gparam => true,
            GparamTransferKind.Group => target.Group != null,
            GparamTransferKind.Field => target.Group != null && target.Field != null,
            GparamTransferKind.Values => target.Field != null && target.Values.Count > 0,
            _ => false
        };
    }

    /// <summary>
    /// Copy the selection's data of the kind to the clipboard.
    /// </summary>
    /// <param name="target">What to copy from; the selection if null.</param>
    public void Copy(GparamTransferKind kind, GparamTransferTarget target = null)
    {
        var payload = Service.BuildPayload(kind, target ?? CurrentTarget, out var error);

        if (payload == null)
        {
            Smithbox.LogError<GparamTransferCommands>(error);
            return;
        }

        PlatformUtils.Instance.SetClipboardText(payload.ToJson());

        Smithbox.Log<GparamTransferCommands>(LOC.Get("GPARAM_Transfer_Copied",
            GparamTransferPayload.KindName(kind), payload.Count));
    }

    #endregion

    #region Paste

    /// <summary>
    /// Paste the clipboard into the selection with the default mode for its kind.
    /// </summary>
    public void Paste()
    {
        var payload = ReadClipboard();

        if (payload == null)
            return;

        Apply(payload, CurrentTarget, GparamPasteModes.FromConfig());
    }

    private GparamTransferPayload ReadClipboard()
    {
        var text = PlatformUtils.Instance.GetClipboardText();
        var payload = GparamTransferPayload.Parse(text, out var error);

        if (payload == null)
        {
            Smithbox.LogError<GparamTransferCommands>(LOC.Get("GPARAM_Transfer_Clipboard_Not_Payload", error));
        }

        return payload;
    }

    /// <summary>
    /// Apply a payload, asking first if it would replace a whole file. Used by every paste and import, so they all
    /// confirm the same way. Returns whether it was applied, or is waiting for the confirmation.
    /// </summary>
    public bool Apply(GparamTransferPayload payload, GparamTransferTarget target, GparamPasteModes modes)
    {
        if (payload.Kind is GparamTransferKind.Gparam && modes.File is GparamFilePasteMode.ReplaceAll)
        {
            var check = Service.Check(payload, target, modes);

            // Nothing to confirm if it can't be applied: Apply logs why
            if (check.CanApply)
            {
                PendingConfirm = (payload, target, modes);
                OpenConfirm = true;
                return true;
            }
        }

        return Service.Apply(payload, target, modes);
    }

    #endregion

    #region Files

    /// <summary>
    /// Export the selection's data of the kind to a JSON file the user names.
    /// </summary>
    /// <param name="target">What to export from; the selection if null.</param>
    public void ExportToFile(GparamTransferKind kind, GparamTransferTarget target = null)
    {
        var payload = Service.BuildPayload(kind, target ?? CurrentTarget, out var error);

        if (payload == null)
        {
            Smithbox.LogError<GparamTransferCommands>(error);
            return;
        }

        SaveToFile(payload);
    }

    /// <summary>
    /// Save a payload to a JSON file the user names.
    /// </summary>
    public void SaveToFile(GparamTransferPayload payload)
    {
        if (!PlatformUtils.Instance.SaveFileDialog(LOC.Get("GPARAM_Transfer_Dialog_Export"),
            new[] { FilterStrings.GparamJsonFilter }, CFG.Current.GparamEditor_Transfer_Last_Folder, out var path)
            || string.IsNullOrEmpty(path))
            return;

        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            path = $"{path}.json";
        }

        if (Service.WriteFile(payload, path))
        {
            RememberFolder(path);
        }
    }

    /// <summary>
    /// Ask for a JSON file and open it in the Paste Special window, to choose how it goes in and see what it will do.
    /// Any kind is accepted: it goes where its kind belongs, as with Paste.
    /// </summary>
    public void ImportFromFile()
    {
        var payload = ReadFile(out var origin);

        if (payload == null)
            return;

        OpenPasteSpecial(payload, origin, true);
    }

    /// <summary>
    /// Ask for a JSON file and read it. Returns null if cancelled or unreadable (the error is logged).
    /// </summary>
    /// <param name="origin">Where it came from, for display.</param>
    public GparamTransferPayload ReadFile(out string origin)
    {
        origin = "";

        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("GPARAM_Transfer_Dialog_Import"),
            new[] { FilterStrings.GparamJsonFilter }, CFG.Current.GparamEditor_Transfer_Last_Folder, out var path)
            || string.IsNullOrEmpty(path))
            return null;

        var payload = Service.ReadFile(path);

        if (payload == null)
            return null;

        RememberFolder(path);

        origin = LOC.Get("GPARAM_Transfer_Origin_File", System.IO.Path.GetFileName(path));
        return payload;
    }

    /// <summary>
    /// Read the clipboard. Returns null if it holds no GPARAM data (the error is logged).
    /// </summary>
    public GparamTransferPayload ReadClipboardPayload(out string origin)
    {
        origin = LOC.Get("GPARAM_Transfer_Origin_Clipboard");
        return ReadClipboard();
    }

    private static void RememberFolder(string path)
    {
        var folder = System.IO.Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(folder))
        {
            CFG.Current.GparamEditor_Transfer_Last_Folder = folder;
        }
    }

    #endregion

    #region Menu items and keybinds

    /// <summary>
    /// Copy, Paste and Paste Special, then Export to File and Import from File, for a list's context menu.
    /// </summary>
    /// <param name="copyKind">What Copy and Export take from this list.</param>
    /// <param name="copyTarget">What they take it from; the selection if null.</param>
    public void ContextMenuItems(GparamTransferKind copyKind, GparamTransferTarget copyTarget = null)
    {
        var canCopy = copyTarget != null || CanCopy(copyKind);

        // Copy
        if (ImGui.MenuItem($"{LOC.Get($"GPARAM_Transfer_Copy_Action_{copyKind}")}##transferCopy",
            InputManager.GetHint(KeybindID.Copy), false, canCopy))
        {
            Copy(copyKind, copyTarget);
        }
        GUI.Tooltip(LOC.Get("GPARAM_Transfer_Copy_Action_TT"));

        PasteMenuItems(false);

        ImGui.Separator();

        // Export to File
        if (ImGui.MenuItem($"{LOC.Get($"GPARAM_Transfer_Export_Action_{copyKind}")}##transferExport", "", false, canCopy))
        {
            ExportToFile(copyKind, copyTarget);
        }
        GUI.Tooltip(LOC.Get("GPARAM_Transfer_Export_Action_TT"));

        ImportMenuItem();
    }

    /// <summary>
    /// Paste and Paste Special, for menus that have no Copy of their own (the Edit menu, an empty list).
    /// </summary>
    /// <param name="withImport">Add Import from File after them.</param>
    public void PasteMenuItems(bool withImport = true)
    {
        var canPaste = View.Selection.GetSelectedGparam() != null;

        // Paste
        if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Paste_Action")}##transferPaste",
            InputManager.GetHint(KeybindID.Paste), false, canPaste))
        {
            Paste();
        }
        GUI.Tooltip(LOC.Get("GPARAM_Transfer_Paste_Action_TT", DescribeDefaults()));

        // Paste Special
        if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Paste_Special_Action")}##transferPasteSpecial", "", false, canPaste))
        {
            OpenPasteSpecial();
        }
        GUI.Tooltip(LOC.Get("GPARAM_Transfer_Paste_Special_Action_TT"));

        if (withImport)
        {
            ImportMenuItem();
        }
    }

    /// <summary>
    /// Import from File.
    /// </summary>
    public void ImportMenuItem()
    {
        if (ImGui.MenuItem($"{LOC.Get("GPARAM_Transfer_Import_Action")}##transferImport", "", false,
            View.Selection.GetSelectedGparam() != null))
        {
            ImportFromFile();
        }
        GUI.Tooltip(LOC.Get("GPARAM_Transfer_Import_Action_TT"));
    }

    private static string DescribeDefaults()
    {
        var modes = GparamPasteModes.FromConfig();

        return string.Join("\n", Enum.GetValues<GparamTransferKind>()
            .Select(k => $"{GparamTransferPayload.KindName(k)}: {modes.Describe(k)}"));
    }

    /// <summary>
    /// Copy and Paste keys, for a list to call while it has focus.
    /// </summary>
    public void Shortcuts(GparamTransferKind copyKind)
    {
        // Leave the keys to a text box being typed in (e.g. the list's search bar)
        if (ImGui.GetIO().WantTextInput)
            return;

        if (InputManager.IsPressed(KeybindID.Copy) && CanCopy(copyKind))
        {
            Copy(copyKind);
        }

        if (InputManager.IsPressed(KeybindID.Paste) && View.Selection.GetSelectedGparam() != null)
        {
            Paste();
        }
    }

    #endregion

    #region Popups

    private const string ConfirmId = "###gparamTransferReplaceConfirm";
    private const string PasteSpecialId = "###gparamTransferPasteSpecial";

    private bool OpenConfirm = false;
    private (GparamTransferPayload payload, GparamTransferTarget target, GparamPasteModes modes)? PendingConfirm;

    private bool OpenSpecial = false;
    private GparamTransferPayload SpecialPayload;
    private GparamPasteModes SpecialModes;
    private bool SpecialSaveDefault = false;

    // Where the window's data came from ("the clipboard", a file name), and whether it is an import
    private string SpecialOrigin = "";
    private bool SpecialIsImport = false;

    // The last check in the Paste Special window, and what it was made for: checking materialises the payload, too
    // much to do every frame for a whole GPARAM
    private GparamTransferCheck SpecialCheck;
    private object SpecialCheckKey;

    /// <summary>
    /// Read the clipboard and open the Paste Special window on it.
    /// </summary>
    public void OpenPasteSpecial()
    {
        var payload = ReadClipboard();

        if (payload == null)
            return;

        OpenPasteSpecial(payload, LOC.Get("GPARAM_Transfer_Origin_Clipboard"), false);
    }

    /// <summary>
    /// Open the Paste Special window on a payload.
    /// </summary>
    /// <param name="origin">Where it came from, for the window to show.</param>
    /// <param name="isImport">It came from a file: the window is titled as an import.</param>
    public void OpenPasteSpecial(GparamTransferPayload payload, string origin, bool isImport)
    {
        SpecialOrigin = origin;
        SpecialIsImport = isImport;
        SpecialPayload = payload;
        SpecialModes = GparamPasteModes.FromConfig();
        SpecialSaveDefault = false;
        SpecialCheck = null;
        SpecialCheckKey = null;
        OpenSpecial = true;
    }

    /// <summary>
    /// Draw the Paste Special window and the Replace All confirmation. Called every frame by the view, outside the
    /// list windows, so they show whichever menu or key opened them.
    /// </summary>
    public void DisplayPopups()
    {
        DisplayPasteSpecial();
        DisplayConfirm();
    }

    private void DisplayPasteSpecial()
    {
        // The ### part is the ID, so the title can change between paste and import
        var title = $"{LOC.Get(SpecialIsImport ? "GPARAM_Transfer_Import_Title" : "GPARAM_Transfer_Paste_Special_Title")}{PasteSpecialId}";

        if (OpenSpecial)
        {
            OpenSpecial = false;
            ImGui.OpenPopup(title);
        }

        var open = true;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(520, 0) * DPI.UIScale(), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal(title, ref open))
        {
            if (!open)
                SpecialPayload = null;

            return;
        }

        var payload = SpecialPayload;
        var close = false;

        if (payload == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("GPARAM_Transfer_Paste_Special_Hint"));

        // Clipboard
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Paste_Special_Clipboard_Header"), LOC.Get("GPARAM_Transfer_Paste_Special_Clipboard_Header_TT"));

        GUI.WrappedText(DescribePayload(payload, SpecialOrigin));

        // Mode
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Paste_Special_Mode_Header"), LOC.Get("GPARAM_Transfer_Paste_Special_Mode_Header_TT"));

        SpecialModes = ModeCombo(payload.Kind, SpecialModes);

        ImGui.Checkbox($"{LOC.Get("GPARAM_Transfer_Paste_Special_Save_Default")}##transferSaveDefault", ref SpecialSaveDefault);
        GUI.Tooltip(LOC.Get("GPARAM_Transfer_Paste_Special_Save_Default_TT"));

        // Result
        var target = CurrentTarget;
        var check = GetSpecialCheck(payload, target, SpecialModes);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Transfer_Paste_Special_Result_Header"), LOC.Get("GPARAM_Transfer_Paste_Special_Result_Header_TT"));

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

        GUI.ConditionalMultiButtonInput("transferPasteSpecialActions",
            "transferPasteSpecialApply", LOC.Get("GPARAM_Transfer_Paste_Special_Apply"), LOC.Get("GPARAM_Transfer_Paste_Special_Apply_TT"),
            () =>
            {
                if (SpecialSaveDefault)
                    SaveDefault(payload.Kind, SpecialModes);

                Apply(payload, target, SpecialModes);
                close = true;
            }, check.CanApply,

            "transferPasteSpecialCancel", LOC.Get("GPARAM_Transfer_Paste_Special_Cancel"), LOC.Get("GPARAM_Transfer_Paste_Special_Cancel_TT"),
            () => close = true, true);

        if (close)
        {
            SpecialPayload = null;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private GparamTransferCheck GetSpecialCheck(GparamTransferPayload payload, GparamTransferTarget target, GparamPasteModes modes)
    {
        // The check depends on the mode and on what is selected
        var key = (modes, target.FileEntry, target.Group, target.Field, target.Values.Count, target.Values.FirstOrDefault(), target.Values.LastOrDefault());

        if (SpecialCheck == null || !Equals(SpecialCheckKey, key))
        {
            SpecialCheck = Service.Check(payload, target, modes);
            SpecialCheckKey = key;
        }

        return SpecialCheck;
    }

    public static string DescribePayload(GparamTransferPayload payload, string origin)
    {
        var source = payload.Source ?? new GparamTransferSource();

        var from = string.Join(" / ", new[] { source.File, source.Group, source.Field }.Where(e => !string.IsNullOrEmpty(e)));

        if (from == "")
            from = LOC.Get("GPARAM_Transfer_Source_Unknown");

        var text = LOC.Get("GPARAM_Transfer_Describe_Payload",
            GparamTransferPayload.KindName(payload.Kind), payload.Count, from, payload.Game ?? "?", origin);

        if (payload.IsLegacy)
            text = $"{text}\n{LOC.Get("GPARAM_Transfer_Describe_Payload_Legacy")}";

        return text;
    }

    /// <summary>
    /// The mode combo for the payload kind. Returns the modes with that kind's changed.
    /// </summary>
    public static GparamPasteModes ModeCombo(GparamTransferKind kind, GparamPasteModes modes)
    {
        ImGui.SetNextItemWidth(-1);

        switch (kind)
        {
            case GparamTransferKind.Gparam:
                return modes with { File = Combo("##transferModeFile", modes.File, GparamPasteModes.Key) };

            case GparamTransferKind.Group:
                return modes with { Group = Combo("##transferModeGroup", modes.Group, GparamPasteModes.Key) };

            case GparamTransferKind.Field:
                return modes with { Field = Combo("##transferModeField", modes.Field, GparamPasteModes.Key) };

            case GparamTransferKind.Values:
                return modes with { Values = Combo("##transferModeValues", modes.Values, GparamPasteModes.Key) };
        }

        return modes;
    }

    /// <param name="key">The mode's localization key; its tooltip is the same key with _TT.</param>
    private static T Combo<T>(string id, T current, Func<T, string> key) where T : struct, Enum
    {
        var result = current;

        if (ImGui.BeginCombo(id, LOC.Get(key(current))))
        {
            foreach (var mode in Enum.GetValues<T>())
            {
                if (ImGui.Selectable($"{LOC.Get(key(mode))}##{mode}", mode.Equals(current)))
                {
                    result = mode;
                }
                GUI.Tooltip(LOC.Get($"{key(mode)}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get($"{key(current)}_TT"));

        return result;
    }

    public static void SaveDefault(GparamTransferKind kind, GparamPasteModes modes)
    {
        switch (kind)
        {
            case GparamTransferKind.Gparam: CFG.Current.GparamEditor_Transfer_File_Mode = modes.File; break;
            case GparamTransferKind.Group: CFG.Current.GparamEditor_Transfer_Group_Mode = modes.Group; break;
            case GparamTransferKind.Field: CFG.Current.GparamEditor_Transfer_Field_Mode = modes.Field; break;
            case GparamTransferKind.Values: CFG.Current.GparamEditor_Transfer_Values_Mode = modes.Values; break;
        }
    }

    private void DisplayConfirm()
    {
        var title = $"{LOC.Get("GPARAM_Transfer_Confirm_Title")}{ConfirmId}";

        if (OpenConfirm)
        {
            OpenConfirm = false;
            ImGui.OpenPopup(title);
        }

        var open = true;
        var confirmed = false;
        var cancelled = false;

        // A fixed width: auto-resize alone sizes the window to its content, and wrapped text sizes itself to the
        // window, so the text never wraps and the window runs off screen. A height of 0 still fits the content.
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var pending = PendingConfirm;

            GUI.WrappedText(LOC.Get("GPARAM_Transfer_Confirm_Text",
                pending?.target.FileEntry?.Filename ?? "",
                pending?.payload.Source?.File ?? LOC.Get("GPARAM_Transfer_Source_Unknown")));

            GUI.MultiButtonInput("transferConfirmActions",
                "transferConfirmApply", LOC.Get("GPARAM_Transfer_Confirm_Apply"), LOC.Get("GPARAM_Transfer_Confirm_Apply_TT"),
                () => confirmed = true,

                "transferConfirmCancel", LOC.Get("GPARAM_Transfer_Confirm_Cancel"), LOC.Get("GPARAM_Transfer_Confirm_Cancel_TT"),
                () => cancelled = true);

            if (confirmed || cancelled)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (confirmed && PendingConfirm is { } apply)
        {
            Service.Apply(apply.payload, apply.target, apply.modes);
        }

        if (confirmed || cancelled || !open)
        {
            PendingConfirm = null;
        }
    }

    #endregion
}
