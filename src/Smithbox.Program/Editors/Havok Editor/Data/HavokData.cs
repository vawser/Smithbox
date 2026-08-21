using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokData : IDisposable
{
    public ProjectEntry Project;

    public HavokAnimationBank AnimationBank;
    public HavokBehaviorBank BehaviorBank;
    public HavokCharacterBank CharacterBank;
    public HavokCollisionBank CollisionBank;
    public HavokCutsceneBank CutsceneBank;
    public HavokNavmeshBank NavmeshBank;
    public HavokPartBank PartBank;
    public HavokRumbleBank RumbleBank;

    public HavokData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        AnimationBank = new(Project);
        BehaviorBank = new(Project);
        CharacterBank = new(Project);
        CollisionBank = new(Project);
        CutsceneBank = new(Project);
        NavmeshBank = new(Project);
        PartBank = new(Project);
        RumbleBank = new(Project);

        // Animation Bank
        Task<bool> animationBankTask = AnimationBank.Setup();
        bool animationBankTaskResult = await animationBankTask;

        if (!animationBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Animation_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Animation_Bank_PASS"));
        }

        // Behavior Bank
        Task<bool> behaviorBankTask = BehaviorBank.Setup();
        bool behaviorBankTaskResult = await behaviorBankTask;

        if (!behaviorBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Behavior_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Behavior_Bank_PASS"));
        }

        // Character Bank
        Task<bool> characterBankTask = CharacterBank.Setup();
        bool characterBankTaskResult = await characterBankTask;

        if (!characterBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Character_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Character_Bank_PASS"));
        }

        // Collision Bank
        Task<bool> collisionBankTask = CollisionBank.Setup();
        bool collisionBankTaskResult = await collisionBankTask;

        if (!collisionBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Collision_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Collision_Bank_PASS"));
        }

        // Cutscene Bank
        Task<bool> cutsceneBankTask = CutsceneBank.Setup();
        bool cutsceneBankTaskResult = await cutsceneBankTask;

        if (!cutsceneBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Cutscene_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Cutscene_Bank_PASS"));
        }

        // Navmesh Bank
        Task<bool> navmeshBankTask = NavmeshBank.Setup();
        bool navmeshBankTaskResult = await navmeshBankTask;

        if (!navmeshBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Navmesh_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Navmesh_Bank_PASS"));
        }

        // Part Bank
        Task<bool> partBankTask = PartBank.Setup();
        bool partBankTaskResult = await partBankTask;

        if (!partBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Part_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Part_Bank_PASS"));
        }

        // Rumble Bank
        Task<bool> rumbleBankTask = RumbleBank.Setup();
        bool rumbleBankTaskResult = await rumbleBankTask;

        if (!animationBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Setup_Rumble_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Data_Setup_Rumble_Bank_PASS"));
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        AnimationBank?.Dispose();
        BehaviorBank?.Dispose();
        CharacterBank?.Dispose();
        CollisionBank?.Dispose();
        CutsceneBank?.Dispose();
        NavmeshBank?.Dispose();
        PartBank?.Dispose();
        RumbleBank?.Dispose();

        AnimationBank = null;
        BehaviorBank = null;
        CharacterBank = null;
        CollisionBank = null;
        CutsceneBank = null;
        NavmeshBank = null;
        PartBank = null;
        RumbleBank = null;
    }
    #endregion

}
