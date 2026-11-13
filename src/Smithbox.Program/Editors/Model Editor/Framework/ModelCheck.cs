using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// One problem <see cref="ModelCheckRules" /> found in a model
/// </summary>
public sealed record ModelCheckIssue(ModelWrapper Model, ModelCheckFinding Finding)
{
    public ModelCheckSeverity Severity => Finding.Severity;

    /// <summary>
    /// The editor object the problem is on, while the model is loaded and the object is still in it
    /// </summary>
    public Entity FindEntity()
    {
        var container = Model.Container;

        if (container == null || Finding.Target == null)
            return null;

        return container.Objects.FirstOrDefault(e => ReferenceEquals(e.WrappedObject, Finding.Target));
    }

    /// <summary>The object, as the report names it</summary>
    public string DescribeObject()
    {
        var f = Finding;

        return f.Target switch
        {
            FLVER2.Mesh => LOC.Get("MODEL_SaveCheck_Object_Mesh", f.TargetIndex),
            FLVER.Node node => LOC.Get("MODEL_SaveCheck_Object_Node", f.TargetIndex, node.Name),
            FLVER.Dummy dummy => LOC.Get("MODEL_SaveCheck_Object_Dummy", f.TargetIndex, dummy.ReferenceID),
            FLVER2.SkeletonSet => LOC.Get("MODEL_SaveCheck_Object_Skeleton_Set"),
            _ => LOC.Get("MODEL_SaveCheck_Object_Model")
        };
    }

    /// <summary>One line for the report</summary>
    public string Describe()
    {
        var f = Finding;

        return f.Kind switch
        {
            ModelCheckKind.MaterialOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Material", f.Field, f.Value, f.Limit),
            ModelCheckKind.NodeOutOfRange when f.Count > 1 => LOC.Get("MODEL_SaveCheck_Issue_Node_Many", f.Field, f.Count, f.Value, f.Limit),
            ModelCheckKind.NodeOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Node", f.Field, f.Value, f.Limit),
            ModelCheckKind.LayoutOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Layout", f.Field, f.Value, f.Limit),
            ModelCheckKind.VertexBoneTableOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Vertex_Bone_Table", f.Count, f.Value, f.Limit),
            ModelCheckKind.VertexNodeOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Vertex_Node", f.Count, f.Value, f.Limit),
            ModelCheckKind.FaceIndexOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Face_Index", f.Field, f.Count, f.Value, f.Limit),
            ModelCheckKind.VertexBufferMissing => LOC.Get("MODEL_SaveCheck_Issue_Vertex_Buffer_Missing", f.Count),
            ModelCheckKind.ParentLoop => LOC.Get("MODEL_SaveCheck_Issue_Parent_Loop", f.Value),
            ModelCheckKind.SiblingChain => LOC.Get("MODEL_SaveCheck_Issue_Sibling_Chain", f.Field, f.Value),
            ModelCheckKind.MissingSkeletonSet => LOC.Get("MODEL_SaveCheck_Issue_Missing_Skeleton_Set", $"0x{f.Value:X}"),
            ModelCheckKind.SkeletonLinkOutOfRange => LOC.Get("MODEL_SaveCheck_Issue_Skeleton_Link", f.Field, f.Value, f.Limit),
            ModelCheckKind.Flver0Limit => LOC.Get("MODEL_SaveCheck_Issue_Flver0_Limit", f.Field, f.Value, f.Limit),
            ModelCheckKind.DuplicateDummyReferenceID => LOC.Get("MODEL_SaveCheck_Issue_Duplicate_Dummy_ID", f.Value, f.Limit),
            _ => f.Kind.ToString()
        };
    }
}

/// <summary>
/// Checks models before they are saved: indices out of range, vertex bones, node links, the skeleton set, and
/// FLVER0's limits for DES. The rules are in <see cref="ModelCheckRules" />, which Tools/ModelCheckSurvey runs over
/// the vanilla models of every game. Used by File › Save and Save All (the Save Check window).
/// </summary>
public static class ModelCheck
{
    public static List<ModelCheckIssue> Run(ProjectEntry project, IEnumerable<ModelWrapper> models)
    {
        var isFlver0 = project.Descriptor.ProjectType is ProjectType.DES;
        var issues = new List<ModelCheckIssue>();

        foreach (var model in models)
        {
            if (model?.FLVER == null)
                continue;

            // The check reads the FLVER, so give it the loaded model's lists as the save would write them
            if (model.Container != null)
            {
                model.UpdateFLVER();
            }

            foreach (var finding in ModelCheckRules.Run(model.FLVER, isFlver0))
            {
                issues.Add(new ModelCheckIssue(model, finding));
            }
        }

        // Errors first, then by model and kind, keeping each model's order
        return issues
            .Select((issue, index) => (issue, index))
            .OrderBy(e => e.issue.Severity)
            .ThenBy(e => e.issue.Model.Name, StringComparer.Ordinal)
            .ThenBy(e => e.issue.Finding.Kind)
            .ThenBy(e => e.index)
            .Select(e => e.issue)
            .ToList();
    }

    /// <summary>Whether the issues should stop a save. Information never does.</summary>
    public static bool StopsSave(IEnumerable<ModelCheckIssue> issues, bool stopOnWarnings)
    {
        return issues.Any(i => i.Severity is ModelCheckSeverity.Error ||
                               (stopOnWarnings && i.Severity is ModelCheckSeverity.Warning));
    }
}
