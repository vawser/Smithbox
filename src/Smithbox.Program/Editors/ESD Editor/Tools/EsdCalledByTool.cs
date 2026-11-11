using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Called By (P5): every call block in the file that starts the selected machine, with the machine and state it's in.
/// A click selects the call. Calls never cross files, so this is the machine's whole list of callers.
/// </summary>
public class EsdCalledByTool
{
    private readonly EsdEditorView View;

    private EsdFileWrapper CachedFile;
    private long? CachedMachine;
    private int CachedRevision = -1;
    private List<EsdSearchHit> Hits = new();

    public EsdCalledByTool(EsdEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ESD_CalledByTool_Header")}##esdCalledByToolHeader"))
            return;

        ImGui.BeginChild("EsdCalledByToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("ESD_CalledByTool_Hint"));

        var selection = View.Selection;
        var file = selection.SelectedFile;
        if (file?.Esd == null || selection.SelectedMachine is not long mid)
        {
            GUI.WrappedText(LOC.Get("ESD_States_No_Machine"));
            return;
        }

        var revision = file.Container.Revision;
        if (file != CachedFile || mid != CachedMachine || revision != CachedRevision)
        {
            CachedFile = file;
            CachedMachine = mid;
            CachedRevision = revision;
            Hits = EsdSearch.CallsTo(file.Esd, mid);
        }

        // Callers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ESD_CalledByTool_Callers_Header", EsdFormat.MachineLabel(file, mid), Hits.Count), LOC.Get("ESD_CalledByTool_Callers_Header_TT"));

        if (Hits.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get(EsdLayout.IsSubMachine(mid) ? "ESD_CalledByTool_None_Sub" : "ESD_CalledByTool_None_Entry"));
            return;
        }

        ImGui.BeginChild("esdCalledByList", Vector2.Zero, ImGuiChildFlags.Borders);
        for (var i = 0; i < Hits.Count; i++)
        {
            var h = Hits[i];
            if (ImGui.Selectable($"{EsdFormat.MachineLabel(file, h.Machine)} · {LOC.Get("ESD_States_State_Title", h.State)}##esdCaller{i}"))
            {
                selection.GoToMachine(h.Machine);
                selection.SelectItem(h.Command, h.State);
            }
        }
        ImGui.EndChild();
    }
}
