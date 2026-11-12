using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Search across every bank without opening them (the sound index): a sound ID (806100, s000806100), part of an event
/// name, or part of a Sounds alias. Click a result to open it, double-click to play it.
/// </summary>
public class SoundSearchTool
{
    private readonly SoundEditorView View;

    private string Query = "";
    private string SearchedQuery;
    private bool SearchedWithIndex;
    private List<SoundMatch> Results = new();
    private bool FocusInput;

    public SoundSearchTool(SoundEditorView view)
    {
        View = view;
    }

    /// <summary>Selects this tool in the Tools window on its next draw.</summary>
    private bool FocusOnNextDraw;

    /// <summary>Sets the query (sound/find/&lt;text&gt;), opens the Tools window on this tool and focuses it.</summary>
    public void SetQuery(string text)
    {
        Query = text ?? "";
        FocusInput = true;
        FocusOnNextDraw = true;
        CFG.Current.Interface_SoundEditor_ToolWindow = true;
        CFG.Current.Interface_SoundEditor_Tool_Search = true;
        ImGui.SetWindowFocus(SoundToolWindow.WindowId);
    }

    public void OnToolWindow()
    {
        if (FocusOnNextDraw)
        {
            FocusOnNextDraw = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("SOUND_Search_Header")}##soundSearchToolHeader"))
            return;

        ImGui.BeginChild("SoundSearchToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var index = View.Data?.Index;
        if (index == null)
            return;
        index.EnsureBuilt();

        GUI.WrappedText(LOC.Get("SOUND_Search_Hint"));

        // Query
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Search_Query_Header"), LOC.Get("SOUND_Search_Query_Header_TT"));
        if (FocusInput) { ImGui.SetKeyboardFocusHere(); FocusInput = false; }
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##soundSearchQuery", ref Query, 255);
        GUI.Tooltip(LOC.Get("SOUND_Search_Query_TT"));

        if (!index.IsBuilt)
        {
            GUI.Spacer();
            if (index.Error != null)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("SOUND_Index_Log_Failed", index.Error));
            else
                ImGui.TextDisabled(LOC.Get("SOUND_Search_Indexing"));
            return;
        }

        // search when the text changes (or the index just finished)
        if (Query != SearchedQuery || !SearchedWithIndex)
        {
            SearchedQuery = Query;
            SearchedWithIndex = true;
            Results = index.Search(Query);
        }

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("SOUND_Search_Results_Header", Results.Count), LOC.Get("SOUND_Search_Results_Header_TT"));
        if (Results.Count == 0)
        {
            ImGui.TextDisabled(string.IsNullOrWhiteSpace(Query) ? LOC.Get("SOUND_Search_Empty") : LOC.Get("SOUND_Search_None"));
            return;
        }

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;
        if (!ImGui.BeginTable("soundSearchResults", 3, flags, Vector2.Zero))
            return;
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Event"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Alias"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Bank"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(Results.Count);
        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var m = Results[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{m.EventName}##soundSearch_{i}", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                    EditorCommandQueue.AddCommand(SoundIndex.Command(m, ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)));
                GUI.Tooltip(LOC.Get("SOUND_Search_Result_TT", m.Container.Path));

                ImGui.TableNextColumn();
                var alias = m.SoundId == null ? null : View.Data.Names.SoundAlias(m.SoundId)?.Name;
                if (!string.IsNullOrEmpty(alias))
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(m.Container.Name);
                if (!string.IsNullOrEmpty(m.Container.FileAlias))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, m.Container.FileAlias);
                }
            }
        }
        clipper.End();
        ImGui.EndTable();
    }
}
