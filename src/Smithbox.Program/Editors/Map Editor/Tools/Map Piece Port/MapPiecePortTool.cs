using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Model Browser's port of another map's map piece into the selected map: the prompt, the write, the model
/// assignment and the texture copy (Claude/Documentation/Map Piece Port.md).
/// </summary>
public class MapPiecePortTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>Assigns the ported model to the selected map pieces of the target map: model name, target map</summary>
    private readonly Action<string, string> Assign;

    private MapPiecePortPlan Plan;
    private string NumberInput = "";
    private bool CopyTextures;
    private bool OpenPrompt;

    // The texture scan of the pending port, and the work in the background
    private TextureTransferResult Scan;
    private CancellationTokenSource Cancellation;
    private volatile bool Running;
    private volatile string ProgressText = "";
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    public string StatusText { get; private set; } = "";

    public MapPiecePortTool(MapEditorView view, ProjectEntry project, Action<string, string> assign)
    {
        View = view;
        Project = project;
        Assign = assign;
    }

    private ProjectType Type => Project.Descriptor.ProjectType;

    /// <summary>
    /// Starts a port of a map piece of another map into the map the selected map pieces are in, through the prompt
    /// (only called while the Prompt Port option is on)
    /// </summary>
    public void Begin(string sourceAssetMap, string sourceStem, string targetMap)
    {
        if (Running)
        {
            StatusText = LOC.Get("MAP_MapPiecePort_Busy");
            return;
        }

        var plan = MapPiecePort.CreatePlan(Project, sourceAssetMap, sourceStem, targetMap, null, out var error);

        if (plan == null)
        {
            StatusText = error;
            return;
        }

        Plan = plan;
        NumberInput = plan.Number;
        CopyTextures = CFG.Current.MapEditor_Model_Selector_Port_Textures && MapPiecePort.CanCopyTextures(Type);
        Scan = null;
        StatusText = "";

        if (MapPiecePort.CanCopyTextures(Type))
            StartScan(plan);

        OpenPrompt = true;
    }

    /// <summary>The prompt and the status of the last port, under the browser's options</summary>
    public void Display()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();

        DisplayPrompt();

        if (Running || !string.IsNullOrEmpty(StatusText))
        {
            // Map Piece Port
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_MapPiecePort_Status_Header"), "");

            if (Running)
                GUI.WrappedText(LOC.Get("MAP_MapPiecePort_Working", ProgressText ?? ""));

            if (!string.IsNullOrEmpty(StatusText))
                GUI.WrappedText(StatusText);
        }
    }

    #region Prompt
    private void DisplayPrompt()
    {
        var title = $"{LOC.Get("MAP_MapPiecePort_Prompt_Title")}###mapPiecePortPrompt";

        if (OpenPrompt)
        {
            ImGui.OpenPopup(title);
            OpenPrompt = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(520, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var plan = Plan;

        if (plan == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_MapPiecePort_Prompt", plan.SourceStem, plan.SourceAssetMap, plan.TargetMap));

        // New ID
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapPiecePort_ID_Header"), LOC.Get("MAP_MapPiecePort_ID_Header_TT"));

        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        ImGui.InputText("##mapPiecePortNumber", ref NumberInput, 16);
        GUI.Tooltip(LOC.Get("MAP_MapPiecePort_ID_TT", plan.SourceName.Number.Length));

        var error = MapPiecePort.ValidateNumber(Project, plan, NumberInput);

        if (error != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, error);
        }
        else
        {
            var stem = MapPiecePort.TargetStem(Type, plan.SourceName, plan.TargetAssetMap, NumberInput);
            GUI.WrappedText(LOC.Get("MAP_MapPiecePort_New_Name", stem,
                ModelFileChoices.GetMapPieceModelName(Project, plan.TargetAssetMap, stem)));
        }

        // Textures
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapPiecePort_Textures_Header"), LOC.Get("MAP_MapPiecePort_Textures_Header_TT"));

        DisplayTextureSummary();

        // Actions
        GUI.Spacer();

        if (Project.VFS.ProjectFS == null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MapPiecePort_No_Project_FS"));

        GUI.WrappedText(LOC.Get("MAP_MapPiecePort_Prompt_Note"));

        var confirmed = false;
        var cancelled = false;

        GUI.ConditionalMultiButtonInput("mapPiecePortPromptActions",
            "mapPiecePortConfirm", LOC.Get("MAP_MapPiecePort_Confirm"), LOC.Get("MAP_MapPiecePort_Confirm_TT"),
            () => confirmed = true, error == null && !Running,

            "mapPiecePortCancel", LOC.Get("MAP_MapPiecePort_Cancel"), LOC.Get("MAP_MapPiecePort_Cancel_TT"),
            () => cancelled = true, true);

        if (confirmed || cancelled || !open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();

        if (confirmed)
        {
            plan.Number = NumberInput;
            Port(plan, CopyTextures);
        }
        else if (cancelled || !open)
        {
            Plan = null;
            Scan = null;
        }
    }

    private void DisplayTextureSummary()
    {
        if (MapPiecePort.TexturesAreGlobal(Type))
        {
            GUI.WrappedText(LOC.Get("MAP_MapPiecePort_Textures_Global"));
            return;
        }

        if (!MapPiecePort.CanCopyTextures(Type))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MapPiecePort_Textures_Unsupported"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_MapPiecePort_Copy_Textures")}##mapPiecePortCopyTextures", ref CopyTextures);
        GUI.Tooltip(LOC.Get("MAP_MapPiecePort_Copy_Textures_TT"));

        var scan = Scan;

        if (scan == null)
        {
            GUI.WrappedText(Running ? LOC.Get("MAP_MapPiecePort_Scanning") : LOC.Get("MAP_MapPiecePort_Scan_Failed"));
            return;
        }

        if (scan.TargetBinders.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MapPiecePort_No_Target_Binders", scan.TargetKey));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_MapPiecePort_Texture_Summary", scan.UsedCount, scan.PresentCount, scan.Missing.Count,
            string.Join(", ", scan.TargetBinders.Select(Path.GetFileName))));

        if (scan.NotFound.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MapPiecePort_Texture_Not_Found", scan.NotFound.Count));
            GUI.Tooltip(string.Join("\n", scan.NotFound.Select(n => n.Name).Take(30)));
        }
    }
    #endregion

    #region Port
    private void Port(MapPiecePortPlan plan, bool copyTextures)
    {
        var result = MapPiecePort.Write(Project, plan);

        if (result.Error != null)
        {
            StatusText = result.Error;
            Smithbox.LogError(typeof(MapPiecePortTool), result.Error);
            Plan = null;
            return;
        }

        // The selected map pieces of the target map take the new model (undoable)
        Assign(result.ModelName, plan.TargetMap);

        StatusText = LOC.Get("MAP_MapPiecePort_Done", plan.SourceStem, result.TargetStem, result.TargetPath);
        Plan = null;

        if (!copyTextures || !MapPiecePort.CanCopyTextures(Type))
            return;

        var bank = Project.Handler.TextureData?.PrimaryBank;

        if (bank == null)
        {
            StatusText += "\n" + LOC.Get("MAP_MapPiecePort_No_Texture_Bank");
            return;
        }

        var key = MapTextureTransferProfile.KeyOfMap(Type, plan.TargetMap);
        var targets = GetBinderEntries().Where(e => MapTextureTransferProfile.KeyOfBinder(Type, e.Path) == key).ToList();

        if (targets.Count == 0)
        {
            StatusText += "\n" + LOC.Get("MAP_MapPiecePort_No_Target_Binders", key);
            return;
        }

        var input = ScanInput(plan);
        var scan = Scan;
        var status = StatusText;

        StartWork(token =>
        {
            // The scan of the prompt, if it finished; else run now
            scan ??= MapTextureTransferScan.Run(input, token, Progress);

            var requests = scan.Missing.Where(m => m.Source != null).Select(m => new TextureTransferRequest(m.Name, m.Source)).ToList();

            if (requests.Count == 0)
            {
                MainThreadQueue.Enqueue(() => StatusText = status + "\n" + LOC.Get("MAP_MapPiecePort_Textures_None_Missing", scan.NotFound.Count));
                return;
            }

            var applied = MapTextureTransferApply.Run(Project, bank, targets, requests, token, Progress);

            MainThreadQueue.Enqueue(() =>
            {
                // Marks the files as project files and reloads the scene textures that came from them
                if (applied.Saved.Count > 0)
                    bank.NotifyContainersSaved(applied.Saved);

                foreach (var e in applied.Errors)
                    Smithbox.LogError(typeof(MapPiecePortTool), e);

                StatusText = status + "\n" + LOC.Get("MAP_MapPiecePort_Textures_Added", applied.Added, applied.LowDetailAdded,
                    applied.Saved.Count, scan.NotFound.Count);

                if (applied.Errors.Count > 0)
                    StatusText += "\n" + LOC.Get("MAP_MapPiecePort_Textures_Errors", applied.Errors.Count);

                Smithbox.Log(typeof(MapPiecePortTool), StatusText);
            });
        });
    }

    private void StartScan(MapPiecePortPlan plan)
    {
        var input = ScanInput(plan);

        StartWork(token =>
        {
            var result = MapTextureTransferScan.Run(input, token, Progress);

            MainThreadQueue.Enqueue(() =>
            {
                // Only for the port it was started for
                if (ReferenceEquals(Plan, plan))
                    Scan = result;
            });
        });
    }

    private TextureTransferScanInput ScanInput(MapPiecePortPlan plan)
    {
        // The source file: the port is the same model, so it uses the same textures
        var model = new TextureTransferModel(plan.SourceStem, false, plan.SourcePath, plan.SourceStem, new List<string> { plan.SourceStem });

        return new TextureTransferScanInput(
            Type,
            Project.VFS.FS,
            MapTextureTransferProfile.KeyOfMap(Type, plan.TargetMap),
            GetBinderEntries().Select(e => e.Path).ToList(),
            new List<TextureTransferModel> { model });
    }

    /// <summary>Every map texture binder of the project (gi_env and the like left out)</summary>
    private List<FileDictionaryEntry> GetBinderEntries()
    {
        return (Project.Locator.TexturePackedFiles?.Entries ?? new())
            .Where(e => MapTextureTransferProfile.KeyOfBinder(Type, e.Path) != null)
            .ToList();
    }

    private void Progress(int done, int total, string text)
    {
        ProgressText = total > 0 ? $"{Math.Min(done, total)}/{total} {text}" : text;
    }

    private void StartWork(Action<CancellationToken> work)
    {
        Cancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        Cancellation = cancellation;

        ProgressText = "";
        Running = true;

        Task.Run(() =>
        {
            try
            {
                work(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                MainThreadQueue.Enqueue(() =>
                {
                    StatusText += (StatusText.Length > 0 ? "\n" : "") + LOC.Get("MAP_MapPiecePort_Failed", e.Message);
                    Smithbox.LogError(typeof(MapPiecePortTool), StatusText, e);
                });
            }
            finally
            {
                // Only the latest run clears the flag
                if (ReferenceEquals(Cancellation, cancellation))
                    Running = false;
            }
        });
    }
    #endregion
}
