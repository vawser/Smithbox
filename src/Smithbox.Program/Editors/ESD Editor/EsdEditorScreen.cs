using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The ESD Editor: EzState machines (NPC talk, character, menu, DS2 AI/event scripts) as blocks, with names from
/// Smithbox's catalogue (Assets/ESD/Catalogues, from ESDLang's documentation, used with permission). P1: a
/// read-only state view; see Claude/Documentation/ESD Block Editor Analysis.md for the phases.
/// </summary>
public class EsdEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public EsdEditorView View;

    public EsdEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new EsdEditorView(this, project);
    }

    public string EditorName => "ESD Editor##EsdEditor";
    public string CommandEndpoint => "esd";
    public string SaveType => "ESD";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    private EsdBank Bank => Project.Handler.EsdData?.PrimaryBank;

    public void OnGUI(string[] commands)
    {
        ParseCommands(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_EsdEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_EsdEditor);

        View.Display(dsid);
    }

    /// <summary>
    /// esd/select/&lt;container&gt;[/&lt;esd&gt;[/&lt;machine id&gt;]], e.g.
    /// esd/select/m10_00_00_00.talkesdbnd/t100000/1 or esd/select/c0000//1 for a loose file
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        if (commands == null || commands.Length < 2 || commands[0] != "select" || Bank == null)
            return;

        var container = Bank.FindContainer(commands[1]);
        if (container == null || !container.EnsureLoaded())
            return;

        View.Selection.SelectContainer(container);
        View.FileList.ScrollToSelection = true;

        if (commands.Length > 2 && !string.IsNullOrEmpty(commands[2]))
        {
            var file = container.Files.FirstOrDefault(f => f.Name.Equals(commands[2], StringComparison.OrdinalIgnoreCase));
            if (file != null)
                View.Selection.SelectFile(file);
        }

        if (commands.Length > 3 && long.TryParse(commands[3], out var machine))
            View.Selection.SelectMachine(machine);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // P1 is read-only: nothing to save yet
            ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", "", false, false);
            GUI.Tooltip(LOC.Get("ESD_Menubar_Save_TT"));

            ImGui.EndMenu();
        }
    }

    public void Save()
    {
        Smithbox.Log(this, LOC.Get("ESD_Save_Read_Only"));
    }

    public void OnDefocus()
    {
    }
}
