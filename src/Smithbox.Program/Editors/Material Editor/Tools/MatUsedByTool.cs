using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Used By (F5): which models use the selected material, from the model scan in <see cref="MaterialUsage"/>
/// </summary>
public class MatUsedByTool
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    public MatUsedByTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialUsage Usage => View.Editor.Usage;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAT_UsedByTool_Header")}##materialUsedByToolHeader"))
            return;

        ImGui.BeginChild("MaterialUsedByToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAT_UsedBy_Not_Scanned"));

        // Scan
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_UsedByTool_Scan_Header"), LOC.Get("MAT_UsedByTool_Scan_Header_TT"));

        if (Usage.IsScanning)
        {
            var fraction = Usage.Total == 0 ? 0 : (float)Usage.Done / Usage.Total;
            ImGui.ProgressBar(fraction, new Vector2(-1, 0), $"{Usage.Done} / {Usage.Total}");
            GUI.WrappedText(Usage.Status);

            GUI.MultiButtonInput("materialUsedByCancelActions",
                "materialUsedByCancel", LOC.Get("MAT_UsedBy_Cancel"), LOC.Get("MAT_UsedByTool_Cancel_TT"),
                Usage.Cancel);

            return;
        }

        if (Usage.HasScan)
        {
            GUI.WrappedText(LOC.Get("MAT_UsedBy_Scanned_At", Usage.ScannedAt?.ToLocalTime().ToString("g"), Usage.FlverCount));

            if (Usage.IsStale)
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAT_UsedBy_Stale"));
        }

        GUI.MultiButtonInput("materialUsedByScanActions",
            "materialUsedByScan", Usage.HasScan ? LOC.Get("MAT_UsedBy_Rescan") : LOC.Get("MAT_UsedBy_Scan"),
            LOC.Get("MAT_UsedBy_Scan_TT"),
            Usage.StartScan);

        // Models
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_UsedByTool_Models_Header"), LOC.Get("MAT_UsedByTool_Models_Header_TT"));

        var file = View.Selection.SelectedFileWrapper;

        if (file == null)
        {
            GUI.WrappedText(LOC.Get("MAT_Tool_No_Material"));
            return;
        }

        if (!Usage.HasScan)
        {
            GUI.WrappedText(LOC.Get("MAT_UsedByTool_Needs_Scan"));
            return;
        }

        var shortName = MaterialBank.ShortNameOf(file.Key);
        var uses = Usage.UsesOf(shortName);

        GUI.WrappedText(LOC.Get("MAT_UsedByTool_Count", shortName, uses.Count));

        if (uses.Count == 0)
            return;

        if (ImGui.BeginChild("materialUsedByList", ImGuiChildFlags.Borders))
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(uses.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var use = uses[i];
                    var label = use.Flver == use.ContainerName
                        ? $"{use.Kind}: {use.ContainerName}"
                        : $"{use.Kind}: {use.ContainerName} · {use.Flver}";

                    if (ImGui.Selectable($"{label}##usedBy{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/load/{use.ContainerName}/{use.Kind}");
                    }
                    GUI.Tooltip(LOC.Get("MAT_UsedBy_Open_TT", use.Container));
                }
            }

            clipper.End();
        }

        ImGui.EndChild();
    }
}
