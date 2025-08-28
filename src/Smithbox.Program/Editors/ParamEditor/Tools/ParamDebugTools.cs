using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor.Tools;

public static class ParamDebugTools
{
    public static void DisplayQuickRowNameExport(ParamEditorScreen editor, ProjectEntry project)
    {
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Copyright}", DPI.IconButtonSize))
        {
            var dir = Path.Combine(CFG.Current.SmithboxBuildFolder, 
                "src", "Smithbox.Data", "Assets", "PARAM", 
                ProjectUtils.GetGameDirectory(project), "Community Row Names");

            var curParam = editor._activeView.Selection.GetActiveParam();

            var store = new RowNameStore();
            store.Params = new();

            foreach (KeyValuePair<string, Param> p in project.ParamData.PrimaryBank.Params)
            {
                if (curParam != "")
                {
                    if (p.Key != curParam)
                        continue;
                }

                var paramEntry = new RowNameParam();
                paramEntry.Name = p.Key;
                paramEntry.Entries = new();

                var groupedRows = p.Value.Rows
                    .GroupBy(r => r.ID)
                    .ToDictionary(g => g.Key, g => g.Select(r => r.Name ?? "").ToList());

                paramEntry.Entries = groupedRows.Select(kvp => new RowNameEntry
                {
                    ID = kvp.Key,
                    Entries = kvp.Value
                }).ToList();

                var fullPath = Path.Combine(dir, $"{p.Key}.json");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };
                var json = JsonSerializer.Serialize(paramEntry, typeof(RowNameParam), options);

                File.WriteAllText(fullPath, json);

                TaskLogs.AddLog($"[{project.ProjectName}:Param Editor] Exported row names to {fullPath}");
            }
        }
    }
}
