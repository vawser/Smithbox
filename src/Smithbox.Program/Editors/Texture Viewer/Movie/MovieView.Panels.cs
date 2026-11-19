using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public partial class MovieView
{
    private string TreeFilter = "";
    private bool TreeFilterExact = false;

    #region Field buffers
    // A field keeps its own value while it is being edited, and is applied when it is left (GUI.InputCommitted)
    private readonly Dictionary<string, object> Buffers = new();
    private string ActiveField;
    private string BufferedFor;

    private void ResetBuffersFor(string key)
    {
        if (BufferedFor == key)
            return;
        BufferedFor = key;
        Buffers.Clear();
        ActiveField = null;
    }

    private T Buffered<T>(string id, T current)
    {
        if (ActiveField != id || !Buffers.TryGetValue(id, out var b) || b is not T)
            Buffers[id] = current;
        return (T)Buffers[id];
    }

    private bool Track(string id)
    {
        if (ImGui.IsItemActive())
            ActiveField = id;
        else if (ActiveField == id)
            ActiveField = null;
        return GUI.InputCommitted();
    }

    private bool FloatField(string id, float current, out float value, string format = "%.2f")
    {
        value = Buffered(id, current);
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat($"##{id}", ref value, 0, 0, format);
        Buffers[id] = value;
        return Track(id);
    }

    private bool IntField(string id, int current, out int value)
    {
        value = Buffered(id, current);
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt($"##{id}", ref value, 0, 0);
        Buffers[id] = value;
        return Track(id);
    }

    private bool TextField(string id, string current, out string value, bool multiline = false)
    {
        value = Buffered(id, current ?? "");
        if (multiline)
            ImGui.InputTextMultiline($"##{id}", ref value, 4096, new Vector2(-1, 80 * DPI.UIScale()));
        else
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##{id}", ref value, 1024);
        }
        Buffers[id] = value;
        return Track(id);
    }

    private static void Row(string label, string tooltip = null)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
        if (tooltip != null)
            GUI.Tooltip(tooltip);
        ImGui.TableSetColumnIndex(1);
    }

    private static bool BeginTable(string id)
    {
        if (!ImGui.BeginTable(id, 2, ImGuiTableFlags.SizingFixedFit))
            return false;
        ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed, 130 * DPI.UIScale());
        ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);
        return true;
    }
    #endregion

    #region Display list (File List window)
    public string HierarchyTitle => LOC.Get("TEXVIEW_Movie_Window_Display_List");

    public void DisplayHierarchy()
    {
        if (!IsActive)
        {
            NoMovie();
            return;
        }

        Scene.Update();

        GUI.TitleHeader(LOC.Get("TEXVIEW_Movie_Display_List_Header"), LOC.Get("TEXVIEW_Movie_Display_List_Header_TT"));

        ImGui.Text(Entry.Path);
        var stage = Contents.Movie.FrameSize;
        ImGui.TextDisabled(LOC.Get("TEXVIEW_Movie_Stage_Info", $"{stage.Width:0}", $"{stage.Height:0}", $"{Contents.Movie.FramesPerSecond:0.##}",
            MovieScene.FrameCount(Contents.Movie.Tags), Contents.Movie.Version));

        EditorFilters.DisplayFramedListFilter("movieDisplayList", ref TreeFilter, ref TreeFilterExact);

        // Show All / Hide All (preview only)
        if (ImGui.Button($"{Icons.Eye}##movieShowAll", DPI.IconButtonSize))
            SetPreviewHidden(Scene.HiddenPaths.ToList(), false);
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Show_All_TT"));
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.EyeSlash}##movieHideAll", DPI.IconButtonSize))
            SetPreviewHidden(Scene.Roots.Where(r => r.DisplaysSomething).Select(r => r.PathKey).ToList(), true);
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Hide_All_TT"));
        if (Scene.HiddenPaths.Count > 0)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(LOC.Get("TEXVIEW_Movie_Hidden_Count", Scene.HiddenPaths.Count));
        }

        ImGui.BeginChild("movieDisplayListTree", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var selected = Selected;
        var reveal = RevealSelection ? selected : null;
        RevealSelection = false;

        foreach (var node in Scene.Roots)
            TreeNode(node, selected, reveal);

        ImGui.EndChild();
    }

    private bool Matches(MovieNode node)
    {
        if (string.IsNullOrEmpty(TreeFilter))
            return true;
        return EditorFilters.IsMatch(TreeFilter, node.Label, TreeFilterExact, node.NamePath, true, true)
            || node.Children.Any(Matches);
    }

    private void TreeNode(MovieNode node, MovieNode selected, MovieNode reveal)
    {
        if (!Matches(node))
            return;

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (node.Children.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        if (selected == node)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (reveal != null && reveal != node && IsAncestor(node, reveal))
            ImGui.SetNextItemOpen(true);

        VisibilityToggle(node);

        var badges = Badges(node);
        var dim = !node.IsDrawn;
        if (dim)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

        var open = ImGui.TreeNodeEx($"{node.Label}{badges}###{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node.Origin)}_{node.Depth}", flags);

        if (dim)
            ImGui.PopStyleColor();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Select(node);

        if (reveal == node)
            ImGui.SetScrollHereY();

        if (ImGui.IsItemHovered())
            HoveredNode = node;

        if (open && node.Children.Count > 0)
        {
            foreach (var child in node.Children)
                TreeNode(child, selected, reveal);
            ImGui.TreePop();
        }
    }

    /// <summary>
    /// The eye toggle of an instance that draws something (as the Map Editor's content list): hides it and what is
    /// inside it in the preview. Shift+click: every instance of the same character takes the new state. Instances that
    /// draw nothing get the same width of space, so the names line up.
    /// </summary>
    private void VisibilityToggle(MovieNode node)
    {
        if (!node.DisplaysSomething)
        {
            ImGui.Dummy(DPI.InlineIconButtonSize);
            ImGui.SameLine();
            return;
        }

        var hidden = Scene.HiddenPaths.Contains(node.PathKey);
        var icon = hidden ? Icons.EyeSlash : Icons.Eye;

        // Hidden by an instance above it: shown dimmed, still its own toggle
        var inherited = !hidden && node.PreviewHidden;

        ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
        if (inherited)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

        if (ImGui.Button($"{icon}##movieVis{node.PathKey}", DPI.InlineIconButtonSize))
        {
            var newState = !hidden;
            if (ImGui.GetIO().KeyShift)
            {
                var same = Scene.Roots.SelectMany(r => r.SelfAndDescendants())
                    .Where(n => n.CharacterId == node.CharacterId && n.DisplaysSomething)
                    .Select(n => n.PathKey)
                    .ToList();
                SetPreviewHidden(same, newState);
            }
            else
            {
                SetPreviewHidden([node.PathKey], newState);
            }
        }

        if (inherited)
            ImGui.PopStyleColor();
        ImGui.PopStyleColor(4);
        ImGui.PopItemFlag();
        GUI.Tooltip(LOC.Get(inherited ? "TEXVIEW_Movie_Visibility_Inherited_TT" : "TEXVIEW_Movie_Visibility_TT"));
        ImGui.SameLine();
    }

    private void SetPreviewHidden(List<string> keys, bool hidden)
    {
        foreach (var key in keys)
        {
            if (hidden)
                Scene.HiddenPaths.Add(key);
            else
                Scene.HiddenPaths.Remove(key);
        }
        Scene.Invalidate();
    }

    private static bool IsAncestor(MovieNode ancestor, MovieNode node)
    {
        for (var n = node.Parent; n != null; n = n.Parent)
            if (n == ancestor)
                return true;
        return false;
    }

    private string Badges(MovieNode node)
    {
        var b = new List<string>();
        if (node.Track?.IsAnimated == true) b.Add(LOC.Get("TEXVIEW_Movie_Badge_Animated", node.Track.KeyframeCount));
        if (node.IsMask) b.Add(LOC.Get("TEXVIEW_Movie_Badge_Mask"));
        if (node.Hidden) b.Add(LOC.Get("TEXVIEW_Movie_Badge_Hidden"));
        if (node.MatrixSource?.Filters?.Count > 0 || node.Origin.Filters?.Count > 0) b.Add(LOC.Get("TEXVIEW_Movie_Badge_Filters"));
        return b.Count == 0 ? "" : "   " + string.Join(" ", b.Select(x => $"[{x}]"));
    }
    #endregion

    #region Images (Texture List window)
    public string ImagesTitle => LOC.Get("TEXVIEW_Movie_Window_Images");

    public void DisplayImages()
    {
        if (!IsActive)
        {
            NoMovie();
            return;
        }

        GUI.TitleHeader(LOC.Get("TEXVIEW_Movie_Images_Header"), LOC.Get("TEXVIEW_Movie_Images_Header_TT"));

        if (ImGui.Button(LOC.Get("TEXVIEW_Movie_Images_Reload")))
            Bank.Images.Reset();
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Images_Reload_TT"));

        ImGui.BeginChild("movieImageList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        foreach (var image in Contents.Characters.Values.OfType<GFX.DefineExternalImage2>().OrderBy(i => i.FileName))
        {
            var resolved = Bank.Images.Get(image.FileName, out var pending);
            var status = resolved != null ? "" : pending ? $"  ({LOC.Get("TEXVIEW_Movie_Image_Loading")})" : $"  ({LOC.Get("TEXVIEW_Movie_Image_Missing")})";

            if (resolved == null && !pending)
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

            ImGui.Selectable($"{image.FileName}{status}###movieImage{image.Id}", false);

            if (resolved == null && !pending)
                ImGui.PopStyleColor();

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"{image.FileName}  {image.TargetWidth} x {image.TargetHeight}");
                ImGui.TextDisabled(resolved?.Source ?? LOC.Get("TEXVIEW_Movie_Image_Missing_TT"));
                if (resolved != null && resolved.IsReady && resolved.TextureId is ImTextureID id)
                {
                    var size = new Vector2(image.TargetWidth, image.TargetHeight);
                    var max = 256 * DPI.UIScale();
                    if (size.X > max || size.Y > max)
                        size *= max / Math.Max(size.X, size.Y);
                    ImGui.Image(id, size, resolved.Uv0, resolved.Uv1);
                }
                ImGui.EndTooltip();
            }
        }

        ImGui.EndChild();
    }
    #endregion

    #region Properties
    public void DisplayProperties()
    {
        if (!IsActive)
        {
            NoMovie();
            return;
        }

        Scene.Update();

        var node = Selected;
        ResetBuffersFor(node == null ? "" : string.Join("/", node.Path.Select(p => p.GetHashCode())));

        GUI.TitleHeader(LOC.Get("TEXVIEW_Movie_Properties_Header"), LOC.Get("TEXVIEW_Movie_Properties_Header_TT"));

        ImGui.Checkbox(LOC.Get("TEXVIEW_Movie_Allow_Rename"), ref AllowRename);
        GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Allow_Rename_TT"));
        if (AllowRename)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXVIEW_Movie_Allow_Rename_Warning"));

        ImGui.Separator();

        if (node == null)
        {
            ImGui.TextDisabled(LOC.Get("TEXVIEW_Movie_No_Selection"));
            LabelsSection(Contents.Movie.Tags, "root");
            return;
        }

        ImGui.BeginChild("movieProperties");

        InstanceSection(node);
        TransformSection(node);
        ColorSection(node);

        switch (node.Character)
        {
            case GFX.DefineSprite sprite:
                SpriteSection(sprite);
                break;
            case GFX.DefineEditText text:
                TextSection(text);
                break;
            case GFX.DefineExternalImage2 image:
                ImageSection(image);
                break;
            case GFX.DefineShape shape:
                ShapeSection(shape);
                break;
        }

        ImGui.EndChild();
    }

    private void InstanceSection(MovieNode node)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Instance"), ImGuiTreeNodeFlags.DefaultOpen))
            return;

        if (!BeginTable("movieInstance"))
            return;

        Row(LOC.Get("TEXVIEW_Movie_Field_Name"), LOC.Get("TEXVIEW_Movie_Field_Name_TT"));
        if (AllowRename)
        {
            if (TextField("name", node.Name, out var name) && name != (node.Name ?? ""))
            {
                var origin = node.Origin;
                Change([origin], () => origin.Name = string.IsNullOrEmpty(name) ? null : name);
            }
        }
        else
        {
            ImGui.Text(string.IsNullOrEmpty(node.Name) ? LOC.Get("TEXVIEW_Movie_Unnamed") : node.Name);
        }

        Row(LOC.Get("TEXVIEW_Movie_Field_Path"), LOC.Get("TEXVIEW_Movie_Field_Path_TT"));
        ImGui.TextWrapped(node.NamePath);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.SetClipboardText(node.NamePath);

        Row(LOC.Get("TEXVIEW_Movie_Field_Depth"));
        ImGui.Text($"{node.Depth}");

        Row(LOC.Get("TEXVIEW_Movie_Field_Character"));
        ImGui.Text($"{node.Character?.ToString() ?? LOC.Get("TEXVIEW_Movie_Missing_Character")} ({node.CharacterId})");

        Row(LOC.Get("TEXVIEW_Movie_Field_Timeline"), LOC.Get("TEXVIEW_Movie_Field_Timeline_TT"));
        var uses = SharedUses(node);
        if (node.TimelineSprite == 0)
            ImGui.Text(LOC.Get("TEXVIEW_Movie_Root_Timeline"));
        else if (uses > 1)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXVIEW_Movie_Shared_Timeline", node.TimelineSprite, uses));
        else
            ImGui.Text(LOC.Get("TEXVIEW_Movie_Sprite_Timeline", node.TimelineSprite));

        var place = node.MatrixSource ?? node.Origin;
        var notes = new List<string>();
        if (node.IsMask) notes.Add(LOC.Get("TEXVIEW_Movie_Note_Mask", node.ClipDepth));
        if (node.Hidden) notes.Add(LOC.Get("TEXVIEW_Movie_Note_Hidden"));
        var filters = place.Filters ?? node.Origin.Filters;
        if (filters?.Count > 0) notes.Add(LOC.Get("TEXVIEW_Movie_Note_Filters", string.Join(", ", filters.Select(f => f.Type))));
        var blend = place.BlendMode ?? node.Origin.BlendMode;
        if (blend > 1) notes.Add(LOC.Get("TEXVIEW_Movie_Note_Blend", blend));
        if (node.Origin.ClassName != null) notes.Add(LOC.Get("TEXVIEW_Movie_Note_Class", node.Origin.ClassName));

        if (notes.Count > 0)
        {
            Row(LOC.Get("TEXVIEW_Movie_Field_Notes"), LOC.Get("TEXVIEW_Movie_Field_Notes_TT"));
            ImGui.TextWrapped(string.Join("\n", notes));
        }

        ImGui.EndTable();
    }

    /// <summary>Scale, rotation (degrees) of a matrix, assuming no skew.</summary>
    private static (float Sx, float Sy, float Rot) Decompose(Matrix3x2 m)
    {
        var sx = MathF.Sqrt(m.M11 * m.M11 + m.M12 * m.M12);
        var rot = MathF.Atan2(m.M12, m.M11);
        var sy = sx > 1e-6f ? (m.M11 * m.M22 - m.M12 * m.M21) / sx : m.M22;
        return (sx, sy, rot * 180f / MathF.PI);
    }

    private static void Compose(GFX.Matrix target, float sx, float sy, float rotDeg)
    {
        var r = rotDeg * MathF.PI / 180f;
        var cos = MathF.Cos(r);
        var sin = MathF.Sin(r);
        var m = target.ToMatrix3x2();
        target.FromMatrix3x2(new Matrix3x2(sx * cos, sx * sin, -sy * sin, sy * cos, m.M31, m.M32));
    }

    private void TransformSection(MovieNode node)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Transform"), ImGuiTreeNodeFlags.DefaultOpen))
            return;

        var track = node.Track;
        if (track?.IsAnimated == true)
        {
            ImGui.TextWrapped(LOC.Get("TEXVIEW_Movie_Track_Info", track.KeyframeCount));
            ImGui.Checkbox(LOC.Get("TEXVIEW_Movie_Move_Whole_Track"), ref MoveWholeTrack);
            GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Move_Whole_Track_TT"));
        }

        if (!BeginTable("movieTransform"))
            return;

        var local = node.Local;

        Row(LOC.Get("TEXVIEW_Movie_Field_X"), LOC.Get("TEXVIEW_Movie_Field_XY_TT"));
        if (FloatField("x", local.M31, out var x, "%.2f"))
            MoveLocal(node, x - local.M31, 0);

        Row(LOC.Get("TEXVIEW_Movie_Field_Y"), LOC.Get("TEXVIEW_Movie_Field_XY_TT"));
        if (FloatField("y", local.M32, out var y, "%.2f"))
            MoveLocal(node, 0, y - local.M32);

        var (sx, sy, rot) = Decompose(local);
        var keyframe = node.MatrixSource ?? node.Origin;

        Row(LOC.Get("TEXVIEW_Movie_Field_Scale_X"), LOC.Get("TEXVIEW_Movie_Field_Keyframe_TT"));
        if (FloatField("sx", sx, out var nsx, "%.4f") && MathF.Abs(nsx - sx) > 1e-5f)
            Change([keyframe], () => Compose(keyframe.Matrix ??= new GFX.Matrix(), nsx, sy, rot));

        Row(LOC.Get("TEXVIEW_Movie_Field_Scale_Y"), LOC.Get("TEXVIEW_Movie_Field_Keyframe_TT"));
        if (FloatField("sy", sy, out var nsy, "%.4f") && MathF.Abs(nsy - sy) > 1e-5f)
            Change([keyframe], () => Compose(keyframe.Matrix ??= new GFX.Matrix(), sx, nsy, rot));

        Row(LOC.Get("TEXVIEW_Movie_Field_Rotation"), LOC.Get("TEXVIEW_Movie_Field_Keyframe_TT"));
        if (FloatField("rot", rot, out var nrot, "%.2f") && MathF.Abs(nrot - rot) > 1e-4f)
            Change([keyframe], () => Compose(keyframe.Matrix ??= new GFX.Matrix(), sx, sy, nrot));

        Row(LOC.Get("TEXVIEW_Movie_Field_World"), LOC.Get("TEXVIEW_Movie_Field_World_TT"));
        if (node.WorldMin.X <= node.WorldMax.X)
            ImGui.TextDisabled($"{node.WorldMin.X:0.#}, {node.WorldMin.Y:0.#}  {node.WorldMax.X - node.WorldMin.X:0.#} x {node.WorldMax.Y - node.WorldMin.Y:0.#}");
        else
            ImGui.TextDisabled("-");

        ImGui.EndTable();
    }

    /// <summary>Moves by a parent-space offset in pixels (the X/Y fields are in the parent's space).</summary>
    private void MoveLocal(MovieNode node, float dxPx, float dyPx)
    {
        var dx = (int)MathF.Round(dxPx * GFX.TwipsPerPixel);
        var dy = (int)MathF.Round(dyPx * GFX.TwipsPerPixel);
        if (dx == 0 && dy == 0)
            return;
        var targets = MoveTargets(node);
        Change(targets, () => Translate(targets, dx, dy));
    }

    private void ColorSection(MovieNode node)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Color")))
            return;

        // The keyframe that set the colour, else the one shown
        var target = node.ColorSource ?? node.MatrixSource ?? node.Origin;
        var cx = target.ColorTransform;

        ImGui.TextWrapped(LOC.Get("TEXVIEW_Movie_Color_Info"));

        if (!BeginTable("movieColor"))
            return;

        void Channel(string id, string label, int current, Action<GFX.ColorTransform, int> set, bool mult)
        {
            Row(label);
            if (IntField(id, current, out var v) && v != current)
            {
                var max = mult ? 4096 : 255;
                v = Math.Clamp(v, mult ? -max : -255, max);
                Change([target], () =>
                {
                    target.ColorTransform ??= new GFX.ColorTransform();
                    if (mult) target.ColorTransform.HasMult = true; else target.ColorTransform.HasAdd = true;
                    set(target.ColorTransform, v);
                });
            }
        }

        var m = cx != null && cx.HasMult;
        var a = cx != null && cx.HasAdd;
        Channel("rm", LOC.Get("TEXVIEW_Movie_Field_Red_Mult"), m ? cx.RedMult : 256, (c, v) => c.RedMult = v, true);
        Channel("gm", LOC.Get("TEXVIEW_Movie_Field_Green_Mult"), m ? cx.GreenMult : 256, (c, v) => c.GreenMult = v, true);
        Channel("bm", LOC.Get("TEXVIEW_Movie_Field_Blue_Mult"), m ? cx.BlueMult : 256, (c, v) => c.BlueMult = v, true);
        Channel("am", LOC.Get("TEXVIEW_Movie_Field_Alpha_Mult"), m ? cx.AlphaMult : 256, (c, v) => c.AlphaMult = v, true);
        Channel("ra", LOC.Get("TEXVIEW_Movie_Field_Red_Add"), a ? cx.RedAdd : 0, (c, v) => c.RedAdd = v, false);
        Channel("ga", LOC.Get("TEXVIEW_Movie_Field_Green_Add"), a ? cx.GreenAdd : 0, (c, v) => c.GreenAdd = v, false);
        Channel("ba", LOC.Get("TEXVIEW_Movie_Field_Blue_Add"), a ? cx.BlueAdd : 0, (c, v) => c.BlueAdd = v, false);
        Channel("aa", LOC.Get("TEXVIEW_Movie_Field_Alpha_Add"), a ? cx.AlphaAdd : 0, (c, v) => c.AlphaAdd = v, false);

        ImGui.EndTable();
    }

    private void SpriteSection(GFX.DefineSprite sprite)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Sprite"), ImGuiTreeNodeFlags.DefaultOpen))
            return;

        var frames = MovieScene.FrameCount(sprite.Tags);
        ImGui.TextWrapped(LOC.Get("TEXVIEW_Movie_Sprite_Info", sprite.Id, frames, Contents.SpriteUses.GetValueOrDefault(sprite.Id)));

        if (frames > 1)
        {
            var frame = Scene.SpriteFrames.GetValueOrDefault(sprite.Id);
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderInt("##spriteFrame", ref frame, 0, frames - 1, LOC.Get("TEXVIEW_Movie_Frame_Format")))
                Scene.SpriteFrames[sprite.Id] = frame;
            GUI.Tooltip(LOC.Get("TEXVIEW_Movie_Sprite_Frame_TT"));

            LabelPicker($"spriteLabels{sprite.Id}", sprite.Tags, f => Scene.SpriteFrames[sprite.Id] = f);
        }

        LabelsSection(sprite.Tags, $"sprite{sprite.Id}");
    }

    private void LabelPicker(string id, List<GFX.Tag> timeline, Action<int> go)
    {
        var labels = MovieScene.Labels(timeline);
        if (labels.Count == 0)
            return;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##{id}", LOC.Get("TEXVIEW_Movie_Go_To_Label")))
        {
            foreach (var (frame, label) in labels)
            {
                if (ImGui.Selectable($"{label.Name}  ({frame})###{id}{label.GetHashCode()}"))
                    go(frame);
            }
            ImGui.EndCombo();
        }
    }

    private void LabelsSection(List<GFX.Tag> timeline, string id)
    {
        var labels = MovieScene.Labels(timeline);
        if (labels.Count == 0)
            return;

        if (!ImGui.CollapsingHeader($"{LOC.Get("TEXVIEW_Movie_Section_Labels", labels.Count)}###labels{id}"))
            return;

        if (!BeginTable($"labels{id}"))
            return;

        foreach (var (frame, label) in labels)
        {
            Row(LOC.Get("TEXVIEW_Movie_Label_Frame", frame), LOC.Get("TEXVIEW_Movie_Label_TT"));
            if (AllowRename)
            {
                var key = $"label{label.GetHashCode()}";
                if (TextField(key, label.Name, out var name) && name.Length > 0 && name != label.Name)
                    Change([label], () => label.Name = name);
            }
            else
            {
                ImGui.Text(label.Name);
            }
        }

        ImGui.EndTable();
    }

    private void TextSection(GFX.DefineEditText text)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Text"), ImGuiTreeNodeFlags.DefaultOpen))
            return;

        ImGui.TextWrapped(LOC.Get("TEXVIEW_Movie_Text_Info"));

        if (!BeginTable("movieText"))
            return;

        var b = text.Bounds;
        void RectField(string id, string label, int current, Action<int> set)
        {
            Row(label, LOC.Get("TEXVIEW_Movie_Field_Text_Bounds_TT"));
            if (FloatField(id, current / GFX.TwipsPerPixel, out var px, "%.2f"))
            {
                var twips = (int)MathF.Round(px * GFX.TwipsPerPixel);
                if (twips != current)
                    Change([text], () => set(twips));
            }
        }

        RectField("tx", LOC.Get("TEXVIEW_Movie_Field_Left"), b.XMin, v => { var w = text.Bounds.XMax - text.Bounds.XMin; text.Bounds.XMin = v; text.Bounds.XMax = v + w; });
        RectField("ty", LOC.Get("TEXVIEW_Movie_Field_Top"), b.YMin, v => { var h = text.Bounds.YMax - text.Bounds.YMin; text.Bounds.YMin = v; text.Bounds.YMax = v + h; });
        RectField("tw", LOC.Get("TEXVIEW_Movie_Field_Width"), b.XMax - b.XMin, v => text.Bounds.XMax = text.Bounds.XMin + Math.Max(0, v));
        RectField("th", LOC.Get("TEXVIEW_Movie_Field_Height"), b.YMax - b.YMin, v => text.Bounds.YMax = text.Bounds.YMin + Math.Max(0, v));

        if (text.FontId != null || text.FontClass != null)
        {
            Row(LOC.Get("TEXVIEW_Movie_Field_Font_Height"), LOC.Get("TEXVIEW_Movie_Field_Font_Height_TT"));
            if (FloatField("fh", text.FontHeight / GFX.TwipsPerPixel, out var fh, "%.1f"))
            {
                var twips = (ushort)Math.Clamp((int)MathF.Round(fh * GFX.TwipsPerPixel), 0, ushort.MaxValue);
                if (twips != text.FontHeight)
                    Change([text], () => text.FontHeight = twips);
            }
        }

        if (text.Layout != null)
        {
            Row(LOC.Get("TEXVIEW_Movie_Field_Align"));
            var align = (int)text.Layout.Align;
            var names = Enum.GetNames<GFX.TextAlign>();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("##align", ref align, names, names.Length) && align != (int)text.Layout.Align)
                Change([text], () => text.Layout.Align = (GFX.TextAlign)align);
        }

        if (text.TextColor is Color c)
        {
            Row(LOC.Get("TEXVIEW_Movie_Field_Text_Color"), LOC.Get("TEXVIEW_Movie_Field_Text_Color_TT"));
            var hex = $"{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
            if (TextField("tc", hex, out var nhex) && nhex != hex && TryParseRgba(nhex, out var nc))
                Change([text], () => text.TextColor = nc);
        }

        Row(LOC.Get("TEXVIEW_Movie_Field_Initial_Text"), LOC.Get("TEXVIEW_Movie_Field_Initial_Text_TT"));
        if (TextField("it", text.InitialText, out var initial, true) && initial != (text.InitialText ?? ""))
            Change([text], () => text.InitialText = initial);

        Row(LOC.Get("TEXVIEW_Movie_Field_Font"));
        ImGui.TextDisabled(text.FontClass ?? (text.FontId != null ? $"{text.FontId}" : "-"));

        ImGui.EndTable();
    }

    private static bool TryParseRgba(string hex, out Color color)
    {
        color = Color.White;
        hex = hex.Trim().TrimStart('#');
        if (hex.Length is not (6 or 8) || !uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var v))
            return false;
        if (hex.Length == 6)
            v = v << 8 | 0xFF;
        color = Color.FromArgb((int)(v & 0xFF), (int)(v >> 24 & 0xFF), (int)(v >> 16 & 0xFF), (int)(v >> 8 & 0xFF));
        return true;
    }

    private void ImageSection(GFX.DefineExternalImage2 image)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Image"), ImGuiTreeNodeFlags.DefaultOpen))
            return;

        ImGui.TextWrapped(LOC.Get("TEXVIEW_Movie_Image_Info"));

        if (!BeginTable("movieImage"))
            return;

        Row(LOC.Get("TEXVIEW_Movie_Field_File_Name"), LOC.Get("TEXVIEW_Movie_Field_File_Name_TT"));
        if (TextField("file", image.FileName, out var file) && file.Length > 0 && file != image.FileName)
            Change([image], () => image.FileName = file);

        Row(LOC.Get("TEXVIEW_Movie_Field_Size"));
        ImGui.Text($"{image.TargetWidth} x {image.TargetHeight}");

        Row(LOC.Get("TEXVIEW_Movie_Field_Source"));
        var resolved = Bank.Images.Get(image.FileName, out var pending);
        ImGui.TextWrapped(resolved?.Source ?? (pending ? LOC.Get("TEXVIEW_Movie_Image_Loading") : LOC.Get("TEXVIEW_Movie_Image_Missing")));

        ImGui.EndTable();
    }

    private void ShapeSection(GFX.DefineShape shape)
    {
        if (!ImGui.CollapsingHeader(LOC.Get("TEXVIEW_Movie_Section_Shape")))
            return;

        var fill = GetFill(shape);
        ImGui.TextWrapped(LOC.Get("TEXVIEW_Movie_Shape_Info", shape.Version, fill?.Kind.ToString() ?? "-",
            fill?.Kind == GFX.FillKind.Bitmap && Contents.Characters.GetValueOrDefault(fill.BitmapId) is GFX.DefineExternalImage2 img ? img.FileName : ""));
    }
    #endregion
}
