using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectDescriptor
{
    public Guid ProjectGUID = Guid.Empty;
    public string ProjectName = "";
    public string ProjectPath = "";
    public string DataPath = "";
    public ProjectType ProjectType = ProjectType.DS1;

    public bool ImportedParamRowNames = false;
    public bool AutoSelect = false;

    public string FolderTag = "";

    public bool EnableExternalMaterialData = true;

    public bool EnableMapEditor = false;
    public bool EnableModelEditor = false;
    public bool EnableTextEditor = false;
    public bool EnableParamEditor = false;
    public bool EnableGparamEditor = false;
    public bool EnableMaterialEditor = false;
    public bool EnableTextureViewer = false;
    public bool EnableFileBrowser = false;
    public bool EnableAnimEditor = false;
    public bool EnableHavokEditor = false;
    public bool EnableFxEditor = false;
    public bool EnableBehaviorEditor = false;
    public bool EnableEmevdEditor = false;
    public bool EnableTimeActEditor = false;

    public List<string> PinnedParams { get; set; } = new();
    public Dictionary<string, List<int>> PinnedRows { get; set; } = new();
    public Dictionary<string, List<string>> PinnedFields { get; set; } = new();

    public ProjectDescriptor() { }

    public ProjectDescriptor(ProjectDescriptor newDescriptor)
    {
        ProjectGUID = Guid.NewGuid();
        ProjectName = newDescriptor.ProjectName;
        ProjectPath = newDescriptor.ProjectPath;
        DataPath = newDescriptor.DataPath;
        ProjectType = newDescriptor.ProjectType;

        AutoSelect = newDescriptor.AutoSelect;
        ImportedParamRowNames = newDescriptor.ImportedParamRowNames;

        EnableMapEditor = newDescriptor.EnableMapEditor;
        EnableModelEditor = newDescriptor.EnableModelEditor;
        EnableTextEditor = newDescriptor.EnableTextEditor;
        EnableParamEditor = newDescriptor.EnableParamEditor;
        EnableGparamEditor = newDescriptor.EnableGparamEditor;
        EnableTextureViewer = newDescriptor.EnableTextureViewer;
        EnableMaterialEditor = newDescriptor.EnableMaterialEditor;
        EnableFileBrowser = newDescriptor.EnableFileBrowser;
        EnableAnimEditor = newDescriptor.EnableAnimEditor;
        EnableHavokEditor = newDescriptor.EnableHavokEditor;
        EnableFxEditor = newDescriptor.EnableFxEditor;
        EnableBehaviorEditor = newDescriptor.EnableBehaviorEditor;
        EnableEmevdEditor = newDescriptor.EnableEmevdEditor;
        EnableTimeActEditor = newDescriptor.EnableTimeActEditor;
        EnableExternalMaterialData = newDescriptor.EnableExternalMaterialData;

        FolderTag = newDescriptor.FolderTag;
    }

    public ProjectDescriptor(LegacyProjectDescriptor legacyDescriptor, string projectPath)
    {
        ProjectGUID = Guid.NewGuid();
        ProjectName = legacyDescriptor.ProjectName;
        ProjectPath = Path.GetDirectoryName(projectPath);
        DataPath = legacyDescriptor.GameRoot;
        ProjectType = legacyDescriptor.GameType;

        // Left to the auto-load assignment on load, so only one project ends up marked
        AutoSelect = false;
        ImportedParamRowNames = false;

        EnableAllEditors();

        FolderTag = "";

        if (legacyDescriptor.PinnedParams != null)
            PinnedParams = new List<string>(legacyDescriptor.PinnedParams);

        if (legacyDescriptor.PinnedRows != null)
            PinnedRows = legacyDescriptor.PinnedRows.ToDictionary(e => e.Key, e => new List<int>(e.Value ?? new()));

        if (legacyDescriptor.PinnedFields != null)
            PinnedFields = legacyDescriptor.PinnedFields.ToDictionary(e => e.Key, e => new List<string>(e.Value ?? new()));
    }

    /// <remarks>
    /// The list form of the pinned rows and fields doesn't say which param they belong to, so only the pinned params
    /// are carried over.
    /// </remarks>
    public ProjectDescriptor(LegacyProjectDescriptorAlt legacyDescriptor, string projectPath)
    {
        ProjectGUID = Guid.NewGuid();
        ProjectName = legacyDescriptor.ProjectName;
        ProjectPath = Path.GetDirectoryName(projectPath);
        DataPath = legacyDescriptor.GameRoot;
        ProjectType = legacyDescriptor.GameType;

        AutoSelect = false;
        ImportedParamRowNames = false;

        EnableAllEditors();

        FolderTag = "";

        if (legacyDescriptor.PinnedParams != null)
            PinnedParams = new List<string>(legacyDescriptor.PinnedParams);
    }

    /// <summary>
    /// Whether the settings the project configuration form edits match <paramref name="other"/>'s. The project type
    /// isn't one of them: it is fixed once the project is created.
    /// </summary>
    public bool ConfigurationEquals(ProjectDescriptor other)
    {
        return ProjectName == other.ProjectName.Trim()
            && ProjectPath == other.ProjectPath
            && DataPath == other.DataPath
            && FolderTag == other.FolderTag
            && EnableMapEditor == other.EnableMapEditor
            && EnableModelEditor == other.EnableModelEditor
            && EnableTextEditor == other.EnableTextEditor
            && EnableParamEditor == other.EnableParamEditor
            && EnableGparamEditor == other.EnableGparamEditor
            && EnableTextureViewer == other.EnableTextureViewer
            && EnableMaterialEditor == other.EnableMaterialEditor
            && EnableFileBrowser == other.EnableFileBrowser
            && EnableAnimEditor == other.EnableAnimEditor
            && EnableHavokEditor == other.EnableHavokEditor
            && EnableFxEditor == other.EnableFxEditor
            && EnableBehaviorEditor == other.EnableBehaviorEditor
            && EnableEmevdEditor == other.EnableEmevdEditor
            && EnableTimeActEditor == other.EnableTimeActEditor
            && EnableExternalMaterialData == other.EnableExternalMaterialData;
    }

    /// <summary>
    /// Copies the settings the project configuration form edits from <paramref name="edited"/>. Identity (GUID), the
    /// project type (fixed once created) and state kept by other editors (pins, auto-load, imported row names) are
    /// left as they are.
    /// </summary>
    public void ApplyConfiguration(ProjectDescriptor edited)
    {
        ProjectName = edited.ProjectName.Trim();
        ProjectPath = edited.ProjectPath;
        DataPath = edited.DataPath;

        FolderTag = edited.FolderTag;

        EnableMapEditor = edited.EnableMapEditor;
        EnableModelEditor = edited.EnableModelEditor;
        EnableTextEditor = edited.EnableTextEditor;
        EnableParamEditor = edited.EnableParamEditor;
        EnableGparamEditor = edited.EnableGparamEditor;
        EnableTextureViewer = edited.EnableTextureViewer;
        EnableMaterialEditor = edited.EnableMaterialEditor;
        EnableFileBrowser = edited.EnableFileBrowser;
        EnableAnimEditor = edited.EnableAnimEditor;
        EnableHavokEditor = edited.EnableHavokEditor;
        EnableFxEditor = edited.EnableFxEditor;
        EnableBehaviorEditor = edited.EnableBehaviorEditor;
        EnableEmevdEditor = edited.EnableEmevdEditor;
        EnableTimeActEditor = edited.EnableTimeActEditor;
        EnableExternalMaterialData = edited.EnableExternalMaterialData;
    }

    public ProjectDescriptor Clone()
    {
        return new ProjectDescriptor
        {
            ProjectGUID = this.ProjectGUID,
            ProjectName = this.ProjectName,
            ProjectPath = this.ProjectPath,
            DataPath = this.DataPath,
            ProjectType = this.ProjectType,

            AutoSelect = this.AutoSelect,
            ImportedParamRowNames = this.ImportedParamRowNames,

            EnableMapEditor = this.EnableMapEditor,
            EnableModelEditor = this.EnableModelEditor,
            EnableTextEditor = this.EnableTextEditor,
            EnableParamEditor = this.EnableParamEditor,
            EnableGparamEditor = this.EnableGparamEditor,
            EnableTextureViewer = this.EnableTextureViewer,
            EnableMaterialEditor = this.EnableMaterialEditor,
            EnableFileBrowser = this.EnableFileBrowser,
            EnableAnimEditor = this.EnableAnimEditor,
            EnableHavokEditor = this.EnableHavokEditor,
            EnableFxEditor = this.EnableFxEditor,
            EnableBehaviorEditor = this.EnableBehaviorEditor,
            EnableEmevdEditor = this.EnableEmevdEditor,
            EnableTimeActEditor = this.EnableTimeActEditor,
            EnableExternalMaterialData = this.EnableExternalMaterialData,

            FolderTag = this.FolderTag,

            PinnedParams = new List<string>(this.PinnedParams),
            PinnedRows = this.PinnedRows.ToDictionary(
                kvp => kvp.Key,
                kvp => new List<int>(kvp.Value)
            ),
            PinnedFields = this.PinnedFields.ToDictionary(
                kvp => kvp.Key,
                kvp => new List<string>(kvp.Value)
            )
        };
    }

    public static ProjectDescriptor CreateNew(ProjectDescriptor template)
    {
        var clone = template.Clone();
        clone.ProjectGUID = Guid.NewGuid();
        return clone;
    }

    public void EnableAllEditors()
    {
        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableTextEditor = true;
        EnableParamEditor = true;
        EnableGparamEditor = true;
        EnableTextureViewer = true;
        EnableMaterialEditor = true;
        EnableFileBrowser = true;
        EnableAnimEditor = true;
        EnableHavokEditor = true;
        EnableFxEditor = true;
        EnableBehaviorEditor = true;
        EnableEmevdEditor = true;
        EnableTimeActEditor = true;
        EnableExternalMaterialData = true;
    }
}

