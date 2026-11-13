using System;
using System.Collections.Generic;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Where the mass edit looks for its targets
/// </summary>
public enum ModelMassEditScope
{
    SelectedObjects,
    CurrentModel,
    CurrentFile,
    AllModels
}

/// <summary>
/// What the mass edit does to its matches
/// </summary>
public enum ModelMassEditAction
{
    Edit,
    Delete
}

/// <summary>
/// One condition on the fields of an object. A field picked from the list is matched by its path exactly; a typed
/// field is matched as text against the path and its last part. An empty field matches any field.
/// </summary>
public record ModelMassEditCondition(string Field, bool PickedField, ModelQueryComparison Comparison, string Value,
    string Value2)
{
    public bool HasField => !string.IsNullOrWhiteSpace(Field);

    /// <summary>
    /// A range can be given by its upper end alone
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value) ||
        (Comparison is ModelQueryComparison.Between && !string.IsNullOrWhiteSpace(Value2));

    /// <summary>
    /// A condition with neither a field nor a value asks nothing
    /// </summary>
    public bool IsEmpty => !HasField && !HasValue;
}

/// <summary>
/// The filter of a mass edit, as it was when Find was pressed. The objects are always the targets: a model on its
/// own is never a match. LimitObjects comes from the Selected Objects scope; null means no limit.
/// </summary>
public class ModelMassEditFilter : IModelQueryFilter
{
    public string Model { get; init; } = "";

    /// <summary>
    /// The kind of object picked, or null for any
    /// </summary>
    public ModelQueryObjectKind? Kind { get; init; }

    public string ObjectText { get; init; } = "";

    public IReadOnlyList<ModelMassEditCondition> Conditions { get; init; } = [];

    public bool Exact { get; init; }

    public IReadOnlySet<object> LimitObjects { get; init; }

    // The typed text is matched as the search matches it
    private ModelQuery textQuery;
    private ModelQuery TextQuery => textQuery ??= new(Model ?? "", ObjectText ?? "", "", "", Exact);

    public bool MatchesObjects => true;

    public bool MatchModel(string modelName, ModelQuerySource source)
    {
        return ModelQueryMatcher.MatchModel(TextQuery, modelName, source);
    }

    public bool MatchObject(ModelQueryObjectKind kind, object obj, string name)
    {
        if (LimitObjects != null && !LimitObjects.Contains(obj))
            return false;

        if (Kind != null && Kind != kind)
            return false;

        // With a kind picked, the text is the name alone
        if (Kind != null)
            return ModelQueryMatcher.MatchText(TextQuery, ObjectText, name);

        return ModelQueryMatcher.MatchObject(TextQuery, kind, name);
    }

    /// <summary>
    /// Every condition must be met by at least one of the fields. Gives the fields that met them.
    /// </summary>
    public bool MatchFields(object obj, out string match)
    {
        match = null;

        List<string> matched = null;
        List<ModelQueryField> fields = null;

        foreach (var condition in Conditions)
        {
            if (condition.IsEmpty)
                continue;

            fields ??= [.. ModelQueryMatcher.GetFields(obj)];

            var found = false;

            foreach (var field in fields)
            {
                if (!MatchFieldName(condition, field.Name))
                    continue;

                if (condition.HasValue &&
                    !ModelQueryMatcher.Compare(condition.Comparison, condition.Value, condition.Value2, field.Value, field.EnumName))
                    continue;

                var shown = field.EnumName != null ? $"{field.EnumName} ({field.Value})" : field.Value;

                matched ??= new();
                matched.Add($"{field.Name} = {shown}");

                found = true;
                break;
            }

            if (!found)
                return false;
        }

        if (matched != null)
        {
            match = string.Join(", ", matched);
        }

        return true;
    }

    private bool MatchFieldName(ModelMassEditCondition condition, string name)
    {
        if (!condition.HasField)
            return true;

        var input = condition.Field.Trim();

        if (condition.PickedField)
            return name.Equals(input, StringComparison.OrdinalIgnoreCase);

        var dot = name.LastIndexOf('.');
        var leaf = dot >= 0 ? name[(dot + 1)..] : name;

        return Exact
            ? name.Equals(input, StringComparison.OrdinalIgnoreCase) || leaf.Equals(input, StringComparison.OrdinalIgnoreCase)
            : name.Contains(input, StringComparison.OrdinalIgnoreCase);
    }
}
