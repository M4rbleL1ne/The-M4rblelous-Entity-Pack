global using LBMergedMods.Creatures;
global using LBMergedMods.Items;
using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using Fisobs.Sandbox;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using Watcher;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace LBMergedMods;

[BepInPlugin("lb-fgf-m4r-ik.modpack", "LB Merged Mods", "10.0.8"), BepInDependency("io.github.dual.fisobs"), BepInDependency("com.rainworldgame.lizardcustomizer.plugin", BepInDependency.DependencyFlags.SoftDependency), BepInDependency("slime-cubed.devconsole", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class LBMergedModsPlugin : BaseUnityPlugin
{
    public static AssetBundle? Bundle;
    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
    [AllowNull] internal static ManualLogSource s_logger;
    public static bool NoXyloHoles = File.Exists("noXyloHoles.txt");

    public void OnEnable()
    {
        s_logger = Logger;
        On.RainWorld.OnModsInit += On_RainWorld_OnModsInit;
        On.RainWorld.UnloadResources += On_RainWorld_UnloadResources;
        On.RainWorld.PostModsInit += On_RainWorld_PostModsInit;
        On.RainWorld.OnModsDisabled += On_RainWorld_OnModsDisabled;
        On.SoundLoader.LoadSounds += On_SoundLoader_LoadSounds;
        On.Player.ctor += On_Player_ctor;
        On.Player.CanBeSwallowed += On_Player_CanBeSwallowed;
        On.Player.Update += On_Player_Update;
        IL.Player.TerrainImpact += IL_Player_TerrainImpact;
        On.Player.Stun += On_Player_Stun;
        On.Player.SwallowObject += On_Player_SwallowObject;
        IL.Player.GrabUpdate += IL_Player_GrabUpdate;
        IL.PlayerGraphics.Update += IL_PlayerGraphics_Update;
        IL.SlugcatHand.Update += IL_SlugcatHand_Update;
        On.ArenaBehaviors.SandboxEditor.GetPerformanceEstimate += On_SandboxEditor_GetPerformanceEstimate;
        On.VultureGrub.RayTraceSky += On_VultureGrub_RayTraceSky;
        IL.BigEel.ctor += IL_BigEel_ctor;
        IL.BigEel.AccessSwimSpace += IL_BigEel_AccessSwimSpace;
        IL.BigEel.Swim += IL_BigEel_Swim;
        IL.BigEel.JawsSnap += IL_BigEel_JawsSnap;
        IL.BigEelPather.FollowPath += IL_BigEelPather_FollowPath;
        IL.BigEelGraphics.ctor += IL_BigEelGraphics_ctor;
        IL.BigEelGraphics.Update += IL_BigEelGraphics_Update;
        IL.BigEelAbstractAI.AbstractBehavior += IL_BigEelAbstractAI_AbstractBehavior;
        IL.BigEelAbstractAI.AddRandomCheckRoom += IL_BigEelAbstractAI_AddRandomCheckRoom;
        IL.BigEelAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_BigEelAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.BigEelAI.Update += IL_BigEelAI_Update;
        IL.GarbageWormAI.Update += IL_GarbageWormAI_Update;
        On.PathFinder.CoordinateReachableAndGetbackable += On_PathFinder_CoordinateReachableAndGetbackable;
        On.LizardCosmetics.BumpHawk.DrawSprites += On_BumpHawk_DrawSprites;
        On.LizardCosmetics.JumpRings.DrawSprites += On_JumpRings_DrawSprites;
        On.LizardCosmetics.LongHeadScales.DrawSprites += On_LongHeadScales_DrawSprites;
        On.LizardCosmetics.LongBodyScales.DrawSprites += On_LongBodyScales_DrawSprites;
        On.LizardGraphics.GenerateIvars += On_LizardGraphics_GenerateIvars;
        new Hook(typeof(LizardGraphics).GetMethod("get_HeadColor1", ALL_FLAGS), On_LizardGraphics_get_HeadColor1);
        new Hook(typeof(LizardGraphics).GetMethod("get_HeadColor2", ALL_FLAGS), On_LizardGraphics_get_HeadColor2);
        On.LizardGraphics.BodyColor += On_LizardGraphics_BodyColor;
        On.LizardCosmetics.SpineSpikes.DrawSprites += On_SpineSpikes_DrawSprites;
        On.LizardCosmetics.TailFin.DrawSprites += On_TailFin_DrawSprites;
        On.LizardCosmetics.WingScales.DrawSprites += On_WingScales_DrawSprites;
        IL.LizardCosmetics.TailGeckoScales.DrawSprites += IL_TailGeckoScales_DrawSprites;
        new Hook(typeof(LizardJumpModule).GetMethod("get_canChainJump", ALL_FLAGS), On_LizardJumpModule_get_canChainJump);
        IL.Lizard.Act += IL_Lizard_Act;
        On.JetFishAI.WantToEatObject += On_JetFishAI_WantToEatObject;
        On.PlayerSessionRecord.AddEat += On_PlayerSessionRecord_AddEat;
        new Hook(typeof(LizardGraphics).GetMethod("get_effectColor", ALL_FLAGS), On_LizardGraphics_get_effectColor);
        On.LizardCosmetics.Whiskers.ctor += On_Whiskers_ctor;
        On.LizardCosmetics.Whiskers.AnchorPoint += On_Whiskers_AnchorPoint;
        On.ShortcutGraphics.ShortCutColor += On_ShortcutGraphics_ShortCutColor;
        IL.Lizard.ActAnimation += IL_Lizard_ActAnimation;
        On.LizardSpit.ApplyPalette += On_LizardSpit_ApplyPalette;
        On.LizardSpit.AddToContainer += On_LizardSpit_AddToContainer;
        IL.LizardSpit.Update += IL_LizardSpit_Update;
        IL.LizardAI.LizardSpitTracker.Update += IL_LizardSpitTracker_Update;
        IL.BigSpider.Update += IL_BigSpider_Update;
        IL.BigSpiderAI.Update += IL_BigSpiderAI_Update;
        IL.BigSpiderAI.SpiderSpitModule.SpitPosScore += IL_SpiderSpitModule_SpitPosScore;
        On.BigSpiderAI.SpiderSpitModule.Update += On_SpiderSpitModule_Update;
        new Hook(typeof(BigSpider).GetMethod("get_CanJump", ALL_FLAGS), On_BigSpider_get_CanJump);
        new Hook(typeof(BigSpider).GetMethod("get_CanIBeRevived", ALL_FLAGS), On_BigSpider_get_CanIBeRevived);
        new Hook(typeof(BigSpiderAI).GetMethod("get_ShyFromLight", ALL_FLAGS), On_BigSpiderAI_get_ShyFromLight);
        IL.BigSpider.FlyingWeapon += IL_BigSpider_FlyingWeapon;
        On.BodyPart.ConnectToPoint += On_BodyPart_ConnectToPoint;
        IL.BigSpider.Spit += IL_BigSpider_Spit;
        On.BigSpider.TryInitiateSpit += On_BigSpider_TryInitiateSpit;
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_BigSpiderAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.BigSpider.Collide += IL_BigSpider_Collide;
        On.BigSpiderAI.ReactToNoise += On_BigSpiderAI_ReactToNoise;
        IL.SporeCloud.Update += IL_SporeCloud_Update;
        On.BigSpider.Revive += On_BigSpider_Revive;
        On.BigSpiderAI.SpiderSpitModule.CanSpit += On_SpiderSpitModule_CanSpit;
        IL.BigSpider.Act += IL_BigSpider_Act;
        On.BigSpider.InitiateJump += On_BigSpider_InitiateJump;
        On.Creature.Abstractize += On_Creature_Abstractize;
        IL.Leech.Swim += IL_Leech_Swim;
        IL.SnailAI.TileIdleScore += IL_SnailAI_TileIdleScore;
        On.Snail.Click += On_Snail_Click;
        On.SnailAI.TileIdleScore += On_SnailAI_TileIdleScore;
        On.Vulture.VultureThruster.Update += On_VultureThruster_Update;
        On.Vulture.AirBrake += On_Vulture_AirBrake;
        On.OverseerAbstractAI.HowInterestingIsCreature += On_OverseerAbstractAI_HowInterestingIsCreature;
        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_VultureAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        new Hook(typeof(CreatureTemplate).GetMethod("get_IsVulture", ALL_FLAGS), On_CreatureTemplate_get_IsVulture);
        IL.VultureGrub.AttemptCallVulture += IL_VultureGrub_AttemptCallVulture;
        On.MirosBirdAI.DoIWantToBiteCreature += On_MirosBirdAI_DoIWantToBiteCreature;
        On.VultureAI.DoIWantToBiteCreature += On_VultureAI_DoIWantToBiteCreature;
        On.Lizard.DamageAttackClosestChunk += On_Lizard_DamageAttackClosestChunk;
        On.Scavenger.MeleeGetFree += On_Scavenger_MeleeGetFree;
        IL.ShortcutHandler.FlyingCreatureArrivedInRealizedRoom += IL_ShortcutHandler_FlyingCreatureArrivedInRealizedRoom;
        On.AbstractCreature.Update += On_AbstractCreature_Update;
        On.AbstractCreature.IsEnteringDen += On_AbstractCreature_IsEnteringDen;
        IL.BigEelAbstractAI.AddRoomClusterToCheckList += IL_BigEelAbstractAI_AddRoomClusterToCheckList;
        IL.BigEelGraphics.ApplyPalette += IL_BigEelGraphics_ApplyPalette;
        On.BigEelAI.WantToChargeJaw += On_BigEelAI_WantToChargeJaw;
        On.BigEel.InBiteArea += On_BigEel_InBiteArea;
        On.BigEel.Crush += On_BigEel_Crush;
        On.YellowAI.YellowPack.RemoveLizard_int += On_YellowPack_RemoveLizard_int;
        On.YellowAI.YellowPack.RemoveLizard_AbstractCreature += On_YellowPack_RemoveLizard_AbstractCreature;
        On.YellowAI.YellowPack.FindLeader += On_YellowPack_FindLeader;
        IL.YellowAI.Update += IL_YellowAI_Update;
        IL.Snail.Click += IL_Snail_Click;
        On.LizardCosmetics.AxolotlGills.DrawSprites += On_AxolotlGills_DrawSprites;
        On.LizardAI.LurkTracker.Utility += On_LurkTracker_Utility;
        IL.LizardAI.LurkTracker.LurkPosScore += IL_LurkTracker_LurkPosScore;
        On.LizardAI.IdleSpotScore += On_LizardAI_IdleSpotScore;
        On.Lizard.SwimBehavior += On_Lizard_SwimBehavior;
        On.LizardPather.HeuristicForCell += On_LizardPather_HeuristicForCell;
        IL.Lizard.EnterAnimation += IL_Lizard_EnterAnimation;
        IL.LizardGraphics.UpdateTailSegment += IL_LizardGraphics_UpdateTailSegment;
        IL.LizardGraphics.Update += IL_LizardGraphics_Update;
        IL.Menu.MultiplayerMenu.FindAllLevels += IL_MultiplayerMenu_FindAllLevels;
        IL.DaddyLongLegs.ctor += IL_DaddyLongLegs_ctor;
        On.DaddyLongLegs.InitiateGraphicsModule += On_DaddyLongLegs_InitiateGraphicsModule;
        On.DaddyGraphics.Eye.RenderSlits += On_Eye_RenderSlits;
        IL.DaddyGraphics.ReactToNoise += IL_DaddyGraphics_ReactToNoise;
        On.DaddyTentacle.CollideWithCreature += On_DaddyTentacle_CollideWithCreature;
        On.DaddyLongLegs.ShortCutColor += On_DaddyLongLegs_ShortCutColor;
        On.TentaclePlantGraphics.InitiateSprites += On_TentaclePlantGraphics_InitiateSprites;
        On.TentaclePlantGraphics.ApplyPalette += On_TentaclePlantGraphics_ApplyPalette;
        On.TentaclePlantAI.UpdateDynamicRelationship += On_TentaclePlantAI_UpdateDynamicRelationship;
        On.TentaclePlant.Collide += On_TentaclePlant_Collide;
        On.TentaclePlant.Update += On_TentaclePlant_Update;
        On.Menu.SandboxSettingsInterface.DefaultKillScores += On_SandboxSettingsInterface_DefaultKillScores;
        On.TubeWorm.NewRoom += On_TubeWorm_NewRoom;
        On.TubeWorm.Tongue.Shoot += On_Tongue_Shoot;
        On.TubeWorm.Update += On_TubeWorm_Update;
        On.TubeWormGraphics.Update += On_TubeWormGraphics_Update;
        On.TubeWormGraphics.DrawSprites += On_TubeWormGraphics_DrawSprites;
        On.DevInterface.MapPage.CreatureVis.CritCol += On_CreatureVis_CritCol;
        On.TubeWormGraphics.Reset += On_TubeWormGraphics_Reset;
        On.TubeWormGraphics.ApplyPalette += On_TubeWormGraphics_ApplyPalette;
        On.SLOracleBehaviorHasMark.CreatureJokeDialog += On_SLOracleBehaviorHasMark_CreatureJokeDialog;
        On.SSOracleBehavior.CreatureJokeDialog += On_SSOracleBehavior_CreatureJokeDialog;
        On.ArenaCreatureSpawner.IsMajorCreature += On_ArenaCreatureSpawner_IsMajorCreature;
        new Hook(typeof(Centipede).GetMethod("get_Centiwing", ALL_FLAGS), On_Centipede_get_Centiwing);
        new Hook(typeof(Centipede).GetMethod("get_Red", ALL_FLAGS), On_Centipede_get_Red);
        IL.CentipedeGraphics.DrawSprites += IL_CentipedeGraphics_DrawSprites;
        IL.CentipedeAI.ctor += IL_CentipedeAI_ctor;
        IL.Centipede.Stun += IL_Centipede_Stun;
        IL.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_ScavengerAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_BigSpiderAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.CentipedeGraphics.InitiateSprites += IL_CentipedeGraphics_InitiateSprites;
        On.CentipedeGraphics.WhiskerLength += On_CentipedeGraphics_WhiskerLength;
        IL.CentipedeGraphics.Update += IL_CentipedeGraphics_Update;
        new Hook(typeof(CentipedeGraphics).GetMethod("get_SecondaryShellColor", ALL_FLAGS), On_CentipedeGraphics_get_SecondaryShellColor);
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_CentipedeAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.CentipedeAI.DoIWantToShockCreature += On_CentipedeAI_DoIWantToShockCreature;
        IL.CentipedeAI.VisualScore += IL_CentipedeAI_VisualScore;
        IL.CentipedeAI.Update += IL_CentipedeAI_Update;
        On.Centipede.GenerateSize += On_Centipede_GenerateSize;
        IL.Centipede.ctor += IL_Centipede_ctor;
        IL.Centipede.Violence += IL_Centipede_Violence;
        IL.Centipede.Crawl += IL_Centipede_Crawl;
        IL.Centipede.Shock += IL_Centipede_Shock;
        On.SlugcatStats.NourishmentOfObjectEaten += On_SlugcatStats_NourishmentOfObjectEaten;
        On.AbstractCreature.ctor += On_AbstractCreature_ctor;
        On.Fly.NewRoom += On_Fly_NewRoom;
        On.Fly.Act += On_Fly_Act;
        On.Fly.BatFlight += On_Fly_BatFlight;
        On.FlyAI.Update += On_FlyAI_Update;
        On.FlyGraphics.InitiateSprites += On_FlyGraphics_InitiateSprites;
        On.FlyGraphics.DrawSprites += On_FlyGraphics_DrawSprites;
        On.DevInterface.MapPage.CreatureVis.CritString += On_CreatureVis_CritString;
        On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += On_MultiplayerUnlocks_SymbolDataForSandboxUnlock;
        On.MultiplayerUnlocks.SandboxUnlockForSymbolData += On_MultiplayerUnlocks_SandboxUnlockForSymbolData;
        IL.SandboxGameSession.SpawnEntity += IL_SandboxGameSession_SpawnEntity;
        On.Menu.SandboxSettingsInterface.IsThisSandboxUnlockVisible += On_SandboxSettingsInterface_IsThisSandboxUnlockVisible;
        On.Limb.FindGrip += On_Limb_FindGrip;
        On.BodyPart.OnOtherSideOfTerrain += On_BodyPart_OnOtherSideOfTerrain;
        On.BodyPart.PushOutOfTerrain += On_BodyPart_PushOutOfTerrain;
        On.EggBugAI.UnpleasantFallRisk += On_EggBugAI_UnpleasantFallRisk;
        On.EggBug.DropEggs += On_EggBug_DropEggs;
        On.EggBugAI.IdleScore += On_EggBugAI_IdleScore;
        IL.EggBug.Act += IL_EggBug_Act;
        IL.EggBug.TryJump += IL_EggBug_TryJump;
        IL.EggBugGraphics.Update += IL_EggBugGraphics_Update;
        On.EggBug.Run += On_EggBug_Run;
        On.EggBug.MoveTowards += On_EggBug_MoveTowards;
        new Hook(typeof(EggBugGraphics).GetMethod("get_ShowEggs", ALL_FLAGS), On_EggBugGraphics_get_ShowEggs);
        On.LizardVoice.ctor += On_LizardVoice_ctor;
        On.LizardTongue.ctor += On_LizardTongue_ctor;
        new Hook(typeof(LizardBreedParams).GetMethod("get_WallClimber", ALL_FLAGS), On_LizardBreedParams_get_WallClimber);
        On.LizardGraphics.DynamicBodyColor += On_LizardGraphics_DynamicBodyColor;
        On.LizardGraphics.HeadColor += On_LizardGraphics_HeadColor;
        On.LizardGraphics.WhiteFlicker += On_LizardGraphics_WhiteFlicker;
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.Lizard.HitHeadShield += On_Lizard_HitHeadShield;
        On.LizardBubble.ctor += On_LizardBubble_ctor;
        On.LizardBubble.DrawSprites += On_LizardBubble_DrawSprites;
        On.LizardGraphics.AddCosmetic += On_LizardGraphics_AddCosmetic;
        On.Player.Grabbed += On_Player_Grabbed;
        IL.HUD.Map.Draw += IL_Map_Draw;
        IL.OverseerAbstractAI.HowInterestingIsCreature += IL_OverseerAbstractAI_HowInterestingIsCreature;
        On.LizardLimb.ctor += On_LizardLimb_ctor;
        On.LizardVoice.GetMyVoiceTrigger += On_LizardVoice_GetMyVoiceTrigger;
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
        On.AbstractConsumable.ctor += On_AbstractConsumable_ctor;
        On.AbstractConsumable.IsTypeConsumable += On_AbstractConsumable_IsTypeConsumable;
        On.SaveState.SetCustomData_AbstractPhysicalObject_string += On_SaveState_SetCustomData_AbstractPhysicalObject_string;
        On.SaveState.AbstractPhysicalObjectFromString += On_SaveState_AbstractPhysicalObjectFromString;
        On.AbstractPhysicalObject.Realize += On_AbstractPhysicalObject_Realize;
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += On_ObjectsPage_DevObjectGetCategoryFromPlacedType;
        On.DevInterface.ObjectsPage.CreateObjRep += On_ObjectsPage_CreateObjRep;
        On.PlacedObject.GenerateEmptyData += On_PlacedObject_GenerateEmptyData;
        On.Room.SpawnMultiplayerItem += On_Room_SpawnMultiplayerItem;
        On.ArenaGameSession.SpawnItem += On_ArenaGameSession_SpawnItem;
        On.ItemSymbol.ColorForItem += On_ItemSymbol_ColorForItem;
        On.ItemSymbol.SpriteNameForItem += On_ItemSymbol_SpriteNameForItem;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += On_SLOracleBehaviorHasMark_TypeOfMiscItem;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += On_MoonConversation_AddEvents;
        On.Player.Grabability += On_Player_Grabability;
        On.ScavengerAI.CollectScore_PhysicalObject_bool += On_ScavengerAI_CollectScore_PhysicalObject_bool;
        On.ScavengerAI.WeaponScore += On_ScavengerAI_WeaponScore;
        On.OverseerCommunicationModule.FoodDelicousScore += On_OverseerCommunicationModule_FoodDelicousScore;
        On.Player.GraspsCanBeCrafted += On_Player_GraspsCanBeCrafted;
        On.Room.Loaded += On_Room_Loaded;
        On.Player.ThrowObject += On_Player_ThrowObject;
        On.Player.TossObject += On_Player_TossObject;
        On.AbstractCreature.setCustomFlags += On_AbstractCreature_setCustomFlags;
        On.PlacedObject.ConsumableObjectData.ctor += On_ConsumableObjectData_ctor;
        On.ShortcutHelper.PopsOutOfDeadShortcuts += On_ShortcutHelper_PopsOutOfDeadShortcuts;
        On.Hazer.Collide += On_Hazer_Collide;
        On.AbstractCreature.WatcherInitiateAI += On_AbstractCreature_WatcherInitiateAI;
        On.Player.IsCreatureLegalToHoldWithoutStun += On_Player_IsCreatureLegalToHoldWithoutStun;
        On.HazerGraphics.ctor += On_HazerGraphics_ctor;
        IL.HazerGraphics.ApplyPalette += IL_HazerGraphics_ApplyPalette;
        On.Hazer.Update += On_Hazer_Update;
        IL.DevInterface.ObjectsPage.AssembleObjectPages += IL_ObjectsPage_AssembleObjectPages;
        On.JetFish.ctor += On_JetFish_ctor;
        On.EggBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_EggBugAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.BigNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_BigNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.SmallNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_SmallNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_DropBugAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.AbstractConsumable.Consume += On_AbstractConsumable_Consume;
        IL.OverseerCommunicationModule.FoodDelicousScore += IL_OverseerCommunicationModule_FoodDelicousScore;
        On.LizardAI.ReactToNoise += On_LizardAI_ReactToNoise;
        IL.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.LizardCosmetics.LongHeadScales.ctor += On_LongHeadScales_ctor;
        On.MirosBird.BirdLeg.RunMode += On_BirdLeg_RunMode;
        On.MirosBirdAbstractAI.PopulateAllowedNodes += On_MirosBirdAbstractAI_PopulateAllowedNodes;
        On.MirosBird.Act += On_MirosBird_Act;
        IL.MirosBirdAbstractAI.Raid += IL_MirosBirdAbstractAI_Raid;
        On.JetFishAI.SocialEvent += On_JetFishAI_SocialEvent;
        IL.Leech.Swim += IL_Leech_Swim;
        On.AbstractCreature.WantToStayInDenUntilEndOfCycle += On_AbstractCreature_WantToStayInDenUntilEndOfCycle;
        IL.ArenaBehaviors.ExitManager.Update += IL_ExitManager_Update;
        IL.SnailAI.CreatureUnease += IL_SnailAI_CreatureUnease;
        IL.SnailAI.TileIdleScore += IL_SnailAI_TileIdleScore;
        On.Snail.VibrateLeeches += On_Snail_VibrateLeeches;
        On.OverseerAbstractAI.AllowSwarmTarget += On_OverseerAbstractAI_AllowSwarmTarget;
        On.GarbageWormAI.CreateTrackerRepresentationForCreature += On_GarbageWormAI_CreateTrackerRepresentationForCreature;
        On.Room.PlaceQCScore += On_Room_PlaceQCScore;
        new Hook(typeof(SandboxRegistry).GetMethod("DoSpawn", ALL_FLAGS), On_SandboxRegistry_DoSpawn);
        On.FlareBomb.Update += On_FlareBomb_Update;
        On.YellowAI.Pack += On_YellowAI_Pack;
        IL.ScavengersWorldAI.WorldFloodFiller.Update += IL_WorldFloodFiller_Update;
        On.AImap.ExitDistanceForCreatureAndCheckNeighbours += On_AImap_ExitDistanceForCreatureAndCheckNeighbours;
        On.World.GetNode += On_World_GetNode;
        On.World.TotalShortCutLengthBetweenTwoConnectedRooms_AbstractRoom_AbstractRoom += On_World_TotalShortCutLengthBetweenTwoConnectedRooms_AbstractRoom_AbstractRoom;
        On.DeerAI.NewRoom += On_DeerAI_NewRoom;
        On.DeerAI.TrackItem += On_DeerAI_TrackItem;
        On.DropBugAI.IUseItemTracker_TrackItem += On_DropBugAI_IUseItemTracker_TrackItem;
        On.ArenaBehaviors.SandboxEditor.AddIcon_IconSymbolData_Vector2_EntityID_bool_bool += On_SandboxEditor_AddIcon_IconSymbolData_Vector2_EntityID_bool_bool;
        IL.AbstractCreature.WantToStayInDenUntilEndOfCycle += IL_AbstractCreature_WantToStayInDenUntilEndOfCycle;
        IL.AbstractCreature.Update += IL_AbstractCreature_Update;
        On.SporePlant.SporePlantInterested += On_SporePlant_SporePlantInterested;
        On.SporePlant.HitByWeapon += On_SporePlant_HitByWeapon;
        IL.OverseerHolograms.OverseerHologram.CreaturePointer.Update += IL_CreaturePointer_Update;
        IL.OverseerCommunicationModule.CreatureDangerScore += IL_OverseerCommunicationModule_CreatureDangerScore;
        On.ThreatDetermination.ThreatOfCreature += On_ThreatDetermination_ThreatOfCreature;
        On.AbstractCreature.IsVoided += On_AbstractCreature_IsVoided;
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += On_RoomSettingsPage_DevEffectGetCategoryFromEffectType;
        On.LightSource.InitiateSprites += On_LightSource_InitiateSprites;
        IL.MeltLights.Update += IL_MeltLights_Update;
        On.LizardAI.GiftRecieved += On_LizardAI_GiftRecieved;
        On.LizardAI.UnpleasantFallRisk += On_LizardAI_UnpleasantFallRisk;
        On.LizardAI.FallRisk += On_LizardAI_FallRisk;
        On.LizardAI.LikeOfPlayer += On_LizardAI_LikeOfPlayer;
        On.BigEel.Act += On_BigEel_Act;
        IL.AbstractCreature.OpportunityToEnterDen += IL_AbstractCreature_OpportunityToEnterDen;
        IL.AbstractCreatureAI.AbstractBehavior += IL_AbstractCreatureAI_AbstractBehavior;
        On.FriendTracker.ItemOffered += On_FriendTracker_ItemOffered;
        On.ScavengerOutpost.FeeRecieved += On_ScavengerOutpost_FeeRecieved;
        On.DeathPersistentSaveData.ctor += On_DeathPersistentSaveData_ctor;
        On.DeathPersistentSaveData.FromString += On_DeathPersistentSaveData_FromString;
        On.DeathPersistentSaveData.SaveToString += On_DeathPersistentSaveData_SaveToString;
        On.Menu.StoryGameStatisticsScreen.TickerIsDone += On_StoryGameStatisticsScreen_TickerIsDone;
        On.Menu.StoryGameStatisticsScreen.GetDataFromGame += On_StoryGameStatisticsScreen_GetDataFromGame;
        IL.MoreSlugcats.CollectiblesTracker.ctor += IL_CollectiblesTracker_ctor;
        On.MoreSlugcats.CollectiblesTracker.MineForSaveData += On_CollectiblesTracker_MineForSaveData;
        On.RainWorld.ClearTokenCacheInMemory += On_RainWorld_ClearTokenCacheInMemory;
        On.Spear.Update += On_Spear_Update;
        On.AbstractCreature.AllowedToExistInRoom += On_AbstractCreature_AllowedToExistInRoom;
        new Hook(typeof(Centipede).GetMethod("get_Small", ALL_FLAGS), On_Centipede_get_Small);
        On.CreatureSymbol.DoesCreatureEarnATrophy += On_CreatureSymbol_DoesCreatureEarnATrophy;
        new Hook(typeof(LizardGraphics).GetMethod("get_HeadLightsUpFromNoise", ALL_FLAGS), On_LizardGraphics_get_HeadLightsUpFromNoise);
        On.CreatureSymbol.LizardSpriteName += On_CreatureSymbol_LizardSpriteName;
        new Hook(typeof(LizardRotGraphics).GetMethod("get_ApplyPaletteInDraw", ALL_FLAGS), On_LizardRotGraphics_get_ApplyPaletteInDraw);
        IL.Player.EatMeatUpdate += On_Player_EatMeatUpdate;
        On.ScavengerBomb.Explode += On_ScavengerBomb_Explode;
        IL.Spear.HitSomething += IL_Spear_HitSomething;
        On.Creature.Violence += On_Creature_Violence;
        On.LizardAI.BitCreature += On_LizardAI_BitCreature;
        On.PlayerGraphics.Update += On_PlayerGraphics_Update;
        new Hook(typeof(AbstractCreature).GetMethod("get_GetNodeType", ALL_FLAGS), On_AbstractCreature_get_GetNodeType);
        On.Watcher.LoachAI.UpdateDynamicRelationship += On_LoachAI_UpdateDynamicRelationship;
        On.Watcher.Loach.Collide += On_Loach_Collide;
        On.DaddyCorruption.Update += On_DaddyCorruption_Update;
        new Hook(typeof(VultureMaskGraphics).GetMethod("get_King", ALL_FLAGS), On_VultureMaskGraphics_get_King);
        new Hook(typeof(VultureMaskGraphics).GetMethod("get_ScavKing", ALL_FLAGS), On_VultureMaskGraphics_get_King); // same hook
        new Hook(typeof(VultureMaskGraphics).GetMethod("get_BaseTotalSprites", ALL_FLAGS), On_VultureMaskGraphics_get_BaseTotalSprites);
        On.MoreSlugcats.VultureMaskGraphics.AddToContainer += On_VultureMaskGraphics_AddToContainer;
        On.MoreSlugcats.VultureMaskGraphics.ApplyPalette += On_VultureMaskGraphics_ApplyPalette;
        On.MoreSlugcats.VultureMaskGraphics.InitiateSprites += On_VultureMaskGraphics_InitiateSprites;
        On.MoreSlugcats.VultureMaskGraphics.DrawSprites += On_VultureMaskGraphics_DrawSprites;
        On.MoreSlugcats.VultureMaskGraphics.Update += On_VultureMaskGraphics_Update;
        On.VultureMask.PlaceInRoom += On_VultureMask_PlaceInRoom;
        On.PlayerCarryableItem.NewRoom += On_PlayerCarryableItem_NewRoom;
        On.VultureMask.Update += On_VultureMask_Update;
        On.VultureMask.ctor += On_VultureMask_ctor;
        IL.SaveState.AbstractPhysicalObjectFromString += IL_SaveState_AbstractPhysicalObjectFromString;
        On.Player.MaulingUpdate += On_Player_MaulingUpdate;
        new Hook(typeof(AbstractCreature).GetMethod("get_karmicPotential", ALL_FLAGS), On_AbstractCreature_get_karmicPotential);
        IL.World.ToggleCreatureAccessFromCutscene += IL_World_ToggleCreatureAccessFromCutscene;
        On.MoreSlugcats.CutsceneArtificer.ToggleScavengerAccessToArtyIntro += On_CutsceneArtificer_ToggleScavengerAccessToArtyIntro;
        IL.ShelterDoor.Update += IL_ShelterDoor_Update;
        IL.OverseerAI.Update += IL_OverseerAI_Update;
        IL.OverseerAI.FlyingWeapon += IL_OverseerAI_FlyingWeapon;
        IL.ScavengerTreasury.Update += IL_ScavengerTreasury_Update;
        IL.DropBugAI.CeilingSitModule.SitUpdate += IL_CeilingSitModule_SitUpdate;
        IL.RainWorldGame.SendScavsToPlayer += IL_RainWorldGame_SendScavsToPlayer;
        IL.OverseerAbstractAI.PlayerGuideUpdate += IL_OverseerAbstractAI_PlayerGuideUpdate;
        IL.FirecrackerPlant.ScareObject.Update += IL_ScareObject_Update;
        On.PreyTracker.TrackedPrey.Attractiveness += On_TrackedPrey_Attractiveness;
        On.MoreSlugcats.MSCRoomSpecificScript.DS_RIVSTARTcutscene.ctor += On_DS_RIVSTARTcutscene_ctor;
        IL.MoreSlugcats.MSCRoomSpecificScript.LC_FINAL.SummonScavengers += IL_LC_FINAL_SummonScavengers;
        IL.Scavenger.NewTile += IL_Scavenger_NewTile;
        new Hook(typeof(Scavenger).GetMethod("get_MeleeRange", ALL_FLAGS), On_Scavenger_get_MeleeRange);
        new Hook(typeof(Scavenger).GetMethod("get_MidRange", ALL_FLAGS), On_Scavenger_get_MidRange);
        On.ScavengerGraphics.GenerateColors += On_ScavengerGraphics_GenerateColors;
        IL.ScavengerAI.CheckThrow += IL_ScavengerAI_CheckThrow;
        IL.ScavengerGraphics.DrawSprites += IL_ScavengerGraphics_DrawSprites;
        IL.ScavengerGraphics.InitiateSprites += IL_ScavengerGraphics_InitiateSprites;
        IL.ScavengerGraphics.ctor += IL_ScavengerGraphics_ctor;
        IL.ScavengerGraphics.Eartlers.GenerateSegments += IL_Eartlers_GenerateSegments;
        On.ScavengerGraphics.IndividualVariations.ctor += On_IndividualVariations_ctor;
        On.ScavengerCosmetic.BackDecals.GeneratePattern += On_BackDecals_GeneratePattern;
        On.ScavengerCosmetic.BackTuftsAndRidges.ctor += On_BackTuftsAndRidges_ctor;
        IL.ScavengerAI.SeeThrownWeapon += IL_ScavengerAI_SeeThrownWeapon;
        IL.ScavengerAI.IdleBehavior += IL_ScavengerAI_IdleBehavior;
        IL.ScavengerAI.AttackBehavior += IL_ScavengerAI_AttackBehavior;
        IL.ScavengerAI.SpearThrowPositionScore += IL_ScavengerAI_SpearThrowPositionScore;
        IL.ScavengerAI.CreatureSpotted += IL_ScavengerAI_CreatureSpotted;
        IL.ScavengerAI.CommunicationModule.EvaluateCommunicationDemand += IL_CommunicationModule_EvaluateCommunicationDemand;
        IL.Scavenger.CombatUpdate += IL_Scavenger_CombatUpdate;
        IL.Scavenger.TryToMeleeCreature += IL_Scavenger_TryToMeleeCreature;
        On.Scavenger.SetUpCombatSkills += On_Scavenger_SetUpCombatSkills;
        IL.Scavenger.MidRangeUpdate += IL_Scavenger_MidRangeUpdate;
        IL.Scavenger.TryThrow_BodyChunk_ViolenceType_Nullable1 += IL_Scavenger_TryThrow_BodyChunk_ViolenceType_Nullable1;
        IL.Scavenger.Throw += IL_Scavenger_Throw;
        IL.Scavenger.GetUnstuckRoutine += IL_Scavenger_GetUnstuckRoutine;
        On.Scavenger.FastReactionCheck += On_Scavenger_FastReactionCheck;
        On.Scavenger.ReactionCheck += On_Scavenger_ReactionCheck;
        IL.ScavengerAbstractAI.GoToRoom += IL_ScavengerAbstractAI_GoToRoom;
        IL.ScavengerAbstractAI.RandomDestinationRoom += IL_ScavengerAbstractAI_RandomDestinationRoom;
        new Hook(typeof(ScavengerAbstractAI.ScavengerSquad).GetMethod("get_HasAMission", ALL_FLAGS), On_ScavengerSquad_get_HasAMission);
        On.ScavengerAbstractAI.IsSpearExplosive += On_ScavengerAbstractAI_IsSpearExplosive;
        IL.ScavengerAbstractAI.InitGearUp += IL_ScavengerAbstractAI_InitGearUp;
        On.LizardCosmetics.Whiskers.ApplyPalette += On_Whiskers_ApplyPalette;
        On.LizardCosmetics.BumpHawk.ApplyPalette += On_BumpHawk_ApplyPalette;
        On.LizardCosmetics.JumpRings.ApplyPalette += On_JumpRings_ApplyPalette;
        On.LizardCosmetics.LongBodyScales.ApplyPalette += On_LongBodyScales_ApplyPalette;
        On.LizardCosmetics.SpineSpikes.ApplyPalette += On_SpineSpikes_ApplyPalette;
        On.LizardCosmetics.TailFin.ApplyPalette += On_TailFin_ApplyPalette;
        On.LizardCosmetics.WingScales.ApplyPalette += On_WingScales_ApplyPalette;
        On.Lizard.EnterAnimation += On_Lizard_EnterAnimation;
        IL.LizardCosmetics.Antennae.ctor += IL_Antennae_ctor;
        new Hook(typeof(Lizard).GetMethod("get_Swimmer", ALL_FLAGS), On_Lizard_get_Swimmer);
        IL.GraffitiCloud.Update += IL_GraffitiCloud_Update;
        On.FlyGraphics.MuddableSprite += On_FlyGraphics_MuddableSprite;
        On.EggBugGraphics.EggAttachPos += On_EggBugGraphics_EggAttachPos;
        On.EggBugGraphics.BackEggSprite += On_EggBugGraphics_BackEggSprite;
        On.EggBugGraphics.AntennaSprite += On_EggBugGraphics_AntennaSprite;
        On.EggBugGraphics.EyeSprite += On_EggBugGraphics_EyeSprite;
        On.EggBugGraphics.LegSprite += On_EggBugGraphics_LegSprite;
        On.EggBugGraphics.FrontEggSprite += On_EggBugGraphics_FrontEggSprite;
        new Hook(typeof(EggBugGraphics).GetMethod("get_TotalSprites", ALL_FLAGS), On_EggBugGraphics_get_TotalSprites);
        new Hook(typeof(EggBugGraphics).GetMethod("get_MeshSprite", ALL_FLAGS), On_EggBugGraphics_get_MeshSprite);
        new Hook(typeof(EggBugGraphics).GetMethod("get_HeadSprite", ALL_FLAGS), On_EggBugGraphics_get_HeadSprite);
        Content.Register(new WaterBlobCritob(),
                        new BouncingBallCritob(),
                        new HazerMomCritob(),
                        new MiniLeechCritob(),
                        new CommonEelCritob(),
                        new NoodleEaterCritob(),
                        new PolliwogCritob(),
                        new AlphaOrangeCritob(),
                        new WaterSpitterCritob(),
                        new MoleSalamanderCritob(),
                        new SilverLizardCritob(),
                        new HunterSeekerCritob(),
                        new HoverflyCritob(),
                        new FatFireFlyCritob(),
                        new BlizzorCritob(),
                        new M4RJawsCritob(),
                        new DivingBeetleCritob(),
                        new SurfaceSwimmerCritob(),
                        new M4RMamaBugCritob(),
                        new ThornBugCritob(),
                        new TintedBeetleCritob(),
                        new MiniScutigeraCritob(),
                        new ScutigeraCritob(),
                        new RedHorrorCritob(),
                        new SporantulaCritob(),
                        new ChipChopCritob(),
                        new KillerpillarCritob(),
                        new GlowpillarCritob(),
                        new MiniLeviathanCritob(),
                        new MiniFlyingBigEelCritob(),
                        new FlyingBigEelCritob(),
                        new DentureCritob(),
                        new XyloCritob(),
                        new XyloWormCritob(),
                        new ScavengerSentinelCritob());
    }

    public void OnDisable()
    {
        StrawberryData = null!;
        Seed = null!;
        Big = null!;
        AbsProps = null!;
        Jelly = null!;
        HoverflyData = null!;
        SporeMemory = null!;
        StationFruit = null!;
        StationPlant = null!;
        PlayerData = null!;
        OnResultAction = null;
        OnFoodItemSpotted = null;
        s_logger = null;
        JLLRooms = null!;
        SeedRooms = null!;
        ScoreData = null!;
        TrackerScoreData = null!;
        RegionScoreData = null!;
        UnregisteredNodeType = null!;
        CreatureTemplateType.M4RCreatureList = null!;
        AbstractObjectType.M4RItemList = null!;
        SandboxUnlockID.M4RUnlockList = null!;
        InternalMatch.Dispose();
    }
}