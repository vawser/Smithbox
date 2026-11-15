#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The selected event: its resource, timing, parameters and transform keys, and its timeline's parameters. With no
/// event selected, what the cut was built from: actors and the state of their models, cameras, placements, problems.
/// Read-only for now.
/// </summary>
public class CutscenePropertiesView
{
    private readonly CutsceneEditorScreen Editor;

    /// <summary>What parameters' values name, shown under them</summary>
    private readonly CutsceneParameterLinks Links;

    public CutscenePropertiesView(CutsceneEditorScreen editor)
    {
        Editor = editor;
        Links = new CutsceneParameterLinks(editor);
    }

    public void Display()
    {
        var mqb = Editor.SelectedFile?.Mqb;
        var scene = Editor.Scene;
        if (mqb == null || scene == null)
            return;

        if (Editor.SelectedPlacement is { } placement)
            DisplayPlacementEditor(placement);

        if (Editor.SelectedEvent is { } e)
        {
            if (ImGui.Button($"{LOC.Get("CUTSCENE_Properties_Back")}##cutsceneDeselect"))
            {
                Editor.SelectedEvent = null;
                Editor.SelectedTimeline = -1;
                Editor.SelectedPlacement = null;
                return;
            }

            DisplayEvent(mqb, scene, e);
        }
        else
        {
            DisplayScene(scene);
        }
    }

    private void DisplayEvent(MQB mqb, CutsceneScene scene, MQB.Event e)
    {
        var resource = e.ResourceIndex >= 0 && e.ResourceIndex < mqb.Resources.Count ? mqb.Resources[e.ResourceIndex] : null;

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Event"));
        Row(LOC.Get("CUTSCENE_Properties_Event_Id"), e.ID.ToString());

        // Start and length, in frames of the cut; the length carries Unk18/Unk1C along where they follow it
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Start"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        // Each value is taken before its widget, whose +/- buttons activate and edit in the same frame
        var startBefore = e.StartFrame;
        var start = e.StartFrame;
        if (ImGui.InputInt("##cutsceneEventStart", ref start))
            e.StartFrame = Math.Max(0, start);
        TrackEdit(("start", e), startBefore, () => e.StartFrame, x => e.StartFrame = x);

        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Duration"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        var durationBefore = (e.Duration, e.Unk18);
        var duration = e.Duration;
        if (ImGui.InputInt("##cutsceneEventDuration", ref duration))
            CutsceneMqbOps.SetDuration(e, Math.Max(1, duration));
        TrackEdit(("duration", e), durationBefore, () => (e.Duration, e.Unk18),
            x => { e.Duration = x.Item1; e.Unk18 = x.Item2; e.Unk1C = x.Item2; });
        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Frames_End", e.StartFrame + e.Duration));

        if (ImGui.TreeNode($"{LOC.Get("CUTSCENE_Properties_Advanced")}##cutsceneAdvanced"))
        {
            ImGui.TextDisabled("Unk18 / Unk1C");
            ImGui.SameLine(140 * DPI.UIScale());
            ImGui.SetNextItemWidth(120 * DPI.UIScale());
            var unkBefore = e.Unk18;
            var unk = e.Unk18;
            if (ImGui.InputInt("##cutsceneEventUnk18", ref unk))
            {
                e.Unk18 = unk;
                e.Unk1C = unk;
            }
            TrackEdit(("unk18", e), unkBefore, () => e.Unk18, x => { e.Unk18 = x; e.Unk1C = x; });
            GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Unk18_TT"));
            ImGui.TreePop();
        }

        if (resource != null)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Resource"));

            // Models, cameras and the HKT are found in the Havok file by name, so only other resources can be renamed
            if (CutsceneMqbOps.CanTemplate(resource.Type))
            {
                ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Name"));
                ImGui.SameLine(140 * DPI.UIScale());
                ImGui.SetNextItemWidth(-1);
                var name = resource.Name;
                if (ImGui.InputText("##cutsceneResourceName", ref name, 31))
                    resource.Name = name;
                TrackEdit(("name", resource), resource.Name, () => resource.Name, x => resource.Name = x);
            }
            else
            {
                Row(LOC.Get("CUTSCENE_Properties_Name"), resource.Name);
            }
            Row(LOC.Get("CUTSCENE_Properties_Type"), $"{CutsceneResourceTypes.Name(resource.Type)} ({resource.Type})");
            Row(LOC.Get("CUTSCENE_Properties_Index"), e.ResourceIndex.ToString());

            if (resource.ParentIndex >= 0 && resource.ParentIndex < mqb.Resources.Count)
                Row(LOC.Get("CUTSCENE_Properties_Parent"), $"{mqb.Resources[resource.ParentIndex].Name} ({resource.ParentIndex})");

            if (!string.IsNullOrEmpty(resource.Path))
                Row(LOC.Get("CUTSCENE_Properties_Path"), resource.Path);

            DisplayParameters("resource", resource.Parameters);

            // What the viewport made of it
            if (scene.Actors.FirstOrDefault(a => a.ResourceIndex == e.ResourceIndex) is { } actor)
            {
                Row(LOC.Get("CUTSCENE_Properties_Model"), actor.Model?.ModelName ?? LOC.Get("CUTSCENE_Properties_None"));
                Row(LOC.Get("CUTSCENE_Properties_State"), $"{actor.State}{(actor.Status != null ? $": {actor.Status}" : "")}");
                Row(LOC.Get("CUTSCENE_Properties_Bones"), actor.Skeleton?.BoneCount.ToString() ?? LOC.Get("CUTSCENE_Properties_None"));

                // Who it is: its map part (where its masks and look come from), or the player
                if (actor.Cast is { } cast)
                {
                    Row(LOC.Get("CUTSCENE_Properties_Map_Part"), $"{cast.Map}: {cast.PartName}");
                    Row(LOC.Get("CUTSCENE_Properties_Npc_Param"), cast.NpcParamId > 0
                        ? $"{cast.NpcParamId}{(actor.Masks != null ? LOC.Get("CUTSCENE_Properties_Masks", actor.Masks.Count(m => m != 0)) : "")}"
                        : LOC.Get("CUTSCENE_Properties_None"));
                    if (actor.IsComposite)
                        Row(LOC.Get("CUTSCENE_Properties_Chara_Init"), cast.CharaInitId >= 0 ? cast.CharaInitId.ToString() : LOC.Get("CUTSCENE_Properties_None"));
                }
                else if (actor.IsPlayer)
                {
                    Row(LOC.Get("CUTSCENE_Properties_Map_Part"), LOC.Get("CUTSCENE_Properties_Player"));
                }
                else if (actor.Model?.Kind == CutsceneModelKind.Character)
                {
                    Row(LOC.Get("CUTSCENE_Properties_Map_Part"), LOC.Get("CUTSCENE_Properties_No_Map_Part"));
                }
            }

            if (scene.Cameras.FirstOrDefault(c => c.ResourceIndex == e.ResourceIndex) is { } camera)
                DisplayCameraClip(camera);
        }

