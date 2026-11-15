#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The cut's fade events as the game shows them: a colour laid over the whole view.
/// <list type="bullet">
/// <item>Fade-in (15, fadeCol): from the colour to clear over the event.</item>
/// <item>Fade-out (16, fadeCol): from clear to the colour over the event.</item>
/// <item>Fade-in/out (17, beginFadeCol, endFadeCol): from one colour to the other, alpha included.</item>
/// </list>
/// <para>Colours are packed 0xAARRGGBB: 0xFF000000 is opaque black, 0xB4000000 black at 70%. Fade-in and Fade-out take
/// only the colour's RGB (their fadeCol is 0 in every surveyed game, yet they fade to and from black), the event's ramp
/// gives the alpha.</para>
/// <para>A fade holds where it ended until the next one starts (a fade-out near the end of a cut leaves it black), so
/// the screen follows the latest fade to have started at the playhead; before the first, the view is clear.</para>
/// </summary>
public class CutsceneFades
{
    public const int FadeIn = 15;
    public const int FadeOut = 16;
    public const int FadeInOut = 17;

    private readonly CutsceneEditorScreen Editor;

    // The fade events of the shown cut, by start; their colours and timing are read when drawn, so edits show at once
    private List<(MQB.Event Event, int Type)> Events = new();
    private (CutsceneScene? Scene, int Version) EventsFor;

    public CutsceneFades(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    private void RefreshEvents(CutsceneScene? scene)
    {
        var key = (scene, Editor.StructureVersion);
        if (EventsFor == key)
            return;

        EventsFor = key;
        Events = new();

        var mqb = scene?.File.Mqb;
        if (scene == null || mqb == null)
            return;

        foreach (var timeline in scene.Cut.Timelines)
        foreach (var e in timeline.Events)
        {
            if (e.ResourceIndex < 0 || e.ResourceIndex >= mqb.Resources.Count)
                continue;

            var type = mqb.Resources[e.ResourceIndex].Type;
            if (type is FadeIn or FadeOut or FadeInOut)
                Events.Add((e, type));
        }
    }

    /// <summary>The colour over the view at a frame (straight alpha), null when clear</summary>
    public Vector4? ColorAt(float frame)
    {
        RefreshEvents(Editor.Scene);

        // The latest to have started; on a tie, the later in timeline order
        (MQB.Event Event, int Type)? latest = null;
        foreach (var e in Events)
        {
            if (e.Event.StartFrame <= frame && (latest == null || e.Event.StartFrame >= latest.Value.Event.StartFrame))
                latest = e;
        }

        if (latest is not { } fade)
            return null;

        var duration = Math.Max(1, fade.Event.Duration);
        var t = Math.Clamp((frame - fade.Event.StartFrame) / duration, 0f, 1f);

        Vector4 color;
        switch (fade.Type)
        {
            case FadeIn:
            case FadeOut:
                var rgb = Unpack(Param(fade.Event, "fadeCol"));
                color = rgb with { W = fade.Type == FadeIn ? 1 - t : t };
                break;
            default:
                color = Vector4.Lerp(Unpack(Param(fade.Event, "beginFadeCol")), Unpack(Param(fade.Event, "endFadeCol")), t);
                break;
        }

        return color.W > 0.001f ? color : null;
    }

    /// <summary>
    /// Lays the fade at the playhead over the viewport's scene. Called first in the viewport's overlay, inside its
    /// window, so the overlay's text (and the subtitles, as in the game) stays on top.
    /// </summary>
    /// <param name="top">The top of the window's content, below its menu bar</param>
    public void DrawOverlay(float x, float y, float width, float height, float top)
    {
        if (!CFG.Current.CutsceneEditor_Display_Fades || Editor.Scene == null || width <= 0 || height <= 0)
            return;

        if (ColorAt(Editor.Playback.Frame) is not { } color)
            return;

        var from = new Vector2(x, Math.Max(y, top));
        var to = new Vector2(x + width, y + height);
        if (to.Y <= from.Y)
            return;

        ImGui.GetWindowDrawList().AddRectFilled(from, to, ImGui.GetColorU32(color));
    }

    private static uint Param(MQB.Event e, string name) =>
        e.Parameters.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value switch
        {
            uint v => v,
            int v => unchecked((uint)v),
            long v => unchecked((uint)v),
            ulong v => unchecked((uint)v),
            _ => 0
        };

    /// <summary>0xAARRGGBB to an RGBA colour</summary>
    private static Vector4 Unpack(uint argb) => new(
        ((argb >> 16) & 0xFF) / 255f,
        ((argb >> 8) & 0xFF) / 255f,
        (argb & 0xFF) / 255f,
        ((argb >> 24) & 0xFF) / 255f);
}
