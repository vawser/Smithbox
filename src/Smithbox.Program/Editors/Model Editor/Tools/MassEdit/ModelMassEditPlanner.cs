using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// One field of an object as the plan changes it
/// </summary>
public record ModelPlannedValue(string Path, PropertyInfo Property, object OldValue, object NewValue);

/// <summary>
/// An object the plan changes, with every field it changes
/// </summary>
public class ModelPlannedObject
{
    public ModelQueryMatch Match;
    public List<ModelPlannedValue> Values = new();
}

/// <summary>
/// A change as the preview lists it
/// </summary>
public record ModelPlannedChange(ModelQueryMatch Match, string Field, string OldText, string NewText);

/// <summary>
/// A match the plan leaves alone, and why
/// </summary>
public record ModelPlannedSkip(ModelQueryMatch Match, string Reason);

public class ModelMassEditPlan
{
    public List<ModelPlannedObject> Objects = new();
    public List<ModelPlannedChange> Changes = new();
    public List<ModelPlannedSkip> Skips = new();

    /// <summary>
    /// Matches the steps leave as they are
    /// </summary>
    public int Unchanged;

    public int ModelCount;
}

/// <summary>
/// Works out what the steps do to every match, without changing anything, so the preview shows exactly what Apply
/// does. An object is changed by every step or by none, so it is never left half edited.
/// </summary>
public static class ModelMassEditPlanner
{
    public static ModelMassEditPlan Plan(IReadOnlyList<ModelQueryMatch> matches, IReadOnlyList<ModelMassEditStep> steps)
    {
        var plan = new ModelMassEditPlan();
        var activeSteps = steps.Where(e => e.HasField).ToList();

        if (activeSteps.Count == 0)
            return plan;

        var models = new HashSet<(ModelContainerWrapper, string)>();

        foreach (var match in matches)
        {
            // The value each field has as the steps go through it, in the order the fields were first edited
            var working = new Dictionary<string, (ModelFieldTarget Target, object Old, object Current)>();
            var order = new List<string>();

            string reason = null;

            foreach (var step in activeSteps)
            {
                if (!working.TryGetValue(step.Field, out var entry))
                {
                    if (!ModelMassEditFields.TryResolve(match.Object, step.Field, out var target))
                    {
                        reason = LOC.Get("MODEL_MassEditTool_Skip_No_Field", step.Field);
                        break;
                    }

                    if (ModelMassEditFields.GetKind(target.Property) is ModelMassEditFieldKind.ReadOnly)
                    {
                        reason = LOC.Get("MODEL_MassEditTool_Skip_Read_Only");
                        break;
                    }

                    var value = target.Get();
                    entry = (target, value, value);
                    order.Add(step.Field);
                }

                reason = ModelMassEditFields.TryApply(step.Operator, entry.Target.Property.PropertyType, entry.Current,
                    step.Value, step.Value2, out var result);

                if (reason != null)
                    break;

                working[step.Field] = (entry.Target, entry.Old, result);
            }

            if (reason != null)
            {
                plan.Skips.Add(new ModelPlannedSkip(match, reason));
                continue;
            }

            var planned = new ModelPlannedObject { Match = match };

            foreach (var path in order)
            {
                var (target, old, current) = working[path];

                if (!Equals(old, current))
                {
                    planned.Values.Add(new ModelPlannedValue(path, target.Property, old, current));
                }
            }

            if (planned.Values.Count == 0)
            {
                plan.Unchanged++;
                continue;
            }

            plan.Objects.Add(planned);
            models.Add((match.Container, match.ModelName.ToLowerInvariant()));

            foreach (var value in planned.Values)
            {
                plan.Changes.Add(new ModelPlannedChange(match, value.Path,
                    ModelMassEditFields.FormatValue(value.OldValue), ModelMassEditFields.FormatValue(value.NewValue)));
            }
        }

        plan.ModelCount = models.Count;

        return plan;
    }
}

/// <summary>
/// Carries a plan out. A model loaded in a view is edited through its history, as one step a single undo takes
/// back; any other model has its FLVER edited directly and is left unsaved, for Save All to write.
/// </summary>
public static class ModelMassEditApplier
{
    public record Result(int Applied, int Stale, int Skipped, int Unloaded, List<string> Reasons);

