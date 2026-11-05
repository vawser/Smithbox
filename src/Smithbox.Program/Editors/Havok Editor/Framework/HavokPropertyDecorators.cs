using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public static class HavokPropertyDecorators
{
    #region Behavior Indices
    // Only one combo popup is open at a time, so they share the filter
    private static string IndexComboFilter = "";
    private static readonly List<(int Value, string Name)> IndexComboOptions = new();
    private static readonly List<int> IndexComboFiltered = new();

    /// <summary>
    /// A row under an event, variable or state index field, showing the name it refers to,
    /// with a searchable combo to pick another. The raw number stays editable in the row above.
    /// </summary>
    public static bool BehaviorIndexRow(HavokEditorView view, HavokClass havokMeta, FieldInfo prop, object val, object containerObj, ref object newval)
    {
        var role = HavokMetaHelper.GetIndexRole(havokMeta, prop.Name);

        if (role is HavokIndexRole.None || val == null)
            return false;

        var symbols = view.PropertyView.BehaviorView.Symbols;

        if (symbols == null)
            return false;

        int index;

        try
        {
            index = Convert.ToInt32(val);
        }
        catch
        {
            return false;
        }

        // A binding's index is a character property when its binding type says so
        if (role is HavokIndexRole.Variable && HavokBehaviorSymbols.IsCharacterPropertyBinding(containerObj))
        {
            role = HavokIndexRole.CharacterProperty;
        }

        object stateMachine = null;
        IList names = null;
        string resolvedName;

        if (role is HavokIndexRole.State)
        {
            stateMachine = symbols.FindOwningStateMachine(containerObj);
            resolvedName = stateMachine == null ? null : HavokBehaviorSymbols.GetStateName(stateMachine, index);
        }
        else
        {
            names = symbols.GetNames(role);
            resolvedName = HavokBehaviorSymbols.GetName(names, index);
        }

        var allowsNone = role is not HavokIndexRole.State;
        var isNone = allowsNone && index == -1;
        var isMissing = !isNone && resolvedName == null;

        var roleName = GetRoleName(role);

        // Name column
        ImGui.NextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled($"  {roleName}");

        // Value column
        ImGui.NextColumn();

        string preview;

        if (isNone)
            preview = LOC.Get("HAVOK_BehaviorIndex_None");
        else if (isMissing)
            preview = LOC.Get("HAVOK_BehaviorIndex_Missing", roleName, index);
        else
            preview = string.IsNullOrEmpty(resolvedName) ? LOC.Get("HAVOK_BehaviorIndex_Unnamed", index) : resolvedName;

        var changed = false;

        if (isMissing)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);
        else if (!isNone)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

        ImGui.SetNextItemWidth(-1);
        var comboOpen = ImGui.BeginCombo($"##behaviorIndex_{prop.Name}", preview, ImGuiComboFlags.HeightLarge);

        if (isMissing || !isNone)
            ImGui.PopStyleColor();

        if (!comboOpen)
        {
            GUI.Tooltip(isMissing
                ? LOC.Get("HAVOK_BehaviorIndex_Missing_TT", roleName, index)
                : LOC.Get("HAVOK_BehaviorIndex_TT", roleName));
        }

        if (comboOpen)
        {
            if (ImGui.IsWindowAppearing())
            {
                IndexComboFilter = "";
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##behaviorIndexFilter", LOC.Get("HAVOK_BehaviorIndex_Search_Hint"), ref IndexComboFilter, 255);

            // Options
            IndexComboOptions.Clear();

            if (role is HavokIndexRole.State)
            {
                if (stateMachine != null)
                    IndexComboOptions.AddRange(HavokBehaviorSymbols.GetStates(stateMachine));
            }
            else if (names != null)
            {
                for (var i = 0; i < names.Count; i++)
                {
                    IndexComboOptions.Add((i, names[i] as string ?? ""));
                }
            }

            IndexComboFiltered.Clear();

            for (var i = 0; i < IndexComboOptions.Count; i++)
            {
                var option = IndexComboOptions[i];

                if (IndexComboFilter == "" ||
                    option.Name.Contains(IndexComboFilter, StringComparison.OrdinalIgnoreCase) ||
                    option.Value.ToString() == IndexComboFilter)
                {
                    IndexComboFiltered.Add(i);
                }
            }

            if (allowsNone && IndexComboFilter == "")
            {
                if (ImGui.Selectable($"{LOC.Get("HAVOK_BehaviorIndex_None")}##behaviorIndexNone", isNone))
                {
                    newval = ConvertIndex(-1, val);
                    changed = newval != null;
                }
            }

            if (stateMachine == null && role is HavokIndexRole.State)
            {
                ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorIndex_No_State_Machine"));
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(IndexComboFiltered.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var option = IndexComboOptions[IndexComboFiltered[i]];
                    var selected = option.Value == index;

                    if (ImGui.Selectable($"{option.Value}: {option.Name}##behaviorIndexOption{i}", selected))
                    {
                        newval = ConvertIndex(option.Value, val);
                        changed = newval != null;
                    }

                    if (selected && ImGui.IsWindowAppearing())
                        ImGui.SetScrollHereY();
                }
            }

            clipper.End();

            ImGui.EndCombo();
        }

        // Type column
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();
        }

        return changed;
    }

    /// <summary>
    /// A row under a field driven by a variable binding, naming the variable (or character property).
    /// Clicking a variable shows it in the Variables tab.
    /// </summary>
    public static void BehaviorBindingRow(HavokEditorView view, FieldInfo prop, object containerObj, int arrayIndex)
    {
        var symbols = view.PropertyView.BehaviorView.Symbols;

        if (symbols == null)
            return;

        var binding = symbols.FindBinding(containerObj, prop.Name, arrayIndex);

        if (binding == null)
            return;

        var isCharacterProperty = HavokBehaviorSymbols.IsCharacterPropertyBinding(binding);
        var index = HavokBehaviorSymbols.GetBindingIndex(binding);
        var names = isCharacterProperty ? symbols.CharacterPropertyNames : symbols.VariableNames;
        var name = HavokBehaviorSymbols.GetName(names, index);
        var roleName = GetRoleName(isCharacterProperty ? HavokIndexRole.CharacterProperty : HavokIndexRole.Variable);

        // Name column
        ImGui.NextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled($"  {LOC.Get("HAVOK_BehaviorBinding_Label")}");

        // Value column
        ImGui.NextColumn();
        ImGui.AlignTextToFramePadding();

        var text = name == null
            ? LOC.Get("HAVOK_BehaviorIndex_Missing", roleName, index)
            : $"{roleName}: {name}";

        var color = name == null ? UI.Current.ImGui_Invalid_Text_Color : UI.Current.ImGui_AliasName_Text;

        if (!isCharacterProperty && name != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            if (ImGui.Selectable($"{text}##behaviorBinding_{prop.Name}"))
            {
                view.PropertyView.BehaviorView.VariableView.RequestSelect(index);
            }
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.TextColored(color, text);
        }

        var bitIndex = HavokBehaviorSymbols.GetValue(binding, "m_bitIndex");

        GUI.Tooltip(LOC.Get("HAVOK_BehaviorBinding_TT",
            HavokBehaviorSymbols.GetMemberPath(binding), index, bitIndex ?? -1));

        // Type column
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();
        }
    }

    private static string GetRoleName(HavokIndexRole role)
    {
        return role switch
        {
            HavokIndexRole.Event => LOC.Get("HAVOK_BehaviorIndex_Role_Event"),
            HavokIndexRole.Variable => LOC.Get("HAVOK_BehaviorIndex_Role_Variable"),
            HavokIndexRole.CharacterProperty => LOC.Get("HAVOK_BehaviorIndex_Role_Character_Property"),
            HavokIndexRole.State => LOC.Get("HAVOK_BehaviorIndex_Role_State"),
            HavokIndexRole.Animation => LOC.Get("HAVOK_BehaviorIndex_Role_Animation"),
            _ => ""
        };
    }

    /// <summary>
    /// The index as the field's own integer type, or null if it doesn't fit
    /// </summary>
    private static object ConvertIndex(int index, object oldVal)
    {
        try
        {
            return Convert.ChangeType(index, oldVal.GetType());
        }
        catch
        {
            return null;
        }
    }
    #endregion

    public static bool ParamRefRow(IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj)
    {
        ParamEditorView activeView = null;

        if(view is MapEditorView mapEditorView)
        {
            if (mapEditorView.Project.Handler.ParamEditor == null)
                return false;

            activeView = mapEditorView.Project.Handler.ParamEditor.ViewHandler.ActiveView;
        }

        if (view is HavokEditorView havokEditorView)
        {
            if (havokEditorView.Project.Handler.ParamEditor == null)
                return false;

            activeView = havokEditorView.Project.Handler.ParamEditor.ViewHandler.ActiveView;
        }

        if (activeView == null)
            return false;

        if (havokMeta == null)
            return false;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (fieldMeta.ParamRef == "")
            return false;

        List<ParamRef> refs = new()
        {
            new ParamRef(null, fieldMeta.ParamRef)
        };

        ImGui.NextColumn();

        ParamReferenceHelper.Label(activeView, refs, null);

        ImGui.NextColumn();

        // The context menu result is returned after the type column, so the columns stay aligned while it's open
        var changed = false;

        if (activeView.Project.Handler.ParamEditor != null)
        {
            ParamReferenceHelper.Hint(activeView, refs, null, val);
            ParamReferenceHelper.Click(activeView, val, null, refs);

            if (ImGui.BeginPopupContextItem($"{prop.Name}EnumContextMenu"))
            {
                changed = ParamReferenceHelper.ContextMenu(activeView, refs, null, val, ref newObj, null);
                ImGui.EndPopup();
            }
        }

        if (view is MapEditorView)
        {
            if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
        else if (view is HavokEditorView)
        {
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return changed;
    }

    public static bool ClipGenFlags(ProjectEntry project, IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj, object sourceObj, ref object newval)
    {
        if (havokMeta == null)
            return false;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (!fieldMeta.ClipGeneratorFlags)
            return false;

        ImGui.NextColumn();
        ImGui.NextColumn();

        int flags = Convert.ToInt32(val);
        bool changed = false;

        if (HavokTypeUtils.IsHKX3(project))
        {
            foreach (HKLib.hk2018.hkbClipGenerator.ClipFlags flag in Enum.GetValues(typeof(HKLib.hk2018.hkbClipGenerator.ClipFlags)))
            {
                int flagVal = (int)flag;
                bool isSet = (flags & flagVal) != 0;

                if (ImGui.Checkbox(flag.ToString(), ref isSet))
                {
                    if (isSet)
                        flags |= flagVal;
                    else
                        flags &= ~flagVal;

                    changed = true;
                }
            }
        }
        else if (HavokTypeUtils.IsHKX2(project))
        {
            foreach (HKX2.hkbClipGenerator.ClipFlags flag in Enum.GetValues(typeof(HKX2.hkbClipGenerator.ClipFlags)))
            {
                int flagVal = (int)flag;
                bool isSet = (flags & flagVal) != 0;

                if (ImGui.Checkbox(flag.ToString(), ref isSet))
                {
                    if (isSet)
                        flags |= flagVal;
                    else
                        flags &= ~flagVal;

                    changed = true;
                }
            }
        }

        // The type column is still advanced on a change, so the rows below stay aligned
        if (changed)
        {
            newval = (sbyte)flags;
        }

        if (view is MapEditorView)
        {
            if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
        else if (view is HavokEditorView)
        {
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return changed;
    }

    public static bool ClipGenInternalID(ProjectEntry project, IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj, object sourceObj, ref object newval)
    {
        if (havokMeta == null)
            return false;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (!fieldMeta.AnimationInternalID)
            return false;

        var changed = false;

        if (view is HavokEditorView havokEditorView)
        {
            ImGui.NextColumn();
            ImGui.NextColumn();

            if (ImGui.Button("Set to Free ID"))
            {
                short freeNum = -1;

                if (HavokTypeUtils.IsHKX3(project))
                {
                    var objects = HavokTreeSearch.FindValueList<HKLib.hk2018.hkbClipGenerator>(sourceObj, havokEditorView.PropertyCache.GetCachedHavokFields, "m_animationInternalId", typeof(short));

                    var shorts = objects.Cast<short>().ToList();


                    for (short i = 0; i < short.MaxValue; i++)
                    {
                        if (!shorts.Contains(i))
                        {
                            freeNum = i;
                            break;
                        }
                    }
                }
                else if (HavokTypeUtils.IsHKX2(project))
                {
                    var objects = HavokTreeSearch.FindValueList<HKX2.hkbClipGenerator>(sourceObj, havokEditorView.PropertyCache.GetCachedHavokFields, "m_animationInternalId", typeof(short));

                    var shorts = objects.Cast<short>().ToList();


                    for (short i = 0; i < short.MaxValue; i++)
                    {
                        if (!shorts.Contains(i))
                        {
                            freeNum = i;
                            break;
                        }
                    }
                }

                if (freeNum == -1)
                {
                    Smithbox.LogError(typeof(HavokPropertyDecorators), "No free ID to assign.");
                }
                else
                {
                    newval = (short)(freeNum);
                    changed = true;
                }
            }
            GUI.Tooltip("Set the value to the next available unused ID.");

            // The animation name at this index, where the string data has animation names (hk2018/hk2019)
            var animationNames = havokEditorView.PropertyView.BehaviorView.Symbols?.AnimationNames;

            if (animationNames != null && animationNames.Count > 0)
            {
                var animationName = HavokBehaviorSymbols.GetName(animationNames, Convert.ToInt32(val));

                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();

                if (animationName == null)
                {
                    ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color,
                        LOC.Get("HAVOK_BehaviorIndex_Missing", LOC.Get("HAVOK_BehaviorIndex_Role_Animation"), val));
                }
                else
                {
                    // The names are the animation file paths (..\..\..\..\Model\chr\c0000\hkx\a000\a000_003000.hkx);
                    // the file name matches the clip's m_animationName in every ER behavior
                    var animationFile = Path.GetFileNameWithoutExtension(animationName.Replace('\\', '/'));

                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, animationFile);
                    GUI.Tooltip(animationName);
                }
            }

            // Advanced even after a click, so the rows below stay aligned
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return changed;
    }
}
