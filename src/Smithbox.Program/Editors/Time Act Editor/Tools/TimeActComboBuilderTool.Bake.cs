using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Bakes the combo into a new animation of the first step's time act: its HKX file is the steps' clips played one
/// after another as the combo plays them (see <see cref="TimeActComboBake" />), held by the source until it is saved
/// as an imported file is, and its events are the steps' events moved to where each step starts. A copy of the HKX
/// file also goes to the export folder.
/// </summary>
public partial class TimeActComboBuilderTool
{
    private const string BakePopupID = "##timeActComboBakePopup";

    private bool OpenBakePopup;
    private bool FocusBakeInput;
    private string BakeIDText = "";

    private TimeActContainerWrapper BakeSource;
    private TimeActEntry BakeEntry;

    /// <summary>
    /// Opens the popup that asks for the ID of the new animation, offering the next free one after the last
    /// animation of the first step's time act
    /// </summary>
    private void BeginBake(TimeActContainerWrapper source, List<ComboStep> combo)
    {
        if (source == null || combo.Count == 0)
            return;

        if (!source.TryFindAnimation(combo[0].AnimationID, out var entry, out _) || entry.TimeAct == null)
        {
            Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Bake_Animation_Missing", 1, source.FormatAnimationName(combo[0].AnimationID)));
            return;
        }

        var animations = entry.TimeAct.Animations;
        var start = animations.Count > 0 ? animations.Max(e => e.ID) + 1 : (long)entry.BindIndex * source.AnimationCategorySize;

        BakeSource = source;
        BakeEntry = entry;
        BakeIDText = $"{entry.GetFreeAnimationID(start)}";

        OpenBakePopup = true;
    }

    private void DisplayBakePopup()
    {
        var title = $"{LOC.Get("TAE_ComboBuilder_Bake_Title")}{BakePopupID}";

        if (OpenBakePopup)
        {
            ImGui.OpenPopup(title);

            OpenBakePopup = false;
            FocusBakeInput = true;
        }

        var open = true;

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var source = BakeSource;
        var entry = BakeEntry;

        // The source may have changed, or the time act gone through an undo, while the popup was open
        if (source == null || entry?.TimeAct == null || source != View.Selection.SelectedTimeActContainerWrapper ||
            !source.TimeActs.Contains(entry))
        {
            BakeSource = null;
            BakeEntry = null;

            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var combo = GetCombo(source);

        GUI.WrappedText(LOC.Get("TAE_ComboBuilder_Bake_Text", combo.Count, entry.Name));

        if (FocusBakeInput)
        {
            ImGui.SetKeyboardFocusHere();
            FocusBakeInput = false;
        }

        ImGui.SetNextItemWidth(200 * DPI.UIScale());

        // Only the Bake button applies it: baking adds an animation, which leaving the field shouldn't do by itself
        ImGui.InputText("##timeActComboBakeID", ref BakeIDText, 32, ImGuiInputTextFlags.CharsDecimal);
        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Bake_ID_TT"));

        var error = ValidateBakeID(entry, BakeIDText, out var id);

        if (error != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, error);
        }
        else
        {
            ImGui.Text(LOC.Get("TAE_ComboBuilder_Bake_Becomes",
                source.FormatAnimationName(entry.GetFullAnimationID(id, source.AnimationCategorySize))));
        }

        GUI.WrappedText(LOC.Get("TAE_ComboBuilder_Bake_Note"));

        var cancelled = ImGui.IsKeyPressed(ImGuiKey.Escape, false);
        var confirmed = false;

        ImGui.BeginDisabled(error != null);

        if (ImGui.Button($"{LOC.Get("TAE_ComboBuilder_Bake_Confirm")}##timeActComboBakeConfirm"))
        {
            confirmed = true;
        }
        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Bake_Confirm_TT"));

        ImGui.EndDisabled();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("TAE_ComboBuilder_Bake_Cancel")}##timeActComboBakeCancel"))
        {
            cancelled = true;
        }

        if (confirmed && !cancelled)
        {
            Bake(source, entry, combo, id);
        }