public class LegacyProjectDescriptor
{
    public string ProjectName { get; set; }
    public string GameRoot { get; set; }
    public ProjectType GameType { get; set; }

    public List<string> PinnedParams { get; set; }
    public Dictionary<string, List<int>> PinnedRows { get; set; }
    public Dictionary<string, List<string>> PinnedFields { get; set; }

    public bool UseLooseParams { get; set; }
    public bool PartialParams { get; set; }
    public string LastFmgLanguageUsed { get; set; }

    public LegacyProjectDescriptor() { }

    public LegacyProjectDescriptor(ProjectEntry curProject)
    {
        ProjectName = curProject.Descriptor.ProjectName;
        GameRoot = curProject.Descriptor.DataPath;
        GameType = curProject.Descriptor.ProjectType;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        UseLooseParams = false;

        // Account for this for DS3 projects
        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS2;
        }

        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS3;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}


public class LegacyProjectDescriptorAlt
{
    public string ProjectName { get; set; }
    public string GameRoot { get; set; }
    public ProjectType GameType { get; set; }

    public List<string> PinnedParams { get; set; }
    public List<int> PinnedRows { get; set; }
    public List<string> PinnedFields { get; set; }

    public bool UseLooseParams { get; set; }
    public bool PartialParams { get; set; }
    public string LastFmgLanguageUsed { get; set; }

    public LegacyProjectDescriptorAlt() { }

    public LegacyProjectDescriptorAlt(ProjectEntry curProject)
    {
        ProjectName = curProject.Descriptor.ProjectName;
        GameRoot = curProject.Descriptor.DataPath;
        GameType = curProject.Descriptor.ProjectType;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        UseLooseParams = false;

        // Account for this for DS3 projects
        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS2;
        }

        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS3;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}