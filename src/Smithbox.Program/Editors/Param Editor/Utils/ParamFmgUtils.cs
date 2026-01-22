using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamFmgUtils
{
    public static List<FMG.Entry> GetRowDecoratorFmgEntries(ParamEditorScreen editor, string paramName)
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
                    if (entry.FmgWrappers != null)
                    {
                        foreach (var fmgInfo in entry.FmgWrappers)
                        {
                            var grouping = TextUtils.GetFmgGrouping(editor.Project, entry, fmgInfo.ID, fmgInfo.Name);

                            if (grouping == "Title" || grouping == "Common")
                            {
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
            }
        }

        return entries;
    }

    public static List<FMG.Entry> GetFmgEntriesByAssociatedParam(ParamEditorScreen editor, string paramName)
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
                    if (entry.FmgWrappers != null)
                    {
                        foreach (var fmgInfo in entry.FmgWrappers)
                        {
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
            return "Spells";
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
