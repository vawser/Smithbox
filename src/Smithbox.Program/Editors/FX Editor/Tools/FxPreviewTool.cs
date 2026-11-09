using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The viewport's playback bar and preview inputs (FX Editor Plan, phase V1): play/pause/restart, speed, the time
/// scrubber, loop, the preview state and the external values, and what the debug shapes show.
/// </summary>
public class FxPreviewTool
{
    private readonly FxEditorScreen Editor;

    private int NewExternalId = 1;

    public FxPreviewTool(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FX_PreviewTool_Header")}##fxPreviewToolHeader"))
            return;

        ImGui.BeginChild("FxPreviewToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var playback = Editor.Playback;

        GUI.WrappedText(LOC.Get("FX_PreviewTool_Hint"));

        // Simulation
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Simulation_Header"), LOC.Get("FX_PreviewTool_Simulation_Header_TT"));
        DisplayStatus();

        // Playback
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Playback_Header"), LOC.Get("FX_PreviewTool_Playback_Header_TT"));

        GUI.MultiButtonInput("fxPlaybackButtons",
            "fxPlayPause", playback.Playing ? LOC.Get("FX_PreviewTool_Pause") : LOC.Get("FX_PreviewTool_Play"),
            LOC.Get("FX_PreviewTool_PlayPause_TT"), () => playback.Playing = !playback.Playing,

            "fxRestart", LOC.Get("FX_PreviewTool_Restart"), LOC.Get("FX_PreviewTool_Restart_TT"),
            () => { playback.Restart(); playback.Playing = true; },

            "fxFrame", LOC.Get("FX_PreviewTool_Frame"), LOC.Get("FX_PreviewTool_Frame_TT"),
            () => Editor.ViewportWindow.Frame());

        // Time
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Time_Header"), LOC.Get("FX_PreviewTool_Time_Header_TT"));

        float time = playback.Time;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##fxTime", ref time, 0, playback.Length, "%.2f s"))
        {
            playback.Seek(time);
            playback.Playing = false;
        }
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Time_TT"));

        // Length
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Length_Header"), LOC.Get("FX_PreviewTool_Length_Header_TT"));

        float length = playback.Length;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##fxLength", ref length, 0.5f, 5f, "%.2f s"))
            playback.Length = length;
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Length_TT"));

        bool loop = playback.Loop;
        if (ImGui.Checkbox($"{LOC.Get("FX_PreviewTool_Loop")}##fxLoop", ref loop))
            playback.Loop = loop;
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Loop_TT"));

        // Speed
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Speed_Header"), LOC.Get("FX_PreviewTool_Speed_Header_TT"));

        float speed = playback.Speed;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##fxSpeed", ref speed, 0.05f, 4f, "%.2fx", ImGuiSliderFlags.Logarithmic))
            playback.Speed = speed;
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Speed_TT"));

        // Preview State
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_State_Header"), LOC.Get("FX_PreviewTool_State_Header_TT"));

        var fxr = Editor.Selection.Entry?.Fxr;
        int stateCount = Math.Max(fxr?.States.Count ?? 1, 1);
        int state = Math.Min(playback.PreviewState, stateCount - 1);
        if (state != playback.PreviewState)
            playback.SetPreviewState(state);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderInt("##fxPreviewState", ref state, 0, stateCount - 1))
            playback.SetPreviewState(state);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_State_TT"));

        // External Values
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_External_Header"), LOC.Get("FX_PreviewTool_External_Header_TT"));

        DisplayExternalValues(playback);

        // Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Display_Header"), LOC.Get("FX_PreviewTool_Display_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("FX_PreviewTool_Display_Billboards")}##fxShowBillboards", ref CFG.Current.FxEditor_Viewport_Display_Billboards);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Display_Billboards_TT"));

        ImGui.Checkbox($"{LOC.Get("FX_PreviewTool_Display_Particles")}##fxShowParticles", ref CFG.Current.FxEditor_Viewport_Display_Particle_Markers);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Display_Particles_TT"));

        ImGui.Checkbox($"{LOC.Get("FX_PreviewTool_Display_Shapes")}##fxShowShapes", ref CFG.Current.FxEditor_Viewport_Display_Emitter_Shapes);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Display_Shapes_TT"));

        ImGui.Checkbox($"{LOC.Get("FX_PreviewTool_Display_Axes")}##fxShowAxes", ref CFG.Current.FxEditor_Viewport_Display_Node_Axes);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Display_Axes_TT"));

        if (fxr != null)
            GUI.WrappedText(LOC.Get("FX_PreviewTool_Display_Count", Editor.ViewportWindow.ShownNodes, Editor.ViewportWindow.ShownShapes));

        // Brightness
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_PreviewTool_Brightness_Header"), LOC.Get("FX_PreviewTool_Brightness_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##fxBillboardBrightness", ref CFG.Current.FxEditor_Viewport_Billboard_Brightness, 0.01f, 10f, "%.2fx", ImGuiSliderFlags.Logarithmic);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Brightness_TT"));

        // Billboards drawn, and textures no binder holds
        var billboards = Editor.ViewportWindow.Billboards;
        if (billboards != null && fxr != null)
            GUI.WrappedText(LOC.Get("FX_PreviewTool_Billboard_Count", billboards.DrawnBillboards, billboards.BatchCount));

        var missing = Editor.ViewportWindow.Textures?.MissingTextures;
        if (missing is { Count: > 0 })
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("FX_PreviewTool_Missing_Textures", string.Join(", ", missing.Take(8)), missing.Count));
        }
    }

    /// <summary>What the runtime is doing: the effect's state, its particles and nodes, and whether it has ended.</summary>
    private void DisplayStatus()
    {
        var runtime = Editor.ViewportWindow.Runtime;

        if (runtime.Error != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("FX_PreviewTool_Status_Error", runtime.Error));
            return;
        }

        var instance = runtime.Instance;
        if (instance == null)
        {
            GUI.WrappedText(LOC.Get("FX_PreviewTool_Status_None"));
            return;
        }

        var state = instance.Finished ? LOC.Get("FX_PreviewTool_Status_Finished")
            : instance.Terminated ? LOC.Get("FX_PreviewTool_Status_Terminated")
            : LOC.Get("FX_PreviewTool_Status_State", instance.State);
        GUI.WrappedText(state);

        var context = runtime.Context;
        GUI.WrappedText(LOC.Get("FX_PreviewTool_Status_Counts", context.ParticleCount, context.NodeCount));

        if (context.ParticleCapHits > 0 || context.NodeCapHits > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_PreviewTool_Status_Limit", context.MaxParticles, context.MaxNodes));
        if (context.ProxyMissing > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_PreviewTool_Status_Proxy_Missing"));

        // Ends the effect as reaching state -1 does; Restart starts it again
        GUI.ConditionalMultiButtonInput("fxRuntimeButtons",
            "fxTerminate", LOC.Get("FX_PreviewTool_Terminate"), LOC.Get("FX_PreviewTool_Terminate_TT"),
            () => runtime.Instance?.Terminate(), !instance.Terminated);
    }

    private void DisplayExternalValues(FxPlayback playback)
    {
        int remove = -1;

        if (ImGui.BeginTable("fxExternalValues", 3, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Remove", ImGuiTableColumnFlags.WidthFixed);

            foreach (var (id, current) in playback.ExternalValues.ToList())
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text(ExternalName(id));
                GUI.Tooltip(LOC.Get("FX_PreviewTool_External_Id_TT", id));

                ImGui.TableSetColumnIndex(1);
                float value = current;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.DragFloat($"##fxExternal_{id}", ref value, 0.05f))
                    playback.SetExternalValue(id, value);
                GUI.Tooltip(LOC.Get("FX_PreviewTool_External_Value_TT"));

                ImGui.TableSetColumnIndex(2);
                if (ImGui.Button($"{Icons.Minus}##fxExternalRemove_{id}"))
                    remove = id;
                GUI.Tooltip(LOC.Get("FX_PreviewTool_External_Remove_TT"));
            }

            ImGui.EndTable();
        }

        if (remove >= 0)
            playback.RemoveExternalValue(remove);

        // Add one by id
        ImGui.SetNextItemWidth(-ImGui.GetFrameHeight() - ImGui.GetStyle().ItemSpacing.X);
        ImGui.InputInt("##fxExternalNewId", ref NewExternalId);
        NewExternalId = Math.Max(NewExternalId, 0);
        GUI.Tooltip(LOC.Get("FX_PreviewTool_External_New_TT"));

        ImGui.SameLine();
        bool exists = playback.ExternalValues.ContainsKey(NewExternalId);
        ImGui.BeginDisabled(exists);
        if (ImGui.Button($"{Icons.Plus}##fxExternalAdd"))
            playback.SetExternalValue(NewExternalId, 0);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("FX_PreviewTool_External_Add_TT"));
    }

    /// <summary>The known external value ids by name (fxr-main's list), others by number.</summary>
    private static string ExternalName(int id) => id switch
    {
        0 => LOC.Get("FX_External_Terminate"),
        1 => LOC.Get("FX_External_Precipitation"),
        2 => LOC.Get("FX_External_TimeOfDay"),
        10000 => LOC.Get("FX_External_Blood"),
        _ => LOC.Get("FX_External_Id", id),
    };
}
