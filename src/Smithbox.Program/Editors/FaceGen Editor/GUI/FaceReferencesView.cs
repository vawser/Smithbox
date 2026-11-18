#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The rows that use the active face (CharaInitParam's NPC players and invaders, through npcPlayerFaceGenId), each a
/// link to the row in the Param Editor; and the rows that point at a face that doesn't exist.
/// </summary>
public class FaceReferencesView
{
    private readonly FaceGenEditorView View;

    public FaceReferencesView(FaceGenEditorView view)
    {
        View = view;
    }

    public void Display()
    {
        var project = View.Project;
        var referenced = FaceGenParams.GetReferencedFaceParam(project);

        if (referenced == null)
            return;

        var users = FaceGenParams.GetUsers(project);

        if (View.ParamName != referenced)
        {
            // The characters point at the other face param, so this one has no users to list
            GUI.WrappedText(LOC.Get("FACEGEN_References_Other_Param", referenced));
        }
        else if (View.ActiveRow is not { } row)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_Properties_No_Face"));
        }
        else
        {
            var faceUsers = users.Get(row.ID);

            GUI.SimpleHeader(LOC.Get("FACEGEN_References_Users", faceUsers.Count), LOC.Get("FACEGEN_References_Users_TT"));

            if (faceUsers.Count == 0)
                ImGui.TextDisabled(LOC.Get("FACEGEN_References_None"));

            UserLinks(faceUsers, "users");
        }

        // Rows that point at a face that isn't there: the game shows them with no face (or a default one)
        if (users.Missing.Count > 0)
        {
            GUI.Spacer();

            if (ImGui.CollapsingHeader($"{LOC.Get("FACEGEN_References_Missing", users.Missing.Count)}##faceGenMissing"))
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FACEGEN_References_Missing_TT"));
                UserLinks(users.Missing, "missing");
            }
        }
    }

    private void UserLinks(IReadOnlyList<FaceGenParams.FaceUser> users, string id)
    {
        var index = 0;
        var dressed = View.CharacterPreview.DressedCharaInitId;

        foreach (var user in users.OrderBy(u => u.ParamName).ThenBy(u => u.Row.ID))
        {
            var label = $"{user.ParamName}: {FaceGenParams.RowLabel(user.Row.ID, user.Row.Name)}";

            if (id == "missing")
                label += $"  → {user.FaceId}";

            // The viewport's outfit only comes from CharaInitParam, as the Character Preview tool dresses it
            var canDress = id != "missing" && user.ParamName == CharaInitParamName;
            var isDressed = canDress && user.Row.ID == dressed;

            if (isDressed)
                label += $"  {LOC.Get("FACEGEN_References_Dressed")}";

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

            if (ImGui.Selectable($"{label}##faceGenUser_{id}_{index}", isDressed))
                EditorCommandQueue.AddCommand($"param/select/-1/{user.ParamName}/{user.Row.ID}");

            ImGui.PopStyleColor();
            GUI.Tooltip(LOC.Get(canDress ? "FACEGEN_References_Link_Dress_TT" : "FACEGEN_References_Link_TT"));

            if (ImGui.BeginPopupContextItem($"faceGenUserContext_{id}_{index}"))
            {
                if (ImGui.Selectable($"{LOC.Get("FACEGEN_References_Dress")}##faceGenDress", false,
                        canDress ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    View.CharacterPreview.DressFromCharaInit(user.Row.ID);
                }
                GUI.Tooltip(LOC.Get("FACEGEN_References_Dress_TT"));

                if (ImGui.Selectable($"{LOC.Get("FACEGEN_References_Undress")}##faceGenUndress", false,
                        dressed >= 0 ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    View.CharacterPreview.DressFromCharaInit(-1);
                }

                if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
                    EditorCommandQueue.AddCommand($"param/select/-1/{user.ParamName}/{user.Row.ID}");

                ImGui.EndPopup();
            }

            index++;
        }
    }

    private const string CharaInitParamName = "CharaInitParam";
}
