using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using static StudioCore.Editors.HavokEditor.HavokRegistrationPlanner;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Register in Behavior: adds the clips that time act animations need to the behavior's selectors.
/// Sent by the Time Act Editor (hkx/register/{character}/{full animation IDs}), it previews every animation and
/// what it would add, and applies the ticked rows as one undo step.
/// </summary>
public partial class HavokBehaviorActionsView
{
    private class RegisterRequest
    {
        public string Character;
        public FileDictionaryEntry Binder;
        public string FilePath;
        public List<long> FullIds;
    }

    // Waiting for its file to be shown (its symbols are built when the file is first drawn)
    private RegisterRequest PendingRegister;

    // The open dialog's request and plan
    private RegisterRequest ActiveRegister;
    private List<Row> RegisterRows = new();
    private List<Row> RegisterVisibleRows = new();
    private bool RegisterShowRegistered = false;
    private bool RegisterShowUnplannable = false;
    private bool RegisterIncludeNotAdmitted = false;
    private bool RegisterRowsDirty = true;

    private bool CloseDialogRequested = false;

    /// <summary>
    /// Opens Register in Behavior for a behavior file once it's shown (the command has just selected it)
    /// </summary>
    public void RequestRegister(string character, FileDictionaryEntry binder, string filePath, List<long> fullIds)
    {
        PendingRegister = new RegisterRequest
        {
            Character = character,
            Binder = binder,
            FilePath = filePath,
            FullIds = fullIds,
        };
    }

    private void UpdatePendingRegister()
    {
        if (PendingRegister == null)
            return;

        // Another file was picked before this one was shown
        if (View.Selection.BinderFileEntry != PendingRegister.Binder || View.Selection.FilePath != PendingRegister.FilePath)
        {
            PendingRegister = null;
            return;
        }

        if (Owner.Symbols == null || !Owner.IsShown)
            return;

        ActiveRegister = PendingRegister;
        PendingRegister = null;

        RegisterRows = Plan(Owner.Symbols, ActiveRegister.FullIds, CategorySize);
        RegisterRowsDirty = true;

        Request(Dialog.Register);
    }

    private int CategorySize => Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

    /// <summary>
    /// The behavior files of the request's binder (Behaviors/*.hkx)
    /// </summary>
    private List<string> GetRegisterFiles()
    {
        if (ActiveRegister?.Binder == null || !View.HavokBank.BehaviorBank.TryGetValue(ActiveRegister.Binder, out var files))
            return new List<string>();

        return files.Keys.Where(IsBehaviorFile).OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static bool IsBehaviorFile(string path)
    {
        return HavokBehaviorScan.IsBehaviorGraphPath(path ?? "");
    }

    private void DisplayRegister(out bool canApply)
    {
        canApply = false;

        if (ActiveRegister == null)
            return;

        var scale = DPI.UIScale();

        // File: the selectors are searched in one behavior file at a time
        var files = GetRegisterFiles();
        var current = ActiveRegister.FilePath;

        ImGui.Text(LOC.Get("HAVOK_Register_Character", ActiveRegister.Character));

        ImGui.SetNextItemWidth(360 * scale);

        if (ImGui.BeginCombo(LOC.Get("HAVOK_Register_File"), Path.GetFileName(current.Replace('\\', '/'))))
        {
            foreach (var file in files)
            {
                if (ImGui.Selectable(Path.GetFileName(file.Replace('\\', '/')), file == current) && file != current)
                {
                    // The other file is selected and shown, then the dialog reopens for it
                    View.FileView.SelectFile(file);
                    RequestRegister(ActiveRegister.Character, ActiveRegister.Binder, file, ActiveRegister.FullIds);
                    CloseDialogRequested = true;
                }
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("HAVOK_Register_File_TT"));

        if (CloseDialogRequested)
            return;

        // Summary
        var registered = RegisterRows.Count(r => r.Status is RowStatus.Registered);
        var addClip = RegisterRows.Count(r => r.Status is RowStatus.AddClip);
        var notAdmitted = RegisterRows.Count(r => r.Status is RowStatus.NotAdmitted);
        var noSelector = RegisterRows.Count(r => r.Status is RowStatus.NoSelector);

        ImGui.Text(LOC.Get("HAVOK_Register_Summary", RegisterRows.Count, addClip, registered, notAdmitted, noSelector));

        if (ImGui.Checkbox(LOC.Get("HAVOK_Register_Show_Registered"), ref RegisterShowRegistered))
            RegisterRowsDirty = true;

        ImGui.SameLine();

        if (ImGui.Checkbox(LOC.Get("HAVOK_Register_Show_No_Selector"), ref RegisterShowUnplannable))
            RegisterRowsDirty = true;

        GUI.Tooltip(LOC.Get("HAVOK_Register_Show_No_Selector_TT"));

        ImGui.Checkbox(LOC.Get("HAVOK_Register_Include_Not_Admitted"), ref RegisterIncludeNotAdmitted);
        GUI.Tooltip(LOC.Get("HAVOK_Register_Include_Not_Admitted_TT"));

        if (RegisterRowsDirty)
        {
            RegisterVisibleRows = RegisterRows.Where(r => r.Status switch
            {
                RowStatus.Registered => RegisterShowRegistered,
                RowStatus.NoSelector => RegisterShowUnplannable,
                _ => true
            }).ToList();

            RegisterRowsDirty = false;
        }

        DisplayRegisterTable(scale);

        var adds = CountAdds(RegisterRows, RegisterIncludeNotAdmitted);

        ImGui.Text(LOC.Get("HAVOK_Register_Will_Add", adds));

        canApply = adds > 0;
    }

    private void DisplayRegisterTable(float scale)
    {
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
            ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable("havokRegisterTable", 4, flags, new System.Numerics.Vector2(820, 340) * scale))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24 * scale);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Register_Column_Animation"), ImGuiTableColumnFlags.WidthStretch, 1.1f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Register_Column_Status"), ImGuiTableColumnFlags.WidthStretch, 1.3f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Register_Column_Selectors"), ImGuiTableColumnFlags.WidthStretch, 4f);
        ImGui.TableHeadersRow();

