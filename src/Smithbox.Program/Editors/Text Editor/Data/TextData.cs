using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextData : IDisposable
{
    public ProjectEntry Project;

    public TextBank PrimaryBank;
    public TextBank VanillaBank;
    public Dictionary<string, TextBank> AuxBanks = new();

    public FileDictionary FmgFiles = new();

    public FmgDescriptors FmgDescriptors = new();

    public IncrementalTemplates Templates = new();

    /// <summary>
    /// The game's text entry groups (Assets/FMG/&lt;game&gt;/Text Entry Groups.json)
    /// </summary>
    public TextEntryGroupDefinitions EntryGroups = new();

    public TextData(ProjectEntry project)
    {
        Project = project;
    }

    /// <param name="preloadOtherLanguages">
    /// Load the other languages' text on a worker thread once setup is done (see
    /// TextBank.PreloadOtherLanguages). For the main project only: auxiliary projects
    /// just supply text for comparison and transfer, so it would only cost memory.
    /// </param>
    public async Task<bool> Setup(bool preloadOtherLanguages = false)
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // FMG Descriptors
        Task<bool> descriptorTask = SetupFmgDescriptors();
        bool descriptorTaskResult = await descriptorTask;

        if (!descriptorTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_FMG_Descriptor_Setup_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TEXT_Data_FMG_Descriptor_Setup_PASS"));
        }

        // Text Entry Groups
        await SetupEntryGroups();

        // Templates
        Task<bool> templatesTask = SetupIncrementalTemplates();
        bool templatesTaskResult = await templatesTask;

        if (!templatesTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Incremental_Templates_Setup_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TEXT_Data_Incremental_Templates_Setup_PASS"));
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Primary_Bank_Setup_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TEXT_Data_Primary_Bank_Setup_PASS"));
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Vanilla_Bank_Setup_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TEXT_Data_Vanilla_Bank_Setup_PASS"));
        }

        // Not awaited: the project opens without waiting for it
        if (preloadOtherLanguages && primaryBankTaskResult)
        {
            var bank = PrimaryBank;
            _ = Task.Run(() => bank.PreloadOtherLanguages());
        }

        return true;
    }

    public async Task<bool> LoadAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Smithbox.Orchestrator.LoadAuxiliaryProject(targetProject, ProjectInitType.TextEditorOnly, reloadProject);

        // Enumerate the auxiliary project's own text files (it used the main project's list,
        // so files only present in the auxiliary project were never loaded)
        var newAuxBank = new TextBank($"{targetProject.Descriptor.ProjectName}", Project, targetProject.VFS.FS,
            targetProject.Locator?.TextFiles);

        // Aux Bank
        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Aux_Bank_Setup_FAIL", targetProject.Descriptor.ProjectName));
        }
        else
        {
            // Only report success when the setup worked (this logged PASS right after FAIL)
            Smithbox.Log(this, LOC.Get("TEXT_Data_Aux_Bank_Setup_PASS", targetProject.Descriptor.ProjectName));
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        return true;
    }

    #region Incremental Templates
    public async Task<bool> SetupIncrementalTemplates()
    {
        await Task.Yield();

        Templates = new();

        var folder = @$"{AppContext.BaseDirectory}/Assets/FMG/Templates";

        // EnumerateFiles throws on a missing folder, which failed the whole text setup
        if (!Directory.Exists(folder))
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_Data_Incremental_Template_Folder_Missing", folder));
            return true;
        }

        foreach(var file in Directory.EnumerateFiles(folder))
        {
            try
            {
                var newTemplate = new IncrementalTemplateEntry();
                var filestring = await File.ReadAllTextAsync(file);

                try
                {
                    newTemplate = JsonSerializer.Deserialize(filestring, TextEditorJsonSerializerContext.Default.IncrementalTemplateEntry);

                    // A file holding just "null" deserializes without error; don't add it
                    if (newTemplate == null)
                    {
                        Smithbox.LogError(this, LOC.Get("TEXT_Data_Deserialize_Incremental_Template_FAIL", file));
                        continue;
                    }

                    Templates.Add(newTemplate);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("TEXT_Data_Deserialize_Incremental_Template_FAIL", file), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TEXT_Data_Read_Incremental_Template_FAIL", file), e);
            }
        }

        return true;
    }
    #endregion

    #region Text Entry Groups

    /// <summary>
    /// Load the game's entry group definitions. A missing file means the game has no groups.
    /// Groups are checked here so the editor can trust them: at most MaxSlots slots, each with a
    /// meaning, and no FMG twice in one group.
    /// </summary>
    public async Task<bool> SetupEntryGroups()
    {
        await Task.Yield();

        EntryGroups = new();

        var file = Path.Combine(AppContext.BaseDirectory, "Assets", "FMG",
            ProjectUtils.GetGameDirectory(Project), "Text Entry Groups.json");

        if (!File.Exists(file))
            return true;

        TextEntryGroupDefinitions loaded;

        try
        {
            var filestring = await File.ReadAllTextAsync(file);
            loaded = JsonSerializer.Deserialize(filestring, TextEditorJsonSerializerContext.Default.TextEntryGroupDefinitions);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Entry_Groups_Read_FAIL", file), e);
            return false;
        }

        foreach (var group in loaded?.Groups ?? new())
        {
            if (group?.Slots == null)
                continue;

            var name = group.Name ?? "";

            // Drop slots without a meaning, and an FMG listed twice in the group
            var seenFmgs = new HashSet<int>();
            var slots = group.Slots
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.Meaning) && seenFmgs.Add(s.FmgID))
                .ToList();

            if (slots.Count < group.Slots.Count)
            {
                Smithbox.LogWarning(this, LOC.Get("TEXT_Data_Entry_Groups_Invalid_Slots", name, group.Slots.Count - slots.Count));
            }

            if (slots.Count > TextEntryGroupDefinition.MaxSlots)
            {
                Smithbox.LogWarning(this, LOC.Get("TEXT_Data_Entry_Groups_Too_Many_Slots",
                    name, slots.Count, TextEntryGroupDefinition.MaxSlots));

                slots = slots.Take(TextEntryGroupDefinition.MaxSlots).ToList();
            }

            if (slots.Count == 0)
                continue;

            EntryGroups.Groups.Add(new TextEntryGroupDefinition { Name = group.Name, Slots = slots });
        }

        return true;
    }

    #endregion

    #region FMG Descriptors

    public async Task<bool> SetupFmgDescriptors()
    {
        var jsonName = "FMG Descriptor Registry.json";

        await Task.Yield();

        var folder = @$"{AppContext.BaseDirectory}/Assets/FMG/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, jsonName);

        if (File.Exists(file))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(file);

                try
                {
                    FmgDescriptors = JsonSerializer.Deserialize(filestring, TextEditorJsonSerializerContext.Default.FmgDescriptors);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("TEXT_Data_Deserialize_FMG_Descriptors_FAIL", file), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TEXT_Data_Read_FMG_Descriptors_FAIL", file), e);
            }
        }

        return true;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        foreach(var entry in AuxBanks)
        {
            entry.Value?.Dispose();
        }

        PrimaryBank = null;
        VanillaBank = null;
        AuxBanks = null;

        FmgFiles = null;
        GC.SuppressFinalize(this);
    }
    #endregion
}