        DisplayParameters("event", e.Parameters, true, e);

        if (e.Transforms.Count > 0)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Transforms", e.Transforms.Count));
            foreach (var t in e.Transforms.Take(16))
                ImGui.TextWrapped($"f{t.Frame:0}: {Vec(t.Translation)}  rot {Vec(t.Rotation)}  scale {Vec(t.Scale)}");
        }

        if (Editor.SelectedTimeline >= 0 && Editor.SelectedTimeline < scene.Cut.Timelines.Count)
        {
            var timeline = scene.Cut.Timelines[Editor.SelectedTimeline];
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Timeline", Editor.SelectedTimeline, timeline.ParentIndex));
            DisplayParameters("timeline", timeline.Parameters, false);
            Links.DisplayTimelineMap("timeline", timeline.Parameters);
        }
    }

    // ---- camera keys (camera authoring): the mode, the keys, and setting them from the view

    private CutsceneCamera? KeysCamera;
    // Kept by the screen, where the viewport's key gizmo reads it
    private int SelectedKey
    {
        get => Editor.SelectedCameraKey;
        set => Editor.SelectedCameraKey = value;
    }

    private int ConvertEvery = 15;
    private CutsceneCameraKeys? KeysEditStart;

    private void DisplayCameraClip(CutsceneCamera camera)
    {
        ImGui.SeparatorText(LOC.Get("CUTSCENE_Camera_Clip"));

        if (KeysCamera != camera)
        {
            KeysCamera = camera;
            SelectedKey = -1;
        }

        var frame = Editor.Playback.Frame;
        var clip = camera.Clip;
        if (clip == null || Editor.SelectedFile is not { } file)
        {
            ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_None"));
            return;
        }

        var kind = clip is StudioCore.Renderer.HavokSplineClip ? LOC.Get("CUTSCENE_Camera_Format_Spline") : LOC.Get("CUTSCENE_Camera_Format_Interleaved");
        var fov = camera.FovRadians(frame);
        ImGui.TextWrapped(LOC.Get("CUTSCENE_Camera_Clip_Summary", kind, clip.FrameCount,
            fov.HasValue ? (fov.Value * 180f / MathF.PI).ToString("0.##") : "-"));

        var keys = camera.Keys;
        var mode = (int)(keys?.Mode ?? CutsceneCameraMode.Vanilla);

        // Mode and format: each change is one undo step
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Keys_Mode"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(-1);
        string[] modes = [LOC.Get("CUTSCENE_Keys_Mode_Vanilla"), LOC.Get("CUTSCENE_Keys_Mode_Offset"), LOC.Get("CUTSCENE_Keys_Mode_Keyed")];
        if (ImGui.Combo("##cutsceneKeysMode", ref mode, modes, modes.Length))
        {
            var after = keys?.Clone() ?? new CutsceneCameraKeys();
            if (after.Mode != (CutsceneCameraMode)mode)
            {
                // The keys of one mode mean nothing in the other
                if (after.Mode != CutsceneCameraMode.Vanilla && (CutsceneCameraMode)mode != CutsceneCameraMode.Vanilla)
                    after.Keys.Clear();
                after.Mode = (CutsceneCameraMode)mode;
                Commit(file, camera, keys, after);
                SelectedKey = -1;
            }
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Keys_Mode_TT"));

        if ((CutsceneCameraMode)mode == CutsceneCameraMode.Offset && !file.HasVanilla(Editor.Project))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("CUTSCENE_Keys_No_Vanilla"));

        var format = (int)(keys?.Format ?? StudioCore.Utilities.HavokClipFormat.Auto);
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Camera_Format"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(-1);
        string[] formats = [LOC.Get("CUTSCENE_Camera_Format_Auto"), LOC.Get("CUTSCENE_Camera_Format_Spline"), LOC.Get("CUTSCENE_Camera_Format_Interleaved")];
        if (ImGui.Combo("##cutsceneKeysFormat", ref format, formats, formats.Length))
        {
            var after = keys?.Clone() ?? new CutsceneCameraKeys();
            after.Format = (StudioCore.Utilities.HavokClipFormat)format;
            Commit(file, camera, keys, after);
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Camera_Format_TT"));

        // Convert: the camera as it plays now, as keys every N frames
        ImGui.SetNextItemWidth(80 * DPI.UIScale());
        ImGui.InputInt("##cutsceneKeysEvery", ref ConvertEvery);
        ConvertEvery = Math.Clamp(ConvertEvery, 1, 1000);
        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("CUTSCENE_Keys_Convert")}##cutsceneKeysConvert"))
        {
            var after = keys?.Clone() ?? new CutsceneCameraKeys();
            after.Mode = CutsceneCameraMode.Keyed;
            after.Keys.Clear();
            var last = camera.FrameCount - 1;
            for (var f = 0; ; f = Math.Min(f + ConvertEvery, last))
            {
                var (bone, fovNow) = camera.Pose(f);
                after.Keys.Add(after.KeyFor(f, bone, fovNow, camera.BaseBone(f), camera.BaseFov(f), after.Keys.LastOrDefault()));
                if (f >= last)
                    break;
            }

            Commit(file, camera, keys, after);
            SelectedKey = -1;
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Keys_Convert_TT"));

        if (keys is not { Applies: true })
        {
            ImGui.TextWrapped(LOC.Get("CUTSCENE_Keys_Vanilla_Hint"));
            return;
        }

        // Adding keys
        var playhead = (int)MathF.Round(frame);
        if (ImGui.Button($"{LOC.Get("CUTSCENE_Keys_Add")}##cutsceneKeysAdd"))
        {
            var (bone, fovNow) = camera.Pose(playhead);
            AddKey(file, camera, keys, keys.KeyFor(playhead, bone, fovNow, camera.BaseBone(playhead), camera.BaseFov(playhead), keys.Nearest(playhead)));
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Keys_Add_TT"));

        ImGui.SameLine();
        var canView = Editor.ViewportWindow.TryGetFreeView(out var eye, out var viewForward, out var viewUp, out var viewFov);
        ImGui.BeginDisabled(!canView);
        if (ImGui.Button($"{LOC.Get("CUTSCENE_Keys_From_View")}##cutsceneKeysView") && Editor.Scene is { } scene)
        {
            // The view in the cut's own space, where the camera bone is
            Matrix4x4.Invert(scene.RootOf(camera.Resource, playhead), out var toCut);
            var current = camera.Pose(playhead);
            var bone = current.Bone;
            bone.Translation = Vector3.Transform(eye, toCut);
            bone.Rotation = CutsceneCameraRotation.FromView(Vector3.Normalize(Vector3.TransformNormal(viewForward, toCut)),
                Vector3.Normalize(Vector3.TransformNormal(viewUp, toCut)));
            // The viewport's FOV too, so the key frames what the view shows
            AddKey(file, camera, keys, keys.KeyFor(playhead, bone, viewFov, camera.BaseBone(playhead), camera.BaseFov(playhead), keys.Nearest(playhead)));
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get(canView ? "CUTSCENE_Keys_From_View_TT" : "CUTSCENE_Keys_From_View_Disabled_TT"));

        ImGui.SameLine();
        ImGui.BeginDisabled(SelectedKey < 0 || SelectedKey >= keys.Keys.Count);
        if (ImGui.Button($"{LOC.Get("CUTSCENE_Keys_Delete")}##cutsceneKeysDelete"))
        {
            var after = keys.Clone();
            after.Keys.RemoveAt(SelectedKey);
            Commit(file, camera, keys, after);
            SelectedKey = Math.Min(SelectedKey, after.Keys.Count - 1);
        }
        ImGui.EndDisabled();

        // The keys: a click selects one and puts the playhead on it
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Keys_Count", keys.Keys.Count));
        if (ImGui.BeginTable("##cutsceneKeysTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY,
                new Vector2(0, Math.Min(8, Math.Max(2, keys.Keys.Count + 1)) * ImGui.GetTextLineHeightWithSpacing())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Keys_Frame"), ImGuiTableColumnFlags.WidthFixed, 60 * DPI.UIScale());
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Keys_Values"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Keys_Interpolation"), ImGuiTableColumnFlags.WidthFixed, 70 * DPI.UIScale());
            ImGui.TableHeadersRow();

            for (var i = 0; i < keys.Keys.Count; i++)
            {
                var k = keys.Keys[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{k.Frame}##cutsceneKey{i}", i == SelectedKey, ImGuiSelectableFlags.SpanAllColumns))
                {
                    SelectedKey = i;
                    Editor.Playback.Frame = k.Frame;
                }
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{Vec(k.Position)}  {Vec(k.Rotation)}°  {k.FovDegrees:0.##}°");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(InterpolationName(k.Interpolation));
            }

            ImGui.EndTable();
        }

        if (SelectedKey >= 0 && SelectedKey < keys.Keys.Count)
            DisplayKey(file, camera, keys, keys.Keys[SelectedKey]);
    }

    private void DisplayKey(CutsceneFile file, CutsceneCamera camera, CutsceneCameraKeys keys, CutsceneCameraKey key)
    {
        var offset = keys.Mode == CutsceneCameraMode.Offset;
        ImGui.SeparatorText(LOC.Get(offset ? "CUTSCENE_Keys_Key_Offset" : "CUTSCENE_Keys_Key", key.Frame));

        // Edited live, so the viewport follows a drag; one undo step when let go
        var changed = false;

        ImGui.TextDisabled(LOC.Get("CUTSCENE_Keys_Frame"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        var keyFrame = key.Frame;
        StartKeysEdit(keys);
        if (ImGui.InputInt("##cutsceneKeyFrame", ref keyFrame))
        {
            key.Frame = Math.Clamp(keyFrame, 0, camera.FrameCount - 1);
            changed = true;
        }
        EndKeysEdit(file, camera, keys, sort: true);

        ImGui.TextDisabled(LOC.Get("CUTSCENE_Keys_Position"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(-1);
        var position = key.Position;
        StartKeysEdit(keys);
        if (ImGui.DragFloat3("##cutsceneKeyPosition", ref position, 0.01f))
        {
            key.Position = position;
            changed = true;
        }
        EndKeysEdit(file, camera, keys);
        GUI.Tooltip(LOC.Get(offset ? "CUTSCENE_Keys_Position_Offset_TT" : "CUTSCENE_Keys_Position_TT"));

        ImGui.TextDisabled(LOC.Get("CUTSCENE_Keys_Rotation"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(-1);
        var rotation = key.Rotation;
        StartKeysEdit(keys);
        if (ImGui.DragFloat3("##cutsceneKeyRotation", ref rotation, 0.1f))
        {
            key.Rotation = rotation;
            changed = true;
        }
        EndKeysEdit(file, camera, keys);
        GUI.Tooltip(LOC.Get(offset ? "CUTSCENE_Keys_Rotation_Offset_TT" : "CUTSCENE_Keys_Rotation_TT"));

        ImGui.TextDisabled(LOC.Get(offset ? "CUTSCENE_Keys_Fov_Offset" : "CUTSCENE_Keys_Fov"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        var keyFov = key.FovDegrees;
        StartKeysEdit(keys);
        if (ImGui.DragFloat("##cutsceneKeyFov", ref keyFov, 0.1f, offset ? -90 : 1, offset ? 90 : 170))
        {
            key.FovDegrees = keyFov;
            changed = true;
        }
        EndKeysEdit(file, camera, keys);

        ImGui.TextDisabled(LOC.Get("CUTSCENE_Keys_Interpolation"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(-1);
        var interpolation = (int)key.Interpolation;
        string[] names = [InterpolationName(CutsceneKeyInterpolation.Smooth), InterpolationName(CutsceneKeyInterpolation.Linear), InterpolationName(CutsceneKeyInterpolation.Hold)];
        if (ImGui.Combo("##cutsceneKeyInterpolation", ref interpolation, names, names.Length))
        {
            var after = keys.Clone();
            after.Keys[keys.Keys.IndexOf(key)].Interpolation = (CutsceneKeyInterpolation)interpolation;
            Commit(file, camera, keys, after);
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Keys_Interpolation_TT"));

        if (changed)
            Editor.ViewportWindow.InvalidateLines();

        if (file.IsDirty)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("CUTSCENE_Properties_Unsaved"));
    }

    // Before a key widget: its keys as they were when it was activated
    private void StartKeysEdit(CutsceneCameraKeys keys)
    {
        // Taken before the widget, whose activation shows only after it; kept for the one that activates
        PendingStart = keys.Clone();
    }

    private CutsceneCameraKeys? PendingStart;

    // After a key widget: when it is let go after an edit, one undo step from where it started
    private void EndKeysEdit(CutsceneFile file, CutsceneCamera camera, CutsceneCameraKeys keys, bool sort = false)
    {
        if (ImGui.IsItemActivated())
            KeysEditStart = PendingStart;

        if (!ImGui.IsItemDeactivatedAfterEdit() || KeysEditStart is not { } start)
            return;

        var after = keys.Clone();
        var selected = SelectedKey >= 0 && SelectedKey < after.Keys.Count ? after.Keys[SelectedKey] : null;
        if (sort)
            after.Sort();

        KeysEditStart = null;
        if (!start.SameAs(after))
        {
            Commit(file, camera, start, after);
            SelectedKey = selected != null ? after.Keys.IndexOf(selected) : -1;
        }
    }

    private void AddKey(CutsceneFile file, CutsceneCamera camera, CutsceneCameraKeys keys, CutsceneCameraKey key)
    {
        var after = keys.Clone();
        after.Keys.Add(key);
        after.Sort();
        Commit(file, camera, keys, after);
        SelectedKey = after.Keys.FindIndex(k => k.Frame == key.Frame);
    }

    private void Commit(CutsceneFile file, CutsceneCamera camera, CutsceneCameraKeys? before, CutsceneCameraKeys after)
    {
        if (string.IsNullOrEmpty(camera.KeyId))
            return;

        Editor.Execute(new CutsceneCameraKeysAction(file, camera.KeyId, before, after));
    }

    private static string InterpolationName(CutsceneKeyInterpolation interpolation) =>
        LOC.Get($"CUTSCENE_Keys_Interpolation_{interpolation}");
    // ---- editing: a field's value when it started being edited, by what it edits, so an edit is one undo step

    private readonly Dictionary<object, object?> EditStart = new();

    /// <summary>
    /// After a widget that edits a value live: remembers the value it had when the widget was activated, and when it
    /// is let go after an edit, records the change as one undo step.
    /// </summary>
    private void TrackEdit<T>(object key, T before, Func<T> get, Action<T> set)
    {
        if (ImGui.IsItemActivated())
            EditStart[key] = before;

        if (ImGui.IsItemDeactivatedAfterEdit() && Editor.SelectedFile is { } file)
        {
            var start = EditStart.TryGetValue(key, out var s) && s is T t ? t : before;
            EditStart.Remove(key);

            var after = get();
            if (!Equals(start, after))
                Editor.Execute(new CutsceneEditAction(file, () => set(after), () => set(start)));
        }
    }

    private void DisplayParameters(string id, List<MQB.Parameter> parameters, bool header = true, MQB.Event? owner = null)
    {
        if (parameters.Count == 0)
            return;

        if (header)
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Parameters", parameters.Count));

        if (!ImGui.BeginTable($"##cutsceneParams_{id}", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            return;

        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Name"));
        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Type"), ImGuiTableColumnFlags.WidthFixed, 70 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Value"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (var i = 0; i < parameters.Count; i++)
        {
            var p = parameters[i];
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(p.Name);
            ImGui.TableNextColumn();
            ImGui.TextDisabled(p.MemberCount > 1 ? $"{p.Type}{p.MemberCount}" : p.Type.ToString());
            ImGui.TableNextColumn();

            ImGui.SetNextItemWidth(-1);
            EditValue($"##{id}_p{i}", p, () => p.Value, v => p.Value = v);
            Links.Display($"{id}_p{i}", parameters, i);

            if (p.Sequences.Count > 0)
                DisplaySequences($"{id}_p{i}", p, owner);
        }

        ImGui.EndTable();
    }

    /// <summary>A widget for a value of the parameter's type, which edits it live; one undo step per edit.</summary>
    private void EditValue(string id, object key, Func<object> get, Action<object> set)
    {
        var value = get();
        switch (value)
        {
            case bool b:
            {
                var v = b;
                if (ImGui.Checkbox(id, ref v)) set(v);
                TrackEdit<object>(key, b, get, set);
                break;
            }
            case sbyte sb:
                IntField(id, key, sb, sbyte.MinValue, sbyte.MaxValue, x => (sbyte)x, get, set);
                break;
            case byte by:
                IntField(id, key, by, byte.MinValue, byte.MaxValue, x => (byte)x, get, set);
                break;
            case short sh:
                IntField(id, key, sh, short.MinValue, short.MaxValue, x => (short)x, get, set);
                break;
            case int n:
                IntField(id, key, n, int.MinValue, int.MaxValue, x => (int)x, get, set);
                break;
            case uint u:
            {
                // Can be past int's range (event flags, colours as ARGB), so it is typed as text
                var text = u.ToString();
                if (ImGui.InputText(id, ref text, 12, ImGuiInputTextFlags.CharsDecimal) && uint.TryParse(text, out var parsed))
                    set(parsed);
                TrackEdit<object>(key, u, get, set);
                break;
            }
            case float f:
            {
                var v = f;
                if (ImGui.DragFloat(id, ref v, MathF.Max(0.001f, MathF.Abs(f) * 0.01f), 0, 0, "%.4f")) set(v);
                TrackEdit<object>(key, f, get, set);
                break;
            }
            case string s:
            {
                var v = s;
                if (ImGui.InputText(id, ref v, 256)) set(v);
                TrackEdit<object>(key, s, get, set);
                break;
            }
            case System.Drawing.Color c:
            // Color and IntColor both read into a Color, with channels of 0-255; the alpha (IntColor's fourth) is kept
            {
                var v = new Vector3(c.R, c.G, c.B) / 255f;
                if (ImGui.ColorEdit3(id, ref v))
                    set(System.Drawing.Color.FromArgb(c.A, (int)(v.X * 255 + 0.5f), (int)(v.Y * 255 + 0.5f), (int)(v.Z * 255 + 0.5f)));
                TrackEdit<object>(key, c, get, set);
                break;
            }
            case Vector2 v2:
            {
                var v = v2;
                if (ImGui.DragFloat2(id, ref v, 0.01f)) set(v);
                TrackEdit<object>(key, v2, get, set);
                break;
            }
            case Vector3 v3:
            {
                var v = v3;
                if (ImGui.DragFloat3(id, ref v, 0.01f)) set(v);
                TrackEdit<object>(key, v3, get, set);
                break;
            }
            case Vector4 v4:
            {
                var v = v4;
                if (ImGui.DragFloat4(id, ref v, 0.01f)) set(v);
                TrackEdit<object>(key, v4, get, set);
                break;
            }
            default:
                ImGui.TextDisabled(ValueText(value));
                break;
        }
    }

    private void IntField(string id, object key, long current, long min, long max, Func<long, object> make, Func<object> get, Action<object> set)
    {
        // Taken before the widget: its +/- buttons activate and edit in the same frame
        var before = get();
        var v = (int)Math.Clamp(current, int.MinValue, int.MaxValue);
        if (ImGui.InputInt(id, ref v))
            set(make(Math.Clamp(v, min, max)));
        TrackEdit(key, before, get, set);
    }

    /// <summary>
    /// The keys of an animated parameter: a frame (from the event's start) and a value each. Keys are added at the
    /// playhead, and taken away with their button; each is one undo step.
    /// </summary>
    private void DisplaySequences(string id, MQB.Parameter p, MQB.Event? owner)
    {
        if (!ImGui.TreeNode($"{LOC.Get("CUTSCENE_Properties_Animated", p.Sequences.Sum(s => s.Points.Count))}##{id}_seq"))
            return;

        for (var s = 0; s < p.Sequences.Count; s++)
        {
            var sequence = p.Sequences[s];
            if (p.Sequences.Count > 1)
                ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Sequence", s, sequence.ValueIndex));

            for (var k = 0; k < sequence.Points.Count; k++)
            {
                var point = sequence.Points[k];

                var frameBefore = point.Unk08;
                var frame = point.Unk08;
                ImGui.SetNextItemWidth(70 * DPI.UIScale());
                if (ImGui.InputInt($"##{id}_s{s}_k{k}_f", ref frame))
                    point.Unk08 = Math.Max(0, frame);
                TrackEdit($"{id}_s{s}_k{k}_f", frameBefore, () => point.Unk08, x => point.Unk08 = x);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(110 * DPI.UIScale());
                EditValue($"##{id}_s{s}_k{k}_v", point, () => point.Value, x => point.Value = x);

                ImGui.SameLine();
                if (ImGui.SmallButton($"x##{id}_s{s}_k{k}_del") && Editor.SelectedFile is { } file && sequence.Points.Count > 1)
                {
                    var index = k;
                    Editor.Execute(new CutsceneEditAction(file,
                        () => sequence.Points.Remove(point),
                        () => sequence.Points.Insert(Math.Min(index, sequence.Points.Count), point)));
                    break;
                }
                GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Key_Delete_TT"));
            }

            if (ImGui.SmallButton($"{LOC.Get("CUTSCENE_Properties_Key_Add")}##{id}_s{s}_add") && Editor.SelectedFile is { } addFile && sequence.Points.Count > 0)
            {
                // At the playhead, from the event's start, with the value of the key before it
                var at = Math.Max(0, (int)Editor.Playback.Frame - (owner?.StartFrame ?? 0));
                var before = sequence.Points.LastOrDefault(pt => pt.Unk08 <= at) ?? sequence.Points[0];
                var key = new MQB.Parameter.Sequence.Point { Value = before.Value, Unk08 = at, Unk10 = before.Unk10, Unk14 = before.Unk14 };
                var index = sequence.Points.Count(pt => pt.Unk08 <= at);

                Editor.Execute(new CutsceneEditAction(addFile,
                    () => sequence.Points.Insert(Math.Min(index, sequence.Points.Count), key),
                    () => sequence.Points.Remove(key)));
            }
            GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Key_Add_TT"));
        }

        ImGui.TreePop();
    }

    private void DisplayScene(CutsceneScene scene)
    {
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Hint"));

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Cut", scene.Cut.Name, scene.FrameCount));

        foreach (var problem in scene.Problems)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, problem);

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Cameras", scene.Cameras.Count));
        foreach (var c in scene.Cameras)
        {
            var range = c.Events.Count > 0 ? $"{c.Events.Min(e => e.StartFrame)}-{c.Events.Max(e => e.StartFrame + e.Duration)}" : "-";
            ImGui.Text($"{c.Name}  f{range}  fov {(c.FovRadians(Editor.Playback.Frame) is { } f ? $"{f * 180 / MathF.PI:0.0}°" : "-")}  near {c.NearPlane:0.###} far {c.FarPlane:0}");
        }

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Actors", scene.Actors.Count));
        if (ImGui.BeginTable("##cutsceneActors", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Name"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Model"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Bones"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_State"));
            ImGui.TableHeadersRow();

            foreach (var a in scene.Actors)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(a.Name);
                ImGui.TableNextColumn();
                ImGui.Text(a.Model?.ModelName ?? "-");
                ImGui.TableNextColumn();
                // Bones of the clip, and once loaded, how many of the model's nodes follow one of them
                ImGui.Text(a.State == CutsceneActorState.Loaded
                    ? $"{a.Skeleton?.BoneCount ?? 0} ({a.MappedNodeCount}/{a.ModelNodeCount})"
                    : a.Skeleton?.BoneCount.ToString() ?? "-");
                ImGui.TableNextColumn();
                if (a.State == CutsceneActorState.Failed)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, a.Status ?? a.State.ToString());
                else
                    ImGui.Text(a.State.ToString());
            }

            ImGui.EndTable();
        }

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Placements", scene.Placements.Count));
        foreach (var p in scene.Placements.Values)
        {
            var m = p.At(Editor.Playback.Frame);

            if (p.OffsetEvent != null)
            {
                if (ImGui.SmallButton($"{LOC.Get("CUTSCENE_Properties_Move")}##cutsceneMove{p.HktResourceIndex}"))
                    Editor.SelectedPlacement = p;
                GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Move_TT"));
                ImGui.SameLine();
            }

            ImGui.Text($"{p.HktName}: {Vec(m.Translation)}{(p.Keys.Count == 0 ? $"  ({LOC.Get("CUTSCENE_Properties_Origin")})" : "")}");
        }
    }

    // The keys of the placement as they were when a drag field started being edited, for one undo step per edit
    private (Vector3, Vector3)[]? FieldEditStart;
    private CutscenePlacement? FieldEditPlacement;

    /// <summary>
    /// The selected placement's position and rotation, as its first key has them. An edit moves every key by as much,
    /// and becomes one undo step when the field is let go. The viewport's gizmo moves it too.
    /// </summary>
    private void DisplayPlacementEditor(CutscenePlacement placement)
    {
        if (placement.OffsetEvent == null || placement.Keys.Count == 0 || Editor.SelectedFile is not { } file)
            return;

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Placement", placement.HktName));
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Placement_Hint"));

        var first = placement.Keys[0];
        var translation = first.Translation;
        var rotation = first.Rotation;

        var changed = false;
        var t = translation;
        var r = rotation;

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.DragFloat3("##cutscenePlacementPos", ref t, 0.01f);
        var activated = ImGui.IsItemActivated();
        var deactivated = ImGui.IsItemDeactivatedAfterEdit();

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.DragFloat3("##cutscenePlacementRot", ref r, 0.25f);
        activated |= ImGui.IsItemActivated();
        deactivated |= ImGui.IsItemDeactivatedAfterEdit();
        GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Placement_Fields_TT"));

        if (activated || FieldEditPlacement != placement)
        {
            FieldEditStart = CutscenePlacementAction.Snapshot(placement.Keys);
            FieldEditPlacement = placement;
        }

        // Live while dragging, from the keys as they were when the drag started
        if (changed && FieldEditStart != null)
        {
            var dt = t - FieldEditStart[0].Item1;
            var dr = r - FieldEditStart[0].Item2;
            CutscenePlacementAction.Apply(placement.Keys, CutscenePlacementAction.Shifted(FieldEditStart, dt, dr));
        }

        if (deactivated && FieldEditStart != null)
        {
            var after = CutscenePlacementAction.Snapshot(placement.Keys);
            Editor.ActionManager.ExecuteAction(new CutscenePlacementAction(file, placement.Keys, FieldEditStart, after));
            FieldEditStart = CutscenePlacementAction.Snapshot(placement.Keys);
        }

        if (file.IsDirty)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("CUTSCENE_Properties_Unsaved"));

        if (ImGui.Button($"{LOC.Get("CUTSCENE_Properties_Placement_Done")}##cutscenePlacementDone"))
            Editor.SelectedPlacement = null;
    }

    private static void Row(string label, string value)
    {
        ImGui.TextDisabled(label);
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.TextWrapped(value);
    }

    private static string Vec(Vector3 v) => $"({v.X:0.###}, {v.Y:0.###}, {v.Z:0.###})";

    private static string ValueText(object? v) => v switch
    {
        null => "null",
        float f => f.ToString("0.####"),
        Vector2 v2 => $"({v2.X:0.###}, {v2.Y:0.###})",
        Vector3 v3 => Vec(v3),
        Vector4 v4 => $"({v4.X:0.###}, {v4.Y:0.###}, {v4.Z:0.###}, {v4.W:0.###})",
        string s => $"\"{s}\"",
        _ => v.ToString() ?? ""
    };
}
