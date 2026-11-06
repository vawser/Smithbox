using Havok.Shared;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Octokit;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

public static class HavokAliasGenerator
{
    public static void GenerateER(ProjectEntry project, HavokEditorView view) => Generate(project, view);

    public static void GenerateDS3(ProjectEntry project, HavokEditorView view) => Generate(project, view);

    /// <summary>
    /// Names each clip of a CustomManualSelectorGenerator after the selector. Read by field name, so it is the same
    /// for every SDK's classes.
    /// </summary>
    private static void Generate(ProjectEntry project, HavokEditorView view)
    {
        var sourceObject = view.HavokBank.BehaviorBank[view.Selection.BinderFileEntry][view.Selection.FilePath].Root;

        var cmsgs = HavokTreeSearch.FindAll<ICustomManualSelectorGenerator>(
            sourceObject, view.PropertyCache.GetCachedHavokFields);

        foreach (var entry in cmsgs)
        {
            var curGroupName = HavokTypeUtils.GetName(entry);

            if (HavokTypeUtils.GetFieldValue(entry, "m_generators") is not System.Collections.IEnumerable generators)
                continue;

            foreach (var clip in generators)
            {
                if (clip is IClipGenerator)
                {
                    GenerateHavokObjectName(
                        project,
                        view.Selection.CategoryMode, view.Selection.BinderFileEntry, view.Selection.FilePath,
                        HavokTypeUtils.GetName(clip), curGroupName);
                }
            }
        }
    }

    public static void GenerateHavokObjectName(ProjectEntry project, HavokCategoryMode category, FileDictionaryEntry binderEntry, string filePath, string havokObjectID, string groupName)
    {
        var srcDir = Path.Combine(ParamDebugTools.ProjectFolder,
            "src", "Smithbox.Data", "Assets", "HAVOK", "Aliases",
            ProjectUtils.GetGameDirectory(project));

        var projDir = Path.Combine(project.Descriptor.ProjectPath, ".smithbox", "Project", "HAVOK", "Aliases");

        var targetDir = srcDir;

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var files = project.Handler.HavokData.HavokObjectAliases.Files;

        var havokObjectName = "";

        var taeID = havokObjectID.Substring(0, 4);

        var adjustedTaeID = taeID.Replace("a", "c0000_");
        var timeActName = project.GetAliasName(MetadataEditor.ProjectAliasType.TimeActs, adjustedTaeID);

        if (timeActName == "")
            timeActName = "Blank";

        var cleanGroup = groupName.Replace("_CMSG", "");
        var cleanName = SplitCamelCase(cleanGroup);
        cleanName = cleanName.Replace("_", " ");

        havokObjectName = $"{timeActName} - {cleanName}";

        if (files.Any(e => e.Path == filePath))
        {
            var fileEntry = files.FirstOrDefault(e => e.Path == filePath);

            if (fileEntry.Aliases.Any(e => e.ID == havokObjectID))
            {
                for (int i = 0; i < fileEntry.Aliases.Count; i++)
                {
                    var curEntry = fileEntry.Aliases[i];

                    if (curEntry.ID == havokObjectID)
                    {
                        curEntry.Name = havokObjectName;
                    }
                }
            }
            else
            {
                var nameEntry = new HavokAliasEntry();
                nameEntry.ID = havokObjectID;
                nameEntry.Name = havokObjectName;

                fileEntry.Aliases.Add(nameEntry);
            }
        }
        else
        {
            var nameEntry = new HavokAliasEntry();
            nameEntry.ID = havokObjectID;
            nameEntry.Name = havokObjectName;

            var fileEntry = new HavokAliasFileEntry();
            fileEntry.Path = filePath;
            fileEntry.Aliases = new() { nameEntry };

            project.Handler.HavokData.HavokObjectAliases.Files.Add(fileEntry);
        }

        if (files.Any(e => e.Path == filePath))
        {
            var fileEntry = files.FirstOrDefault(e => e.Path == filePath);

            if (Directory.Exists(targetDir))
            {
                var filename = GetNiceFileName(category, binderEntry, filePath);
                var targetFile = Path.Combine(targetDir, $"{filename}.json");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };

                var jsonString = JsonSerializer.Serialize(fileEntry, typeof(HavokAliasFileEntry), options);

                File.WriteAllText(targetFile, jsonString);
            }
        }
    }

    public static string GetNiceFileName(HavokCategoryMode category, FileDictionaryEntry binderEntry, string filePath)
    {
        var filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filePath));

        return $"{category.ToString()}-{binderEntry.Filename}-{filename}";
    }
    public static string SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(
            input,
            @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])",
            " ");
    }
}
