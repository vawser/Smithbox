using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamData : IDisposable
{
    public ProjectEntry Project;

    public GparamBank PrimaryBank;
    public GparamBank VanillaBank;

    public GparamAnnotationLanguages GparamAnnotationLanguages = new();

    // Annotations serve as both definitions (for add operations) and meta (for editing operations)
    public GparamAnnotations Annotations = new();
    public GparamEnums Enums;

    public GparamData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Graphics Param Editor] Failed to setup the Primary Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Graphics Param Editor] Setup the Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, $"[Graphics Param Editor] Failed to setup the Vanilla Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Graphics Param Editor] Setup the Vanilla Bank.");
        }

        // Gparam Annotations
        Task<bool> gparamAnnotationTask = SetupGparamAnnotations();
        bool gparamAnnotationTaskResult = await gparamAnnotationTask;

        if (!gparamAnnotationTaskResult)
        {
            Smithbox.LogError(this, $"[Graphics Param Editor] Failed to setup the GPARAM annotations.");
        }
        else
        {
            Smithbox.Log(this, $"[Graphics Param Editor] Setup the GPARAM annotations.");
        }

        // Gparam Enums
        Task<bool> gparamEnumsTask = SetupGparamEnums();
        bool gparamEnumsTaskResult = await gparamEnumsTask;

        if (!gparamEnumsTaskResult)
        {
            Smithbox.LogError(this, $"[Graphics Param Editor] Failed to setup the GPARAM enums.");
        }
        else
        {
            Smithbox.Log(this, $"[Graphics Param Editor] Setup the GPARAM enums.");
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }

    public async Task<bool> SetupGparamAnnotations()
    {
        await Task.Yield();

        GparamAnnotationLanguages = new();

        // Build the language list first
        var sourcefile = Path.Join(StudioCore.Common.FileLocations.Assets, "GPARAM", "Annotation Languages.json");

        if (Path.Exists(sourcefile))
        {
            var file = File.ReadAllText(sourcefile);
            try
            {
                GparamAnnotationLanguages = JsonSerializer.Deserialize(file, GparamEditorJsonSerializerContext.Default.GparamAnnotationLanguages);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Graphics Param Editor] Failed to deserialize gparam annotation languages: {file}", LogPriority.High, e);
            }
        }
        else
        {
            // Default to English if the file is missing
            var english = new GparamAnnotationLanguageEntry();
            english.Name = "English";
            english.Folder = "English";

            GparamAnnotationLanguages.Languages.Add(english);
        }

        // Then build the annotations
        Annotations = new();

        foreach (var lang in GparamAnnotationLanguages.Languages)
        {
            var paramList = new GparamAnnotationList();
            Annotations.Entries.Add(lang, paramList);

            var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "GPARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Gparam Annotations", lang.Folder);

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, GparamEditorJsonSerializerContext.Default.GparamAnnotationEntry);

                        if (!Annotations.Entries[lang].Params.Any(e => e.Name == layout.Name))
                        {
                            Annotations.Entries[lang].Params.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Graphics Param Editor] Failed to deserialize gparam annotation entry: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }


    public async Task<bool> SetupGparamEnums()
    {
        await Task.Yield();

        Enums = new();

        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "GPARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Gparam Enums");

        if (Path.Exists(sourceFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(sourceFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, GparamEditorJsonSerializerContext.Default.GparamEnumEntry);

                    if (!Enums.List.Any(e => e.Key == layout.Key))
                    {
                        Enums.List.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Graphics Param Editor] Failed to deserialize gparam enum entry: {file}", LogPriority.High, e);
                }
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        PrimaryBank = null;
        VanillaBank = null;

        Annotations = null;
        Enums = null;
    }
    #endregion
}
