global using static LBMergedMods.Hooks.ArenaHooks;
using System;
using ArenaBehaviors;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;
using Fisobs.Core;
using Fisobs.Sandbox;
using System.Linq;

namespace LBMergedMods.Hooks;

public static class ArenaHooks
{
    public const int M4R_DATA_NUMBER = 319, M4R_DATA_NUMBER2 = 320, M4R_DATA_NUMBER3 = 321;

    internal static bool On_ArenaCreatureSpawner_IsMajorCreature(On.ArenaCreatureSpawner.orig_IsMajorCreature orig, CreatureTemplate.Type type) => type == CreatureTemplateType.RedHorrorCenti || type == CreatureTemplateType.FlyingBigEel || type == CreatureTemplateType.FatFireFly || type == CreatureTemplateType.Blizzor || type == CreatureTemplateType.SparkEye || orig(type);

    internal static void On_ArenaGameSession_SpawnItem(On.ArenaGameSession.orig_SpawnItem orig, ArenaGameSession self, Room room, PlacedObject placedObj)
    {
        var data = (placedObj.data as PlacedObject.MultiplayerItemData)!;
        var dataTp = data.type;
        if (dataTp != MultiplayerItemType.ThornyStrawberry && dataTp != MultiplayerItemType.LittleBalloon && dataTp != MultiplayerItemType.BouncingMelon && dataTp != MultiplayerItemType.Physalis && dataTp != MultiplayerItemType.LimeMushroom && dataTp != MultiplayerItemType.MarineEye && dataTp != MultiplayerItemType.StarLemon && dataTp != MultiplayerItemType.SporeProjectile && dataTp != MultiplayerItemType.FumeFruit && dataTp != MultiplayerItemType.Durian && dataTp != MultiplayerItemType.DarkGrub)
            orig(self, room, placedObj);
        else if (self.SpawnDefaultRoomItems && Random.value <= data.chance)
        {
            var tp = new AbstractPhysicalObject.AbstractObjectType(dataTp.value);
            if (tp.Index < 0)
                tp = AbstractPhysicalObject.AbstractObjectType.Spear;
            else if (self.arenaSitting.multiplayerUnlocks.ExoticItems < 1f)
            {
                var sandboxID = new MultiplayerUnlocks.SandboxUnlockID(dataTp.value);
                if ((sandboxID.Index < 0 || !self.arenaSitting.multiplayerUnlocks.SandboxItemUnlocked(sandboxID)) && Random.value > self.arenaSitting.multiplayerUnlocks.ExoticItems)
                    tp = AbstractPhysicalObject.AbstractObjectType.Spear;
            }
            if (tp == AbstractPhysicalObject.AbstractObjectType.Spear)
                room.abstractRoom.entities.Add(new AbstractSpear(room.world, null, room.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), false));
            else
                room.abstractRoom.entities.Add(new AbstractConsumable(room.world, tp, null, room.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), -2, -2, null));
        }
    }

    internal static void IL_ExitManager_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdloc_OutLoc2,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Leech,
            s_MatchCall_Any))
        {
            var vars = il.Body.Variables;
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((bool flag, List<AbstractCreature> challengeKillList, int l) =>
             {
                 var tp = challengeKillList[l].creatureTemplate.type;
                 return flag && tp != CreatureTemplateType.MiniBlackLeech && tp != CreatureTemplateType.MiniScutigera && tp != CreatureTemplateType.XyloWorm;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ExitManager.Update!");
    }

    internal static void IL_MultiplayerMenu_FindAllLevels(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdloc_OutLoc2,
            s_MatchLdelemRef,
            s_MatchLdloc_InLoc1,
            s_MatchLdloc_InLoc2,
            s_MatchLdelemRef,
            s_MatchCallOrCallvirt_string_get_Length,
            s_MatchLdcI4_4,
            s_MatchSub,
            s_MatchLdcI4_4,
            s_MatchCallOrCallvirt_string_Substring_int_int,
            s_MatchLdstr__txt,
            s_MatchCall_string_op_Equality_string_string,
            s_MatchBrfalse_OutLabel))
        {
            var stringIneq = il.Import(typeof(string).GetMethod("op_Inequality", [typeof(string), typeof(string)]));
            var substring = il.Import(s_string_Substring_int_int);
            var vars = il.Body.Variables;
            VariableDefinition var0 = vars[s_loc1],
                var2 = vars[s_loc2];
            c.Emit(OpCodes.Ldloc, var0)
             .Emit(OpCodes.Ldloc, var2)
             .Emit(OpCodes.Ldelem_Ref)
             .Emit(OpCodes.Ldloc, var0)
             .Emit(OpCodes.Ldloc, var2)
             .Emit(OpCodes.Ldelem_Ref)
             .Emit<string>(OpCodes.Callvirt, "get_Length")
             .Emit(OpCodes.Ldc_I4, 18)
             .Emit(OpCodes.Sub)
             .Emit(OpCodes.Ldc_I4, 18)
             .Emit(OpCodes.Callvirt, substring)
             .Emit(OpCodes.Ldstr, "_jellylonglegs.txt")
             .Emit(OpCodes.Call, stringIneq)
             .Emit(OpCodes.Brfalse, s_label)
             .Emit(OpCodes.Ldloc, var0)
             .Emit(OpCodes.Ldloc, var2)
             .Emit(OpCodes.Ldelem_Ref)
             .Emit(OpCodes.Ldloc, var0)
             .Emit(OpCodes.Ldloc, var2)
             .Emit(OpCodes.Ldelem_Ref)
             .Emit<string>(OpCodes.Callvirt, "get_Length")
             .Emit(OpCodes.Ldc_I4, 18)
             .Emit(OpCodes.Sub)
             .Emit(OpCodes.Ldc_I4, 18)
             .Emit(OpCodes.Callvirt, substring)
             .Emit(OpCodes.Ldstr, "_seedbats.txt")
             .Emit(OpCodes.Call, stringIneq)
             .Emit(OpCodes.Brfalse, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Menu.MultiplayerMenu.FindAllLevels!");
    }

    internal static MultiplayerUnlocks.SandboxUnlockID On_MultiplayerUnlocks_SandboxUnlockForSymbolData(On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data)
    {
        var intData = data.intData;
        var tp = data.critType;
        if (intData == M4R_DATA_NUMBER)
        {
            if (tp == CreatureTemplate.Type.Fly)
                return SandboxUnlockID.SeedBat;
            else if (tp == CreatureTemplate.Type.TubeWorm)
                return SandboxUnlockID.Bigrub;
        }
        else if (intData is M4R_DATA_NUMBER2 or M4R_DATA_NUMBER3 && tp == CreatureTemplate.Type.TubeWorm)
            return SandboxUnlockID.Bigrub;
        return orig(data);
    }

    internal static IconSymbol.IconSymbolData On_MultiplayerUnlocks_SymbolDataForSandboxUnlock(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        if (unlockID == SandboxUnlockID.SeedBat)
            return new(CreatureTemplate.Type.Fly, AbstractPhysicalObject.AbstractObjectType.Creature, M4R_DATA_NUMBER);
        else if (unlockID == SandboxUnlockID.Bigrub)
            return new(CreatureTemplate.Type.TubeWorm, AbstractPhysicalObject.AbstractObjectType.Creature, M4R_DATA_NUMBER);
        return orig(unlockID);
    }

    internal static SandboxEditor.PlacedIcon On_SandboxEditor_AddIcon_IconSymbolData_Vector2_EntityID_bool_bool(On.ArenaBehaviors.SandboxEditor.orig_AddIcon_IconSymbolData_Vector2_EntityID_bool_bool orig, SandboxEditor self, IconSymbol.IconSymbolData iconData, Vector2 pos, EntityID ID, bool fadeCircle, bool updatePerfEstimate)
    {
        if (iconData.critType == CreatureTemplateType.Denture)
            return self.AddIcon(new SandboxEditor.InDenCreatureIcon(self, pos, iconData, ID), fadeCircle, updatePerfEstimate);
        return orig(self, iconData, pos, ID, fadeCircle, updatePerfEstimate);
    }

    internal static void On_SandboxEditor_GetPerformanceEstimate(On.ArenaBehaviors.SandboxEditor.orig_GetPerformanceEstimate orig, SandboxEditor.PlacedIcon placedIcon, ref float exponentialPart, ref float linearPart)
    {
        if (placedIcon is SandboxEditor.CreatureOrItemIcon i && (i.iconData.itemType == AbstractObjectType.LittleBalloon || i.iconData.itemType == AbstractObjectType.Physalis || i.iconData.itemType == AbstractObjectType.MiniBlueFruit || i.iconData.itemType == AbstractObjectType.FumeFruit))
            linearPart += .6f;
        else
            orig(placedIcon, ref exponentialPart, ref linearPart);
    }

    internal static void IL_SandboxGameSession_SpawnEntity(ILContext il)
    {
        var c = new ILCursor(il);
        var vars = il.Body.Variables;
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_GameSession_game,
            s_MatchCallOrCallvirt_RainWorldGame_get_world,
            s_MatchLdcI4_0,
            s_MatchCallOrCallvirt_World_GetAbstractRoom_int,
            s_MatchLdarg_0,
            s_MatchLdfld_GameSession_game,
            s_MatchCallOrCallvirt_RainWorldGame_get_world,
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_IconSymbol_IconSymbolData_critType,
            s_MatchCall_StaticWorld_GetCreatureTemplate_CreatureTemplate_Type,
            s_MatchLdnull,
            s_MatchLdloc_Any,
            s_MatchLdloc_Any,
            s_MatchNewobj_AbstractCreature))
        {
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .EmitDelegate((AbstractCreature crit, IconSymbol.IconSymbolData data) =>
             {
                 if (data.critType == CreatureTemplate.Type.Fly && Seed.TryGetValue(crit, out var prop))
                 {
                     prop.Born = true;
                     if (data.intData == M4R_DATA_NUMBER)
                        prop.IsSeed = true;
                 }
                 return crit;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SandboxGameSession.SpawnEntity! (part 1)");
        c.Index = 0;
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_AbstractRoom_AddEntity)
        && c.TryGotoNext(MoveType.After,
            s_MatchLdloc_InLoc1,
            s_MatchCallOrCallvirt_AbstractRoom_AddEntity))
        {
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((AbstractCreature crit, SandboxEditor.PlacedIconData placedIconData) =>
             {
                 if (placedIconData.data.critType == CreatureTemplate.Type.TubeWorm && Big.TryGetValue(crit, out var prop))
                 {
                     prop.Born = true;
                     var data = placedIconData.data.intData;
                     if (placedIconData.data.intData is M4R_DATA_NUMBER or M4R_DATA_NUMBER2 or M4R_DATA_NUMBER3)
                     {
                         prop.IsBig = true;
                         var state = Random.state;
                         Random.InitState(placedIconData.ID.RandomSeed);
                         if (Random.value < .1f)
                             crit.superSizeMe = true;
                         if (Random.value < .08f)
                             prop.NormalLook = true;
                         Random.state = state;
                     }
                 }
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SandboxGameSession.SpawnEntity! (part 2)");
    }

    internal static void On_SandboxRegistry_DoSpawn(Action<SandboxGameSession, SandboxEditor.PlacedIconData, EntitySaveData, ISandboxHandler> orig, SandboxGameSession session, SandboxEditor.PlacedIconData p, EntitySaveData data, ISandboxHandler handler)
    {
        var tp = p.data.critType;
        if (tp == CreatureTemplateType.Denture)
        {
            var sandboxUnlock = handler.SandboxUnlocks.FirstOrDefault();
            if (sandboxUnlock is null)
            {
                Debug.LogError($"The fisob \"{handler.Type}\" had no sandbox unlocks.");
                return;
            }
            try
            {
                var world = session.game.world;
                var abstractWorldEntity = handler.ParseFromSandbox(world, data, sandboxUnlock);
                if (abstractWorldEntity is AbstractCreature crit)
                {
                    crit.pos.x = -1;
                    crit.pos.y = -1;
                    crit.pos.abstractNode = p.data.intData;
                    world.GetAbstractRoom(0).entitiesInDens.Add(crit);
                }
                else
                    Debug.LogError($"The sandbox unlock \"{sandboxUnlock.Type}\" didn't return a creature when being parsed in sandbox mode.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"The sandbox unlock \"{sandboxUnlock.Type}\" threw an exception when being parsed in sandbox mode.");
                Debug.LogException(exception);
            }
        }
        else
        {
            orig(session, p, data, handler);
            if (tp == CreatureTemplateType.MiniBlackLeech)
            {
                for (var i = 1; i <= 4; i++)
                    orig(session, p, data, handler);
            }
        }
    }

    internal static void On_SandboxSettingsInterface_DefaultKillScores(On.Menu.SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] killScores)
    {
        orig(ref killScores);
        killScores[(int)SandboxUnlockID.Bigrub] = 2;
    }

    internal static bool On_SandboxSettingsInterface_IsThisSandboxUnlockVisible(On.Menu.SandboxSettingsInterface.orig_IsThisSandboxUnlockVisible orig, MultiplayerUnlocks.SandboxUnlockID sandboxUnlockID) => sandboxUnlockID != SandboxUnlockID.SeedBat && sandboxUnlockID != SandboxUnlockID.Bigrub && sandboxUnlockID != SandboxUnlockID.MiniBlackLeech && sandboxUnlockID != SandboxUnlockID.XyloWorm && orig(sandboxUnlockID);
}