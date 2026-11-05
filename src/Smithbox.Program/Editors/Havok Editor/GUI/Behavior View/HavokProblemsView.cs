using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The validator's results for the shown behavior file. Checked when the tab is first shown for a file,
/// and again with Check. Clicking a problem shows its object in the Outline.
/// </summary>
public class HavokProblemsView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    private List<HavokBehaviorProblem> Problems;

    public HavokProblemsView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        Problems = null;
    }

    public void Setup(object sourceObject)
    {
        // Kept until the next check, but a structural edit can remove what a problem points at
        if (Problems != null && Owner.Symbols != null)
            Problems.RemoveAll(p => !Owner.Symbols.Contains(p.Target));
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    private void Check()
    {
        Problems = HavokBehaviorValidator.Run(Project, Owner.Symbols);
    }

    public void DisplayTab()
    {
        if (Owner.Symbols == null)
        {
            SetTabState(false);
            return;
        }

        // A file shown for the first time is checked straight away
        if (Problems == null)
            Check();

        var label = Problems.Count > 0
            ? LOC.Get("HAVOK_BehaviorView_Tab_Problems_Count", Problems.Count)
            : LOC.Get("HAVOK_BehaviorView_Tab_Problems");

        if (ImGui.BeginTabItem($"{label}###tabProblems"))
        {
            SetTabState(true);

            DisplayTabHeader();
            DisplayTabContents();

            ImGui.EndTabItem();
        }
        else
        {
            SetTabState(false);
        }
    }

    private void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        if (ImGui.Button($"{LOC.Get("HAVOK_ProblemsView_Check")}##problemsCheck"))
            Check();

        GUI.Tooltip(LOC.Get("HAVOK_ProblemsView_Check_TT"));

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();

        if (Problems.Count == 0)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("HAVOK_ProblemsView_None"));
        else
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_ProblemsView_Count", Problems.Count));

        ImGui.EndChild();
    }

    private void DisplayTabContents()
    {
        ImGui.BeginChild("havokBehaviorElementListSection");

        var clipper = new ImGuiListClipper();
        clipper.Begin(Problems.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var problem = Problems[i];

                if (ImGui.Selectable($"{problem.Message}##problem{i}"))
                    Owner.OutlineView.Navigate(problem.Target);

                GUI.Tooltip(LOC.Get("HAVOK_ProblemsView_Entry_TT", HavokBehaviorSymbols.GetLabel(problem.Target)));
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
}
