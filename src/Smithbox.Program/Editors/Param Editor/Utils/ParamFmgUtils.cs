using Andre.Formats;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;


public static class ParamFmgUtils
{
    public static List<FMG.Entry> GetRowDecoratorFmgEntries(ParamEditorScreen editor, string paramName)
    {
        if (editor.Project.Handler.TextEditor == null)
            return new List<FMG.Entry>();

        List<FMG.Entry> entries = new List<FMG.Entry>();

        var annotationEntry = editor.Project.Handler.ParamData.RowFmgAnnotations.Entries.FirstOrDefault(e => e.Param == paramName);

        if(annotationEntry == null)
            return new List<FMG.Entry>();

        foreach (var (path, entry) in editor.Project.Handler.TextData.PrimaryBank.Containers)
        {
            if (entry.IsContainerUnused())
                continue;

            if (entry.ContainerDisplayCategory != CFG.Current.TextEditor_Primary_Category)
                continue;

            if (entry.FmgWrappers == null)
                continue;

            // Base
            foreach (var wrapper in entry.FmgWrappers)
            {
                var internalName = TextUtils.GetFmgInternalName(editor.Project, entry, wrapper.ID, wrapper.Name);

                var proceed = false;

                if (internalName == annotationEntry.FmgName_Base)
                    proceed = true;

                if(proceed)
                {
                    foreach (var fmgEntry in wrapper.File.Entries)
                    {
                        entries.Add(fmgEntry);
                    }
                }
            }

            // DLC1/DLC2
            foreach (var wrapper in entry.FmgWrappers)
            {
                var internalName = TextUtils.GetFmgInternalName(editor.Project, entry, wrapper.ID, wrapper.Name);

                var proceed = false;

                if (internalName == annotationEntry.FmgName_DLC1)
                    proceed = true;

                if (internalName == annotationEntry.FmgName_DLC2)
                    proceed = true;

                if (proceed)
                {
                    foreach (var fmgEntry in wrapper.File.Entries)
                    {
                        // If we prefer base over DLC1/DLC2,
                        // don't add the DLC1/DLC2 entry if a base entry with the same ID already exists
                        if (CFG.Current.ParamEditor_Row_List_Row_FMG_Prefer_Base)
                        {
                            if (!entries.Any(e => e.ID == fmgEntry.ID))
                            {
                                entries.Add(fmgEntry);
                            }
                        }
                        else
                        {
                            entries.Add(fmgEntry);
                        }
                    }
                }
            }
        }

        return entries;
    }

    public static string GetFmgRefCommandLine(ParamEditorScreen editor, Dictionary<int, FMG.Entry> _entryCache, Param.Row row)
    {
        var category = CFG.Current.TextEditor_Primary_Category.ToString();
        if (_entryCache.Values.Count > 0)
        {
            var cachedEntry = _entryCache.Values.Where(e => e.ID == row.ID).FirstOrDefault();

            var containerName = "";
            var fmg = cachedEntry.Parent;
            var fmgName = fmg.Name;

            foreach (var (path, entry) in editor.Project.Handler.TextData.PrimaryBank.Containers)
            {
                if (entry.IsContainerUnused())
                    continue;

                if (entry.ContainerDisplayCategory != CFG.Current.TextEditor_Primary_Category)
                    continue;

                foreach (var fmgInfo in entry.FmgWrappers)
                {
                    var grouping = TextUtils.GetFmgGrouping(editor.Project, entry, fmgInfo.ID, fmgInfo.Name);

                    if (grouping == "Title" || grouping == "Common")
                    {
                        if (fmgInfo.Name == fmgName)
                        {
                            containerName = entry.FileEntry.Filename;
                        }
                    }
                }
            }

            return $@"text/select/{category}/{containerName}/{fmgName}/{row.ID}";
        }

        return "";
    }