    // The links between bones are kept in step by the editor's own actions, which need the model loaded
    private static readonly HashSet<string> NodeLinkFields = new(StringComparer.Ordinal)
    {
        nameof(FLVER.Node.ParentIndex), nameof(FLVER.Node.FirstChildIndex),
        nameof(FLVER.Node.NextSiblingIndex), nameof(FLVER.Node.PreviousSiblingIndex)
    };

    #region Edit
    public static Result Apply(ModelEditorView view, ModelMassEditPlan plan)
    {
        var applied = 0;
        var stale = 0;
        var skipped = 0;
        var unloaded = 0;
        var reasons = new List<string>();

        // The actions for each view a model is loaded in, so each view's history gets one step
        var actions = new Dictionary<ModelEditorView, List<ViewportAction>>();

        foreach (var group in plan.Objects.GroupBy(e => (e.Match.Container, Name: e.Match.ModelName.ToLowerInvariant())))
        {
            var wrapper = FindModel(group.Key.Container, group.First().Match.ModelName);

            if (wrapper == null)
            {
                stale += group.Count();
                continue;
            }

            // A model kept with its history: its objects are held by that history, so it is edited once loaded
            if (wrapper.Parked != null)
            {
                skipped += group.Count();
                reasons.Add(LOC.Get("MODEL_MassEditTool_Skip_Parked", wrapper.Name));
                continue;
            }

            if (wrapper.Container != null)
            {
                var ownerView = wrapper.Container.View;

                if (!actions.TryGetValue(ownerView, out var list))
                    actions[ownerView] = list = new();

                foreach (var planned in group)
                {
                    if (AddLoadedActions(ownerView, wrapper, planned, list))
                        applied++;
                    else
                        stale++;
                }

                continue;
            }

            // Not loaded: its FLVER is read in full (or its unsaved one kept) and edited directly
            wrapper.LoadForEdit();

            if (wrapper.FLVER == null)
            {
                stale += group.Count();
                continue;
            }

            var changed = false;

            foreach (var planned in group)
            {
                var result = ApplyUnloaded(wrapper.FLVER, planned);

                if (result == null)
                {
                    applied++;
                    changed = true;
                }
                else if (result == "")
                {
                    stale++;
                }
                else
                {
                    skipped++;
                    reasons.Add(result);
                }
            }

            if (changed)
            {
                wrapper.IsDirty = true;
                unloaded++;
            }
        }

        foreach (var (ownerView, list) in actions)
        {
            if (list.Count == 0)
                continue;

            ownerView.ViewportActionManager.ExecuteAction(list.Count == 1
                ? list[0]
                : new ModelCompoundAction(list, LOC.Get("MODEL_History_Mass_Edit", list.Count)));
        }

        return new Result(applied, stale, skipped, unloaded, reasons.Distinct().ToList());
    }

    /// <summary>
    /// The actions that make the planned changes to an object of a loaded model. False if the object has gone or
    /// changed since the plan was made.
    /// </summary>
    private static bool AddLoadedActions(ModelEditorView view, ModelWrapper wrapper, ModelPlannedObject planned,
        List<ViewportAction> actions)
    {
        var entity = ModelQueryRunner.Resolve(wrapper.Container, planned.Match);

        if (entity == null)
            return false;

        var root = planned.Match.Kind is ModelQueryObjectKind.Header ? wrapper.FLVER?.Header : entity.WrappedObject;

        if (root == null || root.GetType() != planned.Match.Object.GetType())
            return false;

        var targets = new List<(ModelFieldTarget Target, object NewValue)>();

        foreach (var value in planned.Values)
        {
            if (!ModelMassEditFields.TryResolve(root, value.Path, out var target) || !Equals(target.Get(), value.OldValue))
                return false;

            targets.Add((target, value.NewValue));
        }

        foreach (var (target, newValue) in targets)
        {
            actions.Add(new ModelPropertyChangeAction(view, target.Property, new HashSet<Entity> { entity }, newValue,
                ownerObject: target.Owner));
        }

        return true;
    }

