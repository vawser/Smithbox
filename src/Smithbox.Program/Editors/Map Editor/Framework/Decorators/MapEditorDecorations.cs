using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.Viewport;
using System.Numerics;
using System.Reflection;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class MapEditorDecorations
{

    public static List<string> DS2_ObjectInstanceParams = new List<string>
    {
        "mapobjectinstanceparam",
        "treasureboxparam"
    };

    public static bool ParamRefRow(MapEditorView view, MapEntityPropertyFieldMeta meta, PropertyInfo propinfo, object val, ref object newObj)
    {
        if (view.Project.Handler.ParamEditor == null)
            return false;

        var activeView = view.Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (meta != null && meta.ParamRef.Count > 0)
        {
            List<ParamRef> refs = new();

            foreach (var pRef in meta.ParamRef)
            {
                // DS2 ObjectInstanceParam
                if (meta.SpecialHandling == "ObjectInstanceParam")
                {
                    foreach (var param in DS2_ObjectInstanceParams)
                    {
                        var paramName = param;
                        var selection = view.ViewportSelection;

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
                    var selection = view.ViewportSelection;

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
                    var selection = view.ViewportSelection;

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

            ParamReferenceHelper.Label(activeView, refs, null);

            ImGui.NextColumn();

            if (view.Project.Handler.ParamEditor != null)
            {
                ParamReferenceHelper.Hint(activeView, refs, null, val);
                ParamReferenceHelper.Click(activeView, val, null, refs);

                if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
                {
                    var opened = ParamReferenceHelper.ContextMenu(activeView, refs, null, val, ref newObj, null);
                    ImGui.EndPopup();
                    return opened;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// ESD References: under a field naming a talk script (meta EsdRef, or TalkAlias), the script it resolves to, laid
    /// out like the param references. Ctrl+click or the context menu opens it in the ESD Editor.
    /// </summary>
    public static void EsdRefRow(MapEditorView view, MapEntityPropertyFieldMeta meta, IEnumerable<Entity> entSelection, PropertyInfo propinfo, object val)
    {
        if (meta == null || string.IsNullOrEmpty(meta.EsdRef) || !EsdReferenceHelper.IsAvailable(view.Project))
            return;

        var mapName = entSelection?.FirstOrDefault()?.Container?.Name;
        var target = EsdReferenceHelper.Resolve(view.Project, meta.EsdRef, val, mapName);
        if (target == null)
            return;

        ImGui.NextColumn();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted($"   <{LOC.Get("ESD_Reference_Label")}>");
        ImGui.PopStyleVar();

        ImGui.NextColumn();

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.Text, target.Exists ? UI.Current.ImGui_ParamRef_Text : UI.Current.ImGui_ParamRefMissing_Text);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(target.Exists ? target.Display : "---");
        ImGui.PopStyleColor();
        ImGui.EndGroup();

        if (!target.Exists)
            GUI.Tooltip(target.Display);

        EsdReferenceHelper.Click(target);

        if (ImGui.BeginPopupContextItem($"{propinfo.Name}EsdRefContextMenu"))
        {
            EsdReferenceHelper.ContextMenu(view.Project, meta.EsdRef, val, mapName, target);
            ImGui.EndPopup();
        }
    }

    public static bool FmgRefRow(MapEditorView view, MapEntityPropertyFieldMeta meta, PropertyInfo propinfo, object val, ref object newObj)
    {
        if (view.Project.Handler.ParamEditor == null)
            return false;

        var activeView = view.Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (meta != null && meta.FmgRef.Count > 0)
        {
            List<FMGRef> refs = new();

            foreach (var pRef in meta.FmgRef)
            {
                refs.Add(new FMGRef(null, pRef.fmg));
            }

            ImGui.NextColumn();

            TextReferenceHelper.Label(activeView, refs, null);

            ImGui.NextColumn();

            TextReferenceHelper.Hint(activeView, refs, null, val);
        }

        return false;
    }

    /// <summary>
    /// Enum List
    /// </summary>
    public static bool GenericEnumRow(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal)
    {
        if (view.Project.Handler.ParamData == null)
            return false;

        var enums = view.Project.Handler.ParamData.Enums;

        if (meta != null && meta.EnumType != null)
        {
            var enumName = meta.EnumType;

            var entry = enums.List.FirstOrDefault(e => e.Key == enumName);

            if (entry == null)
                return false;

            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted(@$"    {enumName}");
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            ImGui.NextColumn();

            string currentEntry = "___";

            var matchedOption = entry.Options.Where(x => x.Key == val.ToString()).FirstOrDefault();

            ImGui.BeginGroup();

            if (matchedOption != null)
            {
                currentEntry = matchedOption.GetName();
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

            ProjectEnumHelper.OptionTooltip(matchedOption);

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                var opened = GenericEnumContextMenu(view, meta, propinfo, val, ref newVal, entry.Options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public static bool MsbEnumRow(
        MapEditorView view,
        MapEntityPropertyMeta meta,
        MapEntityPropertyFieldMeta fieldMeta,
        PropertyInfo propinfo,
        object val,
        ref object newVal)
    {
        if (view.Project.Handler.ParamData == null)
            return false;

        if (fieldMeta != null && fieldMeta.EnumType != null)
        {
            var enumName = fieldMeta.EnumType;

            var entry = meta.EnumList.FirstOrDefault(e => e.Key == enumName);

            if (entry.Value == null)
                return false;

            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted(@$"    {enumName}");
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            ImGui.NextColumn();

            string currentEntry = "___";

            var matchedOption = entry.Value.Values.Where(x => x.Key == val.ToString()).FirstOrDefault();

            ImGui.BeginGroup();

            if (matchedOption.Value != null)
            {
                currentEntry = matchedOption.Value;
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
                var opened = MsbEnumContextMenu(view, fieldMeta, propinfo, val, ref newVal, entry.Value.Values);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    private static string enumSearchStr = "";

    public static bool GenericEnumContextMenu(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        List<ParamEnumOption> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350 * DPI.UIScale(), ImGui.GetFrameHeight()), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350f * DPI.UIScale(), ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
        {
            try
            {
                foreach (var entry in options)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Key, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.GetName(), " ")
                        || enumSearchStr == "")
                    {
                        var picked = ImGui.Selectable($"{entry.Key}: {entry.GetName()}");

                        ProjectEnumHelper.OptionTooltip(entry);

                        if (picked)
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

    public static bool MsbEnumContextMenu(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        Dictionary<string, string> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350 * DPI.UIScale(), ImGui.GetFrameHeight()), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350f * DPI.UIScale(), ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
        {
            try
            {
                foreach (var entry in options)
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
        MapEditorView view,
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
            MsbReferenceTarget(view, entity);
        }

        if (ImGui.BeginPopupContextItem($"{msbRef.ReferenceType.Name}RefContextMenu"))
        {
            var changed = PropertyRowMsbRefContextItems(view, meta, msbRef, val, ref newval, map);
            ImGui.EndPopup();
            return changed;
        }

        return false;
    }

    /// <summary>
    /// Map Reference by RegionID: an int field naming a region of the map by its RegionID (e.g. a route's portals)
    /// </summary>
    public static void MsbRegionIdReferenceRow(
        MapEditorView view,
        PropertyInfo propInfo,
        object val,
        IEnumerable<Entity> entities)
    {
        var regionRef = propInfo.GetCustomAttribute<MSBRegionIDReference>();
        if (regionRef == null || val is not int regionId)
            return;

        var refName = regionRef.ReferenceType.Name;

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
            return;

        var map = maps.First();

        var matches = map.Objects
            .Where(e => e.WrappedObject != null && regionRef.ReferenceType.IsInstanceOfType(e.WrappedObject)
                && PropFinderUtil.FindPropertyValue("RegionID", e.WrappedObject) is int id && id == regionId)
            .ToList();

        if (matches.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, $"No {refName} with that Region ID");
            return;
        }

        var entity = matches[0];
        MsbReferenceTarget(view, entity);

        if (ImGui.BeginPopupContextItem($"{propInfo.Name}RegionIdRefContextMenu"))
        {
            if (ImGui.Selectable($@"Select"))
            {
                EditorCommandQueue.AddCommand($@"map/select/{map.Name}/{entity.Name}");
            }

            ImGui.Separator();
            ImGui.Text($"Name: {entity.Name}");
            ImGui.Text($"Region ID: {regionId}");
            ImGui.EndPopup();
        }

        if (matches.Count > 1)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color,
                $"{matches.Count} regions share this Region ID; give each a unique one");
        }
    }

    /// <summary>
    /// The referenced object of a map reference row: frame button and display text
    /// </summary>
    private static void MsbReferenceTarget(MapEditorView view, Entity entity)
    {
        // View Reference in Viewport
        if (ImGui.Button(Icons.Binoculars + "##focusRef" + entity.Name,
            DPI.IconButtonSize))
        {
            BoundingBox box = new();

            if (entity.RenderSceneMesh != null)
            {
                box = entity.RenderSceneMesh.GetBounds();
            }
            else if (entity.Container.RootObject == entity)
            {
                // Selection is transform node
                Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                Vector3 pos = entity.GetLocalTransform().Position;
                BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                box = nodeBox;
            }

            view.FrameAction.ApplyViewportFrameWithBox(box);
        }

        ImGui.SameLine();

        // Name
        var alias = AliasHelper.GetEntityAliasName(view.Project, entity);

        var displayText = $"";

        if(CFG.Current.MapEditor.MapEditor_Properties_Display_Reference_Name)
        {
            displayText = $"{entity.PrettyName}";
        }

        if (CFG.Current.MapEditor.MapEditor_Properties_Display_Reference_Entity_ID)
        {
            // Wherever the format keeps it (NR parts and regions too)
            object entId = MsbEntityIds.TryGet(entity.WrappedObject, out var entIdValue) ? entIdValue : null;

            if (entId != null)
            {
                var newText = $"{entId}";

                if (CFG.Current.MapEditor.MapEditor_Properties_Display_Reference_Name)
                {
                    displayText = $"{displayText} - {newText}";
                }
                else
                {
                    displayText = $"{newText}";
                }
            }
        }

        if (CFG.Current.MapEditor.MapEditor_Properties_Display_Reference_Alias)
        {
            if (!CFG.Current.MapEditor.MapEditor_Properties_Display_Reference_Name && !CFG.Current.MapEditor.MapEditor_Properties_Display_Reference_Entity_ID)
            {
                if (alias is null or "")
                {
                    displayText = $"";
                }
                else
                {
                    displayText = $"{alias}";
                }
            }
            else
            {
                if (alias is null or "")
                {
                    displayText = $"{displayText}";
                }
                else
                {
                    displayText = $"{displayText} - {alias}";
                }
            }
        }

        ImGui.TextColored(UI.Current.ImGui_ParamRef_Text, $"{displayText}");
    }

    public static bool PropertyRowMsbRefContextItems(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        MSBReference reference,
        object oldval,
        ref object newval,
        ObjectContainer container
    )
    {
        if (oldval is not string name) 
            return false;


        var entity = container.GetObjectByName(name);

        if (entity != null)
        {
            if (ImGui.Selectable($@"Select"))
            {
                EditorCommandQueue.AddCommand($@"map/select/{container.Name}/{name}");
            }

            ImGui.Separator();

            ImGui.Text($"Name: {name}");

            // Wherever the format keeps it (NR parts and regions too)
            object entId = MsbEntityIds.TryGet(entity.WrappedObject, out var entIdValue) ? entIdValue : null;

            if (entId != null)
            {
                ImGui.Text($"Entity ID: {entId}");
            }

        }

        /*
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
        */

        return false;
    }

    /// <summary>
    /// Alias List
    /// </summary>
    public static bool AliasEnumRow(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal)
    {
        bool display = false;
        List<AliasEntry> options = null;
        bool isEffect = false;

        var aliases = view.Project.Handler.ProjectData.Aliases;

        if (meta != null && meta.ShowParticleList && aliases.TryGetValue(ProjectAliasType.Particles, out List<AliasEntry> particleAliases))
        {
            options = particleAliases;
            isEffect = true;
            display = true;
        }

        bool isFlag = false;
        if (meta != null && meta.ShowEventFlagList && aliases.TryGetValue(ProjectAliasType.EventFlags, out List<AliasEntry> eventFlagAliases))
        {
            options = eventFlagAliases;
            isEffect = false;
            isFlag = true;
            display = true;
        }

        bool isSound = false;
        if (meta != null && meta.ShowSoundList && aliases.TryGetValue(ProjectAliasType.Sounds, out List<AliasEntry> soundAliases))
        {
            options = soundAliases;
            isEffect = false;
            isSound = true;
            isFlag = false;
            display = true;
        }

        if (meta != null && meta.ShowTalkList && aliases.TryGetValue(ProjectAliasType.TalkScripts, out List<AliasEntry> talkAliases))
        {
            options = talkAliases;
            isEffect = false;
            isSound = false;
            isFlag = false;
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

            // inline Play, in front of the alias text
            if (isSound)
                SoundReferenceHelper.PlayButton(view.Project, val, propinfo.Name);

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

            // ER: a warning when the game doesn't store the flag
            if (isFlag && long.TryParse(val?.ToString(), out var flag))
                EventFlagDisplay.Marker(view.Project, flag);

            ImGui.EndGroup();

            // An effect id: Ctrl+click, or the context menu, opens it in the FX Editor; a sound ID likewise in the Sound Editor
            if (isEffect)
                FxReferenceHelper.Click(view.Project, val);
            if (isSound)
                SoundReferenceHelper.Click(view.Project, val);

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                if (isEffect && FxReferenceHelper.IsAvailable(view.Project) && FxReferenceHelper.TryGetId(val, out _))
                {
                    FxReferenceHelper.ContextMenu(view.Project, val);
                    ImGui.Separator();
                }

                if (isSound && SoundReferenceHelper.IsAvailable(view.Project) && SoundReferenceHelper.TryGetId(val, out _))
                {
                    SoundReferenceHelper.ContextMenu(view.Project, val);
                    ImGui.Separator();
                }

                var opened = MsbAliasEnumContextMenu(view, meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public static bool MsbAliasEnumContextMenu(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        List<AliasEntry> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350 * DPI.UIScale(), ImGui.GetFrameHeight()), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350f * DPI.UIScale(), ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
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
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        int rowID)
    {
        bool display = false;
        string enumName = "";
        var matchId = $"{rowID}";
        List<SpawnStateContents> options = null;

        if (meta != null && meta.ShowSpawnStateList)
        {
            if (matchId.Length > 3)
            {
                matchId = $"{rowID}".Substring(0, 3);

                var states = view.Project.Handler.MapData.SpawnStates.List;
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
                var opened = MsbSpawnStateEnumContextMenu(view, meta, propinfo, val, ref newVal, options);
                ImGui.EndPopup();
                return opened;
            }
        }

        return false;
    }

    public static bool MsbSpawnStateEnumContextMenu(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        PropertyInfo propinfo,
        object val,
        ref object newVal,
        List<SpawnStateContents> options)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350 * DPI.UIScale(), ImGui.GetFrameHeight()), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350f * DPI.UIScale(), ImGui.GetTextLineHeightWithSpacing() * Math.Min(7, options.Count))))
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
        MapEditorView view,
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
            AssetMaskEntry targetEntry = null;

            // Get the entry for the current model
            foreach (var entry in view.Project.Handler.MapData.AssetMasks.List)
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
    /// <summary>
    /// FX Links: under a field holding an effect id (tagged ParticleAlias), a button that opens the effect in the FX
    /// Editor. Under an asset's SFX relative id, the model SFX param row it selects, with a button for each effect
    /// the row places and one for the row itself.
    /// </summary>
    public static void FxRefRow(
        MapEditorView view,
        MapEntityPropertyFieldMeta meta,
        IEnumerable<Entity> entSelection,
        PropertyInfo propinfo,
        object value)
    {
        var project = view.Project;

        if (!FxReferenceHelper.IsAvailable(project))
            return;

        var isEffect = meta != null && meta.ShowParticleList;
        var isRelative = MapEffectPlayback.IsRelativeSfxField(propinfo.Name) && MapEffectPlayback.SupportsAssetEffectsFor(project);

        if (!isEffect && !isRelative)
            return;

        // The links of a relative id: the row, then its effects
        int? rowId = null;
        MapEffectPlayback.ModelSfx[] effects = null;

        if (isRelative)
        {
            var obj = entSelection?.FirstOrDefault()?.WrappedObject;

            if (obj?.GetType().GetProperty("ModelName")?.GetValue(obj) is not string modelName ||
                value is not IConvertible number ||
                MapEffectPlayback.GetModelSfxRowId(project, modelName, number.ToInt32(null)) is not { } id)
            {
                return;
            }

            rowId = id;
            effects = MapEffectPlayback.ReadModelSfx(project, id);
        }
        else if (!FxReferenceHelper.TryGetId(value, out _))
        {
            return;
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

        if (isEffect)
        {
            FxLinkButton(project, Convert.ToInt32(value), "", width, "fxLink");
        }
        else
        {
            var paramName = MapEffectPlayback.ModelSfxParamName(project);

            if (effects == null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
                ImGui.TextUnformatted($"{paramName} {rowId}: no such row");
                ImGui.PopStyleColor();
            }
            else
            {
                if (project.Handler.ParamEditor != null)
                {
                    if (ImGui.Button($"Go to {paramName} {rowId}##fxParamRowLink", DPI.WholeWidthButton(width, 24)))
                        EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{rowId}");
                    GUI.Tooltip($"View the param row this asset's effects come from: its model's number × {MapEffectPlayback.ModelSfxRowStride(project)}, plus this relative ID.");
                }

                if (effects.Length == 0)
                    ImGui.TextDisabled("The row places no effects.");

                foreach (var sfx in effects)
                {
                    var dummy = sfx.DummyId >= 0 ? $"dummy poly {sfx.DummyId}" : "the model's origin";
                    FxLinkButton(project, sfx.EffectId, $"Slot {sfx.Slot}: on {dummy}.", width, $"fxSlotLink{sfx.Slot}");
                }
            }
        }

        ImGui.EndGroup();
    }

    /// <summary>
    /// A button opening an effect in the FX Editor, named by its alias when it has one; once the FX Editor has indexed
    /// the project's effects, an effect no sfx binder holds is said to be missing instead.
    /// </summary>
    private static void FxLinkButton(ProjectEntry project, int id, string detail, float width, string imguiId)
    {
        var stem = $"f{id:D9}";
        var alias = project.GetAliasName(ProjectAliasType.Particles, $"{id}");
        var label = string.IsNullOrEmpty(alias) ? stem : $"{stem}: {alias}";

        var bank = project.Handler.FxData?.Bank;
        if (bank != null && bank.IsIndexed && !bank.Entries.ContainsKey(id))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
            ImGui.TextUnformatted(LOC.Get("FX_Reference_Missing", stem));
            ImGui.PopStyleColor();

            if (detail != "")
                GUI.Tooltip(detail);

            return;
        }

        if (ImGui.Button($"Go to {label}##{imguiId}", DPI.WholeWidthButton(width, 24)))
            FxReferenceHelper.JumpTo(id);

        GUI.Tooltip(detail == "" ? LOC.Get("FX_Reference_Go_To_TT") : $"{detail}\n{LOC.Get("FX_Reference_Go_To_TT")}");
    }

    public static void ModelNameRow(
        MapEditorView view,
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
            if (EntityHelper.IsPartEnemy(ent) || EntityHelper.IsPartDummyEnemy(ent))
            {
                loadType = "Character";
            }
            else if (EntityHelper.IsPartAsset(ent) || EntityHelper.IsPartDummyAsset(ent))
            {
                loadType = "Asset";
            }
            else if (EntityHelper.IsPartMapPiece(ent))
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

        var windowWidth = ImGui.GetColumnWidth() * 0.95f;

        if (ImGui.Button("Go to Model", DPI.WholeWidthButton(windowWidth, 24)))
        {
            if (loadType == "MapPiece")
            {
                var map = (MapContainer)ent.Parent.Container;
                var mapPieceName = $"{value}".Replace("m", $"{map.Name}_");

                EditorCommandQueue.AddCommand($"model/load/{mapPieceName}/{loadType}/{map.Name}/{value}");
            }
            else
            {
                EditorCommandQueue.AddCommand($"model/load/{value}/{loadType}");
            }
        }
        GUI.Tooltip("View this model in the Model Editor, loading it automatically.");

        if (entSelection.Count() == 1)
        {
            LightAtlasLinkButtons(view, ent, windowWidth);
        }

        ImGui.EndGroup();
    }

    /// <summary>
    /// Light Atlas Link Buttons
    /// </summary>
    private static void LightAtlasLinkButtons(MapEditorView view, Entity ent, float windowWidth)
    {
        if (ent?.Container is not MapContainer map)
            return;

        var entries = map.LightAtlasResolver.GetEntries(ent.Name);

        for (int i = 0; i < entries.Count; i++)
        {
            var label = "Go to Light Atlas Entry";

            // A part can have an entry per material, in each of the atlas files of the map
            if (entries.Count > 1)
            {
                var material = ((BTAB.Entry)entries[i].WrappedObject).MaterialName;

                label = $"{label} ({entries[i].Parent.WrappedObject} : {material})";
            }

            if (ImGui.Button($"{label}##lightAtlasLink{i}", DPI.WholeWidthButton(windowWidth, 24)))
            {
                view.MapContentView.GotoEntity(entries[i]);
            }
            GUI.Tooltip("Select the light atlas entry that points to this map object.");
        }
    }
}

