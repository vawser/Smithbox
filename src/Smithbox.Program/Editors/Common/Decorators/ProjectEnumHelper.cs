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
/// Shows the project enum a field uses, and lets a value be picked from it. Shared by the editors whose fields use project enums.
/// </summary>
public static class ProjectEnumHelper
{
    private static string EnumListFilter = "";
    private static bool ExactEnumListFilter = false;

    /// <summary>
    /// Gets a project enum by its key, or null if the project has no enum of that name or hasn't loaded them
    /// </summary>
    public static ParamEnumEntry GetEnum(ProjectEntry project, string enumType)
    {
        return project?.Handler?.ParamData?.Enums?.List?.FirstOrDefault(e => e.Key == enumType);
    }

    public static void Label(ParamEditorView curView, string enumType)
    {
        Label(curView.Project, enumType);
    }

    public static void Hint(ParamEditorView curView, string enumType, string value)
    {
        Hint(curView.Project, enumType, value);
    }

    public static void Label(ProjectEntry project, string enumType)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var enumEntry = GetEnum(project, enumType);

        if (enumEntry != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {enumEntry.GetName()}");
            ImGui.PopStyleColor(1);
        }
    }

    public static void Hint(ProjectEntry project, string enumType, string value)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var enumEntry = GetEnum(project, enumType);

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);

        if (enumEntry != null)
        {
            var enumValueName = "";
            var enumValue = enumEntry.Options.Where(e => e.Key == value).FirstOrDefault();

            if (enumValue != null)
            {
                enumValueName = enumValue.GetName();
            }

            ImGui.TextUnformatted(enumValueName);
        }
        else
        {
            ImGui.TextUnformatted(LOC.Get("PARAM_FieldDecorator_EnumHelper_Not_Enumerated"));
        }

        ImGui.PopStyleColor();
    }

    public static bool ContextMenu(ParamEditorView curView, ParamEnumEntry en, object oldval, ref object newval)
    {
        return ContextMenu(en, oldval, ref newval);
    }

    public static bool ContextMenu(ParamEnumEntry en, object oldval, ref object newval)
    {
        EditorFilters.DisplayFramedListFilter("enumListFilter", ref EnumListFilter, ref ExactEnumListFilter);

        var count = 1;
        if (en.Options.Count > 0)
            count = en.Options.Count;

        if (ImGui.BeginChild("EnumList", new Vector2(350f, GUI.GetEnumListHeight(count)), ImGuiChildFlags.Borders))
        {
            try
            {
                foreach (var option in en.Options)
                {
                    var isMatch = EditorFilters.IsMatch(EnumListFilter, option.Key, ExactEnumListFilter, option.GetName());

                    if (isMatch)
                    {
                        if (ImGui.Selectable($"{option.Key}: {option.GetName()}"))
                        {
                            newval = Convert.ChangeType(option.Key, oldval.GetType());
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
}