    /// <summary>
    /// Makes the planned changes to an object of a model that isn't loaded. Null when applied, "" when the object has
    /// gone or changed since the plan was made, else the reason it was left alone.
    /// </summary>
    private static string ApplyUnloaded(FLVER2 flver, ModelPlannedObject planned)
    {
        var root = GetObject(flver, planned.Match.Kind, planned.Match.Index);

        if (root == null || root.GetType() != planned.Match.Object.GetType())
            return "";

        var targets = new List<(ModelFieldTarget Target, object NewValue)>();

        foreach (var value in planned.Values)
        {
            if (root is FLVER.Node && NodeLinkFields.Contains(value.Path))
                return LOC.Get("MODEL_MassEditTool_Skip_Node_Links");

            if (!ModelMassEditFields.TryResolve(root, value.Path, out var target) || !Equals(target.Get(), value.OldValue))
                return "";

            targets.Add((target, value.NewValue));
        }

        foreach (var (target, newValue) in targets)
        {
            target.Property.SetValue(target.Owner, newValue);

            // A dummy follows its attach bone exactly when it has one, as the Properties view keeps it
            if (target.Owner is FLVER.Dummy dummy && target.Property.Name == nameof(FLVER.Dummy.AttachBoneIndex))
            {
                dummy.FollowAttachBone = dummy.AttachBoneIndex != -1;
            }
        }

        return null;
    }

    public static object GetObject(FLVER2 flver, ModelQueryObjectKind kind, int index)
    {
        return kind switch
        {
            ModelQueryObjectKind.Header => flver.Header,
            ModelQueryObjectKind.Dummy => index >= 0 && index < flver.Dummies.Count ? flver.Dummies[index] : null,
            ModelQueryObjectKind.Material => index >= 0 && index < flver.Materials.Count ? flver.Materials[index] : null,
            ModelQueryObjectKind.Node => index >= 0 && index < flver.Nodes.Count ? flver.Nodes[index] : null,
            ModelQueryObjectKind.Mesh => index >= 0 && index < flver.Meshes.Count ? flver.Meshes[index] : null,
            _ => null
        };
    }
    #endregion

    #region Delete
    /// <summary>
    /// Deletes the matched objects of loaded models, one step in each view's history. The objects of models that
    /// aren't loaded, and the model headers, are left alone.
    /// </summary>
    public static Result Delete(ProjectEntry project, IReadOnlyList<ModelQueryMatch> matches)
    {
        var deleted = 0;
        var stale = 0;
        var skipped = 0;
        var reasons = new List<string>();

        var targets = new Dictionary<ModelEditorView, List<Entity>>();

        foreach (var match in matches)
        {
            if (match.Kind is ModelQueryObjectKind.Header)
            {
                skipped++;
                reasons.Add(LOC.Get("MODEL_MassEditTool_Skip_Delete_Header"));
                continue;
            }

            var wrapper = FindModel(match.Container, match.ModelName);

            if (wrapper?.Container == null)
            {
                skipped++;
                reasons.Add(LOC.Get("MODEL_MassEditTool_Skip_Delete_Not_Loaded"));
                continue;
            }

            var entity = ModelQueryRunner.Resolve(wrapper.Container, match);

            if (entity == null || !ReferenceEquals(entity.WrappedObject, match.Object))
            {
                stale++;
                continue;
            }

            var view = wrapper.Container.View;

            if (!targets.TryGetValue(view, out var list))
                targets[view] = list = new();

            list.Add(entity);
            deleted++;
        }

        foreach (var (view, entities) in targets)
        {
            view.ViewportActionManager.ExecuteAction(new DeleteModelObjectAction(view, project, entities));
        }

        return new Result(deleted, stale, skipped, 0, reasons.Distinct().ToList());
    }
    #endregion

    private static ModelWrapper FindModel(ModelContainerWrapper container, string name)
    {
        var wrapper = container.Models.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

        if (wrapper == null)
        {
            // The list of models is only read once the container is picked
            container.PopulateModelList();
            wrapper = container.Models.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        return wrapper;
    }
}

/// <summary>
/// Several model edits as one step of the history, named for the History tool
/// </summary>
public class ModelCompoundAction : ViewportCompoundAction, IModelHistoryItem
{
    private readonly string Description;

    public ModelCompoundAction(List<ViewportAction> actions, string description) : base(actions)
    {
        Description = description;
        Label = description;
    }

    public string Describe() => Description;
}
