using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Mass Create tab of the Create tool: adds a part for every character, or every asset of a category, to the
/// target map, laid out in a grid that starts at the placement orb.
/// </summary>
public partial class MassCreateTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly CreateAction Owner;

    // How long a mass create may hold up each frame
    private static readonly TimeSpan FrameBudget = TimeSpan.FromMilliseconds(20);

    private MassCreateSource Source = MassCreateSource.Characters;
    private string AssetCategory = null;

    private string Filter = "";
    private bool ExactFilter = false;
    private bool ShowModelList = true;

    // Placement: objects per row (X), rows per grid (Y); a full grid starts the next one beside it
    private int RowLength = 10;
    private int MaxRows = 10;
    private float ColumnSpacing = 5.0f;
    private float RowSpacing = 5.0f;
    private float GridGap = 10.0f;

    // The models of each source, rebuilt when the project's file lists change
    private List<ModelChoice> Characters;
    private int CharacterFileCount = -1;
    private SortedDictionary<string, List<ModelChoice>> AssetCategories;
    private int AssetFileCount = -1;

    // The models that will be created, rebuilt when the source or filter changes
    private List<ModelChoice> Models = new();
    private (MassCreateSource, string, string, bool, int) ModelsKey = (MassCreateSource.Characters, null, null, false, -1);

    private record PendingCreate(string Session, string ProgressName, MapContainer Map, EntAddAction Action);
    private PendingCreate Active;

    [GeneratedRegex(@"^(aeg\d{3})_", RegexOptions.IgnoreCase)]
    private static partial Regex AegCategoryRegex();

    public MassCreateTool(MapEditorView view, ProjectEntry project, CreateAction owner)
    {
        View = view;
        Project = project;
        Owner = owner;
    }

    public bool IsCreating => Active != null;

    #region Display
    public void Display(MapContainer map)
    {
        GUI.WrappedText(LOC.Get("MAP_MassCreate_Hint"));

        // Source
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Source_Header"), LOC.Get("MAP_MassCreate_Source_Header_TT"));

        if (ImGui.RadioButton($"{LOC.Get("MAP_MassCreate_Source_Characters")}##massCreateSourceChr", Source == MassCreateSource.Characters))
        {
            Source = MassCreateSource.Characters;
        }
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Source_Characters_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("MAP_MassCreate_Source_Assets")}##massCreateSourceAsset", Source == MassCreateSource.AssetCategory))
        {
            Source = MassCreateSource.AssetCategory;
        }
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Source_Assets_TT"));

        var partType = GetPartType(Source);

        if (partType == null)
        {
            GUI.WrappedText(LOC.Get(Source == MassCreateSource.Characters
                ? "MAP_MassCreate_No_Enemy_Part"
                : "MAP_MassCreate_No_Asset_Part"));
            return;
        }

        if (Source == MassCreateSource.AssetCategory)
        {
            DisplayCategorySelection();
        }

        // Filter
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Filter_Header"), LOC.Get("MAP_MassCreate_Filter_Header_TT"));

        EditorFilters.DisplayFramedListFilter("massCreate", ref Filter, ref ExactFilter);

        var models = GetModels();

        DisplayPlacementOptions(models.Count);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Actions_Header"), LOC.Get("MAP_MassCreate_Actions_Header_TT"));

        if (Active != null)
        {
            var total = Math.Max(1, Active.Action.TotalCount);
            ImGui.ProgressBar((float)Active.Action.AddedCount / total, new Vector2(-1, 0),
                LOC.Get("MAP_MassCreate_Progress", Active.Action.AddedCount, Active.Action.TotalCount));
        }
        else
        {
            GUI.ConditionalMultiButtonInput("massCreateActions",
                "massCreateButton", LOC.Get("MAP_MassCreate_Create", models.Count), LOC.Get("MAP_MassCreate_Create_TT"),
                () => StartCreate(map, partType, models), models.Count > 0);
        }

        // Models
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_MassCreate_Models_Header", models.Count), LOC.Get("MAP_MassCreate_Models_Header_TT"), ref ShowModelList);

        if (ShowModelList)
        {
            DisplayModelList(models);
        }
    }

    private void DisplayCategorySelection()
    {
        var categories = GetAssetCategories();

        // Asset Category
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Category_Header"), LOC.Get("MAP_MassCreate_Category_Header_TT"));

        if (categories.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MassCreate_No_Assets"));
            return;
        }

        if (AssetCategory == null || !categories.ContainsKey(AssetCategory))
        {
            AssetCategory = categories.Keys.First();
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##massCreateCategory", $"{AssetCategory} ({categories[AssetCategory].Count})"))
        {
            foreach (var (category, entries) in categories)
            {
                if (ImGui.Selectable($"{category} ({entries.Count})##massCreateCategory_{category}", category == AssetCategory))
                {
                    AssetCategory = category;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Category_TT"));
    }

    private void DisplayPlacementOptions(int count)
    {
        // Layout
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Layout_Header"), LOC.Get("MAP_MassCreate_Layout_Header_TT"));

        GUI.SetInputWidth();
        if (ImGui.InputInt($"{LOC.Get("MAP_MassCreate_Row_Length")}##massCreateRowLength", ref RowLength))
        {
            RowLength = Math.Max(1, RowLength);
        }
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Row_Length_TT"));

        GUI.SetInputWidth();
        if (ImGui.InputInt($"{LOC.Get("MAP_MassCreate_Max_Rows")}##massCreateMaxRows", ref MaxRows))
        {
            MaxRows = Math.Max(1, MaxRows);
        }
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Max_Rows_TT"));

        // Spacing
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Spacing_Header"), LOC.Get("MAP_MassCreate_Spacing_Header_TT"));

        GUI.SetInputWidth();
        ImGui.InputFloat($"{LOC.Get("MAP_MassCreate_Column_Spacing")}##massCreateColumnSpacing", ref ColumnSpacing);
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Column_Spacing_TT"));

        GUI.SetInputWidth();
        ImGui.InputFloat($"{LOC.Get("MAP_MassCreate_Row_Spacing")}##massCreateRowSpacing", ref RowSpacing);
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Row_Spacing_TT"));

        GUI.SetInputWidth();
        ImGui.InputFloat($"{LOC.Get("MAP_MassCreate_Grid_Gap")}##massCreateGridGap", ref GridGap);
        GUI.Tooltip(LOC.Get("MAP_MassCreate_Grid_Gap_TT"));

        var perGrid = RowLength * MaxRows;
        var grids = count == 0 ? 0 : (count + perGrid - 1) / perGrid;

        GUI.WrappedText(LOC.Get("MAP_MassCreate_Layout_Summary", count, RowLength, MaxRows, grids));
    }

    private void DisplayModelList(List<ModelChoice> models)
    {
        if (models.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MassCreate_No_Models"));
            return;
        }

        ImGui.BeginChild("##massCreateModelList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(models.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var model = models[i];

                ImGui.Text(model.ID);

                if (model.Alias != "")
                {
                    GUI.DisplayAlias(model.Alias);
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
    #endregion

    #region Models
    private List<ModelChoice> GetCharacters()
    {
        var count = Project.Locator.ChrFiles.Entries.Count;

        if (Characters == null || CharacterFileCount != count)
        {
            CharacterFileCount = count;
            Characters = ModelFileChoices.Characters(Project);
        }

        return Characters;
    }

    /// <summary>
    /// Assets grouped by category: the AEG prefix for the games that use them (AEG099 for AEG099_001), and the
    /// first two digits of the ID for the object games (o00 for o000100)
    /// </summary>
    private SortedDictionary<string, List<ModelChoice>> GetAssetCategories()
    {
        var count = Project.Locator.AssetFiles.Entries.Count;

        if (AssetCategories == null || AssetFileCount != count)
        {
            AssetFileCount = count;
            AssetCategories = new(StringComparer.OrdinalIgnoreCase);

            foreach (var choice in ModelFileChoices.Assets(Project))
            {
                // Low detail variants aren't separate objects
                if (choice.ID.EndsWith("_l", StringComparison.OrdinalIgnoreCase))
                    continue;

                var category = GetAssetCategory(choice.ID);

                if (!AssetCategories.TryGetValue(category, out var list))
                {
                    list = new();
                    AssetCategories.Add(category, list);
                }

                list.Add(choice);
            }
        }

        return AssetCategories;
    }

    private static string GetAssetCategory(string id)
    {
        var aeg = AegCategoryRegex().Match(id);
        if (aeg.Success)
            return aeg.Groups[1].Value.ToUpperInvariant();

        return id.Length > 3 ? id.Substring(0, 3) : id;
    }

    private List<ModelChoice> GetModels()
    {
        var fileCount = Source == MassCreateSource.Characters
            ? Project.Locator.ChrFiles.Entries.Count
            : Project.Locator.AssetFiles.Entries.Count;

        var key = (Source, AssetCategory, Filter, ExactFilter, fileCount);

        if (key == ModelsKey)
            return Models;

        IEnumerable<ModelChoice> source;
        if (Source == MassCreateSource.Characters)
        {
            source = GetCharacters();
        }
        else
        {
            var categories = GetAssetCategories();
            source = AssetCategory != null && categories.TryGetValue(AssetCategory, out var list) ? list : Enumerable.Empty<ModelChoice>();
        }

        Models = source
            .Where(c => EditorFilters.IsMatch(Filter, c.ModelName, ExactFilter, c.Alias, rawSecondaryText: c.ID))
            .ToList();

        ModelsKey = key;

        return Models;
    }

    /// <summary>
    /// The part class the source is created as: the Enemy part for characters, and the Asset (or Object, before
    /// Elden Ring) part for assets. Null if the game's MSB has no such part it can save.
    /// </summary>
    private Type GetPartType(MassCreateSource source)
    {
        if (source == MassCreateSource.Characters)
            return Owner.PartsClasses.FirstOrDefault(p => p.Item2.Name == "Enemy").Item2;

        return Owner.PartsClasses.FirstOrDefault(p => p.Item2.Name == "Asset").Item2
            ?? Owner.PartsClasses.FirstOrDefault(p => p.Item2.Name == "Object").Item2;
    }
    #endregion

    #region Creation
    private void StartCreate(MapContainer map, Type partType, List<ModelChoice> models)
    {
        if (Active != null || models.Count == 0)
            return;

        var parent = map.RootObject;
        var origin = GetPlacementOrigin();
        var parentTransform = GetParentTransform(parent);
        var inverseRotation = Quaternion.Inverse(parentTransform.Rotation);

        var names = new HashSet<string>(map.Objects.Select(o => o.Name).Where(n => n != null), StringComparer.OrdinalIgnoreCase);
        var ids = new InstanceIdTracker(Project.Descriptor.ProjectType, map);

        var entities = new List<MsbEntity>();

        for (var i = 0; i < models.Count; i++)
        {
            object part;
            try
            {
                part = partType.GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            catch (Exception e)
            {
                Smithbox.LogError<MassCreateTool>($"Failed to create a {partType.Name} part.", e);
                return;
            }

            var modelName = GetModelName(models[i]);

            SetProperty(part, "ModelName", modelName);
            ids.Assign(part, modelName, names);

            var world = origin + GetGridOffset(i);
            var local = Vector3.Transform(world - parentTransform.Position, inverseRotation);
            SetProperty(part, "Position", local);

            entities.Add(new MsbEntity(View.Universe, map, part, MsbEntityType.Part));
        }

        var session = $"Mass Create {map.Name}";
        var progressName = LOC.Get("MAP_MassCreate_Progress_Name");

        ResourceLoadTracker.BeginSession(session);
        ResourceLoadTracker.Reserve(progressName);

        Active = new PendingCreate(session, progressName, map, new EntAddAction(View, map, entities, parent));
    }

    /// <summary>
    /// Advances the current mass create. Runs once per frame, regardless of whether the tool window is displayed.
    /// </summary>
    public void Update()
    {
        if (Active == null)
            return;

        if (Active.Map.LoadState is not MapContentLoadState.Loaded)
        {
            Smithbox.LogError<MassCreateTool>("Mass create stopped: the target map was unloaded.");
            End();
            return;
        }

        var finished = Active.Action.ExecuteStep(FrameBudget);

        ResourceLoadTracker.SetTaskProgress(Active.ProgressName, Active.Action.AddedCount, Active.Action.TotalCount);

        if (finished)
        {
            View.ViewportActionManager.ExecuteAction(Active.Action);
            Smithbox.Log(this, LOC.Get("MAP_MassCreate_Done", Active.Action.TotalCount, Active.Map.Name));
            View.DelayPicking();
            End();
        }
    }

    /// <summary>
    /// Finishes the current mass create at once, for when the view is closing and Update would stop being called.
    /// </summary>
    public void Finish()
    {
        while (Active != null)
        {
            Update();
        }
    }

    private void End()
    {
        ResourceLoadTracker.EndSession(Active.Session);
        Active = null;
    }

    /// <summary>
    /// The offset from the placement orb of the index-th object: along +X within a row, rows along +Z, and each full
    /// grid beside the last along +X
    /// </summary>
    private Vector3 GetGridOffset(int index)
    {
        var perGrid = RowLength * MaxRows;

        var grid = index / perGrid;
        var inGrid = index % perGrid;
        var row = inGrid / RowLength;
        var column = inGrid % RowLength;

        var gridWidth = (RowLength - 1) * ColumnSpacing + GridGap;

        return new Vector3(grid * gridWidth + column * ColumnSpacing, 0, row * RowSpacing);
    }

    private Vector3 GetPlacementOrigin()
    {
        if (View.ViewportHandler.ActiveViewport?.Viewport == null)
            return Vector3.Zero;

        return View.ViewportWindow.GetPlacementPosition();
    }

    /// <summary>
    /// The parent chain's transform, which for tiled maps holds the map's offset, so world positions can be made local
    /// </summary>
    private static Transform GetParentTransform(Entity parent)
    {
        var root = Transform.Default;
        for (var p = parent; p != null; p = p.Parent)
        {
            root += p.GetLocalTransform();
        }

        return root;
    }

    private static string GetModelName(ModelChoice choice)
    {
        // The asset files are lower case, but the MSBs name them in upper case
        var name = choice.ModelName;
        return name.StartsWith("aeg", StringComparison.OrdinalIgnoreCase) ? name.ToUpperInvariant() : name;
    }

    private static void SetProperty(object obj, string name, object value)
    {
        var prop = obj.GetType().GetProperty(name);

        if (prop != null && prop.CanWrite && prop.PropertyType.IsInstanceOfType(value))
        {
            prop.SetValue(obj, value);
        }
    }

    /// <summary>
    /// Gives each new part a name the map doesn't use yet, in the game's model_suffix form. For ER and NR the suffix
    /// is the instance ID, which is unique per model, as in c1000_9000. For AC6 the type index is made unique.
    /// </summary>
    private sealed class InstanceIdTracker
    {
        private readonly ProjectType Game;
        private readonly Dictionary<string, HashSet<int>> UsedInstanceIDs = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<int> UsedTypeIndices = new();
        private int NextTypeIndex = 0;

        public InstanceIdTracker(ProjectType game, MapContainer map)
        {
            Game = game;

            foreach (var obj in map.Objects)
            {
                switch (obj.WrappedObject)
                {
                    case MSBE.Part p:
                        Used(p.ModelName).Add(p.InstanceID);
                        break;
                    case MSB_NR.Part p:
                        Used(p.ModelName).Add(p.InstanceID);
                        break;
                    case MSB_AC6.Part p:
                        UsedTypeIndices.Add(p.TypeIndex);
                        break;
                }
            }
        }

        private HashSet<int> Used(string model)
        {
            model ??= "";

            if (!UsedInstanceIDs.TryGetValue(model, out var set))
            {
                set = new();
                UsedInstanceIDs.Add(model, set);
            }

            return set;
        }

        public void Assign(object part, string modelName, HashSet<string> names)
        {
            var usesInstanceID = Game is ProjectType.ER or ProjectType.NR;
            var used = usesInstanceID ? Used(modelName) : null;

            var suffix = usesInstanceID ? 9000 : 0;
            while (names.Contains($"{modelName}_{suffix:D4}") || (used != null && used.Contains(suffix)))
            {
                suffix++;
            }

            var name = $"{modelName}_{suffix:D4}";
            names.Add(name);
            SetProperty(part, "Name", name);

            if (usesInstanceID)
            {
                used.Add(suffix);
                SetProperty(part, "InstanceID", suffix);
            }

            if (Game == ProjectType.AC6)
            {
                while (UsedTypeIndices.Contains(NextTypeIndex))
                {
                    NextTypeIndex++;
                }

                UsedTypeIndices.Add(NextTypeIndex);
                SetProperty(part, "TypeIndex", NextTypeIndex);
            }
        }
    }
    #endregion
}

public enum MassCreateSource
{
    Characters,
    AssetCategory
}
