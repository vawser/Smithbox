using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.HavokEditor;

/// <summary>A number a template asks for, with its default and bounds</summary>
public class HavokTemplateNumber
{
    public int Default { get; set; } = 0;
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 999;

    /// <summary>The reference must be given (the template copies a category's registrations)</summary>
    public bool Required { get; set; } = false;
}

/// <summary>A set of animation IDs the user can tick (e.g. an Ash of War's follow-ups)</summary>
public class HavokTemplateGroup
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Default { get; set; } = true;
    public List<long> Ids { get; set; } = new();
}

/// <summary>
/// A ready-made gameplay pattern (Claude/Documentation/Behavior.md (Havok behavior edits)). The Category kind registers a
/// new animation category: clips aNEW_ID added to the selectors that play ID, for the IDs of the ticked groups, or for
/// every ID a reference category is registered for.
/// </summary>
public class HavokBehaviorTemplate
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>Category (register an animation category) or State (add new states)</summary>
    public string Kind { get; set; } = "Category";

    /// <summary>The behavior file it's made for (c0000.hkx), or null for any</summary>
    public string File { get; set; }

    /// <summary>The selector offset types (m_offsetType) the new category is added under; empty for any</summary>
    public List<string> OffsetTypes { get; set; } = new();

    public HavokTemplateNumber Category { get; set; } = new();

    /// <summary>A category whose clips the new ones copy, and whose registrations can be copied; Default -1 for none</summary>
    public HavokTemplateNumber Reference { get; set; } = new() { Default = -1, Min = -1 };

    public List<HavokTemplateGroup> Groups { get; set; } = new();

    /// <summary>What to do after applying it (params, HKS), shown in the wizard and logged; {Category}, {Reference}, {Ids}</summary>
    public List<string> Notes { get; set; } = new();

    // ---- State kind: new states copied from a template state of a machine

    /// <summary>The state machine the states are added to (its m_name)</summary>
    public string StateMachine { get; set; }

    /// <summary>
    /// A regex the template state's name matches, its first group the ID in the names (^Attack(\d+)$). The first such
    /// state whose number is its selector's animation ID, with a wildcard into it, is the default template.
    /// </summary>
    public string TemplateMatch { get; set; }

    /// <summary>The first new animation ID (the state Attack3040, its event W_Attack3040, its clip a000_003040)</summary>
    public HavokTemplateNumber Id { get; set; } = new() { Default = 0, Min = 0, Max = 999999 };

    /// <summary>How many states (IDs Id, Id + Step, ...)</summary>
    public HavokTemplateNumber Count { get; set; } = new() { Default = 1, Min = 1, Max = 100 };

    public HavokTemplateNumber Step { get; set; } = new() { Default = 1, Min = 1, Max = 1000 };

    /// <summary>Text written once per new state ({Id}): the script functions the states need</summary>
    public string Snippet { get; set; }

    [JsonIgnore]
    public bool IsState => string.Equals(Kind, "State", StringComparison.OrdinalIgnoreCase);

    /// <summary>The file it was read from</summary>
    [JsonIgnore]
    public string FilePath { get; set; }

    /// <summary>Read from the project's templates (replacing a built-in of the same file name, or new)</summary>
    [JsonIgnore]
    public bool FromProject { get; set; }
}

/// <summary>
/// Loads the behavior templates: the built-in ones (Assets/HAVOK/Templates/&lt;GAME&gt;/*.json) and the project's
/// (.smithbox/Project/HAVOK/Templates/*.json), a project file replacing the built-in of the same name, as the aliases do
/// </summary>
public static class HavokBehaviorTemplates
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string BuiltInFolder(ProjectEntry project) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "HAVOK", "Templates", ProjectUtils.GetGameDirectory(project));

    public static string ProjectFolder(ProjectEntry project) =>
        Path.Combine(project.Descriptor.ProjectPath, ".smithbox", "Project", "HAVOK", "Templates");

    /// <summary>
    /// Every template, by name; files that can't be read are skipped and their reasons returned
    /// </summary>
    public static List<HavokBehaviorTemplate> Load(ProjectEntry project, out List<string> errors)
    {
        errors = new List<string>();

        var files = new Dictionary<string, (string Path, bool FromProject)>(StringComparer.OrdinalIgnoreCase);

        foreach (var (folder, fromProject) in new[] { (BuiltInFolder(project), false), (ProjectFolder(project), true) })
        {
            if (!Directory.Exists(folder))
                continue;

            foreach (var path in Directory.EnumerateFiles(folder, "*.json"))
                files[Path.GetFileName(path)] = (path, fromProject);
        }

        var templates = new List<HavokBehaviorTemplate>();

        foreach (var (path, fromProject) in files.Values)
        {
            try
            {
                var template = JsonSerializer.Deserialize<HavokBehaviorTemplate>(System.IO.File.ReadAllText(path), Json);

                if (template == null || string.IsNullOrWhiteSpace(template.Name))
                {
                    errors.Add(LOC.Get("HAVOK_Templates_Error_No_Name", path));
                    continue;
                }

                if (!string.Equals(template.Kind, "Category", StringComparison.OrdinalIgnoreCase) && !template.IsState)
                {
                    errors.Add(LOC.Get("HAVOK_Templates_Error_Kind", path, template.Kind));
                    continue;
                }

                if (template.IsState && (string.IsNullOrWhiteSpace(template.StateMachine) || string.IsNullOrWhiteSpace(template.TemplateMatch)))
                {
                    errors.Add(LOC.Get("HAVOK_Templates_Error_State_Fields", path));
                    continue;
                }

                if (template.IsState)
                {
                    try { _ = new System.Text.RegularExpressions.Regex(template.TemplateMatch); }
                    catch (ArgumentException e)
                    {
                        errors.Add(LOC.Get("HAVOK_Templates_Error_Read", path, e.Message));
                        continue;
                    }
                }

                template.Id ??= new() { Default = 0, Min = 0, Max = 999999 };
                template.Count ??= new() { Default = 1, Min = 1, Max = 100 };
                template.Step ??= new() { Default = 1, Min = 1, Max = 1000 };

                template.FilePath = path;
                template.FromProject = fromProject;
                template.Groups ??= new();
                template.OffsetTypes ??= new();
                template.Notes ??= new();
                template.Category ??= new();
                template.Reference ??= new() { Default = -1, Min = -1 };

                templates.Add(template);
            }
            catch (Exception e)
            {
                errors.Add(LOC.Get("HAVOK_Templates_Error_Read", path, e.Message));
            }
        }

        return templates.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
