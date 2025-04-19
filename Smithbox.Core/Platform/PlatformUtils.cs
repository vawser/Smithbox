using NativeFileDialogSharp;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using Veldrid;

namespace StudioCore.Platform;

/// <summary>
///     Trying to match Winforms version of this
/// </summary>
public enum MessageBoxButtons
{
    OK,
    OKCancel,
    YesNoCancel,
    YesNo
}

/// <summary>
///     Also trying to match Winforms version of this
/// </summary>
[Flags]
public enum MessageBoxIcon
{
    None,
    Error = MessageBoxFlags.Error,
    Warning = MessageBoxFlags.Warning,
    Information = MessageBoxFlags.Information
}

public enum DialogResult
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Yes = 3,
    No = 4
}

/// <summary>
///     Helper to abstract away some platform specific things to make things like a native Linux build
///     maybe more feasible in the future.
/// </summary>
public abstract unsafe class PlatformUtils
{
    private readonly Window* _sdlWindow;

    protected PlatformUtils(Window* window)
    {
        _sdlWindow = window;
    }

    public static PlatformUtils Instance { get; private set; }

    public static void InitializeWindows(Window* window)
    {
        Instance = new WindowsPlatformUtils(window);
    }

    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        var buttonNames = new string[3];
        var buttonResults = new DialogResult[3];
        int buttonCount;
        var returnID = -1;
        var escapeID = -1;
        switch (buttons)
        {
            case MessageBoxButtons.OK:
                buttonCount = 1;
                buttonNames[0] = "Ok";
                buttonResults[0] = DialogResult.OK;
                returnID = 0;
                break;
            case MessageBoxButtons.OKCancel:
                buttonCount = 2;
                buttonNames[1] = "Ok";
                buttonResults[1] = DialogResult.OK;
                buttonNames[0] = "Cancel";
                buttonResults[0] = DialogResult.Cancel;
                returnID = 1;
                escapeID = 0;
                break;
            case MessageBoxButtons.YesNoCancel:
                buttonCount = 3;
                buttonNames[2] = "Yes";
                buttonResults[2] = DialogResult.Yes;
                buttonNames[1] = "No";
                buttonResults[1] = DialogResult.No;
                buttonNames[0] = "Cancel";
                buttonResults[0] = DialogResult.Cancel;
                returnID = 2;
                escapeID = 0;
                break;
            case MessageBoxButtons.YesNo:
                buttonCount = 2;
                buttonNames[1] = "Yes";
                buttonResults[1] = DialogResult.Yes;
                buttonNames[0] = "No";
                buttonResults[0] = DialogResult.No;
                returnID = 1;
                escapeID = 0;
                break;
            default:
                throw new Exception("Invalid button type");
        }

        MessageBoxButtonData* buttonData = stackalloc MessageBoxButtonData[buttonCount];
        for (var i = 0; i < buttonCount; i++)
        {
            MessageBoxButtonFlags buttonflags = 0;
            buttonflags |= i == returnID ? MessageBoxButtonFlags.ReturnkeyDefault : 0;
            buttonflags |= i == escapeID ? MessageBoxButtonFlags.EscapekeyDefault : 0;
            buttonData[i] = new MessageBoxButtonData
            {
                Flags = (uint)buttonflags,
                Buttonid = (int)buttonResults[i],
                Text = (byte*)((FixedUtf8String)buttonNames[i]).StringPtr
            };
        }

        var messageBoxData = new MessageBoxData
        {
            Flags = (uint)icon,
            Window = _sdlWindow,
            Title = (byte*)((FixedUtf8String)caption).StringPtr,
            Message = (byte*)((FixedUtf8String)text).StringPtr,
            Numbuttons = buttonCount,
            Buttons = buttonData,
            ColorScheme = null
        };

        var clickedButton = 0;
        var result = SdlProvider.SDL.Value.ShowMessageBox(&messageBoxData, ref clickedButton);
        if (result == 0)
        {
            return (DialogResult)clickedButton;
        }

        return DialogResult.None;
    }

    public void SetClipboardText(string text)
    {
        SdlProvider.SDL.Value.SetClipboardText((byte*)((FixedUtf8String)text).StringPtr);
    }

    // Title arg is currently unusable. We should restore it back if at all possible.
    public bool OpenFileDialog(string title, IReadOnlyList<string> filters, out string path)
    {
        NativeFileDialogSharp.DialogResult dialogResult = Dialog.FileOpen(CombineNdlFilters(filters, false));
        path = dialogResult.Path;
        return dialogResult.IsOk;
    }

    public bool OpenMultiFileDialog(string title, IReadOnlyList<string> filters, out IReadOnlyList<string> paths)
    {
        NativeFileDialogSharp.DialogResult
            dialogResult = Dialog.FileOpenMultiple(CombineNdlFilters(filters, false));
        paths = dialogResult.Paths;
        return dialogResult.IsOk;
    }

    public bool SaveFileDialog(string title, IReadOnlyList<string> filters, out string path)
    {
        NativeFileDialogSharp.DialogResult dialogResult = Dialog.FileSave(CombineNdlFilters(filters, true));
        path = dialogResult.Path;
        return dialogResult.IsOk;
    }

    public bool OpenFolderDialog(string title, out string path)
    {
        NativeFileDialogSharp.DialogResult dialogResult = Dialog.FolderPicker();
        path = dialogResult.Path;
        return dialogResult.IsOk;
    }

    private static string CombineNdlFilters(IReadOnlyList<string> filters, bool dropdown)
    {
        // Join with , for simultaneous selection and join with ; for dropdown alternatives.
        // Because we don't have custom names explaining each dropdown alternative, it's less confusing to combine them together
        // for file opening cases.
        return filters.Count == 0 ? null : string.Join(dropdown ? ";" : ",", filters);
    }

    public void blah()
    {
    }
}
