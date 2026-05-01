using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public static class GparamMetaUtils
{
    public static GparamAnnotationEntry GetGroupAnnotation(ProjectEntry project, string groupId)
    {
        var gparamData = project.Handler.GparamData;
        var entries = gparamData.Annotations.Entries.FirstOrDefault(e => e.Key.Name == CFG.Current.GparamEditor_Annotation_Language);

        foreach(var entry in entries.Value.Params)
        {
            if (groupId.Contains(entry.ID))
            {
                return entry;
            }
        }

        return null;
    }

    public static string GetGroupName(ProjectEntry project, string groupId)
    {
        var annotation = GetGroupAnnotation(project, groupId);

        if (annotation != null)
        {
            return annotation.Name;
        }

        return groupId;
    }
    public static string GetGroupDescription(ProjectEntry project, string groupId)
    {
        var annotation = GetGroupAnnotation(project, groupId);

        if (annotation != null)
        {
            return annotation.Description;
        }

        return "";
    }

    public static GparamAnnotationFieldEntry GetFieldAnnotation(ProjectEntry project, string groupId, string fieldId)
    {
        var gparamData = project.Handler.GparamData;
        var entries = gparamData.Annotations.Entries.FirstOrDefault(e => e.Key.Name == CFG.Current.GparamEditor_Annotation_Language);

        foreach (var entry in entries.Value.Params)
        {
            if (groupId.Contains(entry.ID))
            {
                foreach(var field in entry.Fields)
                {
                    if (fieldId.Contains(field.ID))
                    {
                        return field;
                    }
                }
            }
        }

        return null;
    }

    public static string GetFieldName(ProjectEntry project, string groupId, string fieldId)
    {
        var annotation = GetFieldAnnotation(project, groupId, fieldId);

        if (annotation != null)
        {
            return annotation.Name;
        }

        return fieldId;
    }
    public static string GetFieldDescription(ProjectEntry project, string groupId, string fieldId)
    {
        var annotation = GetFieldAnnotation(project, groupId, fieldId);

        if (annotation != null)
        {
            return annotation.Description;
        }

        return "";
    }
    public static string GetFieldEnum(ProjectEntry project, string groupId, string fieldId)
    {
        var annotation = GetFieldAnnotation(project, groupId, fieldId);

        if (annotation != null)
        {
            return annotation.Enum;
        }

        return "";
    }

    public static bool IsFieldBoolean(ProjectEntry project, string groupId, string fieldId)
    {
        var annotation = GetFieldAnnotation(project, groupId, fieldId);

        if (annotation != null)
        {
            if (annotation.IsBoolean)
            {
                return true;
            }
        }

        return false;
    }
}