        if (confirmed || cancelled || !open)
        {
            BakeSource = null;
            BakeEntry = null;

            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// A whole number no animation of the time act uses. Gives the error to show, or null when it can be used.
    /// </summary>
    private static string ValidateBakeID(TimeActEntry entry, string text, out long id)
    {
        if (!long.TryParse(text, out id) || id < 0)
            return LOC.Get("TAE_PropertiesWindow_TimeAct_Invalid_Value", text, LOC.Get("TAE_PropertiesWindow_Animation_ID"));

        var newID = id;

        if (entry.TimeAct.Animations.Any(e => e.ID == newID))
            return LOC.Get("TAE_PropertiesWindow_Animation_ID_In_Use", id);

        return null;
    }

    private void Bake(TimeActContainerWrapper source, TimeActEntry entry, List<ComboStep> combo, long id)
    {
        var package = View.Playback.Package;

        if (package?.Skeleton == null)
        {
            Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Bake_No_Model"));
            return;
        }

        // Each step's clip and the stretch of it the combo plays, as the playback works them out
        var steps = new List<TimeActComboBakeStep>();
        AnimationEntry firstClip = null;
        TAE.Animation firstAnimation = null;

        var blending = CFG.Current.TimeActEditor.TimeActEditor_Sim_Blending;

        for (var i = 0; i < combo.Count; i++)
        {
            var step = combo[i];
            var name = source.FormatAnimationName(step.AnimationID);

            var animation = GetStepAnimation(source, step);

            if (animation == null || !StepAnimations.TryGetValue(step, out var found))
            {
                Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Bake_Animation_Missing", i + 1, name));
                return;
            }

            var clipEntry = TimeActPlayback.FindClip(package, source, found.Entry, animation);
            var clip = clipEntry != null ? package.GetClip(clipEntry) : null;

            if (clip == null)
            {
                Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Bake_Clip_Missing", i + 1, name));
                return;
            }

            // Additive clips are laid over another, so they have no pose of their own to bake
            if (clip.BlendMode != HavokBlendMode.Normal)
            {
                Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Bake_Additive", i + 1, name));
                return;
            }

            var start = GetStartTime(step, clip.FrameDuration, clip.Duration);
            var end = GetEndTime(step, animation, clip.FrameDuration, clip.Duration);

            // As the playback crossfades a step in from the last: over the step's own blend, while blending is on
            var blendIn = i > 0 && blending ? View.Playback.Simulation.GetBlendDuration(animation) : 0;

            steps.Add(new TimeActComboBakeStep(animation, clip, start, end, blendIn));

            if (i == 0)
            {
                firstClip = clipEntry;
                firstAnimation = animation;
            }
        }

        var fullID = entry.GetFullAnimationID(id, source.AnimationCategorySize);
        var newName = source.FormatAnimationName(fullID);

        // Where the file goes, as an imported one
        List<string> binderPaths;
        string fileName;

        try
        {
            using var files = TimeActAnimationFiles.Open(source, false);

            // A character variant that plays the animations of another has none of its own to go alongside,
            // and giving it some would stop it playing the shared ones
            if (source.IsCharacter && !files.HasAnimationFiles)
            {
                Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Import_Shared_Set", source.Name));
                return;
            }

            if (files.Contains(fullID))
            {
                var result = PlatformUtils.Instance.MessageBox(
                    LOC.Get("TAE_ComboBuilder_Bake_Replace", newName),
                    LOC.Get("SYS_Warning_Header"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            binderPaths = !files.IsStored(fullID) && source.PendingAnimationFiles.TryGetValue(fullID, out var pending)
                ? pending.BinderPaths.ToList()
                : files.GetImportBinderPaths(fullID);

            fileName = files.GetFileName(fullID);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Read_Failed", source.Name), e);
            return;
        }

        TimeActComboBakeResult baked;

        try
        {
            var baseFile = firstClip.Binder.ReadFile(firstClip.Header).ToArray();

            if (DCX.Is(baseFile))
            {
                baseFile = DCX.Decompress(baseFile).ToArray();
            }

            baked = TimeActComboBake.BakeClip(Project.Descriptor.ProjectType, baseFile, firstClip.Compendium,
                package.Skeleton, steps);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Bake_Failed", newName), e);
            return;
        }

        var bakedAnimation = TimeActComboBake.BuildAnimation(id,
            TimeActAnimationSelectionWindow.GetAnimFileName(source, entry, firstAnimation, id, ""), steps,
            entry.TimeAct.BigEndian);

        // In ID order, as the game stores them
        var animations = entry.TimeAct.Animations;
        var index = animations.FindIndex(e => e.ID > id);

        var actions = new List<ViewportAction>
        {
            new TimeActImportAnimationFileAction(source, new TimeActPendingAnimationFile(fullID, baked.Bytes, binderPaths)),
            new TimeActAddAnimationAction(View, entry, bakedAnimation, index < 0 ? animations.Count : index)
        };

        View.ViewportActionManager.ExecuteAction(new TimeActCompoundAction(actions,
            LOC.Get("TAE_History_Bake_Combo", newName, steps.Count)));

        View.AnimationSelectionWindow.ScrollTo(bakedAnimation);

        Smithbox.Log(this, LOC.Get("TAE_ComboBuilder_Bake_Done", steps.Count, newName, baked.FrameCount,
            baked.Write.Format, string.Join(", ", binderPaths)));

        if (!baked.HasRootMotion)
        {
            Smithbox.Log(this, LOC.Get("TAE_ComboBuilder_Bake_No_Root_Motion"));
        }

        ExportBake(source, fileName, baked.Bytes);
    }

    /// <summary>
    /// Writes a copy of the baked HKX file to the export folder, where Export puts the files of the source
    /// </summary>
    private void ExportBake(TimeActContainerWrapper source, string fileName, byte[] bytes)
    {
        if (!Directory.Exists(CFG.Current.TimeActEditor.TimeActEditor_Export_Directory) && !TimeActSelectionWindow.SelectExportFolder())
        {
            Smithbox.Log(this, LOC.Get("TAE_ComboBuilder_Bake_Export_Skipped"));
            return;
        }

        var folder = Path.Join(CFG.Current.TimeActEditor.TimeActEditor_Export_Directory, source.Name, "hkx");
        var path = Path.Join(folder, fileName);

        if (File.Exists(path))
        {
            var result = PlatformUtils.Instance.MessageBox(LOC.Get("TAE_SelectionWindow_Dialog_Overwrite_File", path),
                LOC.Get("SYS_Warning_Header"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                Smithbox.Log(this, LOC.Get("TAE_ComboBuilder_Bake_Export_Skipped"));
                return;
            }
        }

        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(path, bytes);

            Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Exported", path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Export_Failed", path), e);
        }
    }
}
