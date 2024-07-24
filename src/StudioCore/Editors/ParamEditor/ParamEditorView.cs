using Andre.Formats;
using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorView
{
    private readonly ParamRowEditor _propEditor;
    private readonly Dictionary<string, string> lastRowSearch = new();
    private bool _arrowKeyPressed;
    private bool _focusRows;
    private int _gotoParamRow = -1;
    private bool _mapParamView;
    private bool _eventParamView;
    private bool _gConfigParamView;

    internal ParamEditorScreen _paramEditor;

    public ParamToolbar _paramToolbar;
    public ParamToolbar_ActionList _paramToolbar_ActionList;
    public ParamToolbar_Configuration _paramToolbar_Configuration;

    internal ParamEditorSelectionState _selection;
    internal int _viewIndex;

    private string lastParamSearch = "";

    public ParamEditorView(ParamEditorScreen parent, int index)
    {
        _paramEditor = parent;
        _viewIndex = index;
        _propEditor = new ParamRowEditor(parent.EditorActionManager, _paramEditor);
        _selection = new ParamEditorSelectionState(_paramEditor);

        // Toolbar
        _paramToolbar = new ParamToolbar(parent.EditorActionManager);
        _paramToolbar_ActionList = new ParamToolbar_ActionList();
        _paramToolbar_Configuration = new ParamToolbar_Configuration();
    }

    //------------------------------------
    // Param
    //------------------------------------
    private void ParamView_ParamList_Header(bool isActiveView)
    {
        ImGui.Text("参数 Params");
        ImGui.Separator();

        if (ParamBank.PrimaryBank.ParamVersion != 0)
        {
            ImGui.Text($"参数版本 Param version {Utils.ParseParamVersion(ParamBank.PrimaryBank.ParamVersion)}");

            if (ParamBank.PrimaryBank.ParamVersion < ParamBank.VanillaBank.ParamVersion)
            {
                ImGui.SameLine();
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Warning_Text_Color, "(已过时 out of date)");
            }
        }

        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_SearchParam))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.InputText($"搜索 Search <{KeyBindings.Current.Param_SearchParam.HintText}>",
            ref _selection.currentParamSearchString, 256);
        var resAutoParam = AutoFill.ParamSearchBarAutoFill();

        if (resAutoParam != null)
        {
            _selection.SetCurrentParamSearchString(resAutoParam);
        }

        if (!_selection.currentParamSearchString.Equals(lastParamSearch))
        {
            UICache.ClearCaches();
            lastParamSearch = _selection.currentParamSearchString;
        }

        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            // This game has DrawParams, add UI element to toggle viewing DrawParam and GameParams.
            if (ImGui.Checkbox("编辑绘制参数 Edit Drawparams", ref _mapParamView))
            {
                UICache.ClearCaches();
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            // DS2 has map params, add UI element to toggle viewing map params and GameParams.
            if (ImGui.Checkbox("编辑地图参数 Edit Map Params", ref _mapParamView))
            {
                UICache.ClearCaches();
            }
        }
        else if (Smithbox.ProjectType is ProjectType.ER || Smithbox.ProjectType is ProjectType.AC6)
        {
            // Only show if the user actually has the eventparam file
            if (Path.Exists($"{Smithbox.GameRoot}\\param\\eventparam\\eventparam.parambnd.dcx"))
            {
                if (ImGui.Checkbox("编辑事件参数 Edit Event Params", ref _eventParamView))
                {
                    _gConfigParamView = false;
                    UICache.ClearCaches();
                }
            }

            if (ImGui.Checkbox("编辑图样参数 Edit Graphics Config Params", ref _gConfigParamView))
            {
                _eventParamView = false;
                UICache.ClearCaches();
            }
        }

        ImGui.Separator();
    }

    private void ParamView_ParamList_Pinned(float scale)
    {
        List<string> pinnedParamKeyList = new(Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            //ImGui.Text("        Pinned Params");
            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = ParamBank.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
                Param p = ParamBank.PrimaryBank.Params[paramKey];
                if (p != null)
                {
                    var meta = ParamMetaData.Get(p.AppliedParamdef);
                    var Wiki = meta?.Wiki;
                    if (Wiki != null)
                    {
                        if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                        {
                            meta.Wiki = Wiki;
                        }
                    }
                }

                ImGui.Indent(15.0f * scale);
                if (ImGui.Selectable($"{paramKey}##pin{paramKey}", paramKey == _selection.GetActiveParam()))
                {
                    EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable("解 Unpin " + paramKey))
                    {
                        Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.Remove(paramKey);
                    }

                    EditorDecorations.PinListReorderOptions(Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams, paramKey);

                    if (ImGui.Selectable("全解 Unpin all"))
                    {
                        Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.RemoveAll(x => true);
                    }

                    ImGui.EndPopup();
                }

                ImGui.Unindent(15.0f * scale);
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    private static Dictionary<string, string> paramChs;
    private static List<string> paramKeyList;
    private void ParamView_ParamList_Main(bool doFocus, float scale, float scrollTo)
    {
        if(paramChs == null)
        {
            
            paramChs = new Dictionary<string, string>
            {
                {"ActionButtonParam", "交互按钮参数"},
                {"AiSoundParam", "AI声音参数"},
                {"AssetEnvironmentGeometryParam", "地图物体参数"},
                {"AssetMaterialSfxParam", "地图物体材质特效参数"},
                {"AssetModelSfxParam", "地图物体模型特效参数"},
                {"AtkParam_Npc", "伤害判定atk-非褪色者"},
                {"AtkParam_Pc", "伤害判定atk-褪色者"},
                {"AttackElementCorrectParam", "伤害补正参数"},
                {"AutoCreateEnvSoundParam", "自动生成环境声音参数"},
                {"BaseChrSelectMenuParam", "玩家出身界面参数"},
                {"BehaviorParam", "行为调用组-非褪色者"},
                {"BehaviorParam_PC", "行为调用组-褪色者"},
                {"BonfireWarpParam", "赐福点参数"},
                {"BonfireWarpSubCategoryParam", "赐福点子类别参数"},
                {"BonfireWarpTabParam", "赐福点类别参数"},
                {"BuddyParam", "骨灰同伴参数"},
                {"BuddyStoneParam", "骨灰召唤区域参数"},
                {"BudgetParam", "预算参数"},
                {"Bullet", "子弹"},
                {"BulletCreateLimitParam", "子弹生成限制参数"},
                {"CalcCorrectGraph", "数值增长曲线表"},
                {"CameraFadeParam", "相机淡出参数"},
                {"Ceremony", "地图仪式"},
                {"CharaInitParam", "褪色者预设模板参数"},
                {"CharMakeMenuListItemParam", "玩家出身界面选项参数"},
                {"CharMakeMenuTopParam", "玩家出身界面栏目参数"},
                {"ChrActivateConditionParam", "实体激活条件参数"},
                {"ChrEquipModelParam", "角色装备模型参数"},
                {"ChrModelParam", "角色模型参数"},
                {"ClearCountCorrectParam", "游戏周目加成设置"},
                {"CommonSystemParam", "通用系统参数"},
                {"CoolTimeParam", "冷却时间参数"},
                {"CutsceneGparamTimeParam", "过场动画G参数时间"},
                {"CutsceneGparamWeatherParam", "过场动画G参数天气"},
                {"CutsceneMapIdParam", "过场动画地图ID参数"},
                {"CutSceneTextureLoadParam", "过场动画纹理加载参数"},
                {"CutsceneTimezoneConvertParam", "过场动画时区转换参数"},
                {"CutsceneWeatherOverrideGparamConvertParam", "过场动画天气覆盖G参数转换参数"},
                {"DecalParam", "贴花参数"},
                {"DefaultKeyAssign", "默认键位分配"},
                {"DirectionCameraParam", "方向相机参数"},
                {"EnemyCommonParam", "敌人通用参数"},
                {"EnvObjLotParam", "环境对象生成参数"},
                {"EquipMtrlSetParam", "材料需求表"},
                {"EquipParamAccessory", "护符"},
                {"EquipParamCustomWeapon", "特制武器-npc用"},
                {"EquipParamGem", "战灰"},
                {"EquipParamGoods", "道具"},
                {"EquipParamProtector", "装备"},
                {"EquipParamWeapon", "武器"},
                {"FaceParam", "面部参数"},
                {"FaceRangeParam", "面部范围参数"},
                {"FeTextEffectParam", "FE横幅播报文本效果"},
                {"FinalDamageRateParam", "最终伤害率参数"},
                {"FootSfxParam", "脚步音效参数"},
                {"GameAreaParam", "游戏区域参数"},
                {"GameSystemCommonParam", "游戏系统通用参数"},
                {"GestureParam", "表情对应动作表"},
                {"GparamRefSettings", "G参数参考设置"},
                {"GraphicsCommonParam", "图形通用参数"},
                {"GraphicsConfig", "图形配置"},
                {"GrassLodRangeParam", "草地Lod范围参数"},
                {"GrassTypeParam", "草地类型参数"},
                {"GrassTypeParam_Lv1", "草地类型参数_等级1"},
                {"GrassTypeParam_Lv2", "草地类型参数_等级2"},
                {"HitEffectSeParam", "击中效果SE参数"},
                {"HitEffectSfxConceptParam", "击中效果SFX概念参数"},
                {"HitEffectSfxParam", "击中效果SFX参数"},
                {"HitMtrlParam", "击中材质参数"},
                {"HPEstusFlaskRecoveryParam", "特定单位击杀后恢复血瓶设置"},
                {"ItemLotParam_enemy", "敌人掉落参数"},
                {"ItemLotParam_map", "物品获取表"},
                {"KeyAssignMenuItemParam", "键位分配菜单项参数"},
                {"KeyAssignParam_TypeA", "键位分配参数_类型A"},
                {"KeyAssignParam_TypeB", "键位分配参数_类型B"},
                {"KeyAssignParam_TypeC", "键位分配参数_类型C"},
                {"KnockBackParam", "击退参数"},
                {"KnowledgeLoadScreenItemParam", "知识加载屏幕项参数"},
                {"LegacyDistantViewPartsReplaceParam", "遗留远景部件替换参数"},
                {"LoadBalancerDrawDistScaleParam", "负载均衡器绘制距离缩放参数"},
                {"LoadBalancerDrawDistScaleParam_ps4", "负载均衡器绘制距离缩放参数_PS4"},
                {"LoadBalancerDrawDistScaleParam_ps5", "负载均衡器绘制距离缩放参数_PS5"},
                {"LoadBalancerDrawDistScaleParam_xb1", "负载均衡器绘制距离缩放参数_XB1"},
                {"LoadBalancerDrawDistScaleParam_xb1x", "负载均衡器绘制距离缩放参数_XB1X"},
                {"LoadBalancerDrawDistScaleParam_xss", "负载均衡器绘制距离缩放参数_XSS"},
                {"LoadBalancerDrawDistScaleParam_xsx", "负载均衡器绘制距离缩放参数_XSX"},
                {"LoadBalancerNewDrawDistScaleParam_ps4", "负载均衡器新绘制距离缩放参数_PS4"},
                {"LoadBalancerNewDrawDistScaleParam_ps5", "负载均衡器新绘制距离缩放参数_PS5"},
                {"LoadBalancerNewDrawDistScaleParam_win64", "负载均衡器新绘制距离缩放参数_WIN64"},
                {"LoadBalancerNewDrawDistScaleParam_xb1", "负载均衡器新绘制距离缩放参数_XB1"},
                {"LoadBalancerNewDrawDistScaleParam_xb1x", "负载均衡器新绘制距离缩放参数_XB1X"},
                {"LoadBalancerNewDrawDistScaleParam_xss", "负载均衡器新绘制距离缩放参数_XSS"},
                {"LoadBalancerNewDrawDistScaleParam_xsx", "负载均衡器新绘制距离缩放参数_XSX"},
                {"LoadBalancerParam", "负载均衡器参数"},
                {"LockCamParam", "相机视角参数"},
                {"Magic", "法术（魔法与祷告）"},
                {"MapDefaultInfoParam", "地图默认信息参数"},
                {"MapGdRegionDrawParam", "地图GD区域绘制参数"},
                {"MapGdRegionInfoParam", "地图GD区域信息参数"},
                {"MapGridCreateHeightDetailLimitInfo", "地图网格创建高度细节限制信息"},
                {"MapGridCreateHeightLimitInfoParam", "地图网格创建高度限制信息参数"},
                {"MapMimicryEstablishmentParam", "地图拟态建立参数"},
                {"MapNameTexParam", "地图名称纹理参数"},
                {"MapNameTexParam_m61", "地图名称纹理参数_m61"},
                {"MapPieceTexParam", "地图碎片纹理参数"},
                {"MapPieceTexParam_m61", "地图碎片纹理参数_m61"},
                {"MaterialExParam", "材料扩展参数"},
                {"MenuColorTableParam", "菜单颜色表参数"},
                {"MenuCommonParam", "菜单界面通用参数"},
                {"MenuOffscrRendParam", "菜单离屏渲染参数"},
                {"MenuPropertyLayoutParam", "菜单属性布局参数"},
                {"MenuPropertySpecParam", "菜单属性规格参数"},
                {"MenuValueTableParam", "菜单值表参数"},
                {"MimicryEstablishmentTexParam", "拟态建立纹理参数"},
                {"MimicryEstablishmentTexParam_m61", "拟态建立纹理参数_m61"},
                {"MoveParam", "移动参数"},
                {"MPEstusFlaskRecoveryParam", "特定单位击杀后恢复蓝瓶设置"},
                {"MultiHPEstusFlaskBonusParam", "多人游戏下特定单位击杀后恢复血瓶设置"},
                {"MultiMPEstusFlaskBonusParam", "多人游戏下特定单位击杀后恢复蓝瓶设置"},
                {"MultiPlayCorrectionParam", "多人游戏校正参数"},
                {"MultiSoulBonusRateParam", "多人游戏灵魂奖金率参数"},
                {"NetworkAreaParam", "网络区域参数"},
                {"NetworkMsgParam", "网络消息参数"},
                {"NetworkParam", "网络参数"},
                {"NpcAiActionParam", "NPC AI行动参数"},
                {"NpcAiBehaviorProbability", "NPC AI行为概率"},
                {"NpcParam", "NPC参数"},
                {"NpcThinkParam", "NPCAI感知参数"},
                {"ObjActParam", "对象参数"},
                {"PartsDrawParam", "部件绘制参数"},
                {"PhantomParam", "灵体色调参数"},
                {"PlayerCommonParam", "玩家通用参数"},
                {"PlayRegionParam", "播放区域参数"},
                {"PostureControlParam_Gender", "姿势控制参数_性别"},
                {"PostureControlParam_Pro", "姿势控制参数_职业"},
                {"PostureControlParam_WepLeft", "姿势控制参数_左手武器"},
                {"PostureControlParam_WepRight", "姿势控制参数_右手武器"},
                {"RandomAppearParam", "随机出现参数"},
                {"ReinforceParamProtector", "防具强化曲线"},
                {"ReinforceParamWeapon", "武器强化曲线"},
                {"ResistCorrectParam", "异常抗性变化曲线"},
                {"ReverbAuxSendBusParam", "混响辅助发送总线参数"},
                {"RideParam", "骑乘参数"},
                {"RoleParam", "玩家联机状态设置参数"},
                {"RollingObjLotParam", "对象旋转色设置参数"},
                {"RuntimeBoneControlParam", "运行时骨骼控制参数"},
                {"SeActivationRangeParam", "SE激活范围参数"},
                {"SeMaterialConvertParam", "SE材质转换参数"},
                {"SfxBlockResShareParam", "SFX块资源共享参数"},
                {"ShopLineupParam", "商店参数"},
                {"ShopLineupParam_Recipe", "制作合成参数"},
                {"SignPuddleParam", "标志水洼参数"},
                {"SignPuddleSubCategoryParam", "标志水洼子类别参数"},
                {"SignPuddleTabParam", "标志水洼标签参数"},
                {"SoundAssetSoundObjEnableDistParam", "声音资产声音对象启用距离参数"},
                {"SoundAutoEnvSoundGroupParam", "声音自动环境声音组参数"},
                {"SoundAutoReverbEvaluationDistParam", "声音自动混响评估距离参数"},
                {"SoundAutoReverbSelectParam", "声音自动混响选择参数"},
                {"SoundChrPhysicsSeParam", "声音角色物理SE参数"},
                {"SoundCommonIngameParam", "声音游戏内通用参数"},
                {"SoundCommonSystemParam", "声音系统通用参数"},
                {"SoundCutsceneParam", "声音过场动画参数"},
                {"SpeedtreeParam", "SpeedTree参数"},
                {"SpEffectParam", "效果-speffect参数"},
                {"SpEffectSetParam", "效果集"},
                {"SpEffectVfxParam", "效果衍生表现特效参数"},
                {"SwordArtsParam", "战技参数"},
                {"TalkParam", "NPC对话参数"},
                {"ThrowDirectionSfxParam", "投技方向音效参数"},
                {"ThrowParam", "投技参数"},
                {"ToughnessParam", "耐久度参数"},
                {"TutorialParam", "教程参数"},
                {"WaypointParam", "路径点参数"},
                {"WeatherAssetCreateParam", "天气资产创建参数"},
                {"WeatherAssetReplaceParam", "天气资产替换参数"},
                {"WeatherLotParam", "天气掉落参数"},
                {"WeatherLotTexParam", "天气掉落纹理参数"},
                {"WeatherLotTexParam_m61", "天气掉落纹理参数_m61"},
                {"WeatherParam", "天气参数"},
                {"WepAbsorpPosParam", "武器吸收位置参数"},
                {"WetAspectParam", "湿润方面参数"},
                {"WhiteSignCoolTimeParam", "白色标志冷却时间参数"},
                {"WorldMapLegacyConvParam", "世界地图遗留转换参数"},
                {"WorldMapPieceParam", "世界地图碎片参数"},
                {"WorldMapPlaceNameParam", "世界地图地点名称参数"},
                {"WorldMapPointParam", "世界地图特殊标记点参数"},
                {"WwiseValueToStrParam_BgmBossChrIdConv", "Wwise值转字符串参数_BGM Boss角色ID转换"},
                {"WwiseValueToStrParam_EnvPlaceType", "Wwise值转字符串参数_环境地点类型"},
                {"WwiseValueToStrParam_Switch_AttackStrength", "Wwise值转字符串参数_攻击强度开关"},
                {"WwiseValueToStrParam_Switch_AttackType", "Wwise值转字符串参数_攻击类型开关"},
                {"WwiseValueToStrParam_Switch_DamageAmount", "Wwise值转字符串参数_伤害量开关"},
                {"WwiseValueToStrParam_Switch_DeffensiveMaterial", "Wwise值转字符串参数_防御材料开关"},
                {"WwiseValueToStrParam_Switch_GrassHitType", "Wwise值转字符串参数_草地击中类型开关"},
                {"WwiseValueToStrParam_Switch_HitStop", "Wwise值转字符串参数_击中停顿开关"},
                {"WwiseValueToStrParam_Switch_OffensiveMaterial", "Wwise值转字符串参数_攻击材料开关"},
                {"WwiseValueToStrParam_Switch_PlayerEquipmentBottoms", "Wwise值转字符串参数_玩家装备底部开关"},
                {"WwiseValueToStrParam_Switch_PlayerEquipmentTops", "Wwise值转字符串参数_玩家装备顶部开关"},
                {"WwiseValueToStrParam_Switch_PlayerShoes", "Wwise值转字符串参数_玩家鞋子开关"},
                {"WwiseValueToStrParam_Switch_PlayerVoiceType", "Wwise值转字符串参数_玩家声音类型开关"}
            };

            paramKeyList = UICache.GetCached(_paramEditor, _viewIndex, () =>
            {
                List<(ParamBank, Param)> list =
                    ParamSearchEngine.pse.Search(true, _selection.currentParamSearchString, true, true);
                var keyList = list.Where(param => param.Item1 == ParamBank.PrimaryBank)
                    .Select(param => ParamBank.PrimaryBank.GetKeyForParam(param.Item2)).ToList();

                if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                {
                    if (_mapParamView)
                    {
                        keyList = keyList.FindAll(p => p.EndsWith("Bank"));
                    }
                    else
                    {
                        keyList = keyList.FindAll(p => !p.EndsWith("Bank"));
                    }
                }
                else if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    if (_mapParamView)
                    {
                        keyList = keyList.FindAll(p => ParamBank.DS2MapParamlist.Contains(p.Split('_')[0]));
                    }
                    else
                    {
                        keyList = keyList.FindAll(p => !ParamBank.DS2MapParamlist.Contains(p.Split('_')[0]));
                    }
                }
                else if (Smithbox.ProjectType is ProjectType.ER || Smithbox.ProjectType is ProjectType.AC6)
                {
                    if (_eventParamView)
                    {
                        keyList = keyList.FindAll(p => p.StartsWith("EFID"));
                    }
                    else
                    {
                        keyList = keyList.FindAll(p => !p.StartsWith("EFID"));
                    }

                    if (_gConfigParamView)
                    {
                        if (Smithbox.ProjectType == ProjectType.AC6)
                        {
                            keyList = keyList.FindAll(p => p.StartsWith("GraphicsConfig"));
                        }
                        else
                        {
                            keyList = keyList.FindAll(p => p.StartsWith("Gconfig"));
                        }
                    }
                    else
                    {
                        if (Smithbox.ProjectType == ProjectType.AC6)
                        {
                            keyList = keyList.FindAll(p => !p.StartsWith("GraphicsConfig"));
                        }
                        else
                        {
                            keyList = keyList.FindAll(p => !p.StartsWith("Gconfig"));
                        }
                    }
                }

                if (CFG.Current.Param_AlphabeticalParams)
                {
                    keyList.Sort();
                }

                return keyList;
            });
        }

        //List<string> 

        foreach (var paramKey in paramKeyList)
        {
            HashSet<int> primary = ParamBank.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
            Param p = ParamBank.PrimaryBank.Params[paramKey];

            if (p != null)
            {
                var meta = ParamMetaData.Get(p.AppliedParamdef);
                var Wiki = meta?.Wiki;
                if (Wiki != null)
                {
                    if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                    {
                        meta.Wiki = Wiki;
                    }
                }
            }

            ImGui.Indent(15.0f * scale);

            if (primary != null ? primary.Any() : false)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_PrimaryChanged_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            }

            if (paramChs.ContainsKey(paramKey))
            {
                if (ImGui.Selectable($"{paramKey + " " + paramChs[paramKey]}", paramKey == _selection.GetActiveParam()))
                {
                    //_selection.setActiveParam(param.Key);
                    EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
                }
            }
            else if (ImGui.Selectable($"{paramKey}", paramKey == _selection.GetActiveParam()))
            {
                //_selection.setActiveParam(param.Key);
                EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
            }

            ImGui.PopStyleColor();

            if (doFocus && paramKey == _selection.GetActiveParam())
            {
                scrollTo = ImGui.GetCursorPosY();
            }

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("钉 Pin " + paramKey) &&
                    !Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.Contains(paramKey))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams.Add(paramKey);
                }

                if (ParamEditorScreen.EditorMode && p != null)
                {
                    var meta = ParamMetaData.Get(p.AppliedParamdef);

                    if (meta != null && meta.Wiki == null && ImGui.MenuItem("Add wiki..."))
                    {
                        meta.Wiki = "Empty wiki...";
                    }

                    if (meta?.Wiki != null && ImGui.MenuItem("Remove wiki"))
                    {
                        meta.Wiki = null;
                    }
                }
                if (ImGui.Selectable("复制 Copy"))
                {
                    ImGui.SetClipboardText(paramKey);
                    //string ss ="";
                    //foreach (var s in paramKeyList)
                    //{
                    //    ss += s; ss += "\n";
                    //}
                    //ImGui.SetClipboardText(ss);
                }
                ImGui.EndPopup();
            }

            ImGui.Unindent(15.0f * scale);
        }

        if (doFocus)
        {
            ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
        }
    }

    private void ParamView_ParamList(bool doFocus, bool isActiveView, float scale, float scrollTo)
    {
        ParamView_ParamList_Header(isActiveView);

        if(CFG.Current.Param_PinnedRowsStayVisible)
        {
            ParamView_ParamList_Pinned(scale);
        }

        ImGui.BeginChild("paramTypes");

        if (!CFG.Current.Param_PinnedRowsStayVisible)
        {
            ParamView_ParamList_Pinned(scale);
        }

        ParamView_ParamList_Main(doFocus, scale, scrollTo);
        ImGui.EndChild();
    }
    //------------------------------------
    // Row
    //------------------------------------
    private void ParamView_RowList_Header(ref bool doFocus, bool isActiveView, ref float scrollTo,
        string activeParam)
    {
        ImGui.Text("行数据 Rows");
        ImGui.Separator();

        scrollTo = 0;

        if (ImGui.Button($"筛选 Go to selected <{KeyBindings.Current.Param_GotoSelectedRow.HintText}>") ||
            isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_GotoRowID))
        {
            _paramEditor.GotoSelectedRow = true;
        }

        ImGui.SameLine();

        if (ImGui.Button($"指定 Go to ID <{KeyBindings.Current.Param_GotoRowID.HintText}>") ||
            isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_GotoRowID))
        {
            ImGui.OpenPopup("gotoParamRow");
        }

        if (ImGui.BeginPopup("gotoParamRow"))
        {
            var gotorow = 0;
            ImGui.SetKeyboardFocusHere();
            ImGui.InputInt("指定行 Goto Row ID", ref gotorow);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                _gotoParamRow = gotorow;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        //Row ID/name search
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_SearchRow))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.InputText($"查找 Search <{KeyBindings.Current.Param_SearchRow.HintText}>",
            ref _selection.GetCurrentRowSearchString(), 256);
        var resAutoRow = AutoFill.RowSearchBarAutoFill();

        if (resAutoRow != null)
        {
            _selection.SetCurrentRowSearchString(resAutoRow);
        }

        if (!lastRowSearch.ContainsKey(_selection.GetActiveParam()) || !lastRowSearch[_selection.GetActiveParam()]
                .Equals(_selection.GetCurrentRowSearchString()))
        {
            UICache.ClearCaches();
            lastRowSearch[_selection.GetActiveParam()] = _selection.GetCurrentRowSearchString();
            doFocus = true;
        }

        if (ImGui.IsItemActive())
        {
            _paramEditor._isSearchBarActive = true;
        }
        else
        {
            _paramEditor._isSearchBarActive = false;
        }

        UIHints.AddImGuiHintButton("MassEditHint", ref UIHints.SearchBarHint);

        ImGui.Separator();
    }

    private void ParamView_RowList(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        if (!_selection.ActiveParamExists())
        {
            ImGui.Text("选择一个参数查看\nSelect a param to see rows");
        }
        else
        {
            IParamDecorator decorator = null;

            if (_paramEditor._decorators.ContainsKey(activeParam))
            {
                decorator = _paramEditor._decorators[activeParam];
            }

            ParamView_RowList_Header(ref doFocus, isActiveView, ref scrollTo, activeParam);

            Param para = ParamBank.PrimaryBank.Params[activeParam];
            HashSet<int> vanillaDiffCache = ParamBank.PrimaryBank.GetVanillaDiffRows(activeParam);
            var auxDiffCaches = ParamBank.AuxBanks.Select((bank, i) =>
                (bank.Value.GetVanillaDiffRows(activeParam), bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Param.Column compareCol = _selection.GetCompareCol();
            PropertyInfo compareColProp = typeof(Param.Cell).GetProperty("Value");

            //ImGui.BeginChild("rows" + activeParam);
            if (EditorDecorations.ImGuiTableStdColumns("rowList", compareCol == null ? 1 : 2, false))
            {
                var pinnedRowList = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows
                    .GetValueOrDefault(activeParam, new List<int>()).Select(id => para[id]).ToList();

                ImGui.TableSetupColumn("rowCol", ImGuiTableColumnFlags.None, 1f);
                if (compareCol != null)
                {
                    ImGui.TableSetupColumn("rowCol2", ImGuiTableColumnFlags.None, 0.4f);
                    if (CFG.Current.Param_PinnedRowsStayVisible)
                    {
                        ImGui.TableSetupScrollFreeze(2, 1 + pinnedRowList.Count);
                    }
                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text("ID\t\tName");
                    }

                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text(compareCol.Def.InternalName);
                    }
                }
                else
                {
                    if (CFG.Current.Param_PinnedRowsStayVisible)
                    {
                        ImGui.TableSetupScrollFreeze(1, pinnedRowList.Count);
                    }
                }

                ImGui.PushID("pinned");

                var selectionCachePins = _selection.GetSelectionCache(pinnedRowList, "pinned");
                if (pinnedRowList.Count != 0)
                {
                    var lastCol = false;
                    for (var i = 0; i < pinnedRowList.Count(); i++)
                    {
                        Param.Row row = pinnedRowList[i];
                        if (row == null)
                        {
                            continue;
                        }

                        lastCol = ParamView_RowList_Entry(selectionCachePins, i, activeParam, null, row,
                            vanillaDiffCache, auxDiffCaches, decorator, ref scrollTo, false, true, compareCol,
                            compareColProp);
                    }

                    if (lastCol)
                    {
                        ImGui.Spacing();
                    }

                    if (EditorDecorations.ImguiTableSeparator())
                    {
                        ImGui.Spacing();
                    }
                }

                ImGui.PopID();

                // Up/Down arrow key input
                if ((InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)) && !ImGui.IsAnyItemActive())
                {
                    _arrowKeyPressed = true;
                }

                if (_focusRows)
                {
                    ImGui.SetNextWindowFocus();
                    _arrowKeyPressed = false;
                    _focusRows = false;
                }

                List<Param.Row> rows = UICache.GetCached(_paramEditor, (_viewIndex, activeParam),
                    () => RowSearchEngine.rse.Search((ParamBank.PrimaryBank, para),
                        _selection.GetCurrentRowSearchString(), true, true));

                var enableGrouping = !CFG.Current.Param_DisableRowGrouping && ParamMetaData
                    .Get(ParamBank.PrimaryBank.Params[activeParam].AppliedParamdef).ConsecutiveIDs;

                // Rows
                var selectionCache = _selection.GetSelectionCache(rows, "regular");
                for (var i = 0; i < rows.Count; i++)
                {
                    Param.Row currentRow = rows[i];
                    if (enableGrouping)
                    {
                        Param.Row prev = i - 1 > 0 ? rows[i - 1] : null;
                        Param.Row next = i + 1 < rows.Count ? rows[i + 1] : null;
                        if (prev != null && next != null && prev.ID + 1 != currentRow.ID &&
                            currentRow.ID + 1 == next.ID)
                        {
                            EditorDecorations.ImguiTableSeparator();
                        }

                        ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                            auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp);

                        if (prev != null && next != null && prev.ID + 1 == currentRow.ID &&
                            currentRow.ID + 1 != next.ID)
                        {
                            EditorDecorations.ImguiTableSeparator();
                        }
                    }
                    else
                    {
                        ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                            auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp);
                    }
                }

                if (doFocus)
                {
                    ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
                }

                ImGui.EndTable();
            }
            //ImGui.EndChild();
        }
    }

    //------------------------------------
    // Field
    //------------------------------------
    private void ParamView_FieldList_Header(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ImGui.Text("块 Fields");
        ImGui.Separator();
    }

    private void ParamView_FieldList(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ParamView_FieldList_Header(isActiveView, activeParam, activeRow);

        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");
            ImGui.Text("选择以查看属性\nSelect a row to see properties");
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);
            Param vanillaParam = ParamBank.VanillaBank.Params?.GetValueOrDefault(activeParam);

            _propEditor.PropEditorParamRow(
                ParamBank.PrimaryBank,
                activeRow,
                vanillaParam?[activeRow.ID],
                ParamBank.AuxBanks.Select((bank, i) =>
                    (bank.Key, bank.Value.Params?.GetValueOrDefault(activeParam)?[activeRow.ID])).ToList(),
                _selection.GetCompareRow(),
                ref _selection.GetCurrentPropSearchString(),
                activeParam,
                isActiveView,
                _selection);

            ImGui.EndChild();
        }
    }

    public void ParamView(bool doFocus, bool isActiveView)
    {
        var scale = Smithbox.GetUIScale();

        if (EditorDecorations.ImGuiTableStdColumns("paramsT", 3, true))
        {
            ImGui.TableSetupColumn("paramsCol", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn("paramsCol2", ImGuiTableColumnFlags.None, 0.5f);
            var scrollTo = 0f;
            if (ImGui.TableNextColumn())
            {
                ParamView_ParamList(doFocus, isActiveView, scale, scrollTo);
            }

            var activeParam = _selection.GetActiveParam();
            if (ImGui.TableNextColumn())
            {
                ParamView_RowList(doFocus, isActiveView, scrollTo, activeParam);
            }

            Param.Row activeRow = _selection.GetActiveRow();
            if (ImGui.TableNextColumn())
            {
                ParamView_FieldList(isActiveView, activeParam, activeRow);
            }

            ImGui.EndTable();
        }
    }

    private void ParamView_RowList_Entry_Row(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, IParamDecorator decorator, ref float scrollTo,
        bool doFocus, bool isPinned)
    {
        var diffVanilla = vanillaDiffCache.Contains(r.ID);
        var auxDiffVanilla = auxDiffCaches.Where(cache => cache.Item1.Contains(r.ID)).Count() > 0;

        if (diffVanilla)
        {
            // If the auxes are changed bu
            var auxDiffPrimaryAndVanilla = (auxDiffVanilla ? 1 : 0) + auxDiffCaches
                .Where(cache => cache.Item1.Contains(r.ID) && cache.Item2.Contains(r.ID)).Count() > 1;

            if (auxDiffVanilla && auxDiffPrimaryAndVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_AuxConflict_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_PrimaryChanged_Text);
            }
        }
        else
        {
            if (auxDiffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_AuxAdded_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            }
        }

        var selected = selectionCache != null && selectionCacheIndex < selectionCache.Length
            ? selectionCache[selectionCacheIndex]
            : false;

        if (_gotoParamRow != -1 && !isPinned)
        {
            // Goto row was activated. As soon as a corresponding ID is found, change selection to it.
            if (r.ID == _gotoParamRow)
            {
                selected = true;
                _selection.SetActiveRow(r, true);
                _gotoParamRow = -1;
                ImGui.SetScrollHereY();
            }
        }

        if (_paramEditor.GotoSelectedRow && !isPinned)
        {
            var activeRow = _selection.GetActiveRow();

            if (activeRow == null)
            {
                _paramEditor.GotoSelectedRow = false;
            }
            else if (activeRow.ID == r.ID)
            {
                ImGui.SetScrollHereY();
                _paramEditor.GotoSelectedRow = false;
            }
        }

        var label = $@"{r.ID} {Utils.ImGuiEscape(r.Name)}";
        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            CFG.Current.Param_DisableLineWrapping ? 1 : 3);

        if (ImGui.Selectable($@"{label}##{selectionCacheIndex}", selected))
        {
            _focusRows = true;

            if (InputTracker.GetKey(Key.LControl) || InputTracker.GetKey(Key.RControl))
            {
                _selection.ToggleRowInSelection(r);
            }
            else if (p != null && (InputTracker.GetKey(Key.LShift) || InputTracker.GetKey(Key.RShift)) && _selection.GetActiveRow() != null)
            {
                _selection.CleanSelectedRows();
                var start = p.IndexOf(_selection.GetActiveRow());
                var end = p.IndexOf(r);

                if (start != end && start != -1 && end != -1)
                {
                    foreach (Param.Row r2 in p.GetRange(start < end ? start : end, Math.Abs(end - start)))
                    {
                        _selection.AddRowToSelection(r2);
                    }
                }

                _selection.AddRowToSelection(r);
            }
            else
            {
                _selection.SetActiveRow(r, true);
            }
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused() && r != _selection.GetActiveRow())
        {
            if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
            {
                // Add to selection
                _selection.AddRowToSelection(r);
            }
            else
            {
                // Exclusive selection
                _selection.SetActiveRow(r, true);
            }

            _arrowKeyPressed = false;
        }

        ImGui.PopStyleColor();

        // Param Context Menu
        if (ImGui.BeginPopupContextItem(r.ID.ToString()))
        {
            if (CFG.Current.Param_RowContextMenu_NameInput)
            {
                if (_selection.RowSelectionExists())
                {
                    var name = _selection.GetActiveRow().Name;
                    if (name != null)
                    {
                        ImGui.InputText("##rowName", ref name, 255);
                        _selection.GetActiveRow().Name = name;
                    }
                }
            }

            if (CFG.Current.Param_RowContextMenu_ShortcutTools)
            {
                if (ImGui.Selectable(@$"复制 Copy selection ({KeyBindings.Current.Param_Copy.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.CopySelectionToClipboard(_selection);
                }

                ImGui.Separator();

                if (ImGui.Selectable(@$"粘贴 Paste clipboard ({KeyBindings.Current.Param_Paste.HintText})", false,
                        ParamBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }

                ImGui.Separator();

                if (ImGui.Selectable(@$"删除 Delete selection ({KeyBindings.Current.Core_Delete.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.DeleteSelection(_selection);
                }

                ImGui.Separator();

                if (ImGui.Selectable(@$"复刻 Duplicate selection ({KeyBindings.Current.Core_Duplicate.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    ParamAction_DuplicateRow.DuplicateSelection(_selection);
                }

                ImGui.Separator();
            }

            if (CFG.Current.Param_RowContextMenu_PinOptions)
            {
                if (ImGui.Selectable((isPinned ? "解 Unpin " : "钉 Pin ") + r.ID))
                {
                    if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.ContainsKey(activeParam))
                    {
                        Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Add(activeParam, new List<int>());
                    }

                    List<int> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam];

                    if (isPinned)
                    {
                        pinned.Remove(r.ID);
                    }
                    else if (!pinned.Contains(r.ID))
                    {
                        pinned.Add(r.ID);
                    }
                }

                if (isPinned)
                {
                    EditorDecorations.PinListReorderOptions(Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows[activeParam], r.ID);
                }

                if (ImGui.Selectable("全解 Unpin all"))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows.Clear();
                }

                ImGui.Separator();
            }

            if (decorator != null)
            {
                decorator.DecorateContextMenuItems(r);
            }

            if (CFG.Current.Param_RowContextMenu_CompareOptions)
            {
                if (ImGui.Selectable("比较 Compare..."))
                {
                    _selection.SetCompareRow(r);
                }
            }

            if (CFG.Current.Param_RowContextMenu_ReverseLoopup)
            {
                EditorDecorations.ParamRefReverseLookupSelectables(_paramEditor, ParamBank.PrimaryBank, activeParam, r.ID);
            }

            if (CFG.Current.Param_RowContextMenu_CopyID)
            {
                if (ImGui.Selectable("复制ID到剪辑版 Copy ID to clipboard"))
                {
                    PlatformUtils.Instance.SetClipboardText($"{r.ID}");
                }
            }

            ImGui.EndPopup();
        }

        if (decorator != null)
        {
            decorator.DecorateParam(r);
        }

        if (doFocus && _selection.GetActiveRow() == r)
        {
            scrollTo = ImGui.GetCursorPosY();
        }
    }

    private bool ParamView_RowList_Entry(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, IParamDecorator decorator, ref float scrollTo,
        bool doFocus, bool isPinned, Param.Column compareCol, PropertyInfo compareColProp)
    {
        var scale = Smithbox.GetUIScale();

        if (CFG.Current.UI_CompactParams)
        {
            // ItemSpacing only affects clickable area for selectables in tables. Add additional height to prevent gaps between selectables.
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 2.0f) * scale);
        }

        var lastCol = false;

        if (ImGui.TableNextColumn())
        {
            ParamView_RowList_Entry_Row(selectionCache, selectionCacheIndex, activeParam, p, r, vanillaDiffCache,
                auxDiffCaches, decorator, ref scrollTo, doFocus, isPinned);
            lastCol = true;
        }

        if (compareCol != null)
        {
            if (ImGui.TableNextColumn())
            {
                Param.Cell c = r[compareCol];
                object newval = null;
                ImGui.PushID("compareCol_" + selectionCacheIndex);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ParamEditorCommon.PropertyField(compareCol.ValueType, c.Value, ref newval, false, false);

                if (ParamEditorCommon.UpdateProperty(_propEditor.ContextActionManager, c, compareColProp,
                        c.Value) && !ParamBank.VanillaBank.IsLoadingParams)
                {
                    ParamBank.PrimaryBank.RefreshParamRowDiffs(r, activeParam);
                }

                ImGui.PopStyleVar();
                ImGui.PopID();
                lastCol = true;
            }
            else
            {
                lastCol = false;
            }
        }

        if (CFG.Current.UI_CompactParams)
        {
            ImGui.PopStyleVar();
        }

        return lastCol;
    }
}
