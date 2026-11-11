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

/// <summary>A part of the source map that can be transferred</summary>
public class PartTransferRow
{
    public MsbEntity Entity;
    public string ModelName;
    public bool IsMapPiece;
    public bool Checked;
}

/// <summary>A map piece model of the checked parts, and the copy it becomes in the target map</summary>
public class PartTransferPort
{
    public string SourceModel;
    public MapPiecePortPlan Plan;
    public MapPiecePortResult Result;
    public int PartCount;
}

/// <summary>
/// Part Transfer: copies map pieces and objects of another loaded map into the selected map. Map pieces belong to a
/// map, so their models are ported under new IDs of the target map; objects are shared, but most use their area's
/// map textures, which are copied too (Claude/Documentation/Map Part Transfer.md).
/// </summary>
public class MapPartTransferTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private (string, ObjectContainer) SourceTarget = MapTargetPicker.None;

    // The source map's parts, rebuilt when the source or its object count changes
    private List<PartTransferRow> Rows = new();
    private (MapContainer Map, int Count) RowsKey;
    private string Filter = "";
    private bool FilterExact = false;
    private bool ShowParts = true;

    // The transfer being confirmed
    private bool OpenConfirm;
    private List<PartTransferRow> PendingRows;
    private List<PartTransferPort> PendingPorts;
    private List<string> PendingProblems;
    private MapContainer PendingTarget;
    private MapContainer PendingSource;
    private bool PendingCopyTextures;
    private TextureTransferResult PendingScan;

    // Background work
    private CancellationTokenSource Cancellation;
    private volatile bool Running;
    private volatile string ProgressText = "";
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();
    private string StatusText = "";

    public MapPartTransferTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ProjectType Type => Project.Descriptor.ProjectType;

    public bool CanUse() => Type is not ProjectType.Undefined;

    private bool CanPortMapPieces => MapPiecePort.IsSupported(Type);
    private bool CanCopyTextures => MapTextureTransferProfile.IsSupported(Type);

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAP_PartTransfer_Header")}##mapPartTransferToolHeader"))
            return;

        ImGui.BeginChild("MapPartTransferToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();

        GUI.WrappedText(LOC.Get("MAP_PartTransfer_Description"));

        var target = GetSelectedMap();

        if (target == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_No_Map"));
            DisplayProgress();
            return;
        }

        // Target Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Target_Header"), LOC.Get("MAP_PartTransfer_Target_Header_TT"));

        GUI.WrappedText(MapLabel(target.Name));

        // Source Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Source_Header"), LOC.Get("MAP_PartTransfer_Source_Header_TT"));

        MapTargetPicker.Display(View, "mapPartTransferSource", ref SourceTarget, allowActiveMap: false);

        var source = View.Selection.ResolveTargetMap(SourceTarget);

        if (source == null)
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_No_Source"));
            DisplayProgress();
            return;
        }

        if (source == target)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_PartTransfer_Same_Map"));
            DisplayProgress();
            return;
        }

        var sameAssets = SameAssetMap(source, target);

        if (sameAssets)
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Same_Assets", PathBuilder.GetAssetMapID(Project, target.Name)));

        if (!CanPortMapPieces)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_PartTransfer_No_Map_Pieces"));

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Options_Header"), LOC.Get("MAP_PartTransfer_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_PartTransfer_Move_To_Orb")}##mapPartTransferMoveToOrb", ref CFG.Current.MapEditor_PartTransfer_Move_To_Orb);
        GUI.Tooltip(LOC.Get("MAP_PartTransfer_Move_To_Orb_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_PartTransfer_Rename_Parts")}##mapPartTransferRenameParts", ref CFG.Current.MapEditor_PartTransfer_Rename_Parts);
        GUI.Tooltip(LOC.Get("MAP_PartTransfer_Rename_Parts_TT"));

        if (CanCopyTextures)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_PartTransfer_Copy_Textures")}##mapPartTransferCopyTextures", ref CFG.Current.MapEditor_PartTransfer_Copy_Textures);
            GUI.Tooltip(LOC.Get("MAP_PartTransfer_Copy_Textures_TT"));
        }
        else if (MapPiecePort.TexturesAreGlobal(Type))
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Textures_Global"));
        }

        var rows = GetRows(source);
        var checkedRows = rows.Where(r => r.Checked && (CanPortMapPieces || !r.IsMapPiece)).ToList();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Actions_Header"), LOC.Get("MAP_PartTransfer_Actions_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_PartTransfer_Checked", checkedRows.Count(r => r.IsMapPiece), checkedRows.Count(r => !r.IsMapPiece)));

        GUI.ConditionalMultiButtonInput("mapPartTransferActions",
            "mapPartTransferTransfer", LOC.Get("MAP_PartTransfer_Transfer", checkedRows.Count), LOC.Get("MAP_PartTransfer_Transfer_TT"),
            () => RequestTransfer(source, target, checkedRows), !Running && checkedRows.Count > 0,

            "mapPartTransferCheckSelected", LOC.Get("MAP_PartTransfer_Check_Selected"), LOC.Get("MAP_PartTransfer_Check_Selected_TT"),
            () => CheckSelected(rows), !Running,

            "mapPartTransferCheckNone", LOC.Get("MAP_PartTransfer_Check_None"), LOC.Get("MAP_PartTransfer_Check_None_TT"),
            () => SetChecked(rows, false), !Running);

        DisplayProgress();

        // Parts
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_PartTransfer_Parts_Header"), LOC.Get("MAP_PartTransfer_Parts_Header_TT"), ref ShowParts);

        GUI.WrappedText(LOC.Get("MAP_PartTransfer_Parts_Count", rows.Count(r => r.IsMapPiece), rows.Count(r => !r.IsMapPiece), source.Name));

        if (ShowParts && rows.Count > 0)
            DisplayRows(rows);

        DisplayConfirm();
    }

    #region Parts
    private List<PartTransferRow> GetRows(MapContainer source)
    {
        var key = (source, source.Objects.Count);

        if (RowsKey == key)
            return Rows;

        // The checks of a rebuilt list are kept for the parts still there
        var wasChecked = Rows.Where(r => r.Checked).Select(r => r.Entity).ToHashSet();
        RowsKey = key;

        Rows = source.Objects.OfType<MsbEntity>()
            .Where(e => EntityHelper.IsPartMapPiece(e) || EntityHelper.IsPartAsset(e) || EntityHelper.IsPartDummyAsset(e))
            .Select(e => new PartTransferRow
            {
                Entity = e,
                ModelName = ModelNameOf(e),
                IsMapPiece = EntityHelper.IsPartMapPiece(e),
                Checked = wasChecked.Contains(e)
            })
            .Where(r => !string.IsNullOrEmpty(r.ModelName))
            .ToList();

        return Rows;
    }

    private void DisplayRows(List<PartTransferRow> rows)
    {
        EditorFilters.DisplayFramedListFilter("mapPartTransferFilter", ref Filter, ref FilterExact);

        var shown = rows.Where(r => EditorFilters.IsMatch(Filter, r.Entity.Name, FilterExact, r.ModelName)).ToList();

        GUI.ConditionalMultiButtonInput("mapPartTransferListActions",
            "mapPartTransferCheckShown", LOC.Get("MAP_PartTransfer_Check_Shown"), LOC.Get("MAP_PartTransfer_Check_Shown_TT"),
            () => SetChecked(shown, true), !Running,

            "mapPartTransferUncheckShown", LOC.Get("MAP_PartTransfer_Uncheck_Shown"), LOC.Get("MAP_PartTransfer_Uncheck_Shown_TT"),
            () => SetChecked(shown, false), !Running);

        ImGui.BeginChild("mapPartTransferPartList", new Vector2(-1, 320 * DPI.UIScale()), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);

        if (ImGui.BeginTable("mapPartTransferPartTable", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##check", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch, 0.5f);
            ImGui.TableSetupColumn("##model", ImGuiTableColumnFlags.WidthStretch, 0.3f);
            ImGui.TableSetupColumn("##kind", ImGuiTableColumnFlags.WidthStretch, 0.2f);

            var clipper = new ImGuiListClipper();
            clipper.Begin(shown.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var row = shown[i];
                    var usable = CanPortMapPieces || !row.IsMapPiece;

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.BeginDisabled(!usable);
                    ImGui.Checkbox($"##mapPartTransferCheck_{i}", ref row.Checked);
                    ImGui.EndDisabled();

                    ImGui.TableSetColumnIndex(1);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(row.Entity.Name ?? "");

                    ImGui.TableSetColumnIndex(2);
                    ImGui.TextUnformatted(row.ModelName);

                    ImGui.TableSetColumnIndex(3);
                    ImGui.TextUnformatted(LOC.Get(row.IsMapPiece ? "MAP_PartTransfer_Kind_Map_Piece" : "MAP_PartTransfer_Kind_Object"));
                }
            }

            clipper.End();
            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void CheckSelected(List<PartTransferRow> rows)
    {
        var selected = View.ViewportSelection.GetSelection().OfType<Entity>().ToHashSet();

        foreach (var row in rows)
        {
            if (selected.Contains(row.Entity) && (CanPortMapPieces || !row.IsMapPiece))
                row.Checked = true;
        }
    }

    private void SetChecked(List<PartTransferRow> rows, bool value)
    {
        foreach (var row in rows)
            row.Checked = value && (CanPortMapPieces || !row.IsMapPiece);
    }
    #endregion

    #region Confirm
    private void RequestTransfer(MapContainer source, MapContainer target, List<PartTransferRow> rows)
    {
        PendingSource = source;
        PendingTarget = target;
        PendingRows = rows;
        PendingPorts = new();
        PendingProblems = new();
        PendingScan = null;
        PendingCopyTextures = CanCopyTextures && CFG.Current.MapEditor_PartTransfer_Copy_Textures;
        StatusText = "";

        var sourceAssetMap = PathBuilder.GetAssetMapID(Project, source.Name);

        // A port per map piece model, unless both maps read their pieces from the same folder
        if (!SameAssetMap(source, target))
        {
            var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in rows.Where(r => r.IsMapPiece).GroupBy(r => r.ModelName, StringComparer.OrdinalIgnoreCase))
            {
                var stem = ModelLocator.MapModelNameToAssetName(Project, sourceAssetMap, group.Key);
                var plan = MapPiecePort.CreatePlan(Project, sourceAssetMap, stem, target.Name, reserved, out var error);

                if (plan == null)
                {
                    PendingProblems.Add(LOC.Get("MAP_PartTransfer_Problem_Model", group.Key, error));
                    continue;
                }

                PendingPorts.Add(new PartTransferPort { SourceModel = group.Key, Plan = plan, PartCount = group.Count() });
            }
        }

        if (CanCopyTextures)
            StartScan(sourceAssetMap, target, rows);

        OpenConfirm = true;
    }

    private void DisplayConfirm()
    {
        var title = $"{LOC.Get("MAP_PartTransfer_Confirm_Title")}###mapPartTransferConfirm";

        if (OpenConfirm)
        {
            ImGui.OpenPopup(title);
            OpenConfirm = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(560, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (PendingRows == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var portedModels = PendingPorts.Select(p => p.SourceModel).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skipped = SameAssetMap(PendingSource, PendingTarget)
            ? 0
            : PendingRows.Count(r => r.IsMapPiece && !portedModels.Contains(r.ModelName));

        GUI.WrappedText(LOC.Get("MAP_PartTransfer_Confirm", PendingRows.Count - skipped, PendingSource.Name, PendingTarget.Name));

        // Map Pieces
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Confirm_Ports_Header"), LOC.Get("MAP_PartTransfer_Confirm_Ports_Header_TT"));

        if (SameAssetMap(PendingSource, PendingTarget))
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Confirm_No_Ports"));
        }
        else if (PendingPorts.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Confirm_No_Map_Pieces"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Confirm_Ports", PendingPorts.Count));

            ImGui.BeginChild("mapPartTransferPorts", new Vector2(-1, Math.Min(PendingPorts.Count, 8) * ImGui.GetTextLineHeightWithSpacing() + 8 * DPI.UIScale()), ImGuiChildFlags.Borders);

            foreach (var port in PendingPorts)
                ImGui.TextUnformatted(LOC.Get("MAP_PartTransfer_Confirm_Port_Row", port.Plan.SourceStem, port.Plan.TargetStem, port.PartCount));

            ImGui.EndChild();
        }

        foreach (var problem in PendingProblems)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, problem);

        if (skipped > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_PartTransfer_Confirm_Skipped", skipped));

        // Textures
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Confirm_Textures_Header"), LOC.Get("MAP_PartTransfer_Confirm_Textures_Header_TT"));

        DisplayTextureSummary();

        // Actions
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("MAP_PartTransfer_Confirm_Note"));

        var confirmed = false;
        var cancelled = false;

        GUI.ConditionalMultiButtonInput("mapPartTransferConfirmActions",
            "mapPartTransferConfirmTransfer", LOC.Get("MAP_PartTransfer_Confirm_Transfer"), LOC.Get("MAP_PartTransfer_Confirm_Transfer_TT"),
            () => confirmed = true, !Running && PendingRows.Count - skipped > 0,

            "mapPartTransferConfirmCancel", LOC.Get("MAP_PartTransfer_Cancel"), LOC.Get("MAP_PartTransfer_Cancel_TT"),
            () => cancelled = true, true);

        if (confirmed || cancelled || !open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();

        if (confirmed)
        {
            StartTransfer();
        }
        else if (cancelled || !open)
        {
            Cancellation?.Cancel();
            PendingRows = null;
        }
    }

    private void DisplayTextureSummary()
    {
        if (MapPiecePort.TexturesAreGlobal(Type))
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Textures_Global"));
            return;
        }

        if (!CanCopyTextures)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_PartTransfer_Textures_Unsupported"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_PartTransfer_Copy_Textures")}##mapPartTransferConfirmCopyTextures", ref PendingCopyTextures);

        var scan = PendingScan;

        if (scan == null)
        {
            GUI.WrappedText(Running ? LOC.Get("MAP_PartTransfer_Scanning", ProgressText ?? "") : LOC.Get("MAP_PartTransfer_Scan_Failed"));
            return;
        }

        if (scan.TargetBinders.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_PartTransfer_No_Target_Binders", scan.TargetKey));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_PartTransfer_Texture_Summary", scan.UsedCount, scan.PresentCount, scan.OwnTpfCount,
            scan.Missing.Count, string.Join(", ", scan.TargetBinders.Select(Path.GetFileName))));

        if (scan.NotFound.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_PartTransfer_Texture_Not_Found", scan.NotFound.Count));
            GUI.Tooltip(string.Join("\n", scan.NotFound.Select(n => n.Name).Take(30)));
        }

        foreach (var problem in scan.Problems.Take(5))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, problem);
    }
    #endregion

    #region Transfer
    private void StartScan(string sourceAssetMap, MapContainer target, List<PartTransferRow> rows)
    {
        var input = ScanInput(sourceAssetMap, target, rows);
        var pendingRows = rows;

        StartWork(token =>
        {
            var result = MapTextureTransferScan.Run(input, token, Progress);

            MainThreadQueue.Enqueue(() =>
            {
                // Only for the transfer it was started for
                if (ReferenceEquals(PendingRows, pendingRows))
                    PendingScan = result;
            });
        });
    }

    /// <summary>The checked parts' models in the source map: the source files, which the copies share textures with</summary>
    private TextureTransferScanInput ScanInput(string sourceAssetMap, MapContainer target, List<PartTransferRow> rows)
    {
        var models = new List<TextureTransferModel>();

        foreach (var group in rows.GroupBy(r => (r.IsMapPiece, r.ModelName)))
        {
            var (isMapPiece, modelName) = group.Key;
            var partNames = group.Select(r => r.Entity.Name).ToList();
            string path, inner;

            if (isMapPiece)
            {
                inner = ModelLocator.MapModelNameToAssetName(Project, sourceAssetMap, modelName);
                path = MapPiecePort.FindSourcePath(Project, sourceAssetMap, inner);
            }
            else
            {
                inner = modelName;
                var asset = ModelLocator.GetObjModel(Project, modelName, modelName);
                path = asset.IsValid() ? asset.GetContainerPath(Project) : null;
            }

            models.Add(new TextureTransferModel(modelName, !isMapPiece, path, inner, partNames));
        }

        return new TextureTransferScanInput(
            Type,
            Project.VFS.FS,
            MapTextureTransferProfile.KeyOfMap(Type, target.Name),
            GetBinderEntries().Select(e => e.Path).ToList(),
            models);
    }

    private void StartTransfer()
    {
        var source = PendingSource;
        var target = PendingTarget;
        var rows = PendingRows;
        var copyTextures = PendingCopyTextures;
        var scan = PendingScan;
        var sourceAssetMap = PathBuilder.GetAssetMapID(Project, source.Name);
        var problems = new List<string>(PendingProblems);

        // Names and paths now, on the main thread; the files are written in the background
        var ports = new List<PartTransferPort>();

        foreach (var port in PendingPorts)
        {
            port.Result = MapPiecePort.Prepare(Project, port.Plan);

            if (port.Result.Error != null)
                problems.Add(LOC.Get("MAP_PartTransfer_Problem_Model", port.SourceModel, port.Result.Error));
            else
                ports.Add(port);
        }

        var scanInput = copyTextures ? ScanInput(sourceAssetMap, target, rows) : null;

        PendingRows = null;

        StartWork(token =>
        {
            for (var i = 0; i < ports.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                Progress(i, ports.Count, ports[i].Result.TargetStem);
                MapPiecePort.WriteFile(Project, ports[i].Plan, ports[i].Result);
            }

            MainThreadQueue.Enqueue(() => FinishParts(source, target, rows, ports, problems, copyTextures, scan, scanInput));
        });
    }

    /// <summary>The ports written: listed, then the parts copied into the target and switched to them (one undo step)</summary>
    private void FinishParts(MapContainer source, MapContainer target, List<PartTransferRow> rows, List<PartTransferPort> ports,
        List<string> problems, bool copyTextures, TextureTransferResult scan, TextureTransferScanInput scanInput)
    {
        var modelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var port in ports)
        {
            if (port.Result.Error != null)
            {
                problems.Add(LOC.Get("MAP_PartTransfer_Problem_Model", port.SourceModel, port.Result.Error));
                continue;
            }

            MapPiecePort.Register(Project, port.Result);
            modelMap[port.SourceModel] = port.Result.ModelName;
        }

        // Map pieces of a model that couldn't be ported would point at whatever the target has under that name
        var sameAssets = SameAssetMap(source, target);
        var parts = rows
            .Where(r => !r.IsMapPiece || sameAssets || modelMap.ContainsKey(r.ModelName))
            .Select(r => r.Entity)
            .ToList();

        var transferred = 0;

        if (parts.Count > 0 && View.Selection.GetMapContainerFromMapID(target.Name) is { } targetMap)
        {
            var duplicate = new EntDuplicateAction(
                view: View,
                objects: parts,
                targetMap: targetMap,
                targetBTL: null,
                duplicateToMap: true,
                createMapGroupEntry: false,
                incrementEntityID: CFG.Current.Toolbar_Duplicate_Increment_Entity_ID,
                incrementInstanceID: CFG.Current.Toolbar_Duplicate_Increment_InstanceID,
                incrementPartNames: CFG.Current.Toolbar_Duplicate_Increment_PartNames,
                clearEntityID: CFG.Current.Toolbar_Duplicate_Clear_Entity_ID,
                clearEntityGroupID: CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);

            var actions = new List<ViewportAction> { duplicate };

            // The clones exist only once the duplicate has run
            actions.Add(new DeferredAction(() => BuildModelSwitch(duplicate.CreatedClones, targetMap, modelMap)));

            if (CFG.Current.MapEditor_PartTransfer_Move_To_Orb && View.ViewportHandler.ActiveViewport?.Viewport != null)
            {
                var point = View.ViewportWindow.GetPlacementPosition();
                actions.Add(new DeferredAction(() => PullToCameraAction.BuildMoveToPointAction(duplicate.CreatedClones, point)));
            }

            View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
            View.DelayPicking();

            transferred = duplicate.CreatedClones.Count;
        }

        StatusText = LOC.Get("MAP_PartTransfer_Done", transferred, target.Name, modelMap.Count);

        if (problems.Count > 0)
        {
            StatusText += "\n" + LOC.Get("MAP_PartTransfer_Done_Problems", problems.Count);

            foreach (var problem in problems)
                Smithbox.LogWarning(typeof(MapPartTransferTool), problem);
        }

        Smithbox.Log(typeof(MapPartTransferTool), StatusText);

        if (copyTextures)
            StartTextureCopy(target, scan, scanInput);
    }

    /// <summary>The clones of map pieces switched to their ported models, and renamed after them if asked</summary>
    private ViewportAction BuildModelSwitch(IReadOnlyList<MsbEntity> clones, MapContainer target, Dictionary<string, string> modelMap)
    {
        var actions = new List<ViewportAction>();
        var names = target.Objects.Select(o => o.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var clone in clones)
        {
            if (!EntityHelper.IsPartMapPiece(clone))
                continue;

            var oldModel = ModelNameOf(clone);

            if (oldModel == null || !modelMap.TryGetValue(oldModel, out var newModel))
                continue;

            actions.Add(clone.ChangeObjectProperty("ModelName", newModel));

            if (CFG.Current.MapEditor_PartTransfer_Rename_Parts)
            {
                var name = UniqueName(newModel, names);
                names.Add(name);
                actions.Add(clone.GetPropertyChangeAction(clone, "Name", name));
            }
        }

        return actions.Count > 0 ? new ViewportCompoundAction(actions) : null;
    }

    /// <summary>The game's part name style: the model name and a four digit index (m000500_0000)</summary>
    private static string UniqueName(string modelName, HashSet<string> names)
    {
        for (var i = 0; i < 10000; i++)
        {
            var name = $"{modelName}_{i:0000}";

            if (!names.Contains(name))
                return name;
        }

        return $"{modelName}_{Guid.NewGuid().ToString("N")[..4]}";
    }

    private void StartTextureCopy(MapContainer target, TextureTransferResult scan, TextureTransferScanInput input)
    {
        var bank = Project.Handler.TextureData?.PrimaryBank;

        if (bank == null)
        {
            StatusText += "\n" + LOC.Get("MAP_PartTransfer_No_Texture_Bank");
            return;
        }

        var key = MapTextureTransferProfile.KeyOfMap(Type, target.Name);
        var targets = GetBinderEntries().Where(e => MapTextureTransferProfile.KeyOfBinder(Type, e.Path) == key).ToList();

        if (targets.Count == 0)
        {
            StatusText += "\n" + LOC.Get("MAP_PartTransfer_No_Target_Binders", key);
            return;
        }

        var status = StatusText;

        StartWork(token =>
        {
            // The scan of the prompt, if it finished; else run now
            scan ??= MapTextureTransferScan.Run(input, token, Progress);

            var requests = scan.Missing.Where(m => m.Source != null).Select(m => new TextureTransferRequest(m.Name, m.Source)).ToList();

            if (requests.Count == 0)
            {
                MainThreadQueue.Enqueue(() => StatusText = status + "\n" + LOC.Get("MAP_PartTransfer_Textures_None_Missing", scan.NotFound.Count));
                return;
            }

            var applied = MapTextureTransferApply.Run(Project, bank, targets, requests, token, Progress);

            MainThreadQueue.Enqueue(() =>
            {
                // Marks the files as project files and reloads the scene textures that came from them
                if (applied.Saved.Count > 0)
                    bank.NotifyContainersSaved(applied.Saved);

                foreach (var e in applied.Errors)
                    Smithbox.LogError(typeof(MapPartTransferTool), e);

                StatusText = status + "\n" + LOC.Get("MAP_PartTransfer_Textures_Added", applied.Added, applied.LowDetailAdded,
                    applied.Saved.Count, scan.NotFound.Count);

                if (applied.Errors.Count > 0)
                    StatusText += "\n" + LOC.Get("MAP_PartTransfer_Textures_Errors", applied.Errors.Count);

                Smithbox.Log(typeof(MapPartTransferTool), StatusText);
            });
        });
    }
    #endregion

    #region Helpers
    private MapContainer GetSelectedMap()
    {
        var map = View.Selection.SelectedMapContainer;

        if (map == null && View.Selection.SelectedMapID != null)
            map = View.Selection.GetMapContainerFromMapID(View.Selection.SelectedMapID);

        return map;
    }

    /// <summary>Whether both maps read their map pieces from one folder (DS3's m30_00_00_00 for its sub maps)</summary>
    private bool SameAssetMap(MapContainer a, MapContainer b)
    {
        return string.Equals(PathBuilder.GetAssetMapID(Project, a.Name), PathBuilder.GetAssetMapID(Project, b.Name),
            StringComparison.OrdinalIgnoreCase);
    }

    private string MapLabel(string mapId)
    {
        var alias = AliasHelper.GetMapNameAlias(Project, mapId);
        return string.IsNullOrEmpty(alias) ? mapId : $"{mapId} - {alias}";
    }

    private static string ModelNameOf(Entity e)
    {
        try
        {
            return e.GetPropertyValue<string>("ModelName");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Every map texture binder of the project (gi_env and the like left out)</summary>
    private List<FileDictionaryEntry> GetBinderEntries()
    {
        return (Project.Locator.TexturePackedFiles?.Entries ?? new())
            .Where(e => MapTextureTransferProfile.KeyOfBinder(Type, e.Path) != null)
            .ToList();
    }

    private void DisplayProgress()
    {
        if (!Running && string.IsNullOrEmpty(StatusText))
            return;

        // Progress
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_PartTransfer_Progress_Header"), "");

        if (Running)
        {
            GUI.WrappedText(LOC.Get("MAP_PartTransfer_Working", ProgressText ?? ""));

            GUI.ConditionalMultiButtonInput("mapPartTransferCancelActions",
                "mapPartTransferCancel", LOC.Get("MAP_PartTransfer_Cancel"), LOC.Get("MAP_PartTransfer_Cancel_Work_TT"),
                () => Cancellation?.Cancel(), true);
        }

        if (!string.IsNullOrEmpty(StatusText))
            GUI.WrappedText(StatusText);
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
                MainThreadQueue.Enqueue(() => StatusText = LOC.Get("MAP_PartTransfer_Cancelled"));
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException)
            {
                MainThreadQueue.Enqueue(() => StatusText = LOC.Get("MAP_PartTransfer_Cancelled"));
            }
            catch (Exception e)
            {
                MainThreadQueue.Enqueue(() =>
                {
                    StatusText += (StatusText.Length > 0 ? "\n" : "") + LOC.Get("MAP_PartTransfer_Failed", e.Message);
                    Smithbox.LogError(typeof(MapPartTransferTool), StatusText, e);
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
