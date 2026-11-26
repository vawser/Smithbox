#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// Check before Export (Animation Editor Analysis.md §8, W13): the warnings of every clip Export would write, in one
/// list, before the anibnds are written. With nothing to warn about, Export goes ahead at once; otherwise a prompt lists
/// them per clip, with Export Anyway, Cancel, and a button per clip that selects it (and cancels) to fix it first.
/// <para>
/// The warnings are the ones that can be told without encoding the clip: the root rising without gravity off in the
/// time act (<see cref="AnimationGravityCheck" />), time act events starting after the clip's last frame, a time
/// change not yet passed on to the events (W1), and a new ID with no time act entry. What the encoder makes of a clip
/// (format, size, error) is logged per clip by Export itself.
/// </para>
/// </summary>
public class AnimationExportCheck
{
    private const string PromptId = "###animationEditorExportCheck";

    private readonly AnimationEditorView View;

    // The warnings found when Export was asked for, per clip; null while the prompt isn't open
    private List<(AnimationClipSource Source, List<string> Warnings)>? Found;

    public AnimationExportCheck(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>Checks the clips Export would write: exports at once when nothing is found, else opens the prompt</summary>
    public void Request()
    {
        var found = Collect();
        if (found.Count == 0)
        {
            View.ExportNow();
            return;
        }

        Found = found;
    }

    private List<(AnimationClipSource, List<string>)> Collect()
    {
        var result = new List<(AnimationClipSource, List<string>)>();
        var bank = View.Bank;
        if (bank.Character is not { } character)
            return result;

        foreach (var source in bank.EditedClips.Where(bank.IsUnexported).OrderBy(c => c.AnimationId))
        {
            if (bank.Get(source) is not { } clip)
                continue;

            var warnings = new List<string>();
            var acts = character.TimeActs;
            var events = acts?.For(source.AnimationId);

            if (character.Gravity is { } gravity)
            {
                var played = clip.Baked();
                switch (gravity.Check(source.AnimationId, played, out var rise))
                {
                    case AnimationGravityCheck.Result.NotDisabled:
                        warnings.Add(LOC.Get("ANIM_Export_Check_Gravity", rise.First, rise.Peak));
                        break;
                    case AnimationGravityCheck.Result.PartlyDisabled:
                        warnings.Add(LOC.Get("ANIM_Export_Check_Gravity_Partly", rise.First, rise.Peak));
                        break;
                }
            }

            if (events != null && clip.FrameDuration > 0)
            {
                var last = clip.FrameCount - 1;
                var past = events.Count(e => e.Start / clip.FrameDuration > last + 0.5f);
                if (past > 0)
                    warnings.Add(LOC.Get("ANIM_Export_Check_Events_Past_End", past, last));
            }

            if (View.ClipView.HasPendingRetime(source))
                warnings.Add(LOC.Get("ANIM_Export_Check_Retime_Pending"));

            if (source.IsNew && events == null)
                warnings.Add(LOC.Get("ANIM_Export_Check_No_Time_Act"));

            if (warnings.Count > 0)
                result.Add((source, warnings));
        }

        return result;
    }

    /// <summary>The prompt (drawn outside the windows, as the other prompts)</summary>
    public void Display()
    {
        if (Found != null && !ImGui.IsPopupOpen(PromptId))
            ImGui.OpenPopup(PromptId);

        ImGui.SetNextWindowSizeConstraints(new Vector2(420, 0) * DPI.UIScale(), new Vector2(900, 600) * DPI.UIScale());
        if (!ImGui.BeginPopupModal($"{LOC.Get("ANIM_Export_Check_Title")}{PromptId}", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        // Closed some other way (another character opened): nothing left to ask about
        if (Found == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("ANIM_Export_Check_Text", Found.Count));
        GUI.Spacer();

        AnimationClipSource? goTo = null;
        foreach (var (source, warnings) in Found)
        {
            if (ImGui.SmallButton($"{source.Name}##animationExportCheckGo_{source.BinderId}"))
                goTo = source;
            GUI.Tooltip(LOC.Get("ANIM_Export_Check_Go_TT"));

            ImGui.Indent();
            foreach (var warning in warnings)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, warning);
            ImGui.Unindent();
        }

        GUI.Spacer();

        if (ImGui.Button($"{LOC.Get("ANIM_Export_Check_Export")}##animationExportCheckExport"))
        {
            Close();
            View.ExportNow();
        }
        GUI.Tooltip(LOC.Get("ANIM_Export_Check_Export_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ANIM_Export_Check_Cancel")}##animationExportCheckCancel"))
            Close();

        if (goTo != null)
        {
            Close();
            View.SelectClip(goTo);
        }

        ImGui.EndPopup();
    }

    private void Close()
    {
        Found = null;
        ImGui.CloseCurrentPopup();
    }
}
