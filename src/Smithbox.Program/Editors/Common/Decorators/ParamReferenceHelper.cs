using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Formats;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.Common;

/// <summary>
/// Shows the param rows a field refers to, and lets them be opened or picked. Shared by every editor whose fields refer to params.
/// </summary>
public static class ParamReferenceHelper
{
    public static void Label(ParamEditorView curView, List<ParamRef> paramRefs, Param.Row context)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_References)
            return;

        if (paramRefs == null || paramRefs.Count == 0)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(@"   <");

        List<string> inactiveRefs = new();
        var first = true;
        foreach (ParamRef r in paramRefs)
        {
            var inactiveRef = false;

            if (context != null && r.ConditionField != null)
            {
                Param.Cell? c = context?[r.ConditionField];

                if (c == null)
                    continue;

                var fieldValue = c.Value.Value;
                int intValue = 0;
                var valueConvertSuccess = int.TryParse($"{fieldValue}", out intValue);

                // Only check if field value is valid uint
                if (valueConvertSuccess)
                {
                    inactiveRef = intValue != r.ConditionValue;
                }
            }

            if (inactiveRef)
            {
                inactiveRefs.Add(r.ParamName);
            }
            else
            {
                if (first)
                {
                    ImGui.SameLine();
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(r.ParamName);
                }
                else
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("    " + r.ParamName);
                }

                first = false;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefInactive_Text);

        foreach (var inactive in inactiveRefs)
        {
            ImGui.SameLine();
            if (first)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("!" + inactive);
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("!" + inactive);
            }

            first = false;
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(">");

        ImGui.PopStyleVar();
    }

    public static void Hint(ParamEditorView curView, List<ParamRef> paramRefs, Param.Row context, dynamic oldval)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_References)
            return;

        if (paramRefs == null)
        {
            return;
        }

        // Add named row and context menu
        // Lists located params
        // May span lines
        List<(string, Param.Row, string)> matches = ParamReferenceResolver.ResolveParamReferences(curView, paramRefs, "", context, oldval);

        var entryFound = matches.Count > 0;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
        ImGui.BeginGroup();

        foreach ((var param, Param.Row row, var adjName) in matches)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(adjName);
        }

        ImGui.PopStyleColor();
        if (!entryFound)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("---");
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();
    }

    private static string QuickEditTerm = "";

    public static bool ContextMenu(ParamEditorView curView, List<ParamRef> reftypes, Param.Row context,
        object oldval, ref object newval, ActionManager executor)
    {
        if (curView.GetPrimaryBank().Params == null)
        {
            return false;
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

        // Add Goto statements
        List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, reftypes, "", context, oldval);

        int index = 0;

        foreach ((string, Param.Row, string) rf in refs)
        {
            // Go to X
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldDecorator_ParamRef_Go_To_Action", rf.Item3)}##GoToElement{index}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/-1/{rf.Item1}/{rf.Item2.ID}");
            }

            // Go to X in new view
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldDecorator_ParamRef_Go_To_New_View_Action", rf.Item3)}##GoToElementInView{index}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/new/{rf.Item1}/{rf.Item2.ID}");
            }

            if (context == null || executor == null)
            {
                continue;
            }

            // Inherit referenced row's name
            if (!string.IsNullOrWhiteSpace(rf.Item2.Name) &&
                (InputManager.HasCtrlDown() || string.IsNullOrWhiteSpace(context.Name)) &&
                ImGui.Selectable($"{LOC.Get("PARAM_FieldDecorator_ParamRef_Inherit_Ref_Row_Name_Action", rf.Item2.Name)}##InheritName{index}"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"), context,
                    rf.Item2.Name));
            }
            // Proliferate name to referenced row
            else if ((InputManager.HasCtrlDown() || string.IsNullOrWhiteSpace(rf.Item2.Name)) &&
                     !string.IsNullOrWhiteSpace(context.Name) &&
                     ImGui.Selectable($"{LOC.Get("PARAM_FieldDecorator_ParamRef_Proliferate_Name_to_Ref_Action", rf.Item1)}##ProliferateName{index}"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(rf.Item2.GetType().GetProperty("Name"), rf.Item2,
                    context.Name));
            }

            index++;
        }

        ImGui.PopStyleColor();

        ImGui.Separator();

        // Quick Edit
        ImGui.InputTextWithHint("##value", LOC.Get("PARAM_FieldDecorator_ParamRef_EntrySearch_Hint"), 
            ref QuickEditTerm, 128);
        GUI.Tooltip(LOC.Get("PARAM_FieldDecorator_ParamRef_EntrySearch_TT"));

        if (QuickEditTerm != "")
        {
            ImGui.BeginChild("quickEditSection", new Vector2(0, 250), ImGuiChildFlags.Borders);

            foreach (ParamRef rf in reftypes)
            {
                var rt = rf.ParamName;

                if (!curView.GetPrimaryBank().Params.ContainsKey(rt))
                {
                    continue;
                }

                var meta = curView.GetParamData().GetParamMeta(curView.GetPrimaryBank().Params[rt].AppliedParamdef);

                var maxResultsPerRefType = 15 / reftypes.Count;

                List<Param.Row> rows = curView.MassEdit.RSE.Search((curView.GetPrimaryBank(), curView.GetPrimaryBank().Params[rt]),
                    QuickEditTerm, true, true);

                foreach (Param.Row r in rows)
                {
                    if (maxResultsPerRefType <= 0)
                    {
                        break;
                    }

                    if (ImGui.Selectable($@"{r.ID}: {r.Name}"))
                    {
                        try
                        {
                            if (meta != null && meta.FixedOffset != 0)
                            {
                                newval = Convert.ChangeType(r.ID - meta.FixedOffset - rf.Offset, oldval.GetType());
                            }
                            else
                            {
                                newval = Convert.ChangeType(r.ID - rf.Offset, oldval.GetType());
                            }

                            QuickEditTerm = "";
                            ImGui.EndChild();

                            return true;
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(typeof(ParamReferenceHelper), 
                                LOC.Get("PARAM_FieldDecorator_ParamRef_EntrySearch_Invalid_Convert"), e);
                        }
                    }
                    GUI.Tooltip(LOC.Get("PARAM_FieldDecorator_ParamRef_EntrySearch_Selectable_TT", rt));

                    maxResultsPerRefType--;
                }
            }

            ImGui.EndChild();
        }

        return false;
    }

    public static bool Click(ParamEditorView curView, object oldval, Param.Row context, List<ParamRef> RefTypes)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
        {
            if (RefTypes != null)
            {
                (string, Param.Row, string)? primaryRef =
                    ParamReferenceResolver.ResolveParamReferences(curView, RefTypes, "", context, oldval)?.FirstOrDefault();

                if (primaryRef?.Item2 != null)
                {
                    if (InputManager.HasShiftDown())
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/new/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                    else
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/-1/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                }
            }
        }

        return false;
    }

    public static bool Shortcut(ParamEditorView curView, FieldMetaContext context, Param.Row row, object oldval, ref object newval)
    {
        var result = false;

        if (!ImGui.IsAnyItemActive())
        {
            if (context.ParamReferences != null)
            {
                if (curView.GetPrimaryBank().Params == null)
                {
                    return false;
                }

                if (FocusManager.Focus is EditorFocusContext.ParamEditor_RowList)
                {
                    if (InputManager.IsPressed(KeybindID.ParamEditor_RowList_Inherit_Referenced_Row_Name))
                    {
                        List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, context.ParamReferences, "", row, oldval);

                        foreach ((string, Param.Row, string) rf in refs)
                        {
                            if (row == null || curView.Editor.ActionManager == null)
                            {
                                continue;
                            }

                            curView.Editor.ActionManager.ExecuteAction(new PropertiesChangedAction(row.GetType().GetProperty("Name"), row, rf.Item2.Name));
                        }

                        result = true;
                    }
                }
            }
        }

        return result;
    }
}
