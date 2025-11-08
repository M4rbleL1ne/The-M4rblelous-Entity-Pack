global using static LBMergedMods.InternalMatch;
using LizardCosmetics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods;

static class InternalMatch
{
    internal static int s_loc1, s_loc2;
    internal static ILLabel? s_label;
    internal static MethodReference? s_ref;
    internal static MethodInfo
        s_Math_Max_int_int = typeof(Math).GetMethod("Max", [typeof(int), typeof(int)]),
        s_Random_Range_int_int = typeof(Random).GetMethod("Range", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(int), typeof(int)], null),
        s_Room_MiddleOfTile_Vector2 = typeof(Room).GetMethod("MiddleOfTile", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(Vector2)], null),
        s_Room_ViewedByAnyCamera_Vector2_float = typeof(Room).GetMethod("ViewedByAnyCamera", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(Vector2), typeof(float)], null),
        s_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type = typeof(StaticWorld).GetMethod("GetCreatureTemplate", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(CreatureTemplate.Type)], null),
        s_string_op_Equality_string_string = typeof(string).GetMethod("op_Equality", [typeof(string), typeof(string)]),
        s_string_Substring_int_int = typeof(string).GetMethod(nameof(string.Substring), [typeof(int), typeof(int)]),
        s_World_GetAbstractRoom_int = typeof(World).GetMethod("GetAbstractRoom", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(int)], null),
        s_World_GetAbstractRoom_WorldCoordinate = typeof(World).GetMethod("GetAbstractRoom", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(WorldCoordinate)], null);
    internal static Func<Instruction, bool>
        s_MatchAdd = MatchAdd,
        s_MatchBeq_Any = MatchBeq_Any,
        s_MatchBeq_OutLabel = MatchBeq_OutLabel,
        s_MatchBle_OutLabel = MatchBle_OutLabel,
        s_MatchBneUn_Any = MatchBneUn_Any,
        s_MatchBr_Any = MatchBr_Any,
        s_MatchBrfalse_Any = MatchBrfalse_Any,
        s_MatchBrfalse_OutLabel = MatchBrfalse_OutLabel,
        s_MatchBrtrue_OutLabel = MatchBrtrue_OutLabel,
        s_MatchCall_Any = MatchCall_Any,
        s_MatchCall_Creature_Violence = MatchCall_Creature_Violence,
        s_MatchCall_Math_Max_int_int = MatchCall_Math_Max_int_int,
        s_MatchCall_Mathf_Lerp = MatchCall_Mathf_Lerp,
        s_MatchCall_Mathf_Max = MatchCall_Mathf_Max,
        s_MatchCall_Mathf_RoundToInt = MatchCall_Mathf_RoundToInt,
        s_MatchCall_ModManager_get_DLCShared = MatchCall_ModManager_get_DLCShared,
        s_MatchCall_op_Equality_Any = MatchCall_op_Equality_Any,
        s_MatchCall_Random_Range_int_int = MatchCall_Random_Range_int_int,
        s_MatchCall_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type = MatchCall_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type,
        s_MatchCall_string_op_Equality_string_string = MatchCall_string_op_Equality_string_string,
        s_MatchCall_Vector2_op_Addition = MatchCall_Vector2_op_Addition,
        s_MatchCall_Vector2_op_Multiply = MatchCall_Vector2_op_Multiply,
        s_MatchCallOrCallvirt_AbstractCreature_get_realizedCreature = MatchCallOrCallvirt_AbstractCreature_get_realizedCreature,
        s_MatchCallOrCallvirt_AbstractRoom_AddEntity = MatchCallOrCallvirt_AbstractRoom_AddEntity,
        s_MatchCallOrCallvirt_AbstractRoom_AttractionForCreature = MatchCallOrCallvirt_AbstractRoom_AttractionForCreature,
        s_MatchCallOrCallvirt_AbstractWorldEntity_get_Room = MatchCallOrCallvirt_AbstractWorldEntity_get_Room,
        s_MatchCallOrCallvirt_Any = MatchCallOrCallvirt_Any,
        s_MatchCallOrCallvirt_ArtificialIntelligence_StaticRelationship = MatchCallOrCallvirt_ArtificialIntelligence_StaticRelationship,
        s_MatchCallOrCallvirt_Centipede_get_Centiwing = MatchCallOrCallvirt_Centipede_get_Centiwing,
        s_MatchCallOrCallvirt_Centipede_get_Red = MatchCallOrCallvirt_Centipede_get_Red,
        s_MatchCallOrCallvirt_Creature_get_abstractCreature = MatchCallOrCallvirt_Creature_get_abstractCreature,
        s_MatchCallOrCallvirt_Creature_get_grasps = MatchCallOrCallvirt_Creature_get_grasps,
        s_MatchCallOrCallvirt_Creature_get_Template = MatchCallOrCallvirt_Creature_get_Template,
        s_MatchCallOrCallvirt_CreatureTemplate_get_IsVulture = MatchCallOrCallvirt_CreatureTemplate_get_IsVulture,
        s_MatchCallOrCallvirt_GraphicsModule_AddToContainer = MatchCallOrCallvirt_GraphicsModule_AddToContainer,
        s_MatchCallOrCallvirt_HealthState_get_ClampedHealth = MatchCallOrCallvirt_HealthState_get_ClampedHealth,
        s_MatchCallOrCallvirt_IconSymbol_Draw = MatchCallOrCallvirt_IconSymbol_Draw,
        s_MatchCallOrCallvirt_Lizard_get_Swimmer = MatchCallOrCallvirt_Lizard_get_Swimmer,
        s_MatchCallOrCallvirt_LizardAI_get_lizard = MatchCallOrCallvirt_LizardAI_get_lizard,
        s_MatchCallOrCallvirt_LizardAI_LizardSpitTracker_get_lizardAI = MatchCallOrCallvirt_LizardAI_LizardSpitTracker_get_lizardAI,
        s_MatchCallOrCallvirt_OutRef = MatchCallOrCallvirt_OutRef,
        s_MatchCallOrCallvirt_Player_get_CanPutSpearToBack = MatchCallOrCallvirt_Player_get_CanPutSpearToBack,
        s_MatchCallOrCallvirt_RainWorldGame_get_world = MatchCallOrCallvirt_RainWorldGame_get_world,
        s_MatchCallOrCallvirt_Room_get_abstractRoom = MatchCallOrCallvirt_Room_get_abstractRoom,
        s_MatchCallOrCallvirt_Room_MiddleOfTile_Vector2 = MatchCallOrCallvirt_Room_MiddleOfTile_Vector2,
        s_MatchCallOrCallvirt_Room_ViewedByAnyCamera_Vector2_float = MatchCallOrCallvirt_Room_ViewedByAnyCamera_Vector2_float,
        s_MatchCallOrCallvirt_string_get_Length = MatchCallOrCallvirt_string_get_Length,
        s_MatchCallOrCallvirt_string_Substring_int_int = MatchCallOrCallvirt_string_Substring_int_int,
        s_MatchCallOrCallvirt_World_get_firstRoomIndex = MatchCallOrCallvirt_World_get_firstRoomIndex,
        s_MatchCallOrCallvirt_World_GetAbstractRoom_int = MatchCallOrCallvirt_World_GetAbstractRoom_int,
        s_MatchCallOrCallvirt_World_GetAbstractRoom_WorldCoordinate = MatchCallOrCallvirt_World_GetAbstractRoom_WorldCoordinate,
        s_MatchConvI4 = MatchConvI4,
        s_MatchDiv = MatchDiv,
        s_MatchIsinst_BigSpider = MatchIsinst_BigSpider,
        s_MatchIsinst_Cicada = MatchIsinst_Cicada,
        s_MatchIsinst_Fly = MatchIsinst_Fly,
        s_MatchIsinst_InsectoidCreature = MatchIsinst_InsectoidCreature,
        s_MatchIsinst_IPlayerEdible = MatchIsinst_IPlayerEdible,
        s_MatchIsinst_LizardAI_LizardTrackState = MatchIsinst_LizardAI_LizardTrackState,
        s_MatchIsinst_KarmaFlower = MatchIsinst_KarmaFlower,
        s_MatchIsinst_PuffBall = MatchIsinst_PuffBall,
        s_MatchLdarg_0 = MatchLdarg_0,
        s_MatchLdarg_1 = MatchLdarg_1,
        s_MatchLdarg_2 = MatchLdarg_2,
        s_MatchLdcI4_M1 = MatchLdcI4_M1,
        s_MatchLdcI4_0 = MatchLdcI4_0,
        s_MatchLdcI4_1 = MatchLdcI4_1,
        s_MatchLdcI4_4 = MatchLdcI4_4,
        s_MatchLdcI4_20 = MatchLdcI4_20,
        s_MatchLdcI4_60 = MatchLdcI4_60,
        s_MatchLdcR4_0_1 = MatchLdcR4_0_1,
        s_MatchLdcR4_0_3 = MatchLdcR4_0_3,
        s_MatchLdcR4_0_7 = MatchLdcR4_0_7,
        s_MatchLdcR4_1 = MatchLdcR4_1,
        s_MatchLdcR4_2 = MatchLdcR4_2,
        s_MatchLdcR4_2_3 = MatchLdcR4_2_3,
        s_MatchLdcR4_7 = MatchLdcR4_7,
        s_MatchLdcR4_15 = MatchLdcR4_15,
        s_MatchLdcR4_45 = MatchLdcR4_45,
        s_MatchLdcR4_60 = MatchLdcR4_60,
        s_MatchLdcR4_1000 = MatchLdcR4_1000,
        s_MatchLdelema_AbstractRoomNode = MatchLdelema_AbstractRoomNode,
        s_MatchLdelemAny_WorldCoordinate = MatchLdelemAny_WorldCoordinate,
        s_MatchLdelemI4 = MatchLdelemI4,
        s_MatchLdelemRef = MatchLdelemRef,
        s_MatchLdelemU1 = MatchLdelemU1,
        s_MatchLdfld_AbstractCreature_creatureTemplate = MatchLdfld_AbstractCreature_creatureTemplate,
        s_MatchLdfld_AbstractCreatureAI_parent = MatchLdfld_AbstractCreatureAI_parent,
        s_MatchLdfld_AbstractCreature_state = MatchLdfld_AbstractCreature_state,
        s_MatchLdfld_AbstractCreatureAI_world = MatchLdfld_AbstractCreatureAI_world,
        s_MatchLdfld_AbstractPhysicalObject_type = MatchLdfld_AbstractPhysicalObject_type,
        s_MatchLdfld_AbstractRoom_connections = MatchLdfld_AbstractRoom_connections,
        s_MatchLdfld_AbstractRoom_creatures = MatchLdfld_AbstractRoom_creatures,
        s_MatchLdfld_AbstractRoom_nodes = MatchLdfld_AbstractRoom_nodes,
        s_MatchLdfld_AbstractRoomNode_type = MatchLdfld_AbstractRoomNode_type,
        s_MatchLdfld_AbstractWorldEntity_world = MatchLdfld_AbstractWorldEntity_world,
        s_MatchLdfld_BigEel_swimSpeed = MatchLdfld_BigEel_swimSpeed,
        s_MatchLdfld_BigEelGraphics_eel = MatchLdfld_BigEelGraphics_eel,
        s_MatchLdfld_BigSpider_jumpStamina = MatchLdfld_BigSpider_jumpStamina,
        s_MatchLdfld_BigSpider_spitter = MatchLdfld_BigSpider_spitter,
        s_MatchLdfld_BigSpiderAI_bug = MatchLdfld_BigSpiderAI_bug,
        s_MatchLdfld_Centipede_size = MatchLdfld_Centipede_size,
        s_MatchLdfld_CentipedeAI_centipede = MatchLdfld_CentipedeAI_centipede,
        s_MatchLdfld_CentipedeGraphics_lightSource = MatchLdfld_CentipedeGraphics_lightSource,
        s_MatchLdfld_CollectiblesTracker_displayRegions = MatchLdfld_CollectiblesTracker_displayRegions,
        s_MatchLdfld_CollectiblesTracker_sprites = MatchLdfld_CollectiblesTracker_sprites,
        s_MatchLdfld_CreatureTemplate_type = MatchLdfld_CreatureTemplate_type,
        s_MatchLdfld_Creature_Grasp_grabbed = MatchLdfld_Creature_Grasp_grabbed,
        s_MatchLdfld_GameSession_game = MatchLdfld_GameSession_game,
        s_MatchLdfld_GarbageWormAI_CreatureInterest_crit = MatchLdfld_GarbageWormAI_CreatureInterest_crit,
        s_MatchLdfld_IconSymbol_IconSymbolData_critType = MatchLdfld_IconSymbol_IconSymbolData_critType,
        s_MatchLdfld_Leech_school = MatchLdfld_Leech_school,
        s_MatchLdfld_Leech_LeechSchool_prey = MatchLdfld_Leech_LeechSchool_prey,
        s_MatchLdfld_Leech_LeechSchool_LeechPrey_creature = MatchLdfld_Leech_LeechSchool_LeechPrey_creature,
        s_MatchLdfld_Lizard_animation = MatchLdfld_Lizard_animation,
        s_MatchLdfld_LizardAI_LizardTrackState_vultureMask = MatchLdfld_LizardAI_LizardTrackState_vultureMask,
        s_MatchLdfld_LizardAI_LurkTracker_lizard = MatchLdfld_LizardAI_LurkTracker_lizard,
        s_MatchLdfld_LizardGraphics_lizard = MatchLdfld_LizardGraphics_lizard,
        s_MatchLdfld_PathFinder_world = MatchLdfld_PathFinder_world,
        s_MatchLdfld_Player_objectInStomach = MatchLdfld_Player_objectInStomach,
        s_MatchLdfld_Player_swallowAndRegurgitateCounter = MatchLdfld_Player_swallowAndRegurgitateCounter,
        s_MatchLdfld_Player_InputPackage_jmp = MatchLdfld_Player_InputPackage_jmp,
        s_MatchLdfld_Player_InputPackage_thrw = MatchLdfld_Player_InputPackage_thrw,
        s_MatchLdfld_RelationshipTracker_DynamicRelationship_state = MatchLdfld_RelationshipTracker_DynamicRelationship_state,
        s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep = MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep,
        s_MatchLdfld_RoomBorderExit_type = MatchLdfld_RoomBorderExit_type,
        s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix = MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix,
        s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world = MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world,
        s_MatchLdfld_ShortcutHandler_Vessel_creature = MatchLdfld_ShortcutHandler_Vessel_creature,
        s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature = MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
        s_MatchLdfld_UpdatableAndDeletable_room = MatchLdfld_UpdatableAndDeletable_room,
        s_MatchLdfld_World_seaAccessNodes = MatchLdfld_World_seaAccessNodes,
        s_MatchLdfld_WorldCoordinate_abstractNode = MatchLdfld_WorldCoordinate_abstractNode,
        s_MatchLdfld_WorldCoordinate_room = MatchLdfld_WorldCoordinate_room,
        s_MatchLdfld_YellowAI_lizard = MatchLdfld_YellowAI_lizard,
        s_MatchLdflda_Creature_inputWithDiagonals = MatchLdflda_Creature_inputWithDiagonals,
        s_MatchLdflda_Creature_lastInputWithDiagonals = MatchLdflda_Creature_lastInputWithDiagonals,
        s_MatchLdlen = MatchLdlen,
        s_MatchLdloc_Any = MatchLdloc_Any,
        s_MatchLdloc_InLoc1 = MatchLdloc_InLoc1,
        s_MatchLdloc_InLoc2 = MatchLdloc_InLoc2,
        s_MatchLdloc_OutLoc1 = MatchLdloc_OutLoc1,
        s_MatchLdloc_OutLoc2 = MatchLdloc_OutLoc2,
        s_MatchLdloca_Any = MatchLdloca_Any,
        s_MatchLdnull = MatchLdnull,
        s_MatchLdsfld_AbstractPhysicalObject_AbstractObjectType_JellyFish = MatchLdsfld_AbstractPhysicalObject_AbstractObjectType_JellyFish,
        s_MatchLdsfld_AbstractRoom_CreatureRoomAttraction_Forbidden = MatchLdsfld_AbstractRoom_CreatureRoomAttraction_Forbidden,
        s_MatchLdsfld_AbstractRoomNode_Type_SeaExit = MatchLdsfld_AbstractRoomNode_Type_SeaExit,
        s_MatchLdsfld_CreatureTemplate_Type_BigEel = MatchLdsfld_CreatureTemplate_Type_BigEel,
        s_MatchLdsfld_CreatureTemplate_Type_BigSpider = MatchLdsfld_CreatureTemplate_Type_BigSpider,
        s_MatchLdsfld_CreatureTemplate_Type_BlackLizard = MatchLdsfld_CreatureTemplate_Type_BlackLizard,
        s_MatchLdsfld_CreatureTemplate_Type_CyanLizard = MatchLdsfld_CreatureTemplate_Type_CyanLizard,
        s_MatchLdsfld_CreatureTemplate_Type_Leech = MatchLdsfld_CreatureTemplate_Type_Leech,
        s_MatchLdsfld_CreatureTemplate_Type_MirosBird = MatchLdsfld_CreatureTemplate_Type_MirosBird,
        s_MatchLdsfld_CreatureTemplate_Type_Overseer = MatchLdsfld_CreatureTemplate_Type_Overseer,
        s_MatchLdsfld_CreatureTemplate_Type_PoleMimic = MatchLdsfld_CreatureTemplate_Type_PoleMimic,
        s_MatchLdsfld_CreatureTemplate_Type_Salamander = MatchLdsfld_CreatureTemplate_Type_Salamander,
        s_MatchLdsfld_CreatureTemplate_Type_Scavenger = MatchLdsfld_CreatureTemplate_Type_Scavenger,
        s_MatchLdsfld_CreatureTemplate_Type_Slugcat = MatchLdsfld_CreatureTemplate_Type_Slugcat,
        s_MatchLdsfld_CreatureTemplate_Type_Snail = MatchLdsfld_CreatureTemplate_Type_Snail,
        s_MatchLdsfld_CreatureTemplate_Type_SpitterSpider = MatchLdsfld_CreatureTemplate_Type_SpitterSpider,
        s_MatchLdsfld_CreatureTemplate_Type_Vulture = MatchLdsfld_CreatureTemplate_Type_Vulture,
        s_MatchLdsfld_CreatureTemplate_Type_WhiteLizard = MatchLdsfld_CreatureTemplate_Type_WhiteLizard,
        s_MatchLdsfld_CreatureTemplate_Type_YellowLizard = MatchLdsfld_CreatureTemplate_Type_YellowLizard,
        s_MatchLdsfld_Lizard_Animation_Spit = MatchLdsfld_Lizard_Animation_Spit,
        s_MatchLdsfld_ModManager_MMF = MatchLdsfld_ModManager_MMF,
        s_MatchLdsfld_ModManager_MSC = MatchLdsfld_ModManager_MSC,
        s_MatchLdsfld_RainWorld_ShadPropLeviathanColorA = MatchLdsfld_RainWorld_ShadPropLeviathanColorA,
        s_MatchLdsfld_RainWorld_ShadPropLeviathanColorB = MatchLdsfld_RainWorld_ShadPropLeviathanColorB,
        s_MatchLdsfld_RainWorld_ShadPropLeviathanColorHead = MatchLdsfld_RainWorld_ShadPropLeviathanColorHead,
        s_MatchLdsfld_SoundID_Egg_Bug_Scurry = MatchLdsfld_SoundID_Egg_Bug_Scurry,
        s_MatchLdsfld_SoundID_Leviathan_Bite = MatchLdsfld_SoundID_Leviathan_Bite,
        s_MatchLdstr__txt = MatchLdstr__txt,
        s_MatchMul = MatchMul,
        s_MatchNewarr_BodyChunk = MatchNewarr_BodyChunk,
        s_MatchNewarr_TailSegment = MatchNewarr_TailSegment,
        s_MatchNewarr_PhysicalObject_BodyChunkConnection = MatchNewarr_PhysicalObject_BodyChunkConnection,
        s_MatchNewobj_AbstractCreature = MatchNewobj_AbstractCreature,
        s_MatchNewobj_BodyChunk = MatchNewobj_BodyChunk,
        s_MatchNewobj_CentipedeShell = MatchNewobj_CentipedeShell,
        s_MatchNewobj_Color = MatchNewobj_Color,
        s_MatchNewobj_DaddyBubble = MatchNewobj_DaddyBubble,
        s_MatchNewobj_DaddyRipple = MatchNewobj_DaddyRipple,
        s_MatchRet = MatchRet,
        s_MatchStfld_Antennae_length = MatchStfld_Antennae_length,
        s_MatchStfld_BigEelGraphics_tailSwim = MatchStfld_BigEelGraphics_tailSwim,
        s_MatchStfld_BodyChunk_vel = MatchStfld_BodyChunk_vel,
        s_MatchStfld_CentipedeAI_annoyingCollisions = MatchStfld_CentipedeAI_annoyingCollisions,
        s_MatchStfld_CentipedeAI_excitement = MatchStfld_CentipedeAI_excitement,
        s_MatchStfld_CreatureState_meatLeft = MatchStfld_CreatureState_meatLeft,
        s_MatchStfld_LizardAI_LizardTrackState_vultureMask = MatchStfld_LizardAI_LizardTrackState_vultureMask,
        s_MatchStfld_NoiseTracker_hearingSkill = MatchStfld_NoiseTracker_hearingSkill,
        s_MatchStfld_YellowAI_commFlicker = MatchStfld_YellowAI_commFlicker,
        s_MatchStloc_Any = MatchStloc_Any,
        s_MatchStloc_InLoc1 = MatchStloc_InLoc1,
        s_MatchStloc_OutLoc1 = MatchStloc_OutLoc1,
        s_MatchStloc_OutLoc2 = MatchStloc_OutLoc2,
        s_MatchSub = MatchSub;

    internal static void Dispose()
    {
        s_label = null;
        s_ref = null;
        s_Math_Max_int_int = null!;
        s_Random_Range_int_int = null!;
        s_Room_MiddleOfTile_Vector2 = null!;
        s_Room_ViewedByAnyCamera_Vector2_float = null!;
        s_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type = null!;
        s_string_op_Equality_string_string = null!;
        s_string_Substring_int_int = null!;
        s_World_GetAbstractRoom_int = null!;
        s_World_GetAbstractRoom_WorldCoordinate = null!;
        s_MatchAdd = null!;
        s_MatchBeq_Any = null!;
        s_MatchBeq_OutLabel = null!;
        s_MatchBle_OutLabel = null!;
        s_MatchBneUn_Any = null!;
        s_MatchBr_Any = null!;
        s_MatchBrfalse_Any = null!;
        s_MatchBrfalse_OutLabel = null!;
        s_MatchBrtrue_OutLabel = null!;
        s_MatchCall_Any = null!;
        s_MatchCall_Creature_Violence = null!;
        s_MatchCall_Math_Max_int_int = null!;
        s_MatchCall_Mathf_Lerp = null!;
        s_MatchCall_Mathf_Max = null!;
        s_MatchCall_Mathf_RoundToInt = null!;
        s_MatchCall_ModManager_get_DLCShared = null!;
        s_MatchCall_op_Equality_Any = null!;
        s_MatchCall_Random_Range_int_int = null!;
        s_MatchCall_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type = null!;
        s_MatchCall_string_op_Equality_string_string = null!;
        s_MatchCall_Vector2_op_Addition = null!;
        s_MatchCall_Vector2_op_Multiply = null!;
        s_MatchCallOrCallvirt_AbstractCreature_get_realizedCreature = null!;
        s_MatchCallOrCallvirt_AbstractRoom_AddEntity = null!;
        s_MatchCallOrCallvirt_AbstractRoom_AttractionForCreature = null!;
        s_MatchCallOrCallvirt_AbstractWorldEntity_get_Room = null!;
        s_MatchCallOrCallvirt_Any = null!;
        s_MatchCallOrCallvirt_ArtificialIntelligence_StaticRelationship = null!;
        s_MatchCallOrCallvirt_Centipede_get_Centiwing = null!;
        s_MatchCallOrCallvirt_Centipede_get_Red = null!;
        s_MatchCallOrCallvirt_Creature_get_abstractCreature = null!;
        s_MatchCallOrCallvirt_Creature_get_grasps = null!;
        s_MatchCallOrCallvirt_Creature_get_Template = null!;
        s_MatchCallOrCallvirt_CreatureTemplate_get_IsVulture = null!;
        s_MatchCallOrCallvirt_GraphicsModule_AddToContainer = null!;
        s_MatchCallOrCallvirt_HealthState_get_ClampedHealth = null!;
        s_MatchCallOrCallvirt_IconSymbol_Draw = null!;
        s_MatchCallOrCallvirt_Lizard_get_Swimmer = null!;
        s_MatchCallOrCallvirt_LizardAI_get_lizard = null!;
        s_MatchCallOrCallvirt_LizardAI_LizardSpitTracker_get_lizardAI = null!;
        s_MatchCallOrCallvirt_OutRef = null!;
        s_MatchCallOrCallvirt_Player_get_CanPutSpearToBack = null!;
        s_MatchCallOrCallvirt_RainWorldGame_get_world = null!;
        s_MatchCallOrCallvirt_Room_get_abstractRoom = null!;
        s_MatchCallOrCallvirt_Room_MiddleOfTile_Vector2 = null!;
        s_MatchCallOrCallvirt_Room_ViewedByAnyCamera_Vector2_float = null!;
        s_MatchCallOrCallvirt_string_get_Length = null!;
        s_MatchCallOrCallvirt_string_Substring_int_int = null!;
        s_MatchCallOrCallvirt_World_get_firstRoomIndex = null!;
        s_MatchCallOrCallvirt_World_GetAbstractRoom_int = null!;
        s_MatchCallOrCallvirt_World_GetAbstractRoom_WorldCoordinate = null!;
        s_MatchConvI4 = null!;
        s_MatchDiv = null!;
        s_MatchIsinst_BigSpider = null!;
        s_MatchIsinst_Cicada = null!;
        s_MatchIsinst_Fly = null!;
        s_MatchIsinst_InsectoidCreature = null!;
        s_MatchIsinst_IPlayerEdible = null!;
        s_MatchIsinst_LizardAI_LizardTrackState = null!;
        s_MatchIsinst_KarmaFlower = null!;
        s_MatchIsinst_PuffBall = null!;
        s_MatchLdarg_0 = null!;
        s_MatchLdarg_1 = null!;
        s_MatchLdarg_2 = null!;
        s_MatchLdcI4_M1 = null!;
        s_MatchLdcI4_0 = null!;
        s_MatchLdcI4_1 = null!;
        s_MatchLdcI4_4 = null!;
        s_MatchLdcI4_20 = null!;
        s_MatchLdcI4_60 = null!;
        s_MatchLdcR4_0_1 = null!;
        s_MatchLdcR4_0_3 = null!;
        s_MatchLdcR4_0_7 = null!;
        s_MatchLdcR4_1 = null!;
        s_MatchLdcR4_2 = null!;
        s_MatchLdcR4_2_3 = null!;
        s_MatchLdcR4_7 = null!;
        s_MatchLdcR4_15 = null!;
        s_MatchLdcR4_45 = null!;
        s_MatchLdcR4_60 = null!;
        s_MatchLdcR4_1000 = null!;
        s_MatchLdelema_AbstractRoomNode = null!;
        s_MatchLdelemAny_WorldCoordinate = null!;
        s_MatchLdelemI4 = null!;
        s_MatchLdelemRef = null!;
        s_MatchLdelemU1 = null!;
        s_MatchLdfld_AbstractCreature_creatureTemplate = null!;
        s_MatchLdfld_AbstractCreatureAI_parent = null!;
        s_MatchLdfld_AbstractCreature_state = null!;
        s_MatchLdfld_AbstractCreatureAI_world = null!;
        s_MatchLdfld_AbstractPhysicalObject_type = null!;
        s_MatchLdfld_AbstractRoom_connections = null!;
        s_MatchLdfld_AbstractRoom_creatures = null!;
        s_MatchLdfld_AbstractRoom_nodes = null!;
        s_MatchLdfld_AbstractRoomNode_type = null!;
        s_MatchLdfld_AbstractWorldEntity_world = null!;
        s_MatchLdfld_BigEel_swimSpeed = null!;
        s_MatchLdfld_BigEelGraphics_eel = null!;
        s_MatchLdfld_BigSpider_jumpStamina = null!;
        s_MatchLdfld_BigSpider_spitter = null!;
        s_MatchLdfld_BigSpiderAI_bug = null!;
        s_MatchLdfld_Centipede_size = null!;
        s_MatchLdfld_CentipedeAI_centipede = null!;
        s_MatchLdfld_CentipedeGraphics_lightSource = null!;
        s_MatchLdfld_CollectiblesTracker_displayRegions = null!;
        s_MatchLdfld_CollectiblesTracker_sprites = null!;
        s_MatchLdfld_CreatureTemplate_type = null!;
        s_MatchLdfld_Creature_Grasp_grabbed = null!;
        s_MatchLdfld_GameSession_game = null!;
        s_MatchLdfld_GarbageWormAI_CreatureInterest_crit = null!;
        s_MatchLdfld_IconSymbol_IconSymbolData_critType = null!;
        s_MatchLdfld_Leech_school = null!;
        s_MatchLdfld_Leech_LeechSchool_prey = null!;
        s_MatchLdfld_Leech_LeechSchool_LeechPrey_creature = null!;
        s_MatchLdfld_Lizard_animation = null!;
        s_MatchLdfld_LizardAI_LizardTrackState_vultureMask = null!;
        s_MatchLdfld_LizardAI_LurkTracker_lizard = null!;
        s_MatchLdfld_LizardGraphics_lizard = null!;
        s_MatchLdfld_PathFinder_world = null!;
        s_MatchLdfld_Player_objectInStomach = null!;
        s_MatchLdfld_Player_swallowAndRegurgitateCounter = null!;
        s_MatchLdfld_Player_InputPackage_jmp = null!;
        s_MatchLdfld_Player_InputPackage_thrw = null!;
        s_MatchLdfld_RelationshipTracker_DynamicRelationship_state = null!;
        s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep = null!;
        s_MatchLdfld_RoomBorderExit_type = null!;
        s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix = null!;
        s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world = null!;
        s_MatchLdfld_ShortcutHandler_Vessel_creature = null!;
        s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature = null!;
        s_MatchLdfld_UpdatableAndDeletable_room = null!;
        s_MatchLdfld_World_seaAccessNodes = null!;
        s_MatchLdfld_WorldCoordinate_abstractNode = null!;
        s_MatchLdfld_WorldCoordinate_room = null!;
        s_MatchLdfld_YellowAI_lizard = null!;
        s_MatchLdflda_Creature_inputWithDiagonals = null!;
        s_MatchLdflda_Creature_lastInputWithDiagonals = null!;
        s_MatchLdlen = null!;
        s_MatchLdloc_Any = null!;
        s_MatchLdloc_InLoc1 = null!;
        s_MatchLdloc_InLoc2 = null!;
        s_MatchLdloc_OutLoc1 = null!;
        s_MatchLdloc_OutLoc2 = null!;
        s_MatchLdloca_Any = null!;
        s_MatchLdnull = null!;
        s_MatchLdsfld_AbstractPhysicalObject_AbstractObjectType_JellyFish = null!;
        s_MatchLdsfld_AbstractRoom_CreatureRoomAttraction_Forbidden = null!;
        s_MatchLdsfld_AbstractRoomNode_Type_SeaExit = null!;
        s_MatchLdsfld_CreatureTemplate_Type_BigEel = null!;
        s_MatchLdsfld_CreatureTemplate_Type_BigSpider = null!;
        s_MatchLdsfld_CreatureTemplate_Type_BlackLizard = null!;
        s_MatchLdsfld_CreatureTemplate_Type_CyanLizard = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Leech = null!;
        s_MatchLdsfld_CreatureTemplate_Type_MirosBird = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Overseer = null!;
        s_MatchLdsfld_CreatureTemplate_Type_PoleMimic = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Salamander = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Scavenger = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Slugcat = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Snail = null!;
        s_MatchLdsfld_CreatureTemplate_Type_SpitterSpider = null!;
        s_MatchLdsfld_CreatureTemplate_Type_Vulture = null!;
        s_MatchLdsfld_CreatureTemplate_Type_WhiteLizard = null!;
        s_MatchLdsfld_CreatureTemplate_Type_YellowLizard = null!;
        s_MatchLdsfld_Lizard_Animation_Spit = null!;
        s_MatchLdsfld_ModManager_MMF = null!;
        s_MatchLdsfld_ModManager_MSC = null!;
        s_MatchLdsfld_RainWorld_ShadPropLeviathanColorA = null!;
        s_MatchLdsfld_RainWorld_ShadPropLeviathanColorB = null!;
        s_MatchLdsfld_RainWorld_ShadPropLeviathanColorHead = null!;
        s_MatchLdsfld_SoundID_Egg_Bug_Scurry = null!;
        s_MatchLdsfld_SoundID_Leviathan_Bite = null!;
        s_MatchLdstr__txt = null!;
        s_MatchMul = null!;
        s_MatchNewarr_BodyChunk = null!;
        s_MatchNewarr_TailSegment = null!;
        s_MatchNewarr_PhysicalObject_BodyChunkConnection = null!;
        s_MatchNewobj_AbstractCreature = null!;
        s_MatchNewobj_BodyChunk = null!;
        s_MatchNewobj_CentipedeShell = null!;
        s_MatchNewobj_Color = null!;
        s_MatchNewobj_DaddyBubble = null!;
        s_MatchNewobj_DaddyRipple = null!;
        s_MatchRet = null!;
        s_MatchStfld_Antennae_length = null!;
        s_MatchStfld_BigEelGraphics_tailSwim = null!;
        s_MatchStfld_BodyChunk_vel = null!;
        s_MatchStfld_CentipedeAI_annoyingCollisions = null!;
        s_MatchStfld_CentipedeAI_excitement = null!;
        s_MatchStfld_CreatureState_meatLeft = null!;
        s_MatchStfld_LizardAI_LizardTrackState_vultureMask = null!;
        s_MatchStfld_NoiseTracker_hearingSkill = null!;
        s_MatchStfld_YellowAI_commFlicker = null!;
        s_MatchStloc_Any = null!;
        s_MatchStloc_InLoc1 = null!;
        s_MatchStloc_OutLoc1 = null!;
        s_MatchStloc_OutLoc2 = null!;
        s_MatchSub = null!;
    }

    internal static bool MatchAdd(Instruction x) => x.MatchAdd();

    internal static bool MatchBeq_Any(Instruction x) => x.MatchBeq(out _);

    internal static bool MatchBeq_OutLabel(Instruction x) => x.MatchBeq(out s_label);

    internal static bool MatchBle_OutLabel(Instruction x) => x.MatchBle(out s_label);

    internal static bool MatchBneUn_Any(Instruction x) => x.MatchBneUn(out _);

    internal static bool MatchBr_Any(Instruction x) => x.MatchBr(out _);

    internal static bool MatchBrfalse_Any(Instruction x) => x.MatchBrfalse(out _);

    internal static bool MatchBrfalse_OutLabel(Instruction x) => x.MatchBrfalse(out s_label);

    internal static bool MatchBrtrue_OutLabel(Instruction x) => x.MatchBrtrue(out s_label);

    internal static bool MatchCall_Any(Instruction x) => x.MatchCall(out _);

    internal static bool MatchCall_Creature_Violence(Instruction x) => x.MatchCall<Creature>("Violence");

    internal static bool MatchCall_Math_Max_int_int(Instruction x) => x.MatchCall(s_Math_Max_int_int);

    internal static bool MatchCall_Mathf_Lerp(Instruction x) => x.MatchCall<Mathf>("Lerp");

    internal static bool MatchCall_Mathf_Max(Instruction x) => x.MatchCall<Mathf>("Max");

    internal static bool MatchCall_Mathf_RoundToInt(Instruction x) => x.MatchCall<Mathf>("RoundToInt");

    internal static bool MatchCall_op_Equality_Any(Instruction x) => x.OpCode == OpCodes.Call && x.Operand is MethodReference r && r.Name.Contains("op_Equality");

    internal static bool MatchCall_Random_Range_int_int(Instruction x) => x.MatchCall(s_Random_Range_int_int);

    internal static bool MatchCall_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type(Instruction x) => x.MatchCall(s_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type);

    internal static bool MatchCall_string_op_Equality_string_string(Instruction x) => x.MatchCall(s_string_op_Equality_string_string);

    internal static bool MatchCall_Vector2_op_Addition(Instruction x) => x.MatchCall<Vector2>("op_Addition");

    internal static bool MatchCall_Vector2_op_Multiply(Instruction x) => x.MatchCall<Vector2>("op_Multiply");

    internal static bool MatchCallOrCallvirt_AbstractCreature_get_realizedCreature(Instruction x) => x.MatchCallOrCallvirt<AbstractCreature>("get_realizedCreature");

    internal static bool MatchCallOrCallvirt_AbstractRoom_AddEntity(Instruction x) => x.MatchCallOrCallvirt<AbstractRoom>("AddEntity");

    internal static bool MatchCallOrCallvirt_AbstractRoom_AttractionForCreature(Instruction x) => x.MatchCallOrCallvirt<AbstractRoom>("AttractionForCreature");

    internal static bool MatchCallOrCallvirt_AbstractWorldEntity_get_Room(Instruction x) => x.MatchCallOrCallvirt<AbstractWorldEntity>("get_Room");

    internal static bool MatchCallOrCallvirt_Any(Instruction x) => x.MatchCallOrCallvirt(out _);

    internal static bool MatchCallOrCallvirt_ArtificialIntelligence_StaticRelationship(Instruction x) => x.MatchCallOrCallvirt<ArtificialIntelligence>("StaticRelationship");

    internal static bool MatchCallOrCallvirt_Centipede_get_Centiwing(Instruction x) => x.MatchCallOrCallvirt<Centipede>("get_Centiwing");

    internal static bool MatchCallOrCallvirt_Centipede_get_Red(Instruction x) => x.MatchCallOrCallvirt<Centipede>("get_Red");

    internal static bool MatchCallOrCallvirt_Creature_get_abstractCreature(Instruction x) => x.MatchCallOrCallvirt<Creature>("get_abstractCreature");

    internal static bool MatchCallOrCallvirt_Creature_get_grasps(Instruction x) => x.MatchCallOrCallvirt<Creature>("get_grasps");

    internal static bool MatchCallOrCallvirt_Creature_get_Template(Instruction x) => x.MatchCallOrCallvirt<Creature>("get_Template");

    internal static bool MatchCallOrCallvirt_CreatureTemplate_get_IsVulture(Instruction x) => x.MatchCallOrCallvirt<CreatureTemplate>("get_IsVulture");

    internal static bool MatchCallOrCallvirt_GraphicsModule_AddToContainer(Instruction x) => x.MatchCallOrCallvirt<GraphicsModule>("AddToContainer");

    internal static bool MatchCallOrCallvirt_HealthState_get_ClampedHealth(Instruction x) => x.MatchCallOrCallvirt<HealthState>("get_ClampedHealth");

    internal static bool MatchCallOrCallvirt_IconSymbol_Draw(Instruction x) => x.MatchCallOrCallvirt<IconSymbol>("Draw");

    internal static bool MatchCallOrCallvirt_Lizard_get_Swimmer(Instruction x) => x.MatchCallOrCallvirt<Lizard>("get_Swimmer");

    internal static bool MatchCallOrCallvirt_LizardAI_get_lizard(Instruction x) => x.MatchCallOrCallvirt<LizardAI>("get_lizard");

    internal static bool MatchCallOrCallvirt_LizardAI_LizardSpitTracker_get_lizardAI(Instruction x) => x.MatchCallOrCallvirt<LizardAI.LizardSpitTracker>("get_lizardAI");

    internal static bool MatchCall_ModManager_get_DLCShared(Instruction x) => x.MatchCall<ModManager>("get_DLCShared");

    internal static bool MatchCallOrCallvirt_OutRef(Instruction x) => x.MatchCallOrCallvirt(out s_ref);

    internal static bool MatchCallOrCallvirt_Player_get_CanPutSpearToBack(Instruction x) => x.MatchCallOrCallvirt<Player>("get_CanPutSpearToBack");

    internal static bool MatchCallOrCallvirt_RainWorldGame_get_world(Instruction x) => x.MatchCallOrCallvirt<RainWorldGame>("get_world");

    internal static bool MatchCallOrCallvirt_Room_get_abstractRoom(Instruction x) => x.MatchCallOrCallvirt<Room>("get_abstractRoom");

    internal static bool MatchCallOrCallvirt_Room_MiddleOfTile_Vector2(Instruction x) => x.MatchCallOrCallvirt(s_Room_MiddleOfTile_Vector2);

    internal static bool MatchCallOrCallvirt_Room_ViewedByAnyCamera_Vector2_float(Instruction x) => x.MatchCallOrCallvirt(s_Room_ViewedByAnyCamera_Vector2_float);

    internal static bool MatchCallOrCallvirt_string_get_Length(Instruction x) => x.MatchCallOrCallvirt<string>("get_Length");

    internal static bool MatchCallOrCallvirt_string_Substring_int_int(Instruction x) => x.MatchCallOrCallvirt(s_string_Substring_int_int);

    internal static bool MatchCallOrCallvirt_World_get_firstRoomIndex(Instruction x) => x.MatchCallOrCallvirt<World>("get_firstRoomIndex");

    internal static bool MatchCallOrCallvirt_World_GetAbstractRoom_int(Instruction x) => x.MatchCallOrCallvirt(s_World_GetAbstractRoom_int);

    internal static bool MatchCallOrCallvirt_World_GetAbstractRoom_WorldCoordinate(Instruction x) => x.MatchCallOrCallvirt(s_World_GetAbstractRoom_WorldCoordinate);

    internal static bool MatchConvI4(Instruction x) => x.MatchConvI4();

    internal static bool MatchDiv(Instruction x) => x.MatchDiv();

    internal static bool MatchIsinst_BigSpider(Instruction x) => x.MatchIsinst<BigSpider>();

    internal static bool MatchIsinst_Cicada(Instruction x) => x.MatchIsinst<Cicada>();

    internal static bool MatchIsinst_Fly(Instruction x) => x.MatchIsinst<Fly>();

    internal static bool MatchIsinst_InsectoidCreature(Instruction x) => x.MatchIsinst<InsectoidCreature>();

    internal static bool MatchIsinst_IPlayerEdible(Instruction x) => x.MatchIsinst<IPlayerEdible>();

    internal static bool MatchIsinst_LizardAI_LizardTrackState(Instruction x) => x.MatchIsinst<LizardAI.LizardTrackState>();

    internal static bool MatchIsinst_KarmaFlower(Instruction x) => x.MatchIsinst<KarmaFlower>();

    internal static bool MatchIsinst_PuffBall(Instruction x) => x.MatchIsinst<PuffBall>();

    internal static bool MatchLdarg_0(Instruction x) => x.MatchLdarg(0);

    internal static bool MatchLdarg_1(Instruction x) => x.MatchLdarg(1);

    internal static bool MatchLdarg_2(Instruction x) => x.MatchLdarg(2);

    internal static bool MatchLdcI4_M1(Instruction x) => x.MatchLdcI4(-1);

    internal static bool MatchLdcI4_0(Instruction x) => x.MatchLdcI4(0);

    internal static bool MatchLdcI4_1(Instruction x) => x.MatchLdcI4(1);

    internal static bool MatchLdcI4_4(Instruction x) => x.MatchLdcI4(4);

    internal static bool MatchLdcI4_20(Instruction x) => x.MatchLdcI4(20);

    internal static bool MatchLdcI4_60(Instruction x) => x.MatchLdcI4(60);

    internal static bool MatchLdcR4_0_1(Instruction x) => x.MatchLdcR4(.1f);

    internal static bool MatchLdcR4_0_3(Instruction x) => x.MatchLdcR4(.3f);

    internal static bool MatchLdcR4_0_7(Instruction x) => x.MatchLdcR4(.7f);

    internal static bool MatchLdcR4_1(Instruction x) => x.MatchLdcR4(1f);

    internal static bool MatchLdcR4_2(Instruction x) => x.MatchLdcR4(2f);

    internal static bool MatchLdcR4_2_3(Instruction x) => x.MatchLdcR4(2.3f);

    internal static bool MatchLdcR4_7(Instruction x) => x.MatchLdcR4(7f);

    internal static bool MatchLdcR4_15(Instruction x) => x.MatchLdcR4(15f);

    internal static bool MatchLdcR4_45(Instruction x) => x.MatchLdcR4(45f);

    internal static bool MatchLdcR4_60(Instruction x) => x.MatchLdcR4(60f);

    internal static bool MatchLdcR4_1000(Instruction x) => x.MatchLdcR4(1000f);

    internal static bool MatchLdelema_AbstractRoomNode(Instruction x) => x.MatchLdelema<AbstractRoomNode>();

    internal static bool MatchLdelemAny_WorldCoordinate(Instruction x) => x.MatchLdelemAny<WorldCoordinate>();

    internal static bool MatchLdelemI4(Instruction x) => x.MatchLdelemI4();

    internal static bool MatchLdelemRef(Instruction x) => x.MatchLdelemRef();

    internal static bool MatchLdelemU1(Instruction x) => x.MatchLdelemU1();

    internal static bool MatchLdfld_AbstractCreature_creatureTemplate(Instruction x) => x.MatchLdfld<AbstractCreature>("creatureTemplate");

    internal static bool MatchLdfld_AbstractCreatureAI_parent(Instruction x) => x.MatchLdfld<AbstractCreatureAI>("parent");

    internal static bool MatchLdfld_AbstractCreature_state(Instruction x) => x.MatchLdfld<AbstractCreature>("state");

    internal static bool MatchLdfld_AbstractCreatureAI_world(Instruction x) => x.MatchLdfld<AbstractCreatureAI>("world");

    internal static bool MatchLdfld_AbstractPhysicalObject_type(Instruction x) => x.MatchLdfld<AbstractPhysicalObject>("type");

    internal static bool MatchLdfld_AbstractRoom_connections(Instruction x) => x.MatchLdfld<AbstractRoom>("connections");

    internal static bool MatchLdfld_AbstractRoom_creatures(Instruction x) => x.MatchLdfld<AbstractRoom>("creatures");

    internal static bool MatchLdfld_AbstractRoom_nodes(Instruction x) => x.MatchLdfld<AbstractRoom>("nodes");

    internal static bool MatchLdfld_AbstractRoomNode_type(Instruction x) => x.MatchLdfld<AbstractRoomNode>("type");

    internal static bool MatchLdfld_AbstractWorldEntity_world(Instruction x) => x.MatchLdfld<AbstractWorldEntity>("world");

    internal static bool MatchLdfld_BigEel_swimSpeed(Instruction x) => x.MatchLdfld<BigEel>("swimSpeed");

    internal static bool MatchLdfld_BigEelGraphics_eel(Instruction x) => x.MatchLdfld<BigEelGraphics>("eel");

    internal static bool MatchLdfld_BigSpider_jumpStamina(Instruction x) => x.MatchLdfld<BigSpider>("jumpStamina");

    internal static bool MatchLdfld_BigSpider_spitter(Instruction x) => x.MatchLdfld<BigSpider>("spitter");

    internal static bool MatchLdfld_BigSpiderAI_bug(Instruction x) => x.MatchLdfld<BigSpiderAI>("bug");

    internal static bool MatchLdfld_Centipede_size(Instruction x) => x.MatchLdfld<Centipede>("size");

    internal static bool MatchLdfld_CentipedeAI_centipede(Instruction x) => x.MatchLdfld<CentipedeAI>("centipede");

    internal static bool MatchLdfld_CentipedeGraphics_lightSource(Instruction x) => x.MatchLdfld<CentipedeGraphics>("lightSource");

    internal static bool MatchLdfld_CollectiblesTracker_displayRegions(Instruction x) => x.MatchLdfld<CollectiblesTracker>("displayRegions");

    internal static bool MatchLdfld_CollectiblesTracker_sprites(Instruction x) => x.MatchLdfld<CollectiblesTracker>("sprites");

    internal static bool MatchLdfld_CreatureTemplate_type(Instruction x) => x.MatchLdfld<CreatureTemplate>("type");

    internal static bool MatchLdfld_Creature_Grasp_grabbed(Instruction x) => x.MatchLdfld<Creature.Grasp>("grabbed");

    internal static bool MatchLdfld_GameSession_game(Instruction x) => x.MatchLdfld<GameSession>("game");

    internal static bool MatchLdfld_GarbageWormAI_CreatureInterest_crit(Instruction x) => x.MatchLdfld<GarbageWormAI.CreatureInterest>("crit");

    internal static bool MatchLdfld_IconSymbol_IconSymbolData_critType(Instruction x) => x.MatchLdfld<IconSymbol.IconSymbolData>("critType");

    internal static bool MatchLdfld_Leech_school(Instruction x) => x.MatchLdfld<Leech>("school");

    internal static bool MatchLdfld_Leech_LeechSchool_prey(Instruction x) => x.MatchLdfld<Leech.LeechSchool>("prey");

    internal static bool MatchLdfld_Leech_LeechSchool_LeechPrey_creature(Instruction x) => x.MatchLdfld<Leech.LeechSchool.LeechPrey>("creature");

    internal static bool MatchLdfld_Lizard_animation(Instruction x) => x.MatchLdfld<Lizard>("animation");

    internal static bool MatchLdfld_LizardAI_LizardTrackState_vultureMask(Instruction x) => x.MatchLdfld<LizardAI.LizardTrackState>("vultureMask");

    internal static bool MatchLdfld_LizardAI_LurkTracker_lizard(Instruction x) => x.MatchLdfld<LizardAI.LurkTracker>("lizard");

    internal static bool MatchLdfld_LizardGraphics_lizard(Instruction x) => x.MatchLdfld<LizardGraphics>("lizard");

    internal static bool MatchLdfld_PathFinder_world(Instruction x) => x.MatchLdfld<PathFinder>("world");

    internal static bool MatchLdfld_Player_objectInStomach(Instruction x) => x.MatchLdfld<Player>("objectInStomach");

    internal static bool MatchLdfld_Player_swallowAndRegurgitateCounter(Instruction x) => x.MatchLdfld<Player>("swallowAndRegurgitateCounter");

    internal static bool MatchLdfld_Player_InputPackage_jmp(Instruction x) => x.MatchLdfld<Player.InputPackage>("jmp");

    internal static bool MatchLdfld_Player_InputPackage_thrw(Instruction x) => x.MatchLdfld<Player.InputPackage>("thrw");

    internal static bool MatchLdfld_RelationshipTracker_DynamicRelationship_state(Instruction x) => x.MatchLdfld<RelationshipTracker.DynamicRelationship>("state");

    internal static bool MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep(Instruction x) => x.MatchLdfld<RelationshipTracker.DynamicRelationship>("trackerRep");

    internal static bool MatchLdfld_RoomBorderExit_type(Instruction x) => x.MatchLdfld<RoomBorderExit>("type");

    internal static bool MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix(Instruction x) => x.MatchLdfld<ScavengersWorldAI.WorldFloodFiller>("nodesMatrix");

    internal static bool MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world(Instruction x) => x.MatchLdfld<ScavengersWorldAI.WorldFloodFiller>("world");

    internal static bool MatchLdfld_ShortcutHandler_Vessel_creature(Instruction x) => x.MatchLdfld<ShortcutHandler.Vessel>("creature");

    internal static bool MatchLdfld_Tracker_CreatureRepresentation_representedCreature(Instruction x) => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature");

    internal static bool MatchLdfld_UpdatableAndDeletable_room(Instruction x) => x.MatchLdfld<UpdatableAndDeletable>("room");

    internal static bool MatchLdfld_World_seaAccessNodes(Instruction x) => x.MatchLdfld<World>("seaAccessNodes");

    internal static bool MatchLdfld_WorldCoordinate_abstractNode(Instruction x) => x.MatchLdfld<WorldCoordinate>("abstractNode");

    internal static bool MatchLdfld_WorldCoordinate_room(Instruction x) => x.MatchLdfld<WorldCoordinate>("room");

    internal static bool MatchLdfld_YellowAI_lizard(Instruction x) => x.MatchLdfld<YellowAI>("lizard");

    internal static bool MatchLdflda_Creature_inputWithDiagonals(Instruction x) => x.MatchLdflda<Creature>("inputWithDiagonals");

    internal static bool MatchLdflda_Creature_lastInputWithDiagonals(Instruction x) => x.MatchLdflda<Creature>("lastInputWithDiagonals");

    internal static bool MatchLdlen(Instruction x) => x.MatchLdlen();

    internal static bool MatchLdloc_Any(Instruction x) => x.MatchLdloc(out _);

    internal static bool MatchLdloc_InLoc1(Instruction x) => x.MatchLdloc(s_loc1);

    internal static bool MatchLdloc_InLoc2(Instruction x) => x.MatchLdloc(s_loc2);

    internal static bool MatchLdloc_OutLoc1(Instruction x) => x.MatchLdloc(out s_loc1);

    internal static bool MatchLdloc_OutLoc2(Instruction x) => x.MatchLdloc(out s_loc2);

    internal static bool MatchLdloca_Any(Instruction x) => x.MatchLdloca(out _);

    internal static bool MatchLdnull(Instruction x) => x.MatchLdnull();

    internal static bool MatchLdsfld_AbstractPhysicalObject_AbstractObjectType_JellyFish(Instruction x) => x.MatchLdsfld<AbstractPhysicalObject.AbstractObjectType>("JellyFish");

    internal static bool MatchLdsfld_AbstractRoom_CreatureRoomAttraction_Forbidden(Instruction x) => x.MatchLdsfld<AbstractRoom.CreatureRoomAttraction>("Forbidden");

    internal static bool MatchLdsfld_AbstractRoomNode_Type_SeaExit(Instruction x) => x.MatchLdsfld<AbstractRoomNode.Type>("SeaExit");

    internal static bool MatchLdsfld_CreatureTemplate_Type_BigEel(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("BigEel");

    internal static bool MatchLdsfld_CreatureTemplate_Type_BigSpider(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("BigSpider");

    internal static bool MatchLdsfld_CreatureTemplate_Type_BlackLizard(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("BlackLizard");

    internal static bool MatchLdsfld_CreatureTemplate_Type_CyanLizard(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("CyanLizard");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Leech(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Leech");

    internal static bool MatchLdsfld_CreatureTemplate_Type_MirosBird(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("MirosBird");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Overseer(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Overseer");

    internal static bool MatchLdsfld_CreatureTemplate_Type_PoleMimic(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("PoleMimic");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Salamander(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Salamander");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Scavenger(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Scavenger");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Slugcat(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Slugcat");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Snail(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Snail");

    internal static bool MatchLdsfld_CreatureTemplate_Type_SpitterSpider(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("SpitterSpider");

    internal static bool MatchLdsfld_CreatureTemplate_Type_Vulture(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("Vulture");

    internal static bool MatchLdsfld_CreatureTemplate_Type_WhiteLizard(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("WhiteLizard");

    internal static bool MatchLdsfld_CreatureTemplate_Type_YellowLizard(Instruction x) => x.MatchLdsfld<CreatureTemplate.Type>("YellowLizard");

    internal static bool MatchLdsfld_Lizard_Animation_Spit(Instruction x) => x.MatchLdsfld<Lizard.Animation>("Spit");

    internal static bool MatchLdsfld_ModManager_MMF(Instruction x) => x.MatchLdsfld<ModManager>("MMF");

    internal static bool MatchLdsfld_ModManager_MSC(Instruction x) => x.MatchLdsfld<ModManager>("MSC");

    internal static bool MatchLdsfld_RainWorld_ShadPropLeviathanColorA(Instruction x) => x.MatchLdsfld<RainWorld>("ShadPropLeviathanColorA");

    internal static bool MatchLdsfld_RainWorld_ShadPropLeviathanColorB(Instruction x) => x.MatchLdsfld<RainWorld>("ShadPropLeviathanColorB");

    internal static bool MatchLdsfld_RainWorld_ShadPropLeviathanColorHead(Instruction x) => x.MatchLdsfld<RainWorld>("ShadPropLeviathanColorHead");

    internal static bool MatchLdsfld_SoundID_Egg_Bug_Scurry(Instruction x) => x.MatchLdsfld<SoundID>("Egg_Bug_Scurry");

    internal static bool MatchLdsfld_SoundID_Leviathan_Bite(Instruction x) => x.MatchLdsfld<SoundID>("Leviathan_Bite");

    internal static bool MatchLdstr__txt(Instruction x) => x.MatchLdstr(".txt");

    internal static bool MatchMul(Instruction x) => x.MatchMul();

    internal static bool MatchNewarr_BodyChunk(Instruction x) => x.MatchNewarr<BodyChunk>();

    internal static bool MatchNewarr_TailSegment(Instruction x) => x.MatchNewarr<TailSegment>();

    internal static bool MatchNewarr_PhysicalObject_BodyChunkConnection(Instruction x) => x.MatchNewarr<PhysicalObject.BodyChunkConnection>();

    internal static bool MatchNewobj_AbstractCreature(Instruction x) => x.MatchNewobj<AbstractCreature>();

    internal static bool MatchNewobj_BodyChunk(Instruction x) => x.MatchNewobj<BodyChunk>();

    internal static bool MatchNewobj_CentipedeShell(Instruction x) => x.MatchNewobj<CentipedeShell>();

    internal static bool MatchNewobj_Color(Instruction x) => x.MatchNewobj<Color>();

    internal static bool MatchNewobj_DaddyBubble(Instruction x) => x.MatchNewobj<DaddyBubble>();

    internal static bool MatchNewobj_DaddyRipple(Instruction x) => x.MatchNewobj<DaddyRipple>();

    internal static bool MatchRet(Instruction x) => x.MatchRet();

    internal static bool MatchStfld_Antennae_length(Instruction x) => x.MatchStfld<Antennae>("length");

    internal static bool MatchStfld_BigEelGraphics_tailSwim(Instruction x) => x.MatchStfld<BigEelGraphics>("tailSwim");

    internal static bool MatchStfld_BodyChunk_vel(Instruction x) => x.MatchStfld<BodyChunk>("vel");

    internal static bool MatchStfld_CentipedeAI_annoyingCollisions(Instruction x) => x.MatchStfld<CentipedeAI>("annoyingCollisions");

    internal static bool MatchStfld_CentipedeAI_excitement(Instruction x) => x.MatchStfld<CentipedeAI>("excitement");

    internal static bool MatchStfld_CreatureState_meatLeft(Instruction x) => x.MatchStfld<CreatureState>("meatLeft");

    internal static bool MatchStfld_LizardAI_LizardTrackState_vultureMask(Instruction x) => x.MatchStfld<LizardAI.LizardTrackState>("vultureMask");

    internal static bool MatchStfld_NoiseTracker_hearingSkill(Instruction x) => x.MatchStfld<NoiseTracker>("hearingSkill");

    internal static bool MatchStfld_YellowAI_commFlicker(Instruction x) => x.MatchStfld<YellowAI>("commFlicker");

    internal static bool MatchStloc_Any(Instruction x) => x.MatchStloc(out _);

    internal static bool MatchStloc_InLoc1(Instruction x) => x.MatchStloc(s_loc1);

    internal static bool MatchStloc_OutLoc1(Instruction x) => x.MatchStloc(out s_loc1);

    internal static bool MatchStloc_OutLoc2(Instruction x) => x.MatchStloc(out s_loc2);

    internal static bool MatchSub(Instruction x) => x.MatchSub();
}