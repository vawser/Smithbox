using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActAnimationSelectionWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string AnimationListFilter = "";
    private bool ExactAnimationFilter = false;

    private bool _arrowKeyPressed = false;

    // Animations can import ones that import others, but not endlessly
    private const int MaxImportDepth = 8;

    // A time act can have over a thousand animations, with a name to build for each
    private List<(TAE.Animation Animation, string Name)> CachedFilteredAnimations;
    private TAE CachedAnimationSource;
    private int CachedAnimationsVersion;
    private string CachedAnimationFilter = "";
    private bool CachedExactAnimationFilter = false;

    // The animation to bring into view the next time the list is drawn, such as one jumped to from a search
    private TAE.Animation ScrollTarget;

    public TimeActAnimationSelectionWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_SelectionWindow_Animation_Header"),
            LOC.Get("TAE_SelectionWindow_Animation_Header_TT"));

        DisplayHeader();

        ImGui.BeginChild($"AnimationListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayAnimationList();

        ImGui.EndChild();
    }

    /// <summary>
    /// The filter, with a button after it that steps through the ways aliases are shown
    /// </summary>
    private void DisplayHeader()
    {
        const string id = "timeActEditor_AnimationList_Filter";

        ImGui.BeginChild($"framedListFilter_{id}", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar(id, ref AnimationListFilter, ref ExactAnimationFilter);

        ImGui.SameLine();

        var mode = CFG.Current.TimeActEditor_Animation_List_Alias_Display;
        var next = GetNextAliasDisplayMode(mode);

        var icon = mode switch
        {
            TimeActAliasDisplayMode.Truncate => Icons.TextWidth,
            TimeActAliasDisplayMode.Hide => Icons.EyeSlash,
            _ => Icons.Paragraph
        };

        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{icon}##timeActAnimationAliasDisplay"))
        {
            CFG.Current.TimeActEditor_Animation_List_Alias_Display = next;
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Alias_Display_TT",
            GetAliasDisplayModeName(mode), GetAliasDisplayModeName(next)));

        ImGui.EndChild();
    }

    private static TimeActAliasDisplayMode GetNextAliasDisplayMode(TimeActAliasDisplayMode mode) => mode switch
    {
        TimeActAliasDisplayMode.Wrap => TimeActAliasDisplayMode.Truncate,
        TimeActAliasDisplayMode.Truncate => TimeActAliasDisplayMode.Hide,
        _ => TimeActAliasDisplayMode.Wrap
    };

    private static string GetAliasDisplayModeName(TimeActAliasDisplayMode mode) => mode switch
    {
        TimeActAliasDisplayMode.Truncate => LOC.Get("TAE_AnimationWindow_Alias_Display_Truncate"),
        TimeActAliasDisplayMode.Hide => LOC.Get("TAE_AnimationWindow_Alias_Display_Hide"),
        _ => LOC.Get("TAE_AnimationWindow_Alias_Display_Wrap")
    };

    /// <summary>
    /// Shows the alias after the animation's name, as the header's toggle has it
    /// </summary>
    private static void DisplayAlias(string alias)
    {
        if (alias == "")
            return;

        switch (CFG.Current.TimeActEditor_Animation_List_Alias_Display)
        {
            case TimeActAliasDisplayMode.Wrap:
                GUI.DisplayAlias(alias, true);
                break;

            case TimeActAliasDisplayMode.Truncate:
                ImGui.SameLine();

                var available = ImGui.GetContentRegionAvail().X;
                var text = TruncateToWidth(alias, available);

                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, text);

                // The whole alias is shown on hover when it had to be cut short
                if (text.Length != alias.Length)
                {
                    GUI.Tooltip(alias);
                }
                break;
        }
    }

    /// <summary>
    /// Cuts the text short with an ellipsis so it fits the width, finding the longest start that does
    /// </summary>
    private static string TruncateToWidth(string text, float width)
    {
        if (ImGui.CalcTextSize(text).X <= width)
            return text;

        const string ellipsis = "...";

        if (ImGui.CalcTextSize(ellipsis).X > width)
            return "";

        int low = 0, high = text.Length;

        while (low < high)
        {
            var mid = (low + high + 1) / 2;

            if (ImGui.CalcTextSize(text[..mid].TrimEnd() + ellipsis).X <= width)
                low = mid;
            else
                high = mid - 1;
        }

        return text[..low].TrimEnd() + ellipsis;
    }

    public void DisplayAnimationList()
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;
        var entry = View.Selection.SelectedTimeAct;

        if (source == null || entry?.TimeAct == null)
            return;

        var filteredAnimations = GetFilteredAnimations(source, entry);

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredAnimations.Count);

        var scrollIndex = ScrollTarget != null ? filteredAnimations.FindIndex(e => e.Animation == ScrollTarget) : -1;

        // A hidden animation can't be brought into view
        if (scrollIndex < 0)
        {
            ScrollTarget = null;
        }
        else
        {
            clipper.IncludeItemByIndex(scrollIndex);
        }

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (animation, name) = filteredAnimations[i];

                var selected = View.Selection.SelectedAnimation == animation;

                var alias = CFG.Current.TimeActEditor_Animation_List_Alias_Display == TimeActAliasDisplayMode.Hide
                    ? ""
                    : AliasHelper.GetAnimationAlias(Project, source.Name, name);

                var hasPendingFile = source.PendingAnimationFiles.ContainsKey(
                    entry.GetFullAnimationID(animation, source.AnimationCategorySize));

                var labelName = hasPendingFile ? $"{name}*" : name;

                var vanillaStatus = TimeActVanillaDisplay.Enabled
                    ? source.Vanilla.GetAnimationDiff(entry, animation).Status
                    : TimeActVanillaStatus.Unknown;

                var colored = TimeActVanillaDisplay.PushColor(vanillaStatus);

                if (ImGui.Selectable($"{labelName}##timeActAnimation{animation.ID}", selected))
                {
                    SelectAnimation(animation);
                }

                if (colored)
                {
                    ImGui.PopStyleColor();
                }

                if (TimeActVanillaDisplay.Enabled)
                {
                    GUI.Tooltip(TimeActVanillaDisplay.GetStatusText(vanillaStatus, source.Vanilla.IsReading));
                }

                if (animation == ScrollTarget)
                {
                    ImGui.SetScrollHereY();
                    ScrollTarget = null;
                }

                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    SelectAnimation(animation);

                    _arrowKeyPressed = false;
                }

                // The list is always in ID order, as the game stores it, so animations can't be dragged around
                ContextMenu(source, entry, animation, name);

                DisplayAlias(alias);
            }
        }

        clipper.End();

        // Right-clicking the space below the animations, which is all there is for a time act without any
        if (ImGui.BeginPopupContextWindow("##timeActAnimationListContext",
            ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
        {
            ListActions(source, entry, null);

            ImGui.EndPopup();
        }

        HandleShortcuts(source, entry);

        DisplayChangeIDPopup(source, entry);
    }

    /// <summary>
    /// The list's own keys, while the pointer is over it, as the graph's keys are while it is over the graph.
    /// Copy and Paste work here on whole animations, and on events in the graph.
    /// </summary>
    private void HandleShortcuts(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) || ImGui.IsAnyItemActive())
            return;

        var animation = View.Selection.SelectedAnimation;
        var canEdit = Project.Handler.TimeActData.CanSaveTimeActs;

        if (InputManager.IsPressed(KeybindID.Paste) && canEdit)
        {
            PasteAnimation(source, entry);
        }

        // The rest act on the selected animation, which has to be one of this time act
        if (animation == null || !entry.TimeAct.Animations.Contains(animation))
            return;

        if (InputManager.IsPressed(KeybindID.Copy))
        {
            CopyAnimation(source, entry, animation);
        }

        if (!canEdit)
            return;

        if (InputManager.IsPressed(KeybindID.Duplicate))
        {
            DuplicateAnimation(source, entry, animation);
        }
        else if (InputManager.IsPressed(KeybindID.Delete))
        {
            DeleteAnimation(entry, animation, source.GetAnimationName(entry, animation));
        }
        else if (ImGui.IsKeyPressed(ImGuiKey.F2, false))
        {
            BeginChangeID(animation);
        }
    }

    private List<(TAE.Animation Animation, string Name)> GetFilteredAnimations(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (CachedFilteredAnimations != null &&
            CachedAnimationSource == entry.TimeAct &&
            CachedAnimationsVersion == entry.AnimationsVersion &&
            CachedAnimationFilter == AnimationListFilter &&
            CachedExactAnimationFilter == ExactAnimationFilter)
        {
            return CachedFilteredAnimations;
        }

        CachedAnimationSource = entry.TimeAct;
        CachedAnimationsVersion = entry.AnimationsVersion;
        CachedAnimationFilter = AnimationListFilter;
        CachedExactAnimationFilter = ExactAnimationFilter;

        CachedFilteredAnimations = entry.TimeAct.Animations
            .Select(e => (Animation: e, Name: source.GetAnimationName(entry, e)))
            .Where(e => EditorFilters.IsMatch(AnimationListFilter, e.Name, ExactAnimationFilter,
                AliasHelper.GetAnimationAlias(Project, source.Name, e.Name), true, true))
            .ToList();

        return CachedFilteredAnimations;
    }

    #region Context Menu
    private void ContextMenu(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation, string name)
    {
        if (!ImGui.BeginPopupContextItem($"##timeActAnimationContext{animation.ID}"))
            return;

        var canEdit = Project.Handler.TimeActData.CanSaveTimeActs;

        // Copy
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Copy")}##copyAnimation",
            InputManager.GetHint(KeybindID.Copy)))
        {
            CopyAnimation(source, entry, animation);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Copy_TT"));

        // Duplicate
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Duplicate")}##duplicateAnimation",
            InputManager.GetHint(KeybindID.Duplicate), false, canEdit))
        {
            DuplicateAnimation(source, entry, animation);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Duplicate_TT"));

        // Change ID
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Change_ID")}##changeAnimationID", "F2", false, canEdit))
        {
            BeginChangeID(animation);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Change_ID_TT"));

        // Delete
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Delete")}##deleteAnimation",
            InputManager.GetHint(KeybindID.Delete), false, canEdit))
        {
            DeleteAnimation(entry, animation, name);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Delete_TT"));

        // Revert to Vanilla
        if (ImGui.MenuItem($"{LOC.Get("TAE_Vanilla_Revert_Animation")}##revertAnimation", "", false,
            canEdit && TimeActRevert.CanRevertAnimation(source, entry, animation)))
        {
            TimeActRevert.RevertAnimation(View, source, entry, animation);
        }
        GUI.Tooltip(LOC.Get("TAE_Vanilla_Revert_Animation_TT"));

        ImGui.Separator();

        // Import HKX
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Import_HKX")}##importAnimationFile", "", false, canEdit))
        {
            ImportAnimationFile(source, entry, animation, name);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Import_HKX_TT"));

        // Export HKX
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Export_HKX")}##exportAnimationFile"))
        {
            ExportAnimationFiles(source, entry, [(animation, name)]);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Export_HKX_TT", TimeActSelectionWindow.GetExportFolderLabel()));

        ImGui.Separator();

        // Add to Combo
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Add_To_Combo")}##addAnimationToCombo"))
        {
            View.ComboBuilderTool.AddStep(entry, animation);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Add_To_Combo_TT"));

        ImGui.Separator();

        ListActions(source, entry, animation);

        ImGui.EndPopup();
    }

    /// <summary>
    /// The actions that apply to the whole list, which are also shown when right-clicking the space below it
    /// </summary>
    private void ListActions(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation)
    {
        var canEdit = Project.Handler.TimeActData.CanSaveTimeActs;

        // Create
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Create")}##createAnimation", "", false, canEdit))
        {
            CreateAnimation(source, entry, animation ?? View.Selection.SelectedAnimation);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Create_TT"));

        // Paste
        var copied = View.Editor.CopiedAnimation;

        if (ImGui.MenuItem(copied != null
                ? $"{LOC.Get("TAE_AnimationWindow_Context_Paste_Named", copied.Name)}##pasteAnimation"
                : $"{LOC.Get("TAE_AnimationWindow_Context_Paste")}##pasteAnimation",
            InputManager.GetHint(KeybindID.Paste), false, canEdit && copied != null))
        {
            PasteAnimation(source, entry);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Paste_TT"));

        // Sort
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Sort")}##sortAnimations", "", false, canEdit))
        {
            SortAnimations(entry);
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Sort_TT"));

        ImGui.Separator();

        // Export All HKX
        if (ImGui.MenuItem($"{LOC.Get("TAE_AnimationWindow_Context_Export_All_HKX")}##exportAllAnimationFiles"))
        {
            ExportAnimationFiles(source, entry, GetFilteredAnimations(source, entry));
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Context_Export_All_HKX_TT", TimeActSelectionWindow.GetExportFolderLabel()));

        // Set Export Folder
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Set_Export_Folder")}##setAnimationExportFolder"))
        {
            TimeActSelectionWindow.SelectExportFolder();
        }
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Set_Export_Folder_TT", TimeActSelectionWindow.GetExportFolderLabel()));

        // Open Export Folder
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Open_Export_Folder")}##openAnimationExportFolder", "",
            false, Directory.Exists(CFG.Current.TimeActEditor_Export_Directory)))
        {
            Process.Start("explorer.exe", CFG.Current.TimeActEditor_Export_Directory);
        }
    }
    #endregion

    #region Create / Duplicate / Delete
    /// <summary>
    /// Adds an empty animation after the given one, under the next free ID
    /// </summary>
    private void CreateAnimation(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation after)
    {
        var animations = entry.TimeAct.Animations;

        var start = after != null
            ? after.ID + 1
            : animations.Count > 0
                ? animations.Max(e => e.ID) + 1
                : (long)entry.BindIndex * source.AnimationCategorySize;

        var id = entry.GetFreeAnimationID(start);

        var animation = new TAE.Animation(id, new TAE.Animation.AnimMiniHeader.Standard(),
            GetAnimFileName(source, entry, after, id, ""));

        View.ViewportActionManager.ExecuteAction(new TimeActAddAnimationAction(View, entry, animation,
            GetSortedIndex(animations, id)));

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Created", source.GetAnimationName(entry, animation)));
    }

    /// <summary>
    /// Adds a copy of an animation after it, under the next free ID
    /// </summary>
    private void DuplicateAnimation(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation)
    {
        var id = entry.GetFreeAnimationID(animation.ID + 1);

        var copy = entry.CloneAnimation(animation, id,
            GetAnimFileName(source, entry, animation, id, animation.AnimFileName));

        View.ViewportActionManager.ExecuteAction(new TimeActAddAnimationAction(View, entry, copy,
            GetSortedIndex(entry.TimeAct.Animations, id)));

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Duplicated",
            source.GetAnimationName(entry, animation), source.GetAnimationName(entry, copy)));
    }

    private void DeleteAnimation(TimeActEntry entry, TAE.Animation animation, string name)
    {
        View.ViewportActionManager.ExecuteAction(new TimeActAddAnimationAction(View, entry, animation, -1, true));

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Deleted", name));
    }
    #endregion

    #region Copy / Paste
    /// <summary>
    /// Puts a copy of the animation and its events on the editor's clipboard, so it can be pasted into any
    /// time act of the project, in any view
    /// </summary>
    private void CopyAnimation(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation)
    {
        View.Editor.CopiedAnimation = new TimeActCopiedAnimation(source, entry, animation);

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Copied", View.Editor.CopiedAnimation.Name));
    }

    /// <summary>
    /// Adds the copied animation to the time act under its own ID, or the next free one after it if that is taken.
    /// Like Duplicate, the HKX file isn't copied: the copy plays what its ID or mini header points to here.
    /// </summary>
    private void PasteAnimation(TimeActContainerWrapper source, TimeActEntry entry)
    {
        var copied = View.Editor.CopiedAnimation;

        if (copied == null)
            return;

        if (copied.BigEndian != entry.TimeAct.BigEndian)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Paste_Byte_Order", copied.Name, entry.Name));
            return;
        }

        var id = entry.GetFreeAnimationID(copied.Animation.ID);

        var newName = source.FormatAnimationName(entry.GetFullAnimationID(id, source.AnimationCategorySize));

        var animation = TimeActEntry.CloneAnimation(copied.Animation, id, copied.GetFileName(newName),
            copied.BigEndian, false);

        View.ViewportActionManager.ExecuteAction(new TimeActAddAnimationAction(View, entry, animation,
            GetSortedIndex(entry.TimeAct.Animations, id)));

        ScrollTo(animation);

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Pasted", copied.Name, copied.SourceName,
            source.GetAnimationName(entry, animation)));

        // An animation that plays another's motion needs that one in this source too
        if (copied.GetImportedID() is { } imported && !source.TryFindAnimation(imported, out _, out _))
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Paste_Import_Missing",
                source.GetAnimationName(entry, animation), source.FormatAnimationName(imported), source.Name));
        }
    }
    #endregion

    #region Change ID
    private const string ChangeIDPopupID = "##timeActChangeAnimationID";

    private TAE.Animation ChangeIDTarget;
    private string ChangeIDText = "";
    private bool OpenChangeIDPopup;
    private bool FocusChangeIDInput;

    /// <summary>
    /// Opens the Change ID popup for an animation, the next time the list is drawn. It is opened from outside the
    /// context menu, as a popup opened from inside another closes with it.
    /// </summary>
    private void BeginChangeID(TAE.Animation animation)
    {
        ChangeIDTarget = animation;
        ChangeIDText = $"{animation.ID}";
        OpenChangeIDPopup = true;
    }

    /// <summary>
    /// The ID with the same checks as the Properties field: a whole number not used by another animation of the
    /// time act. Gives the error to show, or null when it can be applied.
    /// </summary>
    private static string ValidateNewID(TimeActEntry entry, TAE.Animation animation, string text, out long id)
    {
        if (!long.TryParse(text, out id))
            return LOC.Get("TAE_PropertiesWindow_TimeAct_Invalid_Value", text, LOC.Get("TAE_PropertiesWindow_Animation_ID"));

        var newID = id;

        if (entry.TimeAct.Animations.Any(e => e != animation && e.ID == newID))
            return LOC.Get("TAE_PropertiesWindow_Animation_ID_In_Use", id);

        return null;
    }

    private void DisplayChangeIDPopup(TimeActContainerWrapper source, TimeActEntry entry)
    {
        var title = $"{LOC.Get("TAE_AnimationWindow_Change_ID_Title")}{ChangeIDPopupID}";

        if (OpenChangeIDPopup)
        {
            ImGui.OpenPopup(title);

            OpenChangeIDPopup = false;
            FocusChangeIDInput = true;
        }

        var open = true;

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var animation = ChangeIDTarget;

        // The animation may have gone, such as through an undo, while the popup was open
        if (animation == null || !entry.TimeAct.Animations.Contains(animation))
        {
            ChangeIDTarget = null;

            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        ImGui.Text(LOC.Get("TAE_AnimationWindow_Change_ID_Text", source.GetAnimationName(entry, animation)));

        if (FocusChangeIDInput)
        {
            ImGui.SetKeyboardFocusHere();
            FocusChangeIDInput = false;
        }

        ImGui.SetNextItemWidth(200);

        var entered = ImGui.InputText("##changeAnimationIDInput", ref ChangeIDText, 32,
            ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CharsDecimal);

        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Change_ID_Input_TT"));

        var error = ValidateNewID(entry, animation, ChangeIDText, out var newID);
        var unchanged = error == null && newID == animation.ID;

        // What the animation will be called, or why the ID can't be used
        if (error != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, error);
        }
        else if (!unchanged)
        {
            ImGui.Text(LOC.Get("TAE_AnimationWindow_Change_ID_Becomes",
                source.FormatAnimationName(entry.GetFullAnimationID(newID, source.AnimationCategorySize))));
        }

        var confirmed = entered && error == null;
        var cancelled = ImGui.IsKeyPressed(ImGuiKey.Escape, false);

        ImGui.BeginDisabled(error != null || unchanged);

        if (ImGui.Button($"{LOC.Get("TAE_AnimationWindow_Change_ID_Confirm")}##changeAnimationIDConfirm"))
        {
            confirmed = true;
        }
        GUI.Tooltip(LOC.Get("TAE_AnimationWindow_Change_ID_Confirm_TT"));

        ImGui.EndDisabled();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("TAE_AnimationWindow_Change_ID_Cancel")}##changeAnimationIDCancel"))
        {
            cancelled = true;
        }

        if (confirmed && !unchanged)
        {
            ApplyChangeID(source, entry, animation, newID);
        }

        if (confirmed || cancelled || !open)
        {
            ChangeIDTarget = null;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// Changes the ID as the Properties field does, so the step undoes the same way and the list stays in ID order
    /// </summary>
    private void ApplyChangeID(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation, long newID)
    {
        var oldName = source.GetAnimationName(entry, animation);

        View.ViewportActionManager.ExecuteAction(new TimeActSetPropertyAction<long>(entry,
            value => animation.ID = value, animation.ID, newID,
            () =>
            {
                entry.ResortAnimation(animation);

                if (View.Selection.SelectedAnimation == animation)
                {
                    View.Playback.OnAnimationChanged();
                }

                ScrollTo(animation);
            },
            LOC.Get("TAE_PropertiesWindow_Animation_ID"), animation));

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Changed_ID", oldName, source.GetAnimationName(entry, animation)));
    }
    #endregion

    #region Order / File Names / Sort

    /// <summary>
    /// Where an animation of the ID goes to keep the list in ID order, which is how the game stores them.
    /// The next free ID after one animation isn't always the next in the list (3000, 3001, 3002 gives 3003),
    /// so going in after the animation it was made from would put the list out of order.
    /// </summary>
    private static int GetSortedIndex(List<TAE.Animation> animations, long id)
    {
        var index = animations.FindIndex(e => e.ID > id);

        return index < 0 ? animations.Count : index;
    }

    /// <summary>
    /// The file name of an animation often holds its own name, which is changed to the name of the new ID.
    /// Otherwise the fallback is used.
    /// </summary>
    public static string GetAnimFileName(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation template,
        long id, string fallback)
    {
        var fileName = template?.AnimFileName;

        if (string.IsNullOrEmpty(fileName))
            return fallback ?? "";

        var oldName = source.GetAnimationName(entry, template);

        if (!fileName.Contains(oldName, StringComparison.OrdinalIgnoreCase))
            return fallback ?? "";

        var newName = source.FormatAnimationName(entry.GetFullAnimationID(id, source.AnimationCategorySize));

        return fileName.Replace(oldName, newName, StringComparison.OrdinalIgnoreCase);
    }

    private void SortAnimations(TimeActEntry entry)
    {
        var action = new TimeActSortAnimationsAction(entry);

        if (!action.ChangesOrder)
            return;

        View.ViewportActionManager.ExecuteAction(action);

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Sorted", entry.Name));
    }

    #endregion

    #region Import / Export
    /// <summary>
    /// Imports a HKX file as the motion of an animation. It is stored under the animation's own ID, in the binder
    /// that already holds it, or with the other animations of its category. It is written when the source is saved.
    /// </summary>
    private void ImportAnimationFile(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation, string name)
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("TAE_AnimationWindow_Dialog_Select_Import_File"),
            ["hkx"], out var path))
            return;

        byte[] bytes;

        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Import_Failed", path), e);
            return;
        }

        if (bytes.Length == 0)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Import_Failed", path));
            return;
        }

        var id = entry.GetFullAnimationID(animation, source.AnimationCategorySize);

        List<string> binderPaths;

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

            if (files.Contains(id))
            {
                var result = PlatformUtils.Instance.MessageBox(
                    LOC.Get("TAE_AnimationWindow_Dialog_Import_Replace", name),
                    LOC.Get("SYS_Warning_Header"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            binderPaths = !files.IsStored(id) && source.PendingAnimationFiles.TryGetValue(id, out var pending)
                ? pending.BinderPaths.ToList()
                : files.GetImportBinderPaths(id);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Read_Failed", source.Name), e);
            return;
        }

        View.ViewportActionManager.ExecuteAction(new TimeActImportAnimationFileAction(source,
            new TimeActPendingAnimationFile(id, bytes, binderPaths)));

        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Imported", path, name, string.Join(", ", binderPaths)));

        // The game plays the motion the animation points to, which isn't its own
        if (ResolveAnimationFileID(source, entry, animation, null) is var target && target != id)
        {
            Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Import_Uses_Other", name, source.FormatAnimationName(target)));
        }
    }

    /// <summary>
    /// Writes the HKX files that the animations point to, under a folder named after their source.
    /// Animations that import another export the file they import. Unsaved imports are included.
    /// </summary>
    private void ExportAnimationFiles(TimeActContainerWrapper source, TimeActEntry entry,
        List<(TAE.Animation Animation, string Name)> animations)
    {
        if (animations.Count == 0)
            return;

        if (!Directory.Exists(CFG.Current.TimeActEditor_Export_Directory) && !TimeActSelectionWindow.SelectExportFolder())
            return;

        var folder = Path.Join(CFG.Current.TimeActEditor_Export_Directory, source.Name, "hkx");

        TimeActAnimationFiles files;

        try
        {
            files = TimeActAnimationFiles.Open(source, true);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Read_Failed", source.Name), e);
            return;
        }

        using (files)
        {
            // Animations that import the same one share its file
            var ids = new List<long>();
            var missing = new List<string>();

            foreach (var (animation, name) in animations)
            {
                var id = ResolveAnimationFileID(source, entry, animation, files);

                if (!files.Contains(id))
                {
                    missing.Add(name);
                }
                else if (!ids.Contains(id))
                {
                    ids.Add(id);
                }
            }

            foreach (var name in missing)
            {
                Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_No_Animation_File", name));
            }

            if (ids.Count == 0)
                return;

            var exports = new List<(string Path, long ID)>();

            foreach (var id in ids)
            {
                exports.Add((Path.Join(folder, files.GetFileName(id)), id));
            }

            var existing = exports.Count(e => File.Exists(e.Path));

            if (existing > 0)
            {
                var message = exports.Count == 1
                    ? LOC.Get("TAE_SelectionWindow_Dialog_Overwrite_File", exports[0].Path)
                    : LOC.Get("TAE_AnimationWindow_Dialog_Overwrite_Files", existing, folder);

                var result = PlatformUtils.Instance.MessageBox(message, LOC.Get("SYS_Warning_Header"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            var exported = 0;

            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Export_Failed", folder), e);
                return;
            }

            foreach (var (exportPath, id) in exports)
            {
                try
                {
                    if (!files.TryRead(id, out var bytes))
                        continue;

                    File.WriteAllBytes(exportPath, bytes);

                    exported++;

                    if (exports.Count == 1)
                    {
                        Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Exported", exportPath));
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("TAE_AnimationWindow_Export_Failed", exportPath), e);
                }
            }

            if (exports.Count > 1)
            {
                Smithbox.Log(this, LOC.Get("TAE_AnimationWindow_Exported_All", exported, folder));
            }
        }
    }

    /// <summary>
    /// Follows the imports of an animation to the full ID of the HKX file it plays. Without the files to check
    /// against, only the animation's own import is followed.
    /// </summary>
    private static long ResolveAnimationFileID(TimeActContainerWrapper source, TimeActEntry entry,
        TAE.Animation animation, TimeActAnimationFiles files)
    {
        var size = source.AnimationCategorySize;

        var firstID = -1L;

        for (var depth = 0; depth < MaxImportDepth; depth++)
        {
            var id = entry.GetFullAnimationID(animation, size);

            long clipID = animation.MiniHeader switch
            {
                TAE.Animation.AnimMiniHeader.Standard { ImportsHKX: true } standard => standard.ImportHKXSourceAnimID,
                TAE.Animation.AnimMiniHeader.ImportOtherAnim import => import.ImportFromAnimID,
                _ => id
            };

            if (firstID < 0)
            {
                firstID = clipID;
            }

            if (files == null)
                return clipID;

            if (files.Contains(clipID))
                return clipID;

            if (clipID == id || !source.TryFindAnimation(clipID, out entry, out animation))
                break;
        }

        return firstID;
    }
    #endregion

    public void ClearSelection()
    {
        View.Selection.SelectedAnimation = null;
        View.Selection.SelectedEvent = null;

        View.Playback.OnAnimationChanged();
    }

    /// <summary>
    /// Brings an animation into view in the list, once it is next drawn
    /// </summary>
    public void ScrollTo(TAE.Animation animation)
    {
        ScrollTarget = animation;
    }

    public void SelectAnimation(TAE.Animation animation)
    {
        var selection = View.Selection;

        var template = Project.Handler.TimeActData.GetTemplate(selection.SelectedTimeActContainerWrapper);

        selection.SelectedTimeAct.ApplyTemplate(animation, template);

        selection.SelectedAnimation = animation;
        selection.SelectedEvent = null;

        View.Playback.OnAnimationChanged();
    }
}
