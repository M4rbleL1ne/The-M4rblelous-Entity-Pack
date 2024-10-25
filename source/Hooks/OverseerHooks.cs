﻿global using static LBMergedMods.Hooks.OverseerHooks;
using MonoMod.Cil;
using UnityEngine;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class OverseerHooks
{
    internal static void IL_OverseerAbstractAI_HowInterestingIsCreature(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("BlackLizard"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == CreatureTemplateType.SilverLizard || testCrit.creatureTemplate.type == CreatureTemplateType.Polliwog || testCrit.creatureTemplate.type == CreatureTemplateType.WaterSpitter || testCrit.creatureTemplate.type == CreatureTemplateType.HunterSeeker || testCrit.creatureTemplate.type == CreatureTemplateType.MoleSalamander);
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature!");
    }

    internal static float On_OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit)
    {
        var res = orig(self, testCrit);
        if (testCrit is not null)
        {
            var tpl = testCrit.creatureTemplate.type;
            if (tpl == CreatureTemplateType.FatFireFly)
            {
                if (ModManager.MSC && self.safariOwner)
                    return testCrit == self.targetCreature ? 1f : 0f;
                res = .25f;
                if (testCrit.state.dead)
                    res /= 10f;
                res *= testCrit.Room.AttractionValueForCreature(self.parent.creatureTemplate.type);
                res *= Mathf.Lerp(.5f, 1.5f, self.world.game.SeededRandom(self.parent.ID.RandomSeed + testCrit.ID.RandomSeed));
            }
            else if (tpl == CreatureTemplateType.FlyingBigEel)
            {
                if (ModManager.MSC && self.safariOwner)
                    return testCrit == self.targetCreature ? 1f : 0f;
                res = .55f;
                if (testCrit.state.dead)
                    res /= 10f;
                res *= testCrit.Room.AttractionValueForCreature(self.parent.creatureTemplate.type);
                res *= Mathf.Lerp(.5f, 1.5f, self.world.game.SeededRandom(self.parent.ID.RandomSeed + testCrit.ID.RandomSeed));
            }
        }
        return res;
    }

    internal static float On_OverseerCommunicationModule_FoodDelicousScore(On.OverseerCommunicationModule.orig_FoodDelicousScore orig, OverseerCommunicationModule self, AbstractPhysicalObject foodObject, Player player)
    {
        if (foodObject?.realizedObject is PhysicalObject obj && (obj is ThornyStrawberry or BouncingMelon or LittleBalloon or Physalis or MarineEye) && !foodObject.slatedForDeletion && foodObject.Room == player.abstractCreature.Room)
        {
            var num = Mathf.InverseLerp(1100f, 400f, Vector2.Distance(obj.FirstChunk().pos, player.DangerPos));
            if (num == 0f)
                return 0f;
            if (self.GuideState.itemTypes.Contains(foodObject.type))
            {
                if (num <= .2f || !self.room.ViewedByAnyCamera(obj.FirstChunk().pos, 0f))
                    return 0f;
                num = .3f;
            }
            var objs = self.objectsAlreadyTalkedAbout;
            for (var i = 0; i < objs.Count; i++)
            {
                if (objs[i] == foodObject.ID)
                    return 0f;
            }
            if (foodObject == self.mostDeliciousFoodInRoom && self.currentConcern == OverseerCommunicationModule.PlayerConcern.FoodItemInRoom)
                num *= 1.1f;
            return num * Mathf.Lerp(self.GeneralPlayerFoodNeed(player), .6f, .5f);
        }
        else
            return orig(self, foodObject, player);
    }

    internal static void IL_OverseerCommunicationModule_FoodDelicousScore(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<AbstractPhysicalObject>("type"),
            x => x.MatchLdsfld<AbstractPhysicalObject.AbstractObjectType>("JellyFish"),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, AbstractPhysicalObject foodObject) => flag && foodObject.type != AbstractObjectType.GummyAnther);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerCommunicationModule.FoodDelicousScore!");
    }
}