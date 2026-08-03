using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Holds the data banks for Materials.
/// Data Flow: Full Load
/// </summary>
public class MaterialData : IDisposable
{
    public ProjectEntry Project;

    public MaterialBank PrimaryBank;

    // Disable for now to reduce loading time
    // TODO: Add toggle if the Material Editor reaches a point where it can make use of the Vanilla Bank
    //public MaterialBank VanillaBank;

    public MaterialDisplayConfiguration MaterialDisplayConfiguration;

    public MaterialData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        //VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Material Display Configuration
        Task<bool> matDispTask = SetupMaterialDisplayConfiguration();
        bool matDispTaskResult = await matDispTask;

        if (!matDispTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Setup_Material_Display_Config_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAT_Data_Setup_Material_Display_Config_PASS"));
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Data_Setup_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAT_Data_Setup_Primary_Bank_PASS"));
        }

        return true;
    }

    /// <summary>
    /// Setup the material display configuration for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMaterialDisplayConfiguration()
    {
        await Task.Yield();

        MaterialDisplayConfiguration = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MATERIAL", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Display Configuration.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "MATERIAL", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Display Configuration.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    MaterialDisplayConfiguration = JsonSerializer.Deserialize(filestring, MaterialEditorJsonSerializerContext.Default.MaterialDisplayConfiguration);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Deserialize_Material_Display_Config", targetFile), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAT_Data_Log_Failed_to_Read_Material_Display_Config", targetFile), e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        //VanillaBank?.Dispose();

        PrimaryBank = null;
        //VanillaBank = null;

        MaterialDisplayConfiguration = null;
    }
    #endregion
}
