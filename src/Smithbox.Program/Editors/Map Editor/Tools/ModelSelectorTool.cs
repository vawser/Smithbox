using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;
public class ModelSelectorTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private string _selectedEntry = "";

    private bool SelectNextEntry = false;

    public string ModelFilter = "";
    public bool ExactModelFilter = false;

    private ModelPreviewBrowser PreviewBrowser;

    // The entries for the previews, built along with Choices
    private List<ModelPreviewEntry> PreviewEntries = new();

    public ModelSelectorTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        PreviewBrowser = new ModelPreviewBrowser(project);
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        ImGui.BeginChild("ModelSelectorToolSection", ImGuiChildFlags.Borders);

        if (ImGui.RadioButton("List##modelSelectorListMode", !CFG.Current.MapEditor_Model_Selector_Use_Previews))
        {
            CFG.Current.MapEditor_Model_Selector_Use_Previews = false;
        }
        GUI.Tooltip("Display the models as a list of names.");

        ImGui.SameLine();

        if (ImGui.RadioButton("3D Previews##modelSelectorPreviewMode", CFG.Current.MapEditor_Model_Selector_Use_Previews))
        {
            CFG.Current.MapEditor_Model_Selector_Use_Previews = true;
        }
        GUI.Tooltip("Display the models as a grid of 3D previews.");

        GUI.Spacer();

        GUI.WrappedText("Use this to switch a selected map object to a different model.");

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        ImGui.Checkbox("Update Name on Switch", ref CFG.Current.MapEditor_Model_Selector_Update_Name);
        GUI.Tooltip("When a map object is switched to a new form, update the name to match the new form.");

        if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            ImGui.Checkbox("Update Instance ID on Switch", ref CFG.Current.MapEditor_Model_Selector_Update_Instance_ID);
            GUI.Tooltip("When a map object is switched to a new form, update the Instance ID to account for the new form.");
        }

        // TOOD
        //ImGui.Checkbox("Update Params on Switch", ref CFG.Current.MapEditor_Model_Selector_Update_NpcParams);
        //UIHelper.Tooltip("When a map object is switched to a new form, update the NpcParam and NpcThinkParam to the closest suitable rows (based on the character ID).");

        var usePreviews = CFG.Current.MapEditor_Model_Selector_Use_Previews;

        GUI.Spacer();
        GUI.SimpleHeader(usePreviews ? "3D Previews" : "List", "");

        if (usePreviews)
        {
            PreviewBrowser.DisplayOptions();
        }

        EditorFilters.DisplayFramedListFilter("modelSelector", ref ModelFilter, ref ExactModelFilter);

        // The selection can hold non-entities (a reference to an unloaded map), which have no model
        var firstSelection = View.ViewportSelection.GetSelection().OfType<Entity>().FirstOrDefault();

        if (firstSelection != null)
        {
            if (EntityHelper.IsPartEnemy(firstSelection) || EntityHelper.IsPartDummyEnemy(firstSelection))
            {
                DisplayModels(ModelPreviewKind.Character, FileSelectionType.Character, "", usePreviews);
            }
            else if (EntityHelper.IsPartAsset(firstSelection) || EntityHelper.IsPartDummyAsset(firstSelection))
            {
                DisplayModels(ModelPreviewKind.Asset, FileSelectionType.Asset, "", usePreviews);
            }
            else if (EntityHelper.IsPartMapPiece(firstSelection))
            {
                // Map pieces are specific to a map, so the only ones that can be switched to are in this one
                if (firstSelection.Parent != null)
                {
                    DisplayModels(ModelPreviewKind.MapPiece, FileSelectionType.MapPiece, firstSelection.Parent.Name, usePreviews);
                }
            }
            else if (EntityHelper.IsPartCollision(firstSelection))
            {
                if (Project.Descriptor.ProjectType is ProjectType.ER
                    or ProjectType.AC6
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    DisplayCollisionList();
                }
            }
            else
            {
                ImGui.Text("Your current selection does not use a model.");
            }
        }
        else
        {
            ImGui.Text("You must select a valid map object first.");
        }

        ImGui.EndChild();
    }

    // The models on offer, read from the project's files (see ModelFileChoices); rebuilt when the list or filter changes
    private List<ModelChoice> Choices = new();
    private (ModelPreviewKind Kind, string MapID, int FileCount, string Filter, bool Exact, bool LowDetail) ChoicesKey;

    private List<ModelChoice> GetChoices(ModelPreviewKind kind, string mapId)
    {
        var fileCount = kind switch
        {
            ModelPreviewKind.Character => Project.Locator.ChrFiles.Entries.Count,
            ModelPreviewKind.Asset => Project.Locator.AssetFiles.Entries.Count,
            _ => Project.Locator.MapPieceFiles.Entries.Count
        };

        var key = (kind, mapId, fileCount, ModelFilter, ExactModelFilter, CFG.Current.MapEditor_Model_Selector_Display_Low_Detail_Entries);

        if (key == ChoicesKey)
            return Choices;

        ChoicesKey = key;

        var all = kind switch
        {
            ModelPreviewKind.Character => ModelFileChoices.Characters(Project),
            ModelPreviewKind.Asset => ModelFileChoices.Assets(Project),
            _ => ModelFileChoices.MapPieces(Project, mapId)
        };

        Choices = all.Where(FilterSelectionList).ToList();
        PreviewEntries = Choices.Select(c => new ModelPreviewEntry(c.ID, c.Alias, kind, mapId)).ToList();

        return Choices;
    }

    private void DisplayModels(ModelPreviewKind kind, FileSelectionType selectionType, string mapId, bool usePreviews)
    {
        var choices = GetChoices(kind, mapId);

        if (choices.Count == 0)
        {
            ImGui.Text(kind is ModelPreviewKind.MapPiece && Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S
                ? "This game keeps each map's pieces in one file, so they can't be listed."
                : "No models were found in the project's files.");
            return;
        }

        if (usePreviews)
        {
            PreviewBrowser.SelectedID = _selectedEntry;

            PreviewBrowser.Display("modelSelector", PreviewEntries, e =>
                ApplyChoice(choices.FirstOrDefault(c => c.ID == e.ID), selectionType, mapId));

            _selectedEntry = PreviewBrowser.SelectedID;
            return;
        }

        if (kind is ModelPreviewKind.MapPiece)
        {
            var displayedMapName = $"{mapId} - {AliasHelper.GetMapNameAlias(View.Project, mapId)}";
            GUI.SimpleHeader($"{mapId}_header", displayedMapName, "", UI.Current.ImGui_Default_Text_Color);
        }

        ImGui.BeginChild("##modelSelectorList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(choices.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var choice = choices[i];

                if (ImGui.Selectable($"{choice.ModelName}##{choice.ID}", choice.ID == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedEntry = choice.ID;

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        ApplyChoice(choice, selectionType, mapId);
                    }
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectNextEntry)
                {
                    SelectNextEntry = false;
                    _selectedEntry = choice.ID;
                    ApplyChoice(choice, selectionType, mapId);
                }

                if (ImGui.IsItemFocused())
                {
                    if (InputManager.HasArrowSelection())
                    {
                        SelectNextEntry = true;
                    }
                }

                DisplaySelectableAlias(choice);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void ApplyChoice(ModelChoice choice, FileSelectionType selectionType, string mapId)
    {
        if (choice != null)
        {
            ApplyMapAssetSelection(choice.ModelName, selectionType, mapId);
        }
    }

    private void DisplayCollisionList()
    {
        var mapID = View.Selection.SelectedMapID;
        var collisionList = View.HavokCollisionBank.MapCollisions.GetValueOrDefault(mapID);

        if (collisionList == null)
            return;

        GUI.SimpleHeader($"{mapID}_header", $"{mapID}", "", UI.Current.ImGui_Default_Text_Color);

        ImGui.BeginChild("##collisionSelectorList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        foreach (var col in collisionList)
        {
            var shortName = col.Replace(".hkx.dcx", "");
            shortName = shortName.Replace($"h{mapID.Replace("m", "")}_", "h");

            if (!col.StartsWith("h"))
                continue;

            var displayedName = $"{shortName}";

            if (ImGui.Selectable(displayedName, col == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
            {
                _selectedEntry = col;

                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    ApplyMapAssetSelection(shortName, FileSelectionType.Collision);
                }
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && SelectNextEntry)
            {
                SelectNextEntry = false;
                _selectedEntry = col;
                ApplyMapAssetSelection(shortName, FileSelectionType.Collision);
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    SelectNextEntry = true;
                }
            }
        }

        ImGui.EndChild();
    }

    private bool FilterSelectionList(ModelChoice choice)
    {
        if (!CFG.Current.MapEditor_Model_Selector_Display_Low_Detail_Entries
            && choice.ID.EndsWith("_l", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // The model's own name as well as its alias, now that a model need not have one
        return EditorFilters.IsMatch(ModelFilter, choice.ModelName, ExactModelFilter, choice.Alias,
            rawSecondaryText: choice.ID);
    }

    private void DisplaySelectableAlias(ModelChoice choice)
    {
        if (CFG.Current.MapEditor_Model_Selector_Display_Aliases && choice.Alias != "")
        {
            GUI.DisplayAlias(choice.Alias);
        }

        // Tags
        if (CFG.Current.MapEditor_Model_Selector_Display_Tags && choice.Tags.Count > 0)
        {
            AliasHelper.DisplayTagAlias(string.Join(" ", choice.Tags));
        }
    }

    public void ApplyMapAssetSelection(string _selectedName, FileSelectionType type, string mapId = "")
    {
        var modelName = _selectedName;

        if (modelName.Contains("aeg"))
        {
            modelName = modelName.Replace("aeg", "AEG");
        }

        if (type == FileSelectionType.MapPiece)
        {
            SetObjectModelForSelection(modelName, type, mapId);
        }
        else
        {
            SetObjectModelForSelection(modelName, type, "");
        }
    }

    private void SetObjectModelForSelection(string modelName, FileSelectionType assetType, string assetMapId)
    {
        var actlist = new List<ViewportAction>();

        var selected = View.ViewportSelection.GetFilteredSelection<Entity>();

        foreach (var s in selected)
        {
            var isValidObjectType = false;

            if (assetType == FileSelectionType.Character)
            {
                switch (View.Project.Descriptor.ProjectType)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB_NT:
                        if (s.WrappedObject is MSB_DB_NT.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB:
                        throw new ArgumentException("Selected entity type must be Enemy");
                    default:
                        throw new ArgumentException("Selected entity type must be Enemy");
                }
            }
            if (assetType == FileSelectionType.Asset)
            {
                switch (View.Project.Descriptor.ProjectType)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        if (s.WrappedObject is MSB2.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.Asset)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.Asset)
                            isValidObjectType = true;
                        break;
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.Asset)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB_NT:
                        if (s.WrappedObject is MSB_DB_NT.Part.Asset)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB:
                        throw new ArgumentException("Selected entity type must be Object/Asset");
                    default:
                        throw new ArgumentException("Selected entity type must be Object/Asset");
                }
            }

            if (assetType == FileSelectionType.Collision)
            {
                switch (View.Project.Descriptor.ProjectType)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        if (s.WrappedObject is MSB2.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB_NT:
                        if (s.WrappedObject is MSB_DB_NT.Part.Collision)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB:
                        throw new ArgumentException("Selected entity type must be Object/Asset");
                    default:
                        throw new ArgumentException("Selected entity type must be Object/Asset");
                }
            }

            if (assetType == FileSelectionType.MapPiece)
            {
                switch (View.Project.Descriptor.ProjectType)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        if (s.WrappedObject is MSB2.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB_NT:
                        if (s.WrappedObject is MSB_DB_NT.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DB:
                        throw new ArgumentException("Selected entity type must be MapPiece");
                    default:
                        throw new ArgumentException("Selected entity type must be MapPiece");
                }
            }

            if (assetType == FileSelectionType.MapPiece)
            {
                if (s.Parent != null)
                {
                    var mapName = s.Parent.Name;
                    if (mapName != assetMapId)
                    {
                        PlatformUtils.Instance.MessageBox($"Map Pieces are specific to each map.\nYou cannot change a Map Piece in {mapName} to a Map Piece from {assetMapId}.", "Object Browser", MessageBoxButtons.OK);

                        isValidObjectType = false;
                    }
                }
                else
                {
                    isValidObjectType = false;
                }
            }

            if (isValidObjectType)
            {
                if (assetType == FileSelectionType.MapPiece)
                {
                    // Adjust modelName for mappieces, since by default they are mXX_YY_ZZ_AA_<id>
                    string newName = modelName.Replace($"{assetMapId}_", "m");
                    modelName = newName;
                }

                // ModelName
                var action = s.ChangeObjectProperty("ModelName", modelName);
                actlist.Add(action);


                if (CFG.Current.MapEditor_Model_Selector_Update_Name)
                {
                    var updateNameAction = UpdateEntityName(modelName, s);
                    actlist.Add(updateNameAction);
                }

                if (CFG.Current.MapEditor_Model_Selector_Update_Instance_ID)
                {
                    if (s.WrappedObject is MSBE.Part || s.WrappedObject is MSB_AC6.Part)
                    {
                        var updateInstanceAction = UpdateInstanceID(modelName, (MsbEntity)s);
                        actlist.Add(updateInstanceAction);
                    }
                }
            }
        }

        foreach (var (ent, oldName) in PendingNames)
        {
            ent.Name = oldName;
        }
        PendingNames.Clear();

        if (actlist.Any())
        {
            var action = new ViewportCompoundAction(actlist);

            View.ViewportActionManager.ExecuteAction(action);
        }
    }

    private readonly List<(Entity Entity, string OldName)> PendingNames = new();

    private ViewportAction UpdateEntityName(string modelName, Entity ent)
    {
        var name = GetUniqueNameString(modelName);

        // Built before the name is set so the action records the old name
        var action = ent.GetPropertyChangeAction(ent, "Name", name);

        // Applied now so the next selected entity's unique name takes it into account,
        // and reverted before the actions execute
        PendingNames.Add((ent, ent.Name));
        ent.Name = name;

        return action;
    }

    private ViewportAction UpdateInstanceID(string modelName, MsbEntity ent)
    {
        MapContainer m;
        m = View.Selection.GetMapContainerFromMapID(ent.MapID);

        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        // ER
        if (ent.WrappedObject is MSBE.Part msbePart)
        {
            if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
            {
                foreach (Entity tEnt in m.Objects)
                {
                    if (tEnt.WrappedObject != null && tEnt.WrappedObject is MSBE.Part)
                    {
                        mapPartEntities[m].Add((MsbEntity)tEnt);
                    }
                }
            }

            var newInstanceID = 9000; // Default start value

            while (mapPartEntities[m].FirstOrDefault(e =>
                       ((MSBE.Part)e.WrappedObject).ModelName == msbePart.ModelName
                       && ((MSBE.Part)e.WrappedObject).InstanceID == newInstanceID
                       && msbePart != (MSBE.Part)e.WrappedObject) != null)
            {
                newInstanceID++;
            }

            return ent.GetPropertyChangeAction(ent, "InstanceID", newInstanceID);
        }

        // AC6
        if (ent.WrappedObject is MSB_AC6.Part msb_ac6Part)
        {
            if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
            {
                foreach (Entity tEnt in m.Objects)
                {
                    if (tEnt.WrappedObject != null && tEnt.WrappedObject is MSB_AC6.Part)
                    {
                        mapPartEntities[m].Add((MsbEntity)tEnt);
                    }
                }
            }

            var newInstanceID = 0; // Default start value

            while (mapPartEntities[m].FirstOrDefault(e =>
                       ((MSB_AC6.Part)e.WrappedObject).ModelName == msb_ac6Part.ModelName
                       && ((MSB_AC6.Part)e.WrappedObject).TypeIndex == newInstanceID
                       && msb_ac6Part != (MSB_AC6.Part)e.WrappedObject) != null)
            {
                newInstanceID++;
            }

            return ent.GetPropertyChangeAction(ent, "TypeIndex", newInstanceID);
        }

        return null;
    }

    public string GetUniqueNameString(string modelName)
    {
        var postfix = 0;
        var baseName = $"{modelName}_0000";

        var names = new List<string>();

        // Collect names
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
            {
                continue;
            }

            foreach (var ob in entry.Value.MapContainer.Objects)
            {
                if (ob is MsbEntity e)
                {
                    names.Add(ob.Name);
                }
            }
        }

        var validName = false;
        while (!validName)
        {
            var matchesName = false;

            foreach (var name in names)
            {
                // Name already exists
                if (name == baseName)
                {
                    // Increment postfix number by 1
                    var old_value = postfix;
                    postfix = postfix + 1;

                    // Replace baseName postfix number
                    baseName = baseName.Replace($"{PadNameString(old_value)}", $"{PadNameString(postfix)}");

                    matchesName = true;
                }
            }

            // If it does not match any name during 1 full iteration, then it must be valid
            if (!matchesName)
            {
                validName = true;
            }
        }

        return baseName;
    }

    private string PadNameString(int value)
    {
        if (value < 10)
        {
            return $"000{value}";
        }

        if (value >= 10 && value < 100)
        {
            return $"00{value}";
        }

        if (value >= 100 && value < 1000)
        {
            return $"0{value}";
        }

        return $"{value}";
    }
}
