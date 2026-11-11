using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The EMEVD Editor: event scripts as Scratch-style blocks, using Smithbox's own instruction layouts
/// (Assets/EMEVD/Layouts). P1: read-only flat blocks; see Claude/Documentation/EMEVD Block Editor Analysis.md for
/// the phases.
/// </summary>
public class EmevdEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public EmevdEditorView View;

    public EmevdEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new EmevdEditorView(this, project);
    }

    public string EditorName => "EMEVD Editor##EmevdEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    private EmevdBank Bank => Project.Handler.EmevdData?.PrimaryBank;

    public void OnGUI(string[] commands)
    {
        MonitorShortcuts();
        ParseCommands(commands);

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_EmevdEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_EmevdEditor);

        View.Display(dsid);
    }

    /// <summary>
    /// emevd/select/&lt;file&gt;[/&lt;event id&gt;[/&lt;instruction index&gt;]], e.g. emevd/select/m10_00_00_00/10000/3
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        if (commands == null || commands.Length < 2 || commands[0] != "select" || Bank == null)
            return;

        var file = Bank.FindFile(commands[1]);
        if (file == null)
            return;

        View.Selection.SelectFile(file);
        View.FileList.ScrollToSelection = true;

        if (commands.Length > 2 && long.TryParse(commands[2], out var id) && file.EnsureLoaded())
        {
            var ev = file.FindEvent(id);
            var index = commands.Length > 3 && int.TryParse(commands[3], out var i) ? i : -1;

            if (ev != null)
                View.Selection.JumpTo(file, ev, index);
        }
    }

    private void MonitorShortcuts()
    {
        // Nothing to save or undo until P2 (argument editing)
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_View_Toggle_Arg_Names")}##argNamesToggle"))
            {
                CFG.Current.EmevdEditor_Show_Arg_Names = !CFG.Current.EmevdEditor_Show_Arg_Names;
            }
            GUI.Tooltip(LOC.Get("EMEVD_View_Toggle_Arg_Names_TT"));
            GUI.ShowActiveStatus(CFG.Current.EmevdEditor_Show_Arg_Names);

            ImGui.EndMenu();
        }
    }

    public List<string> GetUnsavedFiles()
    {
        return new();
    }

    public void SaveUnsaved()
    {
    }

    public void Save()
    {
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