    public static List<FMG.Entry> GetFmgEntriesByAssociatedParam(ParamEditorScreen editor, string paramName, string grouping = "")
    {
        if (editor.Project.Handler.TextEditor == null)
            return new List<FMG.Entry>();

        List<FMG.Entry> entries = new List<FMG.Entry>();

        var searchStr = GetDomainName(paramName);
        if (searchStr != "")
        {
            foreach (var (path, entry) in editor.Project.Handler.TextData.PrimaryBank.Containers)
            {
                if (entry.ContainerDisplayCategory == CFG.Current.TextEditor_Primary_Category)
                {
                    if (entry.FmgWrappers == null)
                        continue;

                    foreach (var fmgInfo in entry.FmgWrappers)
                    {
                        var curGrouping = TextUtils.GetFmgGrouping(editor.Project, entry, fmgInfo.ID, fmgInfo.Name);

                        if(grouping != "")
                        {
                            if (!(curGrouping == "Common" || curGrouping == grouping))
                                continue;
                        }

                        var enumName = TextUtils.GetFmgInternalName(editor.Project, entry, fmgInfo.ID, fmgInfo.Name);

                        if (enumName.Contains(searchStr))
                        {
                            foreach (var fmgEntry in fmgInfo.File.Entries)
                            {
                                entries.Add(fmgEntry);
                            }
                        }
                    }
                }
            }
        }

        return entries;
    }

    // TODO: use this one once the FMG descriptor stuff has been implemented
    //public static List<FMG.Entry> GetFmgEntriesByAssociatedParam_TODO(ParamEditorScreen editor, string paramName)
    //{
    //    if (editor.Project.Handler.TextEditor == null)
    //    {
    //        return new List<FMG.Entry>();
    //    }

    //    List<FMG.Entry> entries = new List<FMG.Entry>();

    //    var language = CFG.Current.TextEditor_Primary_Category;

    //    var domainString = GetDomainName(paramName);

    //    if (domainString != "")
    //    {
    //        foreach (var entry in editor.Project.Handler.TextData.PrimaryBank.Containers)
    //        {
    //            var container = entry.Value;

    //            if (container.Language.Language != language)
    //                continue;

    //            foreach (var fmgEntry in container.FmgWrappers)
    //            {
    //                // Is the Domain 
    //                if (fmgEntry.Descriptor.Domain == domainString)
    //                {
    //                    if (fmgEntry.Descriptor.Role == "None" ||
    //                        fmgEntry.Descriptor.Role == "Title")
    //                    {
    //                        foreach (var textEntry in fmgEntry.File.Entries)
    //                        {
    //                            entries.Add(textEntry);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    return entries;
    //}

    public static string GetDomainName(string paramName)
    {
        if (paramName == "EquipParamAccessory")
        {
            return "Accessories";
        }
        if (paramName == "EquipParamGoods")
        {
            return "Goods";
        }
        if (paramName == "EquipParamWeapon")
        {
            return "Weapons";
        }
        if (paramName == "Magic")
        {
            if (Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectType == ProjectType.DES)
            {
                return "Magic";
            }
            else
            {
                return "Goods";
            }
        }
        if (paramName == "EquipParamProtector")
        {
            return "Armor";
        }
        if (paramName == "EquipParamGem")
        {
            return "Ashes of War";
        }
        if (paramName == "SwordArtsParam")
        {
            return "Skills";
        }
        if (paramName == "EquipParamGenerator")
        {
            return "Generators";
        }
        if (paramName == "EquipParamFcs")
        {
            return "FCS";
        }
        if (paramName == "EquipParamBooster")
        {
            return "Boosters";
        }
        if (paramName == "MissionParam")
        {
            return "Mission Text";
        }
        if (paramName == "ArchiveParam")
        {
            return "Archive Text";
        }
        if (paramName == "EquipParamAntique")
        {
            return "Relics";
        }
        if (paramName == "AttachEffectParam")
        {
            return "Attached Effects";
        }

        if (paramName == "MessageBoxParamSystem")
        {
            return "System Messages";
        }
        if (paramName == "MessageBoxParamDialog")
        {
            return "Dialogue";
        }
        if (paramName == "PermanentBuffParam")
        {
            return "Permanent Buffs";
        }

        return "";
    }
}