        var animations = Owner.Animations is { IsReady: true, Error: null } ? Owner.Animations : null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(RegisterVisibleRows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = RegisterVisibleRows[i];

                ImGui.TableNextRow();

                // Include
                ImGui.TableSetColumnIndex(0);

                if (row.Status is RowStatus.AddClip or RowStatus.NotAdmitted)
                {
                    ImGui.Checkbox($"##registerInclude{row.FullId}", ref row.Include);
                }

                // Animation
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(row.AnimationName);

                if (animations != null && !animations.HasAnimationFile(row.AnimationName))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, "!");
                    GUI.Tooltip(LOC.Get("HAVOK_Register_No_Animation_File_TT", row.AnimationName));
                }

                // Status
                ImGui.TableSetColumnIndex(2);

                var (statusKey, color) = row.Status switch
                {
                    RowStatus.AddClip => ("HAVOK_Register_Status_Add", UI.Current.ImGui_Benefit_Text_Color),
                    RowStatus.Registered => ("HAVOK_Register_Status_Registered", UI.Current.ImGui_Default_Text_Color),
                    RowStatus.NotAdmitted => ("HAVOK_Register_Status_Not_Admitted", UI.Current.ImGui_Warning_Text_Color),
                    _ => ("HAVOK_Register_Status_No_Selector", UI.Current.ImGui_Invalid_Text_Color)
                };

                ImGui.TextColored(color, LOC.Get(statusKey));
                GUI.Tooltip(LOC.Get(statusKey + "_TT"));

                // Selectors: what each one does
                ImGui.TableSetColumnIndex(3);

                if (row.Targets.Count == 0)
                {
                    ImGui.TextDisabled("-");
                    continue;
                }

                var forced = RegisterIncludeNotAdmitted || row.Status is RowStatus.NotAdmitted;
                var parts = row.Targets.Select(t =>
                {
                    var name = HavokBehaviorSymbols.GetValue(t.Selector, "m_name") as string ?? "";
                    var mark = t.HasClip ? "=" : t.Admitted || forced ? "+" : "-";
                    return $"{mark}{name}";
                });

                ImGui.TextWrapped(string.Join("  ", parts));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(LOC.Get("HAVOK_Register_Selectors_TT"));
                    ImGui.Separator();

                    foreach (var t in row.Targets)
                    {
                        var name = HavokBehaviorSymbols.GetValue(t.Selector, "m_name") as string ?? "";
                        var what = t.HasClip ? LOC.Get("HAVOK_Register_Target_Has")
                            : t.Admitted ? LOC.Get("HAVOK_Register_Target_Add")
                            : LOC.Get("HAVOK_Register_Target_Not_Admitted", t.OffsetType, row.Category);

                        ImGui.Text($"{name} ({t.OffsetType}): {what}");
                    }

                    ImGui.EndTooltip();
                }
            }
        }

        clipper.End();

        ImGui.EndTable();
    }

    private EditorAction ApplyRegister(HavokBehaviorContext context, out string error)
    {
        error = null;

        var action = HavokRegistrationPlanner.Apply(context, RegisterRows, RegisterIncludeNotAdmitted, out var added, out var errors);

        foreach (var e in errors.Distinct().Take(10))
            Smithbox.LogError(this, e);

        if (action == null)
        {
            error = errors.FirstOrDefault() ?? LOC.Get("HAVOK_Register_Nothing");
            return null;
        }

        Smithbox.Log(this, LOC.Get("HAVOK_Register_Done", added, ActiveRegister?.Character ?? "", errors.Count));

        return action;
    }
}
