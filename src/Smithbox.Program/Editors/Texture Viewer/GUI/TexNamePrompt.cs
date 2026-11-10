using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// A modal asking for a name (Rename TPF, Copy Container As). Opened from a context menu by Open, and drawn every
/// frame by the view, outside the menu (Tool UI Styling: Modals).
/// </summary>
public class TexNamePrompt
{
    private bool PendingOpen;
    private string Title = "";
    private string Hint = "";
    private string Text = "";
    private Func<string, string> Validate;
    private Action<string> Accept;

    /// <param name="validate">Returns an error to show, or null when the name can be used.</param>
    public void Open(string title, string hint, string initial, Func<string, string> validate, Action<string> accept)
    {
        Title = title;
        Hint = hint;
        Text = initial ?? "";
        Validate = validate;
        Accept = accept;
        PendingOpen = true;
    }

    public void Draw()
    {
        if (PendingOpen)
        {
            PendingOpen = false;
            ImGui.OpenPopup("###texNamePrompt");
        }

        ImGui.SetNextWindowSize(new Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;
        if (!ImGui.BeginPopupModal($"{Title}###texNamePrompt", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        GUI.WrappedText(Hint);
        GUI.Spacer();

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##texNamePromptInput", ref Text, 255);
        GUI.Tooltip(Hint);

        var error = Validate?.Invoke(Text.Trim());
        if (error != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, error);
        }

        GUI.Spacer();

        var confirm = error == null && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter));

        GUI.ConditionalMultiButtonInput("texNamePromptActions",
            "accept", LOC.Get("TEXVIEW_NamePrompt_Accept"), LOC.Get("TEXVIEW_NamePrompt_Accept_TT"), () => confirm = true, error == null,
            "cancel", LOC.Get("TEXVIEW_NamePrompt_Cancel"), LOC.Get("TEXVIEW_NamePrompt_Cancel_TT"), () => open = false, true);

        if (confirm)
        {
            Accept?.Invoke(Text.Trim());
            open = false;
        }

        if (!open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }
}
