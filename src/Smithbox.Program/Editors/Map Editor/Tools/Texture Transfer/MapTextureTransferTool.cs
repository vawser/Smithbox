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
/// Texture Transfer: finds the map textures the selected map's map pieces and objects use that its map texture binders
/// don't hold, and copies them in from the binders that do (Claude/Documentation/Map Texture Transfer.md).
/// </summary>
public class MapTextureTransferTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private TextureTransferResult Result;
    private string ResultMapId;

    // Background work: a scan or an add
    private CancellationTokenSource Cancellation;
    private volatile bool Running;
    private volatile int ProgressDone;
    private volatile int ProgressTotal;
    private volatile string ProgressText = "";
    private string RunningLabel = "";
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();
    private string StatusText = "";

    // Lists
    private bool ShowMissing = true;
    private bool ShowNotFound = false;
    private bool ShowProblems = false;
    private string MissingFilter = "";
    private bool MissingFilterExact = false;

    // Confirm
    private bool OpenConfirm;
    private List<TextureTransferRequest> PendingRequests;
    private bool PendingTargetHasEdits;

    public MapTextureTransferTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool CanUse() => MapTextureTransferProfile.IsSupported(Project.Descriptor.ProjectType);

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAP_TextureTransfer_Header")}##mapTextureTransferToolHeader"))
            return;

        ImGui.BeginChild("MapTextureTransferToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        // Results and completions of the background work
        while (MainThreadQueue.TryDequeue(out var action))
            action();

        GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Description"));

        var map = GetSelectedMap();

        if (map == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_TextureTransfer_No_Map"));
            DisplayProgress();
            return;
        }

        var key = MapTextureTransferProfile.KeyOfMap(Project.Descriptor.ProjectType, map.Name);

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_TextureTransfer_Map_Header"), LOC.Get("MAP_TextureTransfer_Map_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Map", map.Name, key));

        var targets = GetBinderEntries().Where(e => MapTextureTransferProfile.KeyOfBinder(Project.Descriptor.ProjectType, e.Path) == key).ToList();

        if (targets.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_TextureTransfer_No_Target"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Targets", string.Join(", ", targets.Select(t => Path.GetFileName(t.Path)))));
        }

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_TextureTransfer_Scope_Header"), LOC.Get("MAP_TextureTransfer_Scope_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_TextureTransfer_Selection_Only")}##mapTextureTransferSelectionOnly", ref CFG.Current.MapEditor_TextureTransfer_SelectionOnly);
        GUI.Tooltip(LOC.Get("MAP_TextureTransfer_Selection_Only_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_TextureTransfer_Actions_Header"), LOC.Get("MAP_TextureTransfer_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("mapTextureTransferScanActions",
            "mapTextureTransferScan", LOC.Get("MAP_TextureTransfer_Scan"), LOC.Get("MAP_TextureTransfer_Scan_TT"),
            () => StartScan(map, key), !Running && targets.Count > 0,

            "mapTextureTransferClear", LOC.Get("MAP_TextureTransfer_Clear"), LOC.Get("MAP_TextureTransfer_Clear_TT"),
            Clear, !Running && Result != null);

        DisplayProgress();

        if (Result == null)
            return;

        if (ResultMapId != map.Name)
        {
            GUI.Spacer();
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_TextureTransfer_Stale", ResultMapId));
            return;
        }

        DisplayResults(targets);
        DisplayConfirm(targets);
    }

    #region Results
    private void DisplayResults(List<FileDictionaryEntry> targets)
    {
        var result = Result;

        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_TextureTransfer_Summary_Header"), LOC.Get("MAP_TextureTransfer_Summary_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Summary", result.UsedCount, result.ModelCount, result.PresentCount,
            result.OwnTpfCount, result.Missing.Count, result.NotFound.Count));

        // Missing
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_TextureTransfer_Missing_Header"), LOC.Get("MAP_TextureTransfer_Missing_Header_TT"), ref ShowMissing);

        if (result.Missing.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_TextureTransfer_None_Missing"));
        }
        else
        {
            var checkedCount = result.Missing.Count(m => m.Checked);

            GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Checked", checkedCount, result.Missing.Count));

            GUI.ConditionalMultiButtonInput("mapTextureTransferAddActions",
                "mapTextureTransferAddChecked", LOC.Get("MAP_TextureTransfer_Add_Checked", checkedCount), LOC.Get("MAP_TextureTransfer_Add_Checked_TT"),
                () => RequestAdd(result.Missing.Where(m => m.Checked), targets), !Running && checkedCount > 0,

                "mapTextureTransferCheckAll", LOC.Get("MAP_TextureTransfer_Check_All"), LOC.Get("MAP_TextureTransfer_Check_All_TT"),
                () => SetChecked(true), !Running,

                "mapTextureTransferCheckNone", LOC.Get("MAP_TextureTransfer_Check_None"), LOC.Get("MAP_TextureTransfer_Check_None_TT"),
                () => SetChecked(false), !Running);

            if (ShowMissing)
                DisplayMissingList(result);
        }

        // Not Found
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_TextureTransfer_Not_Found_Header"), LOC.Get("MAP_TextureTransfer_Not_Found_Header_TT"), ref ShowNotFound);

        GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Not_Found_Count", result.NotFound.Count));

        if (ShowNotFound && result.NotFound.Count > 0)
            DisplayNotFoundList(result);

        // Problems
        if (result.Problems.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(LOC.Get("MAP_TextureTransfer_Problems_Header"), LOC.Get("MAP_TextureTransfer_Problems_Header_TT"), ref ShowProblems);

            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_TextureTransfer_Problems_Count", result.Problems.Count));

            if (ShowProblems)
            {
                ImGui.BeginChild("mapTextureTransferProblems", new Vector2(-1, 150 * DPI.UIScale()), ImGuiChildFlags.Borders);

                foreach (var problem in result.Problems)
                    GUI.WrappedText(problem);

                ImGui.EndChild();
            }
        }
    }

    private void DisplayMissingList(TextureTransferResult result)
    {
        EditorFilters.DisplayFramedListFilter("mapTextureTransferMissingFilter", ref MissingFilter, ref MissingFilterExact);

        var rows = result.Missing.Where(m => EditorFilters.IsMatch(MissingFilter, m.Name, MissingFilterExact)).ToList();

        ImGui.BeginChild("mapTextureTransferMissingList", new Vector2(-1, 300 * DPI.UIScale()), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);

        if (ImGui.BeginTable("mapTextureTransferMissingTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##check", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch, 0.6f);
            ImGui.TableSetupColumn("##source", ImGuiTableColumnFlags.WidthStretch, 0.4f);

            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var row = rows[i];

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Checkbox($"##mapTextureTransferCheck_{row.Name}", ref row.Checked);
                    GUI.Tooltip(LOC.Get("MAP_TextureTransfer_Row_Check_TT"));

                    ImGui.TableSetColumnIndex(1);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(row.Name);
                    GUI.Tooltip(LOC.Get("MAP_TextureTransfer_Row_TT", string.Join(", ", row.UsedBy), string.Join("\n", row.TexturePaths)));

                    ImGui.TableSetColumnIndex(2);
                    ImGui.SetNextItemWidth(-1);

                    if (row.Sources.Count == 1)
                    {
                        ImGui.AlignTextToFramePadding();
                        ImGui.TextUnformatted(Path.GetFileName(row.Source));
                        GUI.Tooltip(LOC.Get("MAP_TextureTransfer_Row_Source_TT", row.Source));
                    }
                    else if (ImGui.BeginCombo($"##mapTextureTransferSource_{row.Name}", Path.GetFileName(row.Source)))
                    {
                        for (var s = 0; s < row.Sources.Count; s++)
                        {
                            var label = s == 0
                                ? LOC.Get("MAP_TextureTransfer_Source_Preferred", Path.GetFileName(row.Sources[s]))
                                : Path.GetFileName(row.Sources[s]);

                            if (ImGui.Selectable($"{label}##mapTextureTransferSource_{row.Name}_{s}", s == row.SourceIndex))
                                row.SourceIndex = s;
                        }

                        ImGui.EndCombo();
                    }
                    else
                    {
                        GUI.Tooltip(LOC.Get("MAP_TextureTransfer_Row_Sources_TT", row.Sources.Count));
                    }
                }
            }

            clipper.End();
            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void DisplayNotFoundList(TextureTransferResult result)
    {
        ImGui.BeginChild("mapTextureTransferNotFoundList", new Vector2(-1, 200 * DPI.UIScale()), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);

        var clipper = new ImGuiListClipper();
        clipper.Begin(result.NotFound.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = result.NotFound[i];
                ImGui.TextUnformatted(row.Name);
                GUI.Tooltip(LOC.Get("MAP_TextureTransfer_Row_TT", string.Join(", ", row.UsedBy), row.TexturePath));
            }
        }

        clipper.End();
        ImGui.EndChild();
    }

    private void SetChecked(bool value)
    {
        foreach (var row in Result?.Missing ?? new())
        {
            if (EditorFilters.IsMatch(MissingFilter, row.Name, MissingFilterExact))
                row.Checked = value;
        }
    }

    private void Clear()
    {
        Cancellation?.Cancel();
        Result = null;
        ResultMapId = null;
        StatusText = "";
    }
    #endregion

    #region Progress
    private void DisplayProgress()
    {
        if (!Running && string.IsNullOrEmpty(StatusText))
            return;

        // Progress
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_TextureTransfer_Progress_Header"), "");

        if (Running)
        {
            var total = Math.Max(ProgressTotal, 1);
            ImGui.ProgressBar(Math.Min(ProgressDone, total) / (float)total, new Vector2(-1, 0), RunningLabel);
            GUI.WrappedText(ProgressText ?? "");

            GUI.ConditionalMultiButtonInput("mapTextureTransferCancelActions",
                "mapTextureTransferCancel", LOC.Get("MAP_TextureTransfer_Cancel"), LOC.Get("MAP_TextureTransfer_Cancel_TT"),
                () => Cancellation?.Cancel(), true);
        }

        if (!string.IsNullOrEmpty(StatusText))
            GUI.WrappedText(StatusText);
    }

    private void Progress(int done, int total, string text)
    {
        ProgressDone = done;
        ProgressTotal = total;
        ProgressText = text;
    }
    #endregion

    #region Scan
    private MapContainer GetSelectedMap()
    {
        var map = View.Selection.SelectedMapContainer;

        if (map == null && View.Selection.SelectedMapID != null)
            map = View.Selection.GetMapContainerFromMapID(View.Selection.SelectedMapID);

        return map;
    }

    /// <summary>Every map texture binder of the project (gi_env, DS2's _low twins and menu binders left out)</summary>
    private List<FileDictionaryEntry> GetBinderEntries()
    {
        var type = Project.Descriptor.ProjectType;
        return (Project.Locator.TexturePackedFiles?.Entries ?? new())
            .Where(e => MapTextureTransferProfile.KeyOfBinder(type, e.Path) != null)
            .ToList();
    }

    private void StartScan(MapContainer map, string key)
    {
        // Everything the scan needs is read here, on the main thread
        var parts = map.Objects.Where(e => EntityHelper.IsPartMapPiece(e) || EntityHelper.IsPartAsset(e));

        if (CFG.Current.MapEditor_TextureTransfer_SelectionOnly)
        {
            var selected = View.ViewportSelection.GetSelection().OfType<Entity>().ToHashSet();
            parts = parts.Where(selected.Contains);
        }

        var models = CollectModels(map, parts.ToList());

        if (models.Count == 0)
        {
            StatusText = LOC.Get("MAP_TextureTransfer_No_Parts");
            return;
        }

        var input = new TextureTransferScanInput(
            Project.Descriptor.ProjectType,
            Project.VFS.FS,
            key,
            GetBinderEntries().Select(e => e.Path).ToList(),
            models);

        var mapId = map.Name;

        StartWork(LOC.Get("MAP_TextureTransfer_Scanning"), token =>
        {
            var result = MapTextureTransferScan.Run(input, token, Progress);

            MainThreadQueue.Enqueue(() =>
            {
                Result = result;
                ResultMapId = mapId;
                StatusText = "";

                foreach (var problem in result.Problems)
                    Smithbox.LogWarning(this, problem);
            });
        });
    }

    private List<TextureTransferModel> CollectModels(MapContainer map, List<Entity> parts)
    {
        var adjustedMapId = PathBuilder.GetAssetMapID(Project, map.Name);
        var byModel = new Dictionary<(bool, string), List<string>>();

        foreach (var part in parts)
        {
            string modelName;
            try
            {
                modelName = part.GetPropertyValue<string>("ModelName");
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrEmpty(modelName))
                continue;

            var key = (EntityHelper.IsPartAsset(part), modelName);
            if (!byModel.TryGetValue(key, out var names))
                byModel[key] = names = new();
            names.Add(part.Name);
        }

        var models = new List<TextureTransferModel>();

        foreach (var ((isObject, modelName), partNames) in byModel)
        {
            ResourceDescriptor asset;
            string innerName;

            if (isObject)
            {
                asset = ModelLocator.GetObjModel(Project, modelName, modelName);
                innerName = modelName;
            }
            else
            {
                innerName = ModelLocator.MapModelNameToAssetName(Project, adjustedMapId, modelName);
                asset = ModelLocator.GetMapModel(Project, adjustedMapId, innerName, innerName);
            }

            var path = asset.IsValid() ? asset.GetContainerPath(Project) : null;
            models.Add(new TextureTransferModel(modelName, isObject, path, innerName, partNames));
        }

        return models.OrderBy(m => m.ModelName, StringComparer.OrdinalIgnoreCase).ToList();
    }
    #endregion

    #region Add
    private void RequestAdd(IEnumerable<TextureTransferMissing> rows, List<FileDictionaryEntry> targets)
    {
        PendingRequests = rows.Where(r => r.Source != null).Select(r => new TextureTransferRequest(r.Name, r.Source)).ToList();

        var bank = Project.Handler.TextureData?.PrimaryBank;
        PendingTargetHasEdits = bank != null && targets.Any(t => bank.PackedEntries.TryGetValue(t, out var c) && c != null && c.IsModified);

        OpenConfirm = PendingRequests.Count > 0;
    }

    private void DisplayConfirm(List<FileDictionaryEntry> targets)
    {
        var title = $"{LOC.Get("MAP_TextureTransfer_Confirm_Title")}###mapTextureTransferConfirm";

        if (OpenConfirm)
        {
            ImGui.OpenPopup(title);
            OpenConfirm = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(480, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        GUI.WrappedText(LOC.Get("MAP_TextureTransfer_Confirm", PendingRequests?.Count ?? 0, ResultMapId,
            string.Join(", ", targets.Select(t => Path.GetFileName(t.Path)))));

        if (PendingTargetHasEdits)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_TextureTransfer_Confirm_Edits"));

        var confirmed = false;
        var cancelled = false;

        GUI.MultiButtonInput("mapTextureTransferConfirmActions",
            "mapTextureTransferConfirmAdd", LOC.Get("MAP_TextureTransfer_Confirm_Add"), LOC.Get("MAP_TextureTransfer_Confirm_Add_TT"),
            () => confirmed = true,

            "mapTextureTransferConfirmCancel", LOC.Get("MAP_TextureTransfer_Cancel"), LOC.Get("MAP_TextureTransfer_Confirm_Cancel_TT"),
            () => cancelled = true);

        if (confirmed || cancelled)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();

        if (confirmed)
            StartAdd(targets, PendingRequests);
    }

    private void StartAdd(List<FileDictionaryEntry> targets, List<TextureTransferRequest> requests)
    {
        var bank = Project.Handler.TextureData?.PrimaryBank;

        if (bank == null)
        {
            StatusText = LOC.Get("MAP_TextureTransfer_Error_No_Bank");
            Smithbox.LogError(this, StatusText);
            return;
        }

        var map = GetSelectedMap();
        var key = map != null ? MapTextureTransferProfile.KeyOfMap(Project.Descriptor.ProjectType, map.Name) : null;

        StartWork(LOC.Get("MAP_TextureTransfer_Adding"), token =>
        {
            var result = MapTextureTransferApply.Run(Project, bank, targets, requests, token, Progress);

            MainThreadQueue.Enqueue(() =>
            {
                // Marks the files as project files and reloads the scene textures that came from them
                if (result.Saved.Count > 0)
                    bank.NotifyContainersSaved(result.Saved);

                foreach (var error in result.Errors)
                    Smithbox.LogError(this, error);

                StatusText = LOC.Get("MAP_TextureTransfer_Added", result.Added, result.LowDetailAdded, result.Saved.Count);

                if (result.Skipped.Count > 0)
                    StatusText += "\n" + LOC.Get("MAP_TextureTransfer_Added_Skipped", result.Skipped.Count);

                if (result.Errors.Count > 0)
                    StatusText += "\n" + LOC.Get("MAP_TextureTransfer_Added_Errors", result.Errors.Count);

                Smithbox.Log(this, StatusText);

                // Scan again, so the list shows what is still missing
                if (map != null && key != null)
                {
                    var status = StatusText;
                    StartScan(map, key);
                    StatusText = status;
                }
            });
        });
    }
    #endregion

    private void StartWork(string label, Action<CancellationToken> work)
    {
        Cancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        Cancellation = cancellation;

        RunningLabel = label;
        ProgressDone = 0;
        ProgressTotal = 1;
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
                MainThreadQueue.Enqueue(() => StatusText = LOC.Get("MAP_TextureTransfer_Cancelled"));
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException)
            {
                MainThreadQueue.Enqueue(() => StatusText = LOC.Get("MAP_TextureTransfer_Cancelled"));
            }
            catch (Exception e)
            {
                MainThreadQueue.Enqueue(() =>
                {
                    StatusText = LOC.Get("MAP_TextureTransfer_Failed", e.Message);
                    Smithbox.LogError(this, StatusText, e);
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
}
