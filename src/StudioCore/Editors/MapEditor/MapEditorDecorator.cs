using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.META;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Resources.JSON;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditorNS;

public class MapEditorDecorator
{
    public MapEditor Editor;

    public MapEditorDecorator(MapEditor editor)
    {
        Editor = editor;
    }

    public List<string> DS2_ObjectInstanceParams = new List<string>
    {
        "mapobjectinstanceparam",
        "treasureboxparam"
    };

    /// <summary>
    /// Param References
    /// </summary>
    public bool ParamRefRow(MapEntityPropertyFieldMeta meta, PropertyInfo propinfo, object val, ref object newObj)
    {
        if (meta != null && meta.ParamRef.Count > 0)
        {
            List<ParamRef> refs = new();

            foreach (ParamRef pRef in meta.ParamRef)
            {
                // DS2 ObjectInstanceParam
                if (meta.SpecialHandling == "ObjectInstanceParam")
                {
                    foreach (var param in DS2_ObjectInstanceParams)
                    {
                        var paramName = param;
                        var selection = Editor.Selection;

                        if (selection.IsSelection())
                        {
                            // Get cur map name, apend to ParamName
                            var sel = selection.GetSelection().First() as Entity;
                            var map = sel.Parent.Name;

                            paramName = $"{paramName}_{map}".ToLower();

                            refs.Add(new ParamRef(paramName));
                        }
                    }
                }
                // DS1 Bank Params
                else if (meta.SpecialHandling == "BankParam")
                {
                    var selection = Editor.Selection;

                    if (selection.IsSelection())
                    {
                        // Get cur map name, append to ParamName
                        var sel = selection.GetSelection().First() as Entity;
                        var map = sel.Parent.Name;

                        var paramName = pRef.ParamName;

                        var mapPrefix = map.Substring(0, 3);

                        // Handling for m15_1 in DS1
                        if (map == "m15_01_00_00")
                            mapPrefix = "m51_1";

                        refs.Add(new ParamRef($"{mapPrefix}_{paramName}"));
                    }
                }
                // DS2 Map Params
                else if (meta.SpecialHandling == "MapParam")
                {
                    var selection = Editor.Selection;

                    if (selection.IsSelection())
                    {
                        // Get cur map name, append to ParamName
                        var sel = selection.GetSelection().First() as Entity;
                        var map = sel.Parent.Name;

                        var paramName = pRef.ParamName;

                        refs.Add(new ParamRef($"{paramName}_{map}"));
                    }
                }
                else
                {
                    refs.Add(new ParamRef(pRef.ParamName));
                }
            }

            ImGui.NextColumn();

            EditorDecorations.ParamRefText(refs, null);

            ImGui.NextColumn();
            EditorDecorations.ParamRefsSelectables(ParamBank.PrimaryBank, refs, null, val);
            EditorDecorations.ParamRefEnumQuickLink(ParamBank.PrimaryBank, val, refs, null, null, null, null);

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                var opened = EditorDecorations.ParamRefEnumContextMenuItems(ParamBank.PrimaryBank, null, val, ref newObj, refs, null, null, null, null, null, null);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    /// <summary>
    /// Text References
    /// </summary>
    /// <summary>
    /// Param References
    /// </summary>
    public bool FmgRefRow(MapEntityPropertyFieldMeta meta, PropertyInfo propinfo, object val, ref object newObj)
    {
        if (meta != null && meta.FmgRef.Count > 0)
        {
            List<FMGRef> refs = new();

            foreach (FMGRef pRef in meta.FmgRef)
            {
                refs.Add(new FMGRef(pRef.fmg));
            }

            ImGui.NextColumn();

            EditorDecorations.FmgRefText(refs, null);

            ImGui.NextColumn();
            EditorDecorations.FmgRefSelectable(Editor, refs, null, val);
        }

        return false;
    }

    /// <summary>
    /// Enum List
    /// </summary>
    public bool GenericEnumRow(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal)
    {
        if (meta != null && meta.EnumType != null)
        {
            var enumName = meta.EnumType.Name;
            var options = meta.EnumType.Values;

            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted(@$"    {enumName}");
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            ImGui.NextColumn();

            string currentEntry = "___";

            KeyValuePair<string, string> match = options.Where(x => x.Key == val.ToString()).FirstOrDefault();

            ImGui.BeginGroup();

            if (match.Key != null)
            {
                currentEntry = match.Value;
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
                ImGui.TextUnformatted(currentEntry);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
                ImGui.TextUnformatted(currentEntry);
                ImGui.PopStyleColor();
            }

            ImGui.EndGroup();

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                var opened = MsbEnumContextMenu(meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    private string enumSearchStr = "";

    public bool MsbEnumContextMenu(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        Dictionary<string, string> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
        {
            try
            {
                foreach (KeyValuePair<string, string> entry in options)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Key, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Value, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{entry.Key}: {entry.Value}"))
                        {
                            newVal = Convert.ChangeType(entry.Key, val.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();
        return false;
    }

    /// <summary>
    /// Map Reference
    /// </summary>
    public bool MsbReferenceRow(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propInfo,
        object val,
        ref object newval,
        IEnumerable<Entity> entities)
    {
        // TODO: still uses the in-built reference in SF, change to use the Meta string

        var msbRef = propInfo.GetCustomAttribute<MSBReference>();
        if (msbRef == null) return false;

        var name = val as string;
        // Empty values are usually valid, so we just skip
        if (name == null || name == "") return false;

        var refName = msbRef.ReferenceType.Name;

        ImGui.NextColumn();
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.TextUnformatted(@$"   <{refName}>");
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.NextColumn();


        var maps = entities
            .Select((e) => e.Container)
            .Distinct();
        // We're bailing out on multiple maps
        if (maps.Count() != 1)
        {
            return false;
        }


        var map = maps.First();
        var entity = map.GetObjectByName((string)val);
        if (entity == null)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, "No object by that name");
        }
        else
        {
            var alias = AliasUtils.GetEntityAliasName(entity);
            if (alias is null or "")
            {
                ImGui.TextColored(UI.Current.ImGui_ParamRef_Text, $"{entity.PrettyName}");
            }
            else
            {
                ImGui.TextColored(UI.Current.ImGui_ParamRef_Text, $"{entity.PrettyName} - {alias}");
            }
        }

        if (ImGui.BeginPopupContextItem($"{msbRef.ReferenceType.Name}RefContextMenu"))
        {
            var changed = PropertyRowMsbRefContextItems(meta, msbRef, val, ref newval, map);
            ImGui.EndPopup();
            return changed;
        }

        return false;
    }

    public string autocomplete = "";

    public bool PropertyRowMsbRefContextItems(
        MapEntityPropertyFieldMeta meta,
        MSBReference reference,
        object oldval,
        ref object newval,
        ObjectContainer container
    )
    {
        if (oldval is not string name) return false;

        var entity = container.GetObjectByName(name);

        if (entity != null)
        {
            if (ImGui.Selectable($@"Select {name}"))
            {
                EditorCommandQueue.AddCommand($@"map/select/{container.Name}/{name}");
            }
        }
        ImGui.InputTextWithHint("##value", "Search...", ref autocomplete, 128);
        if (autocomplete != "")
        {
            var shownList = container.Objects
                .Where((obj) => reference.ReferenceType.IsInstanceOfType(obj.WrappedObject))
                .Where((obj) => obj.Name.ToLower().Contains(autocomplete.ToLower()))
                .Take(15);
            foreach (var shown in shownList)
            {
                if (ImGui.Selectable($@"{shown.PrettyName}"))
                {
                    newval = shown.Name;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Alias List
    /// </summary>
    public bool AliasEnumRow(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal)
    {
        bool display = false;
        string enumName = "";
        List<AliasEntry> options = null;

        if (meta != null && meta.ShowParticleList)
        {
            options = Editor.Project.Aliases.Particles;
            enumName = "PARTICLES";
            display = true;
        }

        if (meta != null && meta.ShowEventFlagList)
        {
            options = Editor.Project.Aliases.EventFlags;
            enumName = "FLAGS";
            display = true;
        }

        if (meta != null && meta.ShowSoundList)
        {
            options = Editor.Project.Aliases.Sounds;
            enumName = "SOUNDS";
            display = true;
        }

        if (meta != null && meta.ShowTalkList)
        {
            options = Editor.Project.Aliases.TalkScripts;
            enumName = "EZSTATE";
            display = true;
        }

        if (display && options != null)
        {
            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.TextUnformatted(@$"");
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            ImGui.NextColumn();

            string currentEntry = "___";

            var match = options.Where(x => x.ID == val.ToString()).FirstOrDefault();

            ImGui.BeginGroup();

            if (match != null)
            {
                currentEntry = match.Name;

                // Revert if the stored name is empty
                if (currentEntry == "")
                    currentEntry = "___";

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
                ImGui.TextUnformatted(currentEntry);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
                ImGui.TextUnformatted(currentEntry);
                ImGui.PopStyleColor();
            }

            ImGui.EndGroup();

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                var opened = MsbAliasEnumContextMenu(meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public bool MsbAliasEnumContextMenu(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        List<AliasEntry> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
        {
            try
            {
                foreach (var entry in options)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.ID, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Name, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{entry.ID}: {entry.Name}"))
                        {
                            newVal = Convert.ChangeType(entry.ID, val.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();
        return false;
    }

    /// <summary>
    /// DS2: Spawn State List
    /// </summary>
    public bool SpawnStateListRow(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        int rowID)
    {
        bool display = false;
        string enumName = "";
        var matchId = $"{rowID}";
        List<SpawnStatePair> options = null;

        if (meta != null && meta.ShowSpawnStateList)
        {
            if (Editor.Project.MapData.SpawnStates.list != null && matchId.Length > 3)
            {
                matchId = $"{rowID}".Substring(0, 3);

                var states = Editor.Project.MapData.SpawnStates.list;
                var matchedState = states.Where(e => e.id == matchId).FirstOrDefault();
                if (matchedState != null)
                {
                    options = matchedState.states;
                    enumName = "SPAWN STATES";
                    display = true;
                }
            }
        }

        if (display)
        {
            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.TextUnformatted(@$"   <{enumName}>");
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            ImGui.NextColumn();

            string currentEntry = "___";

            var match = options.Where(x => x.value == val.ToString()).FirstOrDefault();

            ImGui.BeginGroup();

            if (match != null)
            {
                currentEntry = match.name;

                // Revert if the stored name is empty
                if (currentEntry == "")
                    currentEntry = "___";

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
                ImGui.TextUnformatted(currentEntry);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
                ImGui.TextUnformatted(currentEntry);
                ImGui.PopStyleColor();
            }

            ImGui.EndGroup();

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                var opened = MsbSpawnStateEnumContextMenu(meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public bool MsbSpawnStateEnumContextMenu(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        List<SpawnStatePair> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
        {
            try
            {
                foreach (var entry in options)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.value, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.name, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{entry.value}: {entry.name}"))
                        {
                            newVal = Convert.ChangeType(entry.value, val.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();
        return false;
    }

    /// <summary>
    /// ER: Mask List
    /// </summary>
    public bool EldenRingAssetMaskAndAnimRow(
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object oldValue,
        ref object newValue,
        ViewportSelection selection)
    {
        if (Editor.Project.MapData.MaskInformation.list == null)
        {
            return false;
        }

        bool changedValue = false;

        Entity ent = selection.GetFilteredSelection<Entity>().First();

        if (ent.WrappedObject is MSBE.Part.Asset assetEnt)
        {
            FormatMaskEntry targetEntry = null;

            // Get the entry for the current model
            foreach (var entry in Editor.Project.MapData.MaskInformation.list)
            {
                if (assetEnt.ModelName == entry.model)
                {
                    targetEntry = entry;
                }
            }

            if (targetEntry != null)
            {
                var mask = oldValue.ToString();

                string sectionOne = "";
                string sectionTwo = "";
                string sectionThree = "";

                bool hasSectionOne = false;
                bool hasSectionTwo = false;
                bool hasSectionThree = false;

                if (mask.Length == 1)
                {
                    mask = $"0{mask}";

                    hasSectionThree = true;

                    sectionThree = mask.Substring(0, 2);
                }
                else if (mask.Length == 2)
                {
                    hasSectionThree = true;

                    sectionThree = mask.Substring(0, 2);
                }
                else if (mask.Length == 4)
                {
                    hasSectionTwo = true;
                    hasSectionThree = true;

                    sectionTwo = mask.Substring(0, 2);
                    sectionThree = mask.Substring(2, 2);
                }
                else if (mask.Length == 6)
                {
                    hasSectionOne = true;
                    hasSectionTwo = true;
                    hasSectionThree = true;

                    sectionOne = mask.Substring(0, 2);
                    sectionTwo = mask.Substring(2, 2);
                    sectionThree = mask.Substring(4, 2);
                }

                ImGui.NextColumn();

                // Text
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);

                ImGui.TextUnformatted($"<Form>");
                ImGui.TextUnformatted($"<Equipment>");
                ImGui.TextUnformatted($"<Position>");

                ImGui.PopStyleColor();
                ImGui.PopStyleVar();

                ImGui.NextColumn();

                // Section 1
                var curSectionOne = targetEntry.section_one.Find(x => x.mask == sectionOne);

                var sectionOneName = "None";
                if (curSectionOne != null)
                {
                    sectionOneName = curSectionOne.name;
                }

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
                ImGui.TextUnformatted($"{sectionOneName}");
                ImGui.PopStyleColor();

                if (ImGui.BeginPopupContextItem($"{targetEntry.model}contextMenu"))
                {
                    if (ImGui.Selectable($"None"))
                    {
                        var newMask = $"{sectionTwo}{sectionThree}";

                        if (!hasSectionTwo)
                        {
                            newMask = $"00{sectionThree}";
                        }

                        newValue = Convert.ChangeType(newMask, oldValue.GetType());
                        changedValue = true;
                    }

                    foreach (var entry in targetEntry.section_one)
                    {
                        if (ImGui.Selectable($"{entry.name}##{entry.mask}{entry.name}"))
                        {
                            var newMask = $"{entry.mask}{sectionTwo}{sectionThree}";

                            if (!hasSectionTwo)
                            {
                                newMask = $"00{sectionThree}";
                            }

                            newValue = Convert.ChangeType(newMask, oldValue.GetType());
                            changedValue = true;
                        }
                    }

                    ImGui.EndPopup();
                }

                // Section 2
                var curSectionTwo = targetEntry.section_two.Find(x => x.mask == sectionTwo);

                var sectionTwoName = "None";
                if (curSectionTwo != null)
                {
                    sectionTwoName = curSectionTwo.name;
                }

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
                ImGui.TextUnformatted($"{sectionTwoName}");
                ImGui.PopStyleColor();

                if (ImGui.BeginPopupContextItem($"{targetEntry.model}contextMenu2"))
                {
                    if (ImGui.Selectable($"None"))
                    {
                        var newMask = $"00{sectionThree}";

                        if (hasSectionOne)
                        {
                            newMask = $"{sectionOne}00{sectionThree}";
                        }

                        newValue = Convert.ChangeType(newMask, oldValue.GetType());
                        changedValue = true;
                    }

                    foreach (var entry in targetEntry.section_two)
                    {
                        if (ImGui.Selectable($"{entry.name}##{entry.mask}{entry.name}"))
                        {
                            var newMask = $"{entry.mask}{sectionThree}";

                            if (hasSectionOne)
                            {
                                newMask = $"{sectionOne}{entry.mask}{sectionThree}";
                            }

                            newValue = Convert.ChangeType(newMask, oldValue.GetType());
                            changedValue = true;
                        }
                    }

                    ImGui.EndPopup();
                }

                var curSectionThree = targetEntry.section_three.Find(x => x.mask == sectionThree);

                var sectionThreeName = "None";
                if (curSectionThree != null)
                {
                    sectionThreeName = curSectionThree.name;
                }

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
                ImGui.TextUnformatted($"{sectionThreeName}");
                ImGui.PopStyleColor();

                if (ImGui.BeginPopupContextItem($"{targetEntry.model}contextMenu3"))
                {
                    if (ImGui.Selectable($"None"))
                    {
                        var newMask = $"-1";

                        newValue = Convert.ChangeType(newMask, oldValue.GetType());
                        changedValue = true;
                    }

                    foreach (var entry in targetEntry.section_three)
                    {
                        if (ImGui.Selectable($"{entry.name}##{entry.mask}{entry.name}"))
                        {
                            var newMask = $"{entry.mask}";

                            if (hasSectionOne)
                            {
                                newMask = $"{sectionTwo}{entry.mask}";
                            }
                            if (hasSectionTwo)
                            {
                                newMask = $"{sectionOne}{sectionTwo}{entry.mask}";
                            }

                            newValue = Convert.ChangeType(newMask, oldValue.GetType());
                            changedValue = true;
                        }
                    }

                    ImGui.EndPopup();
                }
            }
        }

        return changedValue;
    }

    /// <summary>
    /// Model Link Button
    /// </summary>
    public void ModelNameRow(
        MapEntityPropertyFieldMeta meta,
        IEnumerable<Entity> entSelection,
        PropertyInfo propinfo,
        object value)
    {
        if (meta.ShowModelLinkButton == false)
        {
            return;
        }

        var ent = entSelection.FirstOrDefault();
        var loadType = "";

        if (ent != null)
        {
            if (ent.IsPartEnemy() || ent.IsPartDummyEnemy())
            {
                loadType = "Character";
            }
            else if (ent.IsPartAsset() || ent.IsPartDummyAsset())
            {
                loadType = "Asset";
            }
            else if (ent.IsPartMapPiece())
            {
                loadType = "MapPiece";
            }
            else
            {
                return;
            }
        }

        ImGui.NextColumn();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.TextUnformatted(@$"");
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        ImGui.NextColumn();

        ImGui.BeginGroup();

        var width = ImGui.GetColumnWidth() * 0.95f;

        if (ImGui.Button("Go to Model", new Vector2(width, 24)))
        {
            if (loadType == "MapPiece")
            {
                var map = (MapContainer)ent.Parent.Container;
                var mapPieceName = $"{value}".Replace("m", $"{map.Name}_");

                EditorCommandQueue.AddCommand($"model/load/{mapPieceName}/{loadType}/{map.Name}");
            }
            else
            {
                EditorCommandQueue.AddCommand($"model/load/{value}/{loadType}");
            }
        }
        UIHelper.Tooltip("View this model in the Model Editor, loading it automatically.");

        ImGui.EndGroup();
    }

    public void DisplayParamLink(Entity firstEnt, ref ViewportSelection selection, ref int refID)
    {
        var scale = DPI.Scale;
        var width = ImGui.GetWindowWidth() / 100;

        if (firstEnt.References.Count == 0)
            return;

        // Only relevant to assets
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 && firstEnt.IsPartPureAsset())
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            UIHelper.Tooltip("The current selection references these rows in params");

            if (ImGui.Button(ForkAwesome.Binoculars + "##ParamJump_ViewRef_Asset" + refID, new Vector2(width * 5, 20 * scale)))
            {
                BoundingBox box = new();

                if (firstEnt.RenderSceneMesh != null)
                {
                    box = firstEnt.RenderSceneMesh.GetBounds();
                }
                else if (firstEnt.Container.RootObject == firstEnt)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = firstEnt.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    box = nodeBox;
                }

                Editor.MapViewport.FrameBox(box);
            }

            if (firstEnt is Entity e)
            {
                // Jump to AssetEnvironmentGeometryParam param row
                var displayName = $"AssetEnvironmentGeometryParam: {e.Name}";
                var modelName = e.GetPropertyValue<string>("ModelName");
                var aliasName = "";
                var assetRowId = GetAssetEnvironmentGeometryParamRow(modelName);

                if (modelName != null)
                {
                    modelName = modelName.ToLower();

                    if (e.IsPartAsset() || e.IsPartDummyAsset())
                    {
                        aliasName = AliasUtils.GetAssetAlias(modelName);
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##AssetEnvironmentGeometryParam_ParamJump" + refID, new Vector2(width * 94, 20 * scale)))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/AssetEnvironmentGeometryParam/{assetRowId}");
                }
            }
        }

        // Only relevant to characters
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6
            && (firstEnt.IsPartEnemy() || firstEnt.IsPartDummyEnemy()))
        {
            ImGui.Separator();
            ImGui.Text("Params:");
            ImGui.Separator();
            UIHelper.Tooltip("The current selection references these rows in params");

            if (ImGui.Button(ForkAwesome.Binoculars + "##ParamJump_ViewRef_Enemy" + refID, new Vector2(width * 5, 20 * scale)))
            {
                BoundingBox box = new();

                if (firstEnt.RenderSceneMesh != null)
                {
                    box = firstEnt.RenderSceneMesh.GetBounds();
                }
                else if (firstEnt.Container.RootObject == firstEnt)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = firstEnt.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    box = nodeBox;
                }

                Editor.MapViewport.FrameBox(box);
            }

            if (firstEnt is Entity e)
            {
                // Jump to AssetEnvironmentGeometryParam param row
                var displayName = $"ChrModelParam : {e.Name}";
                var modelName = e.GetPropertyValue<string>("ModelName");
                var aliasName = "";
                var modelRowId = GetChrModelParamRow(modelName);

                if (modelName != null)
                {
                    modelName = modelName.ToLower();

                    if (e.IsPartEnemy() || e.IsPartDummyEnemy())
                    {
                        aliasName = AliasUtils.GetCharacterAlias(modelName);
                    }

                    if (aliasName != "")
                    {
                        displayName = displayName + " - " + aliasName;
                    }
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.Button(displayName + "##ChrModelParam_ParamJump" + refID, new Vector2(width * 94, 20 * scale)))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/ChrModelParam/{modelRowId}");
                }
            }
        }
    }

    private string GetAssetEnvironmentGeometryParamRow(string modelName)
    {
        string assetId = modelName.Replace("AEG", "").Replace("_", "");
        return assetId;
    }

    private string GetChrModelParamRow(string modelName)
    {
        string chrId = modelName.Replace("c", "");
        return chrId;
    }

    public void DisplayConnectCollisionInformation(Entity firstEnt)
    {
        if (firstEnt.IsPartConnectCollision())
        {
            byte[] mapIds = (byte[])PropFinderUtil.FindPropertyValue("MapID", firstEnt.WrappedObject);

            string mapString = "m";

            if (mapIds != null)
            {
                for (int i = 0; i < mapIds.Length; i++)
                {
                    var num = mapIds[i];
                    var str = "";

                    if (num == 255)
                    {
                        num = 0;
                    }

                    if (num < 10)
                    {
                        str = $"0{num}";
                    }
                    else
                    {
                        str = num.ToString();
                    }

                    if (i < mapIds.Length - 1)
                    {
                        mapString = mapString + $"{str}_";
                    }
                    else
                    {
                        mapString = mapString + $"{str}";
                    }
                }
            }
            ImGui.Separator();
            ImGui.Text($"Target Map ID:");
            ImGui.Separator();

            ImGui.Text(mapString);
            UIHelper.DisplayAlias(AliasUtils.GetMapNameAlias(mapString));
            ImGui.Text("");
        }
    }

    public void DisplayReferenceTo(Entity firstEnt, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.References.Count == 0)
            return;

        var scale = DPI.Scale;

        ImGui.Separator();
        ImGui.Text("References:");
        ImGui.Separator();
        UIHelper.Tooltip("The current selection references these map objects.");

        var width = ImGui.GetWindowWidth() / 100;

        foreach (KeyValuePair<string, object[]> m in firstEnt.References)
        {
            foreach (var n in m.Value)
            {
                if (n is Entity e)
                {
                    // View Reference in Viewport
                    if (ImGui.Button(ForkAwesome.Binoculars + "##MSBRefBy" + refID, new Vector2(width * 5, 20 * scale)))
                    {
                        BoundingBox box = new();

                        if (e.RenderSceneMesh != null)
                        {
                            box = e.RenderSceneMesh.GetBounds();
                        }
                        else if (e.Container.RootObject == e)
                        {
                            // Selection is transform node
                            Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                            Vector3 pos = e.GetLocalTransform().Position;
                            BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                            box = nodeBox;
                        }

                        Editor.MapViewport.FrameBox(box);
                    }

                    // Change Selection to Reference
                    var displayName = $"{e.WrappedObject.GetType().Name}: {e.Name}";
                    var modelName = e.GetPropertyValue<string>("ModelName");
                    var aliasName = "";

                    if (modelName != null)
                    {
                        modelName = modelName.ToLower();

                        if (e.IsPartEnemy() || e.IsPartDummyEnemy())
                        {
                            aliasName = AliasUtils.GetCharacterAlias(modelName);
                        }
                        if (e.IsPartAsset() || e.IsPartDummyAsset())
                        {
                            aliasName = AliasUtils.GetAssetAlias(modelName);
                        }
                        if (e.IsPartMapPiece())
                        {
                            aliasName = AliasUtils.GetMapPieceAlias(modelName);
                        }

                        if (aliasName != "")
                        {
                            displayName = displayName + " - " + aliasName;
                        }
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.Button(displayName + "##MSBRefTo" + refID, new Vector2(width * 94, 20 * scale)))
                    {
                        selection.ClearSelection();
                        selection.AddSelection(e);
                    }
                }
                else if (n is ObjectContainerReference r)
                {
                    // Try to select the map's RootObject if it is loaded, and the reference otherwise.
                    // It's not the end of the world if we choose the wrong one, as SceneTree can use either,
                    // but only the RootObject has the TransformNode and Viewport integration.
                    var mapid = r.Name;
                    var prettyName = $"{ForkAwesome.Cube} {mapid}";
                    prettyName = $"{prettyName} {AliasUtils.GetMapNameAlias(mapid)}";

                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.Button(prettyName + "##MSBRefTo" + refID, new Vector2(width * 94, 20 * scale)))
                    {
                        ISelectable rootTarget = r.GetSelectionTarget();
                        selection.ClearSelection();
                        selection.AddSelection(rootTarget);
                        // For this type of connection, jump to the object in the list to actually load the map
                        // (is this desirable in other cases?). It could be possible to have a Load context menu
                        // here, but that should be shared with SceneTree.
                        selection.GotoTreeTarget = rootTarget;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        var universe = Editor.Universe;
                        MapContainer map = universe.GetLoadedMapContainer(mapid);
                        if (map == null)
                        {
                            if (ImGui.Selectable("Load Map"))
                            {
                                universe.LoadMap(mapid);
                                Editor.MapListView.SignalLoad(mapid);
                            }
                        }
                        else
                        {
                            if (ImGui.Selectable("Unload Map"))
                            {
                                universe.UnloadContainer(map);
                            }
                        }

                        ImGui.EndPopup();
                    }
                }

                refID++;
            }
        }
    }

    public void DisplayReferenceBy(Entity firstEnt, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.GetReferencingObjects().Count == 0)
            return;

        var scale = DPI.Scale;

        ImGui.Separator();
        ImGui.Text("Referenced By:");
        ImGui.Separator();
        UIHelper.Tooltip("The current selection is referenced by these map objects.");

        var width = ImGui.GetWindowWidth() / 100;

        foreach (Entity m in firstEnt.GetReferencingObjects())
        {
            // View Reference in Viewport
            if (ImGui.Button(ForkAwesome.Binoculars + "##MSBRefBy" + refID, new Vector2(width * 5, 20 * scale)))
            {
                BoundingBox box = new();

                if (m.RenderSceneMesh != null)
                {
                    box = m.RenderSceneMesh.GetBounds();
                }
                else if (m.Container.RootObject == m)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = m.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    box = nodeBox;
                }

                Editor.MapViewport.FrameBox(box);
            }

            // Change Selection to Reference
            var displayName = $"{m.WrappedObject.GetType().Name}: {m.Name}";
            var modelName = m.GetPropertyValue<string>("ModelName");
            var aliasName = "";

            if (modelName != null)
            {
                modelName = modelName.ToLower();

                if (m.IsPartEnemy() || m.IsPartDummyEnemy())
                {
                    aliasName = AliasUtils.GetCharacterAlias(modelName);
                }
                if (m.IsPartAsset() || m.IsPartDummyAsset())
                {
                    aliasName = AliasUtils.GetAssetAlias(modelName);
                }
                if (m.IsPartMapPiece())
                {
                    aliasName = AliasUtils.GetMapPieceAlias(modelName);
                }

                if (aliasName != "")
                {
                    displayName = displayName + " - " + aliasName;
                }
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);

            if (ImGui.Button(displayName + "##MSBRefBy" + refID, new Vector2(width * 94, 20 * scale)))
            {
                selection.ClearSelection();
                selection.AddSelection(m);
            }

            refID++;
        }
    }

    public void DisplayConnectionInformation(Entity firstEnt)
    {
        if (firstEnt.IsRegionConnection())
        {
            sbyte[] mapIds = (sbyte[])PropFinderUtil.FindPropertyValue("TargetMapID", firstEnt.WrappedObject);

            string mapString = "m";

            if (mapIds != null)
            {
                for (int i = 0; i < mapIds.Length; i++)
                {
                    var num = mapIds[i];
                    var str = "";
                    if (num < 10)
                    {
                        str = $"0{num}";
                    }
                    else
                    {
                        str = num.ToString();
                    }

                    if (i < mapIds.Length - 1)
                    {
                        mapString = mapString + $"{str}_";
                    }
                    else
                    {
                        mapString = mapString + $"{str}";
                    }
                }
            }

            ImGui.Separator();
            ImGui.Text($"Target Map ID:");
            ImGui.Separator();

            ImGui.Text(mapString);
            UIHelper.DisplayAlias(AliasUtils.GetMapNameAlias(mapString));
            ImGui.Text("");
        }
    }

    /// <summary>
    /// Scale Property: Copy action
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="obj"></param>
    public void CopyCurrentScaleValue(PropertyInfo prop, object obj)
    {
        CFG.Current.SavedScale = (Vector3)prop.GetValue(obj, null);
    }

    /// <summary>
    /// Scale Property: Paste action
    /// </summary>
    public void PasteCurrentScaleVale()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in Editor.Selection.GetFilteredSelection<Entity>())
        {
            actlist.Add(sel.ApplySavedScale());
        }

        CompoundAction action = new(actlist);
        Editor.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Rotation Property: Copy action
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="obj"></param>
    public void CopyCurrentRotationValue(PropertyInfo prop, object obj)
    {
        CFG.Current.SavedRotation = (Vector3)obj;
    }

    /// <summary>
    /// Rotation Property: Paste action
    /// </summary>
    public void PasteCurrentRotationValue()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in Editor.Selection.GetFilteredSelection<Entity>())
        {
            actlist.Add(sel.ApplySavedRotation());
        }

        CompoundAction action = new(actlist);
        Editor.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Position Property: Copy action
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="obj"></param>
    public void CopyCurrentPositionValue(PropertyInfo prop, object obj)
    {
        CFG.Current.SavedPosition = (Vector3)obj;
    }

    /// <summary>
    /// Position Property: Paste action
    /// </summary>
    public void PasteCurrentPositionValue()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in Editor.Selection.GetFilteredSelection<Entity>())
        {
            actlist.Add(sel.ApplySavedPosition());
        }

        CompoundAction action = new(actlist);
        Editor.EditorActionManager.ExecuteAction(action);
    }
}

