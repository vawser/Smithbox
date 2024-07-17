using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKLib.hk2018;
using StudioCore.HavokEditor;

namespace StudioCore.Editors.HavokEditor;

public class HavokBehaviorGraph
{
    private HavokEditorScreen Screen;

    public HavokBehaviorGraph(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void DisplayGraph()
    {
        ImGui.Begin("Behavior Graph##BehaviorGraph");

        if (Screen.CurrentBehaviorFile != null)
        {
            HKLib.hk2018.hkbBehaviorGraph behaviorGraph = (HKLib.hk2018.hkbBehaviorGraph)Screen.CurrentBehaviorFile.m_namedVariants[0].m_variant;

            if (ImGui.CollapsingHeader("hkbStateMachine"))
            {
                if (ImGui.TreeNodeEx("StateMachine"))
                {
                    // State Machines
                    hkbStateMachine hkbStateMachine = (hkbStateMachine)behaviorGraph.m_rootGenerator;
                    int depth = 0;
                    HavokStateMachineView(hkbStateMachine, depth);

                    ImGui.TreePop();
                }
            }

            if (ImGui.CollapsingHeader("General"))
            {
                ImGui.Text($"m_variableMode: {behaviorGraph.m_variableMode}");
            }

            // Information
            if (ImGui.CollapsingHeader("Attributes"))
            {
                // m_attributeDefaults
                for (int i = 0; i < behaviorGraph.m_data.m_attributeDefaults.Count; i++)
                {
                    ImGui.Text($"{i}: {behaviorGraph.m_data.m_attributeDefaults[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Variable Infos"))
            {
                // m_variableInfos
                for (int i = 0; i < behaviorGraph.m_data.m_variableInfos.Count; i++)
                {
                    ImGui.Text($"m_role: {i}: {behaviorGraph.m_data.m_variableInfos[i].m_role.m_role}");
                    ImGui.Text($"m_flags: {i}: {behaviorGraph.m_data.m_variableInfos[i].m_role.m_flags}");
                    ImGui.Text($"m_type: {i}: {behaviorGraph.m_data.m_variableInfos[i].m_type}");
                }
            }
            if (ImGui.CollapsingHeader("Character Property Infos"))
            {
                // m_characterPropertyInfos
                for (int i = 0; i < behaviorGraph.m_data.m_characterPropertyInfos.Count; i++)
                {
                    ImGui.Text($"m_role: {i}: {behaviorGraph.m_data.m_characterPropertyInfos[i].m_role.m_role}");
                    ImGui.Text($"m_flags: {i}: {behaviorGraph.m_data.m_characterPropertyInfos[i].m_role.m_flags}");
                    ImGui.Text($"m_type: {i}: {behaviorGraph.m_data.m_characterPropertyInfos[i].m_type}");
                }
            }
            if (ImGui.CollapsingHeader("Event Infos"))
            {
                // m_eventInfos
                for (int i = 0; i < behaviorGraph.m_data.m_eventInfos.Count; i++)
                {
                    ImGui.Text($"m_flags: {i}: {behaviorGraph.m_data.m_eventInfos[i].m_flags}");
                }
            }
            if (ImGui.CollapsingHeader("Variable Bounds"))
            {
                // m_variableBounds
                for (int i = 0; i < behaviorGraph.m_data.m_variableBounds.Count; i++)
                {
                    ImGui.Text($"m_min: {i}: {behaviorGraph.m_data.m_variableBounds[i].m_min.m_value}");
                    ImGui.Text($"m_max: {i}: {behaviorGraph.m_data.m_variableBounds[i].m_max.m_value}");
                }
            }
            if (ImGui.CollapsingHeader("Word Variable Values"))
            {
                // m_wordVariableValues
                for (int i = 0; i < behaviorGraph.m_data.m_variableInitialValues.m_wordVariableValues.Count; i++)
                {
                    ImGui.Text($"m_value: {i}: {behaviorGraph.m_data.m_variableInitialValues.m_wordVariableValues[i].m_value}");
                }
            }
            if (ImGui.CollapsingHeader("Quad Variable Values"))
            {
                // m_quadVariableValues
                for (int i = 0; i < behaviorGraph.m_data.m_variableInitialValues.m_quadVariableValues.Count; i++)
                {
                    ImGui.Text($"Vector4: {i}: {behaviorGraph.m_data.m_variableInitialValues.m_quadVariableValues[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Variant Variable Values"))
            {
                // m_variantVariableValues
                for (int i = 0; i < behaviorGraph.m_data.m_variableInitialValues.m_variantVariableValues.Count; i++)
                {
                    if (behaviorGraph.m_data.m_variableInitialValues.m_variantVariableValues[i] != null)
                        ImGui.Text($"m_propertyBag: {i}: {behaviorGraph.m_data.m_variableInitialValues.m_variantVariableValues[i].m_propertyBag}");
                }
            }
            if (ImGui.CollapsingHeader("Event Names"))
            {
                // m_eventNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_eventNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_eventNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Attribute Names"))
            {
                // m_attributeNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_attributeNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_attributeNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Variable Names"))
            {
                // m_variableNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_variableNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_variableNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Character Property Names"))
            {
                // m_characterPropertyNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_characterPropertyNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_characterPropertyNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Animation Names"))
            {
                // m_animationNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_animationNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_animationNames[i]}");
                }
            }
        }

        ImGui.End();
    }

    // Recursive
    public void HavokStateMachineView(hkbStateMachine stateMachine, int depth)
    {
        if (stateMachine != null)
        {
            if (ImGui.TreeNodeEx($"HavokStateMachineView##HavokStateMachineView{depth}"))
            {
                ImGui.Text($"m_eventToSendWhenStateOrTransitionChanges: {stateMachine.m_eventToSendWhenStateOrTransitionChanges}");
                ImGui.Text($"m_startStateIdSelector: {stateMachine.m_startStateIdSelector}");
                ImGui.Text($"m_startStateId: {stateMachine.m_startStateId}");
                ImGui.Text($"m_returnToPreviousStateEventId: {stateMachine.m_returnToPreviousStateEventId}");
                ImGui.Text($"m_randomTransitionEventId: {stateMachine.m_randomTransitionEventId}");
                ImGui.Text($"m_transitionToNextHigherStateEventId: {stateMachine.m_transitionToNextHigherStateEventId}");
                ImGui.Text($"m_transitionToNextLowerStateEventId: {stateMachine.m_transitionToNextLowerStateEventId}");
                ImGui.Text($"m_syncVariableIndex: {stateMachine.m_syncVariableIndex}");
                ImGui.Text($"m_wrapAroundStateId: {stateMachine.m_wrapAroundStateId}");
                ImGui.Text($"m_maxSimultaneousTransitions: {stateMachine.m_maxSimultaneousTransitions}");
                ImGui.Text($"m_startStateMode: {stateMachine.m_startStateMode}");
                ImGui.Text($"m_selfTransitionMode: {stateMachine.m_selfTransitionMode}");
                ImGui.Text($"m_wildcardTransitions: {stateMachine.m_wildcardTransitions}");

                foreach (var entry in stateMachine.m_states)
                {
                    hkbStateMachineStateInfoView(depth, entry);
                }

                if (stateMachine.m_wildcardTransitions != null)
                {
                    foreach (var wEntry in stateMachine.m_wildcardTransitions.m_transitions)
                    {
                        ImGui.Text($"m_triggerInterval: {wEntry.m_triggerInterval}");
                        ImGui.Text($"m_initiateInterval: {wEntry.m_initiateInterval}");
                        ImGui.Text($"m_transition: {wEntry.m_transition}");
                        ImGui.Text($"m_condition: {wEntry.m_condition}");
                        ImGui.Text($"m_eventId: {wEntry.m_eventId}");
                        ImGui.Text($"m_toStateId: {wEntry.m_toStateId}");
                        ImGui.Text($"m_fromNestedStateId: {wEntry.m_fromNestedStateId}");
                        ImGui.Text($"m_priority: {wEntry.m_priority}");
                        ImGui.Text($"m_flags: {wEntry.m_flags}");
                    }
                }

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbStateMachineStateInfoView(int depth, hkbStateMachine.StateInfo entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbStateMachineStateInfoView##hkbStateMachineStateInfoView{depth}"))
            {
                ImGui.Text($"m_listeners: {entry.m_listeners}");

                if (entry.m_enterNotifyEvents != null)
                {
                    foreach (var sEntry in entry.m_enterNotifyEvents.m_events)
                    {
                        ImGui.Text($"m_events: {sEntry}");
                    }
                }
                if (entry.m_exitNotifyEvents != null)
                {
                    foreach (var sEntry in entry.m_exitNotifyEvents.m_events)
                    {
                        ImGui.Text($"m_events: {sEntry}");
                    }
                }
                if (entry.m_transitions != null)
                {
                    foreach (var tEntry in entry.m_transitions.m_transitions)
                    {
                        ImGui.Text($"m_triggerInterval: {tEntry.m_triggerInterval}");
                        ImGui.Text($"m_initiateInterval: {tEntry.m_initiateInterval}");
                        ImGui.Text($"m_transition: {tEntry.m_transition}");
                        ImGui.Text($"m_condition: {tEntry.m_condition}");
                        ImGui.Text($"m_eventId: {tEntry.m_eventId}");
                        ImGui.Text($"m_toStateId: {tEntry.m_toStateId}");
                        ImGui.Text($"m_fromNestedStateId: {tEntry.m_fromNestedStateId}");
                        ImGui.Text($"m_priority: {tEntry.m_priority}");
                        ImGui.Text($"m_flags: {tEntry.m_flags}");
                    }
                }

                if (entry.m_generator is hkbScriptGenerator)
                {
                    var scriptGenerator = (hkbScriptGenerator)entry.m_generator;
                    hkbScriptGeneratorView(depth, scriptGenerator);
                }
                else if (entry.m_generator is hkbBlenderGenerator)
                {
                    var blenderGenerator = (hkbBlenderGenerator)entry.m_generator;
                    hkbBlenderGeneratorView(depth, blenderGenerator);
                }
                else
                {
                    TaskLogs.AddLog($"hkbStateMachineStateInfoView: Unimplemented type: {entry.m_generator.GetType()}");
                }

                ImGui.Text($"m_name: {entry.m_name}");
                ImGui.Text($"m_stateId: {entry.m_stateId}");
                ImGui.Text($"m_probability: {entry.m_probability}");
                ImGui.Text($"m_enable: {entry.m_enable}");

                ImGui.TreePop();
            }
        }
        depth++;
    }

    public void hkbScriptGeneratorView(int depth, hkbScriptGenerator entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbScriptGeneratorView##hkbScriptGeneratorView{depth}"))
            {
                ImGui.Text($"m_name: {entry.m_name}");

                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(depth, variableBindingSet);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");

                if (entry.m_child is hkbModifierGenerator)
                {
                    var child_generator = (hkbModifierGenerator)entry.m_child;
                    hkbModifierGeneratorView(depth, child_generator);
                }
                else
                {
                    TaskLogs.AddLog($"hkbScriptGeneratorView: Unimplemented type: {entry.m_child.GetType()}");
                }

                ImGui.Text($"m_onActivateScript: {entry.m_onActivateScript}");
                ImGui.Text($"m_onPreUpdateScript: {entry.m_onPreUpdateScript}");
                ImGui.Text($"m_onGenerateScript: {entry.m_onGenerateScript}");
                ImGui.Text($"m_onHandleEventScript: {entry.m_onHandleEventScript}");
                ImGui.Text($"m_onDeactivateScript: {entry.m_onDeactivateScript}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbBlenderGeneratorView(int depth, hkbBlenderGenerator entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbBlenderGeneratorView##hkbBlenderGeneratorView{depth}"))
            {
                ImGui.Text($"m_referencePoseWeightThreshold: {entry.m_referencePoseWeightThreshold}");
                ImGui.Text($"m_blendParameter: {entry.m_blendParameter}");
                ImGui.Text($"m_minCyclicBlendParameter: {entry.m_minCyclicBlendParameter}");
                ImGui.Text($"m_maxCyclicBlendParameter: {entry.m_maxCyclicBlendParameter}");
                ImGui.Text($"m_indexOfSyncMasterChild: {entry.m_indexOfSyncMasterChild}");
                ImGui.Text($"m_flags: {entry.m_flags}");
                ImGui.Text($"m_subtractLastChild: {entry.m_subtractLastChild}");

                foreach (hkbBlenderGeneratorChild cEntry in entry.m_children)
                {
                    hkbBlenderGeneratorChildView(depth, cEntry);
                }

                ImGui.TreePop();
            }
        }
        depth++;
    }

    public void hkbBlenderGeneratorChildView(int depth, hkbBlenderGeneratorChild entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbBlenderGeneratorChildView##hkbBlenderGeneratorChildView{depth}"))
            {
                if (entry.m_generator is hkbScriptGenerator)
                {
                    var scriptGenerator = (hkbScriptGenerator)entry.m_generator;
                    hkbScriptGeneratorView(depth, scriptGenerator);
                }
                else if (entry.m_generator is hkbBlenderGenerator)
                {
                    var blenderGenerator = (hkbBlenderGenerator)entry.m_generator;
                    hkbBlenderGeneratorView(depth, blenderGenerator);
                }
                else
                {
                    TaskLogs.AddLog($"hkbStateMachineStateInfoView: Unimplemented type: {entry.m_generator.GetType()}");
                }

                foreach (var bEntry in entry.m_boneWeights.m_boneWeights)
                {
                    ImGui.Text($"m_boneWeight: {bEntry}");
                }

                ImGui.Text($"m_weight: {entry.m_weight}");
                ImGui.Text($"m_worldFromModelWeight: {entry.m_worldFromModelWeight}");

                ImGui.TreePop();
            }
        }
        depth++;
    }

    public void variableBindingSetView(int depth, hkbVariableBindingSet entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"variableBindingSetView##variableBindingSetView{depth}"))
            {
                foreach (var bEntry in entry.m_bindings)
                {
                    ImGui.Text($"m_memberPath: {bEntry.m_memberPath}");
                    ImGui.Text($"m_variableIndex: {bEntry.m_variableIndex}");
                    ImGui.Text($"m_bitIndex: {bEntry.m_bitIndex}");
                    ImGui.Text($"m_bindingType: {bEntry.m_bindingType}");
                }

                ImGui.Text($"m_indexOfBindingToEnable: {entry.m_indexOfBindingToEnable}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbModifierGeneratorView(int depth, hkbModifierGenerator entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbModifierGeneratorView##hkbModifierGeneratorView{depth}"))
            {
                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(depth, variableBindingSet);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");

                if (entry.m_modifier is hkbModifierList)
                {
                    var modifierList = (hkbModifierList)entry.m_modifier;

                    if (modifierList != null)
                        hkbModifierListView(depth, modifierList);
                }
                else
                {
                    TaskLogs.AddLog($"hkbModifierGenerator: Unimplemented type: {entry.m_modifier.GetType()}");
                }

                var scriptGenerator = (hkbScriptGenerator)entry.m_generator;

                if (scriptGenerator != null)
                    hkbScriptGeneratorView(depth, scriptGenerator);

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbModifierListView(int depth, hkbModifierList entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbModifierListView##hkbModifierListView{depth}"))
            {
                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(depth, variableBindingSet);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");
                ImGui.Text($"m_enable: {entry.m_enable}");

                foreach (var mEntry in entry.m_modifiers)
                {
                    if (mEntry is hkbTwistModifier)
                    {
                        hkbTwistModifierView(depth, (hkbTwistModifier)mEntry);
                    }
                    else if (mEntry is hkbModifierList)
                    {
                        hkbModifierListView(depth, (hkbModifierList)mEntry);
                    }
                    else
                    {
                        TaskLogs.AddLog($"hkbModifierListView: Unimplemented type: {mEntry.GetType()}");
                    }
                }

                ImGui.TreePop();
            }
        }

        depth++;
    }
    public void hkbTwistModifierView(int depth, hkbTwistModifier entry)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbTwistModifierView##hkbTwistModifierView{depth}"))
            {
                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(depth, variableBindingSet);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");
                ImGui.Text($"m_enable: {entry.m_enable}");
                ImGui.Text($"m_axisOfRotation: {entry.m_axisOfRotation}");
                ImGui.Text($"m_twistAngle: {entry.m_twistAngle}");
                ImGui.Text($"m_startBoneIndex: {entry.m_startBoneIndex}");
                ImGui.Text($"m_endBoneIndex: {entry.m_endBoneIndex}");
                ImGui.Text($"m_setAngleMethod: {entry.m_setAngleMethod}");
                ImGui.Text($"m_rotationAxisCoordinates: {entry.m_rotationAxisCoordinates}");
                ImGui.Text($"m_isAdditive: {entry.m_isAdditive}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

}
