using Andre.IO.VFS;
using StudioCore.Core;
using StudioCore.FileBrowserNS;
using StudioCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Formats.JSON;
using Google.Protobuf.Collections;
using Octokit;
using StudioCore.Resource.Locators;

namespace VisualDataNS;

/// <summary>
/// Special class that holds the file dictionaries for all data used by Map/Model editor for rendering
/// i.e. models and textures
/// </summary>
public class VisualData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary CharacterModels = new();

    public FileDictionary AssetModels = new();

    public FileDictionary PartModels = new();

    public FileDictionary MapPieceModels = new();


    public FileDictionary CharacterTextures = new();

    public FileDictionary AssetTextures = new();

    public FileDictionary PartTextures = new();

    public FileDictionary MapPieceTextures = new();

    public FileDictionary MiscTextures = new();


    public FileDictionary Collisions = new();

    public FileDictionary Navmeshes = new();

    public VisualData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        CharacterModels.Entries = new();
        AssetModels.Entries = new();
        PartModels.Entries = new();
        MapPieceModels.Entries = new();

        CharacterTextures.Entries = new();
        AssetTextures.Entries = new();
        PartTextures.Entries = new();
        MapPieceTextures.Entries = new();
        MiscTextures.Entries = new();

        Collisions.Entries = new();
        Navmeshes.Entries = new();

        // Character Models
        switch (Project.ProjectType)
        {
            case ProjectType.DS2:
            case ProjectType.DS2S:
                CharacterModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/chr")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            default:
                CharacterModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/chr")
                .Where(e => e.Extension == "chrbnd")
                .ToList();
                break;
        }

        // Object Models
        switch (Project.ProjectType)
        {
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.ACFA:
            case ProjectType.ACV:
            case ProjectType.ACVD:
                AssetModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/obj")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            case ProjectType.ER:
            case ProjectType.NR:
                AssetModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder.Contains("/asset/aeg"))
                .Where(e => e.Extension == "geombnd")
                .ToList();
                break;

            case ProjectType.AC6:
                AssetModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/asset/environment/geometry")
                .Where(e => e.Extension == "geombnd")
                .ToList();
                break;

            default:
                AssetModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/obj")
                .Where(e => e.Extension == "objbnd")
                .ToList();
                break;
        }

        // Part Models
        switch (Project.ProjectType)
        {
            case ProjectType.DS2:
            case ProjectType.DS2S:
                PartModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/parts")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            default:
                PartModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/parts")
                .Where(e => e.Extension == "partsbnd")
                .ToList();
                break;
        }


        // Map Piece Models
        switch (Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.BB:
                MapPieceModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "flver")
                .ToList();
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                MapPieceModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "mapbhd")
                .ToList();
                break;
            case ProjectType.DS3:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.NR:
            case ProjectType.AC6:
                MapPieceModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "mapbnd")
                .ToList();
                break;
            case ProjectType.ACFA:
                MapPieceModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;
            case ProjectType.ACV:
            case ProjectType.ACVD:
                MapPieceModels.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;
        }

        // Collision Models
        switch (Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
                Collisions.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "hkx")
                .ToList();
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                Collisions.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "hkxbhd")
                .ToList();
                break;
            case ProjectType.BB:
            case ProjectType.DS3:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.NR:
            case ProjectType.AC6:
                Collisions.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "hkxbhd")
                .ToList();
                break;
        }

        // Navmesh Models
        switch (Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
                Navmeshes.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "nvm")
                .ToList();
                break;

            default:
                Navmeshes.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "nvmhktbnd")
                .ToList();
                break;
        }

        // Character Textures
        switch (Project.ProjectType)
        {
            case ProjectType.DES:
                CharacterTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/chr")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                CharacterTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/chr")
                .Where(e => e.Extension == "texbnd")
                .ToList();
                break;

            case ProjectType.DS3:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.NR:
            case ProjectType.AC6:
                CharacterTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/chr")
                .Where(e => e.Extension == "texbnd")
                .ToList();
                break;

            default:
                var bndDict = new FileDictionary();
                bndDict.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/chr")
                .Where(e => e.Extension == "chrbnd")
                .ToList();

                var tpfDict = new FileDictionary();
                tpfDict.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/chr")
                .Where(e => e.Extension == "tpf")
                .ToList();

                CharacterTextures = ProjectUtils.MergeFileDictionaries(bndDict, tpfDict);
                break;
        }

        // Object Textures
        switch (Project.ProjectType)
        {
            case ProjectType.AC6:
                AssetTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/asset/environment/texture")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;

            case ProjectType.ER:
            case ProjectType.NR:
                AssetTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/asset/aet")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                AssetTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/obj")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            case ProjectType.ACFA:
                AssetTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/obj")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            case ProjectType.ACV:
            case ProjectType.ACVD:
                AssetTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/obj")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;

            default:
                AssetTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/obj")
                .Where(e => e.Extension == "objbnd")
                .ToList();
                break;
        }

        // Part Textures
        switch (Project.ProjectType)
        {
            case ProjectType.DS2:
            case ProjectType.DS2S:
                CharacterTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/parts")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            default:
                var bndDict = new FileDictionary();
                bndDict.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/parts")
                .Where(e => e.Extension == "partsbnd")
                .ToList();

                var tpfDict = new FileDictionary();
                tpfDict.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/parts")
                .Where(e => e.Extension == "tpf")
                .ToList();

                PartTextures = ProjectUtils.MergeFileDictionaries(bndDict, tpfDict);
                break;
        }

        // Map Piece Textures
        switch (Project.ProjectType)
        {
            case ProjectType.DES:
                MapPieceTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                MapPieceTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "tpfbhd")
                .ToList();
                break;
            case ProjectType.ACFA:
                MapPieceTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;
            case ProjectType.ACV:
                MapPieceTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;
            case ProjectType.ACVD:
                MapPieceTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/model/map")
                .Where(e => e.Extension == "bnd")
                .ToList();
                break;

            default:
                MapPieceTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/map")
                .Where(e => e.Extension == "tpfbhd")
                .ToList();
                break;
        }

        // Misc Textures
        switch (Project.ProjectType)
        {
            default:
                MiscTextures.Entries = Project.FileDictionary.Entries
                .Where(e => e.Folder == "/other")
                .Where(e => e.Extension == "tpf")
                .ToList();
                break;
        }

        return true;
    }

    //// Character Model
    //public ResourceDescriptor PrepareCharacterModel(string chrID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = CharacterModels.Entries
    //        .FirstOrDefault(e => e.Filename == chrID);

    //    resDesc.InnerPath = chrID;

    //    return resDesc;
    //}

    //// Character Texture
    //public ResourceDescriptor PrepareCharacterTexture(string chrID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = CharacterTextures.Entries
    //        .FirstOrDefault(e => e.Filename == chrID);

    //    resDesc.InnerPath = chrID;

    //    return resDesc;
    //}

    //// Object Model
    //public ResourceDescriptor PrepareAssetModel(string objID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = AssetModels.Entries
    //        .FirstOrDefault(e => e.Filename == objID);

    //    resDesc.InnerPath = objID;

    //    return resDesc;
    //}

    //// Object Texture
    //public ResourceDescriptor PrepareAssetTexture(string objID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = AssetTextures.Entries
    //        .FirstOrDefault(e => e.Filename == objID);

    //    resDesc.InnerPath = objID;

    //    return resDesc;
    //}

    //// Map Piece Model
    //public ResourceDescriptor PrepareMapModel(string modelID, string mapID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = MapPieceModels.Entries
    //        .FirstOrDefault(e => e.Filename == modelID && e.Folder.Contains(mapID));

    //    resDesc.InnerPath = modelID;

    //    return resDesc;
    //}

    //public List<ResourceDescriptor> PrepareCurrentMapTextures(string mapID)
    //{
    //    var newList = new List<ResourceDescriptor>();

    //    var mid = mapID.Substring(0, 3);

    //    switch (Project.ProjectType)
    //    {
    //        case ProjectType.DES:
    //            foreach (var entry in MapPieceTextures.Entries)
    //            {
    //                if (entry.Folder.Contains(mid))
    //                {
    //                    var match = PrepareMapTexture(entry.Filename, mapID);
    //                    newList.Add(match);
    //                }
    //            }
    //            break;

    //        case ProjectType.DS2:
    //        case ProjectType.DS2S:
    //            foreach (var entry in MapPieceTextures.Entries)
    //            {
    //                if (entry.Filename.Contains(mapID.Substring(1)))
    //                {
    //                    var match = PrepareMapTexture(entry.Filename, mapID);
    //                    newList.Add(match);
    //                }
    //            }
    //            break;

    //        default:
    //            foreach (var entry in MapPieceTextures.Entries)
    //            {
    //                if(entry.Folder.Contains(mid))
    //                {
    //                    var match = PrepareMapTexture(entry.Filename, mapID);
    //                    newList.Add(match);
    //                }
    //            }

    //            break;
    //    }

    //    return newList;
    //}

    //public ResourceDescriptor PrepareMapTexture(string modelID, string mapID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = MapPieceModels.Entries
    //        .FirstOrDefault(e => e.Filename == modelID && e.Folder.Contains(mapID));

    //    resDesc.InnerPath = modelID;

    //    return resDesc;
    //}

    //// Collision Model
    //public ResourceDescriptor PrepareCollisionModel(string modelID, string mapID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = Collisions.Entries
    //        .FirstOrDefault(e => e.Filename == modelID && e.Folder.Contains(mapID));

    //    resDesc.InnerPath = modelID;

    //    return resDesc;
    //}

    //// Navmesh Model
    //public ResourceDescriptor PrepareNavmeshModel(string modelID, string mapID)
    //{
    //    var resDesc = new ResourceDescriptor();
    //    resDesc.FileEntry = Navmeshes.Entries
    //        .FirstOrDefault(e => e.Filename == modelID && e.Folder.Contains(mapID));

    //    resDesc.InnerPath = modelID;

    //    return resDesc;
    //}
}
