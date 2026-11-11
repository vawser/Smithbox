using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Called By: the Initialize Event calls that start the selected event. The game takes the calling file's own event
/// first, then common_func's, then common's, so a map event can only be started from its own file (searched at once),
/// while a common/common_func event can be started from any file (searched on request, in the background).
/// </summary>
public class EmevdCalledByTool
{
    private readonly EmevdEditorView View;
    private readonly EmevdScan Scan;

    private bool ShowResults = true;

    // The event the results are for, to search a map event again when the selection changes
    private EMEVD.Event ScannedEvent;
    private int ScannedRevision = -1;

    public EmevdCalledByTool(EmevdEditorView view)
    {
        View = view;
        Scan = new(view, "emevdCalledBy");
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EMEVD_CalledByTool_Header")}##emevdCalledByToolHeader"))
            return;

        ImGui.BeginChild("EmevdCalledByToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("EMEVD_CalledByTool_Hint"));

        var file = View.Selection.SelectedFile;
        var ev = View.Selection.SelectedEvent;

        if (file is not { IsLoaded: true } || ev == null)
        {
            GUI.WrappedText(LOC.Get("EMEVD_CalledByTool_No_Event"));
            return;
        }

        // Event
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_CalledByTool_Event_Header"), LOC.Get("EMEVD_CalledByTool_Event_Header_TT"));
        GUI.WrappedText(LOC.Get(file.IsCommon ? "EMEVD_CalledByTool_Scope_All" : "EMEVD_CalledByTool_Scope_File",
            EmevdFormat.EventLabel(ev), file.Name));

        if (file.IsCommon)
        {
            // Actions: every file is read, so only on request
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("EMEVD_CalledByTool_Actions_Header"), LOC.Get("EMEVD_CalledByTool_Actions_Header_TT"));

            GUI.MultiButtonInput("emevdCalledByActions",
                "emevdCalledByFind", LOC.Get("EMEVD_CalledByTool_Find"), LOC.Get("EMEVD_CalledByTool_Find_TT"),
                () => Run(file, ev));
        }
        else if (ev != ScannedEvent || file.Revision != ScannedRevision)
        {
            // A map event: its own file only, so search straight away
            Run(file, ev);
        }

        Scan.DrawProgress();
        Scan.DrawResults("EMEVD_CalledByTool_Results_Header", ref ShowResults);
    }

    private void Run(EmevdFileWrapper target, EMEVD.Event ev)
    {
        ScannedEvent = ev;
        ScannedRevision = target.Revision;

        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        var catalog = View.Project.Handler.EmevdData?.Catalog;
        if (bank == null)
            return;

        var id = ev.ID;
        var files = target.IsCommon ? bank.Files.ToList() : new List<EmevdFileWrapper> { target };

        Scan.Start(LOC.Get("EMEVD_CalledByTool_Subject", id), files, f =>
        {
            // Does a call from f to this ID reach the selected event?
            var (resolvedFile, resolvedEvent) = bank.ResolveEvent(f, id);
            return EmevdEventOps.CallsTo(catalog, f.Name, f.Emevd, id, resolvedFile == target && resolvedEvent == ev);
        });
    }
}
