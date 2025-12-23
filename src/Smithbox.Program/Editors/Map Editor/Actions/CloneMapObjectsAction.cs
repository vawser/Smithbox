using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;


public class CloneMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MsbEntity> Clones = new();
    private readonly bool SetSelection;
    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;
    private readonly bool Silent = false;

    public CloneMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects, bool setSelection, MapContainer targetMap = null, Entity targetBTL = null, bool silent = false)
    {
        Editor = editor;
        Clonables.AddRange(objects);
        SetSelection = setSelection;
        TargetMap = targetMap;
        TargetBTL = targetBTL;
        Silent = silent;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        var clonesCached = Clones.Count() > 0;

        var objectnames = new Dictionary<string, HashSet<string>>();
        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        for (var i = 0; i < Clonables.Count(); i++)
        {
            if (Clonables[i].MapID == null)
            {
                if (!Silent)
                {
                    TaskLogs.AddLog($"Failed to dupe {Clonables[i].Name}, as it had no defined MapID",
                        LogLevel.Warning);
                }
                continue;
            }

            MapContainer m;
            if (TargetMap != null)
            {
                m = Editor.Selection.GetMapContainerFromMapID(TargetMap.Name);
            }
            else
            {
                m = Editor.Selection.GetMapContainerFromMapID(Clonables[i].MapID);
            }

            if (m != null)
            {
                // Get list of names that exist so our duplicate names don't trample over them
                if (!objectnames.ContainsKey(Clonables[i].MapID))
                {
                    var nameset = new HashSet<string>();
                    foreach (Entity n in m.Objects)
                    {
                        nameset.Add(n.Name);
                    }

                    objectnames.Add(Clonables[i].MapID, nameset);
                }

                // If this was executed in the past we reused the cloned objects so because redo
                // actions that follow this may reference the previously cloned object
                MsbEntity newobj = clonesCached ? Clones[i] : (MsbEntity)Clonables[i].Clone();

                // Persist the supports name bool so duplicates don't show null names when the source ent has it set to false
                if(!clonesCached && !Clonables[i].SupportsName)
                {
                    newobj.SupportsName = false;
                }
                else if(clonesCached && !Clones[i].SupportsName)
                {
                    newobj.SupportsName = false;
                }

                // Use pattern matching to attempt renames based on appended ID
                Match idmatch = TrailIDRegex.Match(Clonables[i].Name);
                if (idmatch.Success)
                {
                    var idstring = idmatch.Result("${id}");
                    var id = int.Parse(idstring);
                    var newid = idstring;
                    while (objectnames[Clonables[i].MapID]
                           .Contains(Clonables[i].Name.Substring(0, Clonables[i].Name.Length - idstring.Length) +
                                     newid))
                    {
                        id++;
                        newid = id.ToString("D" + idstring.Length);
                    }

                    newobj.Name = Clonables[i].Name.Substring(0, Clonables[i].Name.Length - idstring.Length) +
                                  newid;
                    objectnames[Clonables[i].MapID].Add(newobj.Name);
                }
                else
                {
                    var idstring = "0001";
                    var id = int.Parse(idstring);
                    var newid = idstring;
                    while (objectnames[Clonables[i].MapID].Contains(Clonables[i].Name + "_" + newid))
                    {
                        id++;
                        newid = id.ToString("D" + idstring.Length);
                    }

                    newobj.Name = Clonables[i].Name + "_" + newid;
                    objectnames[Clonables[i].MapID].Add(newobj.Name);
                }

                if (TargetMap == null)
                {
                    m.Objects.Insert(m.Objects.IndexOf(Clonables[i]) + 1, newobj);
                }
                else
                {
                    m.Objects.Add(newobj);
                }

                if (TargetBTL != null && newobj.WrappedObject is BTL.Light)
                {
                    TargetBTL.AddChild(newobj);
                }
                else if (TargetMap != null)
                {
                    // Duping to a targeted map, update parent.
                    if (TargetMap.MapOffsetNode != null)
                    {
                        TargetMap.MapOffsetNode.AddChild(newobj);
                    }
                    else
                    {
                        TargetMap.RootObject.AddChild(newobj);
                    }
                }
                else if (Clonables[i].Parent != null)
                {
                    var idx = Clonables[i].Parent.ChildIndex(Clonables[i]);
                    Clonables[i].Parent.AddChild(newobj, idx + 1);
                }

                if (TargetMap != null)
                {
                    TaskLogs.AddLog($"Duplicated {newobj.Name} to {TargetMap.Name}");
                }

                if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                {
                    MapEditorActionHelper.SetUniqueEntityID(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Increment_InstanceID)
                {
                    MapEditorActionHelper.SetUniqueInstanceID(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Increment_PartNames)
                {
                    MapEditorActionHelper.SetSelfPartNames(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                {
                    MapEditorActionHelper.ClearEntityID(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs)
                {
                    MapEditorActionHelper.ClearEntityGroupID(Editor, newobj, m);
                }

                newobj.UpdateRenderModel(Editor);
                if (newobj.RenderSceneMesh != null)
                {
                    newobj.RenderSceneMesh.SetSelectable(newobj);
                }

                if (!clonesCached)
                {
                    Clones.Add(newobj);
                    CloneMaps.Add(m);
                    m.HasUnsavedChanges = true;
                }
                else
                {
                    if (Clones[i].RenderSceneMesh != null)
                    {
                        Clones[i].RenderSceneMesh.AutoRegister = true;
                        Clones[i].RenderSceneMesh.Register();
                    }
                }
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity c in Clones)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

        for (var i = 0; i < Clones.Count(); i++)
        {
            CloneMaps[i].Objects.Remove(Clones[i]);
            if (Clones[i] != null)
            {
                Clones[i].Parent.RemoveChild(Clones[i]);
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        // Clones.Clear();
        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity c in Clonables)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
