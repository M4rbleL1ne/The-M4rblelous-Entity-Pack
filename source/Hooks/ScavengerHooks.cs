global using static LBMergedMods.Hooks.ScavengerHooks;
using MonoMod.Cil;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using ScavengerCosmetic;
using RWCustom;

namespace LBMergedMods.Hooks;

public static class ScavengerHooks
{
    internal static void On_BackDecals_GeneratePattern(On.ScavengerCosmetic.BackDecals.orig_GeneratePattern orig, BackDecals self, BackDecals.Pattern newPattern)
    {
        if (self.scavGrphs is ScavengerSentinelGraphics)
            newPattern = BackDecals.Pattern.RandomBackBlotch;
        orig(self, newPattern);
    }

    internal static void IL_ScavengerAbstractAI_InitGearUp(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared)
         && c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAbstractAI.InitGearUp! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared)
         && c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared)
         && c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared)
         && c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAbstractAI.InitGearUp! (part 2)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAbstractAI.InitGearUp! (part 2)");
        var instrs = il.Instrs;
        var num = 0;
        for (var i = 0; i < instrs.Count - 1; i++)
        {
            if (instrs[i].MatchLdsfld<DLCSharedEnums.CreatureTemplateType>("ScavengerElite") && instrs[i + 1].MatchCall(out _))
            {
                ++num;
                if (num % 2 == 1)
                {
                    c.Goto(i + 1, MoveType.After)
                     .Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
                }
                if (num == 5)
                    break;
            }
        }
    }

    internal static bool On_ScavengerAbstractAI_IsSpearExplosive(On.ScavengerAbstractAI.orig_IsSpearExplosive orig, ScavengerAbstractAI self, int cycleNum) => self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel || orig(self, cycleNum);

    internal static void IL_ScavengerAbstractAI_GoToRoom(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MSC))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAbstractAI.GoToRoom!");
    }

    internal static void IL_ScavengerAbstractAI_RandomDestinationRoom(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdsfld<ModManager>("MSC"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || self.parent.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
            }
        }
    }

    internal static void On_BackTuftsAndRidges_ctor(On.ScavengerCosmetic.BackTuftsAndRidges.orig_ctor orig, BackTuftsAndRidges self, ScavengerGraphics owner, int firstSprite)
    {
        orig(self, owner, firstSprite);
        if (owner is ScavengerSentinelGraphics)
            self.colored = Math.Max(.2f, Mathf.Pow(Random.value, .5f));
    }

    internal static void IL_CommunicationModule_EvaluateCommunicationDemand(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdsfld<ModManager>("MMF"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
            }
        }
    }

    internal static void IL_Eartlers_GenerateSegments(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? 2f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_15))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? 70f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 2)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_45)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_45))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? 65f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 3)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? .5f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 4)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? 1.5f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 5)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? .75f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 6)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? .33f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 7)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? 1.67f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 8)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? 2f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor! (part 9)");
    }

    internal static void On_IndividualVariations_ctor(On.ScavengerGraphics.IndividualVariations.orig_ctor orig, ref ScavengerGraphics.IndividualVariations self, Scavenger scavenger)
    {
        orig(ref self, scavenger);
        if (scavenger is ScavengerSentinel)
        {
            self.generalMelanin = Mathf.Lerp(self.generalMelanin, 0f, .1f);
            self.fatness = 1f;
            self.neckThickness = 1.5f;
            self.narrowEyes *= .2f;
            self.eartlerWidth *= .6f;
            self.wideTeeth *= .5f;
            self.eyeSize *= 1.3f;
            self.headSize *= 1.05f;
            ++self.tailSegs;
            self.armThickness = 1f;
            self.legsSize = 1f;
            self.coloredEartlerTips = true;
            self.pupilSize *= 1.3f;
            self.narrowWaist *= .5f;
            self.scruffy = 1f;
        }
    }

    internal static void IL_Scavenger_CombatUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.CombatUpdate! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MMF))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.CombatUpdate! (part 2)");
    }

    internal static float On_Scavenger_get_MeleeRange(Func<Scavenger, float> orig, Scavenger self) => self is ScavengerSentinel ? 20f + 50f * self.meleeSkill : orig(self);

    internal static float On_Scavenger_get_MidRange(Func<Scavenger, float> orig, Scavenger self) => self is ScavengerSentinel ? 400f + 200f * self.midRangeSkill : orig(self);

    internal static void On_Scavenger_MeleeGetFree(On.Scavenger.orig_MeleeGetFree orig, Scavenger self, Creature target, bool eu)
    {
        if (self.grabbedBy is List<Creature.Grasp> l && l.Count > 0 && l[0].grabber is FatFireFly c && c == target && self.grabbedAttackCounter != 25 && !(self.Elite && self.grabbedAttackCounter % 75 == 0))
            return;
        orig(self, target, eu);
    }

    internal static void IL_ScavengerAI_CheckThrow(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchCallOrCallvirt<Scavenger>("get_Elite"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
                break;
            }
        }
    }

    internal static bool On_Scavenger_FastReactionCheck(On.Scavenger.orig_FastReactionCheck orig, Scavenger self)
    {
        if (ModManager.DLCShared && self is ScavengerSentinel)
            self.ReactionCheck();
        return orig(self);
    }

    internal static void IL_Scavenger_GetUnstuckRoutine(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MMF))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.GetUnstuckRoutine!");
    }

    internal static void IL_Scavenger_MidRangeUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.MidRangeUpdate!");
        var instrs = il.Instrs;
        var num = 0;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchCallOrCallvirt<Scavenger>("get_Elite"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
                ++num;
                if (num == 2)
                    break;
            }
        }
    }

    internal static void IL_Scavenger_NewTile(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, int i, Scavenger self) => flag || self.AI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.NewTile!");
    }

    internal static bool On_Scavenger_ReactionCheck(On.Scavenger.orig_ReactionCheck orig, Scavenger self)
    {
        if (self is ScavengerSentinel)
        {
            self.reflexBuildUp = Math.Min(1f, self.reflexBuildUp + Mathf.Lerp(1f / 120f, .5f, Mathf.Pow(self.reactionSkill, 3.5f)));
            return self.reflexBuildUp >= 1f;
        }
        return orig(self);
    }

    internal static void On_Scavenger_SetUpCombatSkills(On.Scavenger.orig_SetUpCombatSkills orig, Scavenger self)
    {
        if (self is ScavengerSentinel)
        {
            ref readonly var personality = ref self.abstractCreature.personality;
            var num = Mathf.Lerp(personality.dominance, 1f, .15f);
            var state = Random.state;
            Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
            self.dodgeSkill = Mathf.Lerp(Custom.PushFromHalf(Mathf.Lerp(Random.value < .5f ? personality.nervous : personality.aggression, Random.value, Random.value), 1f + Random.value), 0f, .3f - num * .15f);
            self.midRangeSkill = Mathf.Lerp(Custom.PushFromHalf(Mathf.Lerp(Random.value < .5f ? personality.energy : personality.aggression, Random.value, Random.value), 1f + Random.value), 1f, .25f + num * .1f);
            self.meleeSkill = Mathf.Lerp(Custom.PushFromHalf(Random.value, 1f + Random.value), 0f, .25f - num * .1f);
            self.blockingSkill = Mathf.Lerp(Custom.PushFromHalf(Mathf.InverseLerp(.35f, 1f, Mathf.Lerp(Random.value < .5f ? personality.bravery : personality.energy, Random.value, Random.value)), 1f + Random.value), 1f, .25f + num * .1f);
            self.reactionSkill = Mathf.Lerp(Custom.PushFromHalf(Mathf.Lerp(personality.energy, Random.value, Random.value), 1f + Random.value), 1f, .2f + num * .1f);
            Random.state = state;
        }
        else
            orig(self);
    }

    internal static void IL_Scavenger_Throw(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.Throw!");
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchCallOrCallvirt<Scavenger>("get_Elite"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
            }
        }
    }

    internal static void IL_Scavenger_TryThrow_BodyChunk_ViolenceType_Nullable1(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchCallOrCallvirt<Scavenger>("get_Elite"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
            }
        }
    }

    internal static void IL_Scavenger_TryToMeleeCreature(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Scavenger.TryToMeleeCreature!");
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdsfld<ModManager>("MMF"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Scavenger self) => flag || self is ScavengerSentinel);
            }
        }
    }

    internal static void IL_ScavengerAI_AttackBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdsfld<ModManager>("MMF"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
            }
        }
    }

    internal static int On_ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var res = orig(self, obj, weaponFiltered);
        if (obj is ThornyStrawberry st)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (weaponFiltered && self.NeedAWeapon)
                res = self.WeaponScore(st, true);
            else
                res = st.SpikesRemoved() ? 1 : 3;
        }
        else if (obj is FumeFruit or Durian)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (weaponFiltered && self.NeedAWeapon)
                res = self.WeaponScore(obj, true);
            else
                res = obj is Durian ? 3 : 2;
        }
        else if (obj is SmallPuffBall)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (weaponFiltered && self.NeedAWeapon)
                res = self.WeaponScore(obj, true);
            else
                res = ModManager.Watcher && self.scavenger.room?.world.name == "WBLA" ? 4 : 2;
        }
        else if (obj is BlobPiece or Physalis or LimeMushroom or MarineEye or StarLemon)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (!(weaponFiltered && self.NeedAWeapon))
                res = obj is Physalis ? 5 : (obj is LimeMushroom or StarLemon ? 3 : 2);
        }
        return res;
    }

    internal static void IL_ScavengerAI_CreatureSpotted(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MMF))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.CreatureSpotted!");
    }

    internal static void IL_ScavengerAI_IdleBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MMF))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IdleBehavior!");
    }

    internal static void IL_ScavengerAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_PoleMimic,
            s_MatchCall_Any,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((RelationshipTracker.DynamicRelationship dRelation) => dRelation.trackerRep?.representedCreature?.creatureTemplate.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Scutigera || tp == CreatureTemplateType.RedHorrorCenti || tp == CreatureTemplateType.Sporantula || tp == CreatureTemplateType.DivingBeetle || tp == CreatureTemplateType.Killerpillar || tp == CreatureTemplateType.Glowpillar));
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_MirosBird,
            s_MatchCall_Any,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((RelationshipTracker.DynamicRelationship dRelation) => dRelation.trackerRep?.representedCreature?.creatureTemplate.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.Blizzor || tp == CreatureTemplateType.SparkEye));
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 2)");
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdsfld<ModManager>("MMF"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
            }
        }
    }

    internal static void IL_ScavengerAI_SeeThrownWeapon(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdsfld<ModManager>("MMF"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
            }
        }
    }

    internal static void IL_ScavengerAI_SpearThrowPositionScore(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MMF))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAI self) => flag || self is ScavengerSentinelAI);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.SpearThrowPositionScore!");
    }

    internal static int On_ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection, bool reallyWantsSpear)
    {
        var res = orig(self, obj, pickupDropInsteadOfWeaponSelection, reallyWantsSpear);
        if (obj is ThornyStrawberry st)
        {
            if (st.SpikesRemoved())
                res = 0;
            else
            {
                if (!pickupDropInsteadOfWeaponSelection && (self.currentViolenceType == ScavengerAI.ViolenceType.NonLethal || self.currentViolenceType == ScavengerAI.ViolenceType.ForFun))
                    res = 2;
                else
                    res = 3;
            }
        }
        else if (obj is SmallPuffBall or FumeFruit)
            res = 2;
        else if (obj is Durian)
            res = self.targetedBodyChunk is BodyChunk b && b.owner is Lizard and not CommonEel && b.index == 0 ? 6 : 3;
        return res;
    }

    internal static void On_ScavengerBomb_Explode(On.ScavengerBomb.orig_Explode orig, ScavengerBomb self, BodyChunk hitChunk)
    {
        if (hitChunk?.owner is Xylo)
        {
            self.smoke?.Destroy();
            self.Destroy();
        }
        else
            orig(self, hitChunk);
    }

    internal static void IL_ScavengerGraphics_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_0_1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float f, ScavengerGraphics self) => self is ScavengerSentinelGraphics ? -1f : f);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.ctor!");
        var num = 0;
        for (var i = c.Index; i < instrs.Count; i++)
        {
            if (instrs[i].MatchCallOrCallvirt<Scavenger>("get_Elite"))
            {
                ++num;
                if (num > 1)
                {
                    var varFs = 0;
                    var vars = il.Body.Variables;
                    for (var k = 0; k < vars.Count; k++)
                    {
                        if (vars[k].VariableType.Name.Contains("Int32"))
                        {
                            varFs = k;
                            break;
                        }
                    }
                    c.Goto(i, MoveType.After)
                     .Emit(OpCodes.Ldarg_0)
                     .Emit(OpCodes.Ldloc, il.Body.Variables[varFs])
                     .EmitDelegate((bool flag, ScavengerGraphics self, int fs) =>
                     {
                         if (self is ScavengerSentinelGraphics)
                         {
                             self.maskGfx = new M4RScavMaskGraphics(self.scavenger, fs, "M4RScavMask" + Random.Range(1, 3) + "_" + (char)(65 + Random.Range(0, 3)));
                             self.maskGfx.GenerateColor(self.scavenger.abstractPhysicalObject.ID.RandomSeed);
                             return false;
                         }
                         return flag;
                     });
                    break;
                }
            }
        }
    }

    internal static void IL_ScavengerGraphics_DrawSprites(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerGraphics self) => flag || self is ScavengerSentinelGraphics);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.DrawSprites!");
    }

    internal static void On_ScavengerGraphics_GenerateColors(On.ScavengerGraphics.orig_GenerateColors orig, ScavengerGraphics self)
    {
        orig(self);
        if (self is ScavengerSentinelGraphics)
        {
            self.bodyColor.saturation *= .5f;
            self.headColor.saturation *= .5f;
            self.eyeColor.hue = Mathf.Lerp(self.eyeColor.hue, 125f / 360f, .5f);
            self.eyeColor.lightness = Mathf.Lerp(self.eyeColor.lightness, .5f, .6f);
            self.bellyColor.saturation *= .5f;
            self.decorationColor.hue = Mathf.Lerp(self.decorationColor.hue, 1f / 12f, .6f);
            self.decorationColor.saturation = Mathf.Lerp(self.decorationColor.saturation, .9f, .5f);
            self.decorationColor.lightness = Mathf.Lerp(self.decorationColor.lightness, .5f, .6f);
        }
    }

    internal static void IL_ScavengerGraphics_InitiateSprites(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_ModManager_get_DLCShared))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerGraphics self) => flag || self is ScavengerSentinelGraphics);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerGraphics.InitiateSprites!");
    }

    internal static void On_ScavengerOutpost_FeeRecieved(On.ScavengerOutpost.orig_FeeRecieved orig, ScavengerOutpost self, Player player, AbstractPhysicalObject item, int value)
    {
        if (self.worldOutpost is ScavengersWorldAI.Outpost outpost && item.type == AbstractObjectType.Physalis)
        {
            var flag1 = false;
            var recs = self.receivedItems;
            for (var i = 0; i < recs.Count; i++)
            {
                if (recs[i] == item.ID)
                {
                    flag1 = true;
                    break;
                }
            }
            var flag2 = false;
            var stls = self.room.socialEventRecognizer.stolenProperty;
            for (var j = 0; j < stls.Count; j++)
            {
                if (item.ID == stls[j])
                {
                    flag2 = true;
                    break;
                }
            }
            if (!flag1 && !flag2)
                outpost.feePayed += value;
        }
        orig(self, player, item, value);
    }

    internal static bool On_ScavengerSquad_get_HasAMission(Func<ScavengerAbstractAI.ScavengerSquad, bool> orig, ScavengerAbstractAI.ScavengerSquad self)
    {
        if (!ModManager.MSC && self.leader?.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel && self.targetCreature is AbstractCreature crit && crit.world.GetAbstractRoom(crit.pos.room) is AbstractRoom rm && (rm.shelter || rm.gate))
            return false;
        return orig(self);
    }

    internal static void IL_ScavengerTreasury_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((bool flag, ScavengerTreasury self, int i) => flag || self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerTreasury.Update!");
    }

    internal static void IL_WorldFloodFiller_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var vars = il.Body.Variables;
        if (c.TryGotoNext(
            s_MatchLdloc_OutLoc2,
            s_MatchLdfld_AbstractRoom_connections,
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_WorldCoordinate_abstractNode,
            s_MatchLdelemI4,
            s_MatchLdcI4_M1,
            s_MatchBle_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((ScavengersWorldAI.WorldFloodFiller self, WorldCoordinate worldCoordinate, AbstractRoom rm) => worldCoordinate.abstractNode < rm.connections.Length);
            c.Emit(OpCodes.Brfalse, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerWorldAI.WorldFloodFiller.Update! (part 1)");
        if (c.TryGotoNext(
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix,
            s_MatchLdloc_OutLoc2,
            s_MatchLdfld_WorldCoordinate_room,
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world,
            s_MatchCallOrCallvirt_World_get_firstRoomIndex,
            s_MatchSub,
            s_MatchLdelemRef,
            s_MatchLdloc_InLoc2,
            s_MatchLdfld_WorldCoordinate_abstractNode,
            s_MatchLdelemU1,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((ScavengersWorldAI.WorldFloodFiller self, WorldCoordinate item) =>
             {
                 var index = item.room - self.world.firstRoomIndex;
                 return index >= 0 && index < self.nodesMatrix.Length && index < self.roomsMatrix.Length && item.abstractNode >= 0 && item.abstractNode < self.nodesMatrix[index].Length;
             });
            c.Emit(OpCodes.Brfalse, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerWorldAI.WorldFloodFiller.Update! (part 2)");
        if (c.TryGotoNext(
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix,
            s_MatchLdloc_InLoc1,
            s_MatchLdfld_WorldCoordinate_room,
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world,
            s_MatchCallOrCallvirt_World_get_firstRoomIndex,
            s_MatchSub,
            s_MatchLdelemRef,
            s_MatchLdloc_OutLoc2,
            s_MatchLdelemU1,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((ScavengersWorldAI.WorldFloodFiller self, WorldCoordinate worldCoordinate, int i) =>
             {
                 var index = worldCoordinate.room - self.world.firstRoomIndex;
                 return index >= 0 && index < self.nodesMatrix.Length && index < self.roomsMatrix.Length && i < self.nodesMatrix[index].Length;
             });
            c.Emit(OpCodes.Brfalse, s_label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerWorldAI.WorldFloodFiller.Update! (part 3)");
    }
}