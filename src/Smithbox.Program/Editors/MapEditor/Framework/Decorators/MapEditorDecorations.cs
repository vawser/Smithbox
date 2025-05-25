using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor.Framework.Decorators;

public static class MapEditorDecorations
{

    public static List<string> DS2_ObjectInstanceParams = new List<string>
    {
        "mapobjectinstanceparam",
        "treasureboxparam"
    };

    /// <summary>
    /// Param References
    /// </summary>
    public static bool ParamRefRow(MapEditorScreen editor, MapEntityPropertyFieldMeta meta, PropertyInfo propinfo, object val, ref object newObj)
    {
        if (editor.Project.ParamEditor == null)
            return false;

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
                        var selection = editor.ViewportSelection;

                        if (selection.IsSelection())
                        {
                            // Get cur map name, apend to ParamName
                            var sel = selection.GetSelection().First() as Entity;
                            var map = sel.Parent.Name;

                            paramName = $"{paramName}_{map}".ToLower();

                            refs.Add(new ParamRef(null, paramName));
                        }
                    }
                }
                // DS1 Bank Params
                else if (meta.SpecialHandling == "BankParam")
                {
                    var selection = editor.ViewportSelection;

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

                        refs.Add(new ParamRef(null, $"{mapPrefix}_{paramName}"));
                    }
                }
                // DS2 Map Params
                else if (meta.SpecialHandling == "MapParam")
                {
                    var selection = editor.ViewportSelection;

                    if (selection.IsSelection())
                    {
                        // Get cur map name, append to ParamName
                        var sel = selection.GetSelection().First() as Entity;
                        var map = sel.Parent.Name;

                        var paramName = pRef.ParamName;

                        refs.Add(new ParamRef(null, $"{paramName}_{map}"));
                    }
                }
                else
                {
                    refs.Add(new ParamRef(null, pRef.ParamName));
                }
            }

            ImGui.NextColumn();

            FieldDecorators.ParamReference_Title(refs, null);

            ImGui.NextColumn();

            if (editor.Project.ParamEditor != null)
            {
                FieldDecorators.ParamReference_Value(editor.Project.ParamEditor, editor.Project.ParamData.PrimaryBank, refs, null, val);
                FieldDecorators.ParamReference_ContextMenu(editor.Project.ParamEditor, editor.Project.ParamData.PrimaryBank, val, null, refs);

                if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
                {
                    var opened = FieldDecorators.Decorator_ContextMenuItems(editor.Project.ParamEditor, editor.Project.ParamData.PrimaryBank, null, val, ref newObj, refs, null, null, null, null, null, null);
                    ImGui.EndPopup();
                    return opened;
                }
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
    public static bool FmgRefRow(MapEditorScreen editor, MapEntityPropertyFieldMeta meta, PropertyInfo propinfo, object val, ref object newObj)
    {
        if (meta != null && meta.FmgRef.Count > 0)
        {
            List<FMGRef> refs = new();

            foreach (FMGRef pRef in meta.FmgRef)
            {
                refs.Add(new FMGRef(null, pRef.fmg));
            }

            ImGui.NextColumn();

            FieldDecorators.TextReference_Title(refs, null);

            ImGui.NextColumn();
            FieldDecorators.TextReference_Value(editor, refs, null, val);
        }

        return false;
    }

    /// <summary>
    /// Enum List
    /// </summary>
    public static bool GenericEnumRow(
        MapEditorScreen editor,
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
                var opened = MsbEnumContextMenu(editor, meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    private static string enumSearchStr = "";

    public static bool MsbEnumContextMenu(
        MapEditorScreen editor,
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
    public static bool MsbReferenceRow(
        MapEditorScreen editor,
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
            var alias = AliasUtils.GetEntityAliasName(editor.Project, entity);
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
            var changed = PropertyRowMsbRefContextItems(editor, meta, msbRef, val, ref newval, map);
            ImGui.EndPopup();
            return changed;
        }

        return false;
    }

    static string autocomplete = "";
    public static bool PropertyRowMsbRefContextItems(
        MapEditorScreen editor,
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
    public static bool AliasEnumRow(
        MapEditorScreen editor,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal)
    {
        bool display = false;
        List<AliasEntry> options = null;

        if (meta != null && meta.ShowParticleList)
        {
            options = editor.Project.Aliases.Particles;
            display = true;
        }

        if (meta != null && meta.ShowEventFlagList)
        {
            options = editor.Project.Aliases.EventFlags;
            display = true;
        }

        if (meta != null && meta.ShowSoundList)
        {
            options = editor.Project.Aliases.Sounds;
            display = true;
        }

        if (meta != null && meta.ShowTalkList)
        {
            options = editor.Project.Aliases.TalkScripts;
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
                var opened = MsbAliasEnumContextMenu(editor, meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public static bool MsbAliasEnumContextMenu(
        MapEditorScreen editor,
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
    public static bool SpawnStateListRow(
        MapEditorScreen editor,
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
            if (matchId.Length > 3)
            {
                matchId = $"{rowID}".Substring(0, 3);

                var states = editor.Project.MapSpawnStates.list;
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
                var opened = MsbSpawnStateEnumContextMenu(editor, meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public static bool MsbSpawnStateEnumContextMenu(
        MapEditorScreen editor,
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
    public static bool EldenRingAssetMaskAndAnimRow(
        MapEditorScreen editor,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object oldValue,
        ref object newValue,
        ViewportSelection selection)
    {
        bool changedValue = false;

        Entity ent = selection.GetFilteredSelection<Entity>().First();

        if (ent.WrappedObject is MSBE.Part.Asset assetEnt)
        {
            FormatMaskEntry targetEntry = null;

            // Get the entry for the current model
            foreach (var entry in editor.Project.MsbMasks.list)
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

                if (mask.Length == 1)
                {
                    mask = $"0{mask}";

                    sectionThree = mask.Substring(0, 2);
                }
                else if (mask.Length == 2)
                {
                    sectionThree = mask.Substring(0, 2);
                }
                else if (mask.Length == 4)
                {
                    hasSectionTwo = true;

                    sectionTwo = mask.Substring(0, 2);
                    sectionThree = mask.Substring(2, 2);
                }
                else if (mask.Length == 6)
                {
                    hasSectionOne = true;
                    hasSectionTwo = true;

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
    public static void ModelNameRow(
        MapEditorScreen editor,
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
}

