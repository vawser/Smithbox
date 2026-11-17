using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using System.Diagnostics;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Applies the active SpEffectParam row to the player in the running game (e.g. an NR dormant power).
/// Supported when the selected Param Reloader offsets define spEffectGive.
/// </summary>
public class SpEffectGib
{
    public ParamEditorView View;
    public ProjectEntry Project;

    private readonly ItemGib itemGib;

    public SpEffectGib(ParamEditorView view, ProjectEntry project, ItemGib itemGib)
    {
        View = view;
        Project = project;
        this.itemGib = itemGib;
    }

    private SpEffectGive GetSpEffectGive(out GameOffsetsEntry offsets)
    {
        offsets = itemGib.GetGameOffsets();
        return offsets?.Bases.Select(b => b.SpEffectGive).FirstOrDefault(s => s != null);
    }

    public void Display()
    {
        var give = GetSpEffectGive(out _);
        if (give == null)
        {
            return;
        }

        if (!ToolSections.Header($"{LOC.Get("PARAM_Tools_SpEffectGib_Header")}##spEffectGibHeader"))
        {
            return;
        }

        ImGui.BeginChild("SpEffectGibSection", ImGuiChildFlags.Borders);

        DisplayContents(give);

        ImGui.EndChild();
    }

    private void DisplayContents(SpEffectGive give)
    {
        GUI.WrappedText(LOC.Get("PARAM_Tools_SpEffectGib_Hint"));

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var activeParam = activeView.Selection.GetActiveParam();
        if (activeParam != give.Param)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_Tools_SpEffectGib_Select_Param", give.Param));
            return;
        }

        var activeRow = activeView.Selection.GetActiveRow();
        if (activeRow == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_Tools_SpEffectGib_No_Active_Row"));
            return;
        }

        // SpEffect
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_SpEffectGib_SpEffect_Header"),
            LOC.Get("PARAM_Tools_SpEffectGib_SpEffect_Header_TT"));

        GUI.WrappedText(string.IsNullOrWhiteSpace(activeRow.Name) ? $"{activeRow.ID}" : $"{activeRow.ID} {activeRow.Name}");

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_SpEffectGib_Actions_Header"),
            LOC.Get("PARAM_Tools_SpEffectGib_Actions_Header_TT"));

        GUI.MultiButtonInput("spEffectGibActions",
            "giveSpEffectAction", LOC.Get("PARAM_Tools_SpEffectGib_Action_Give"), LOC.Get("PARAM_Tools_SpEffectGib_Action_Give_TT"),
            () => GiveSpEffect(activeRow.ID));
    }

    public void GiveSpEffect(int spEffectId)
    {
        var give = GetSpEffectGive(out var offsets);
        if (give == null)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_No_Offsets"), LogPriority.High);
            return;
        }

        var name = offsets.exeName.Replace(".exe", "");
        var process = Process.GetProcessesByName(name).FirstOrDefault();
        if (process == null)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Tools_Memory_No_Game_Process", offsets.exeName), LogPriority.High);
            return;
        }

        SoulsMemoryHandler memoryHandler = new(View.Editor, process);
        try
        {
            if (!memoryHandler.TryResolveItemGiveAddress("SpEffectFunc", give.Func, out var funcOffset, out var funcFromAob) ||
                !memoryHandler.TryResolveItemGiveAddress("SpEffectChrManager", give.ChrManager, out var managerOffset, out var managerFromAob))
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Tools_SpEffectGib_Unresolved"), LogPriority.High);
                return;
            }

            // A hard offset is only valid for the exe version it was taken from.
            if (!funcFromAob || !managerFromAob)
            {
                var info = process.MainModule.FileVersionInfo;
                var runningVersion = $"{info.FileMajorPart}.{info.FileMinorPart}.{info.FileBuildPart}.{info.FilePrivatePart}";
                if (runningVersion != offsets.exeVersion)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_Version_Mismatch", runningVersion, offsets.exeVersion), LogPriority.High);
                    return;
                }
            }

            var player = memoryHandler.ReadPointerPath(managerOffset, give.PlayerPath);
            if (player == 0)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Tools_SpEffectGib_No_Player"), LogPriority.High);
                return;
            }

            memoryHandler.PlayerSpEffectGive(funcOffset, player, spEffectId, give.Flag);
            Smithbox.Log(this, LOC.Get("PARAM_Tools_SpEffectGib_Given", spEffectId));
        }
        finally
        {
            memoryHandler.Terminate();
        }
    }
}
