global using static LBMergedMods.Hooks.LizardHooks;
using LizardCosmetics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using Noise;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class LizardHooks
{
    internal static void IL_Antennae_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            s_MatchStfld_Antennae_length))
        {
            c.MoveAfterLabels()
             .Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float value, Antennae self) =>
             {
                 if (self.lGraphics.lizard is AlphaOrange l)
                 {
                     var state = Random.state;
                     Random.InitState(l.abstractPhysicalObject.ID.RandomSeed);
                     value = Random.Range(.75f, 1f);
                     Random.state = state;
                 }
                 return value;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Antennae.ctor!");
    }

    internal static void On_AxolotlGills_DrawSprites(On.LizardCosmetics.AxolotlGills.orig_DrawSprites orig, AxolotlGills self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is PolliwogGraphics lg && lg.lizard is Polliwog l && l.AI?.yellowAI is PolliwogCommunication c)
        {
            var flicker = Mathf.Lerp(c.LastFlicker, c.CurrentFlicker, timeStacker);
            if (!l.Consious)
                flicker = 0f;
            var sprites = sLeaser.sprites;
            var lgt = self.scalesPositions.Length;
            var headColor = lg.HeadColor(timeStacker);
            for (var num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
            {
                sprites[num].color = Color.Lerp(headColor, Color.Lerp(headColor, lg.effectColor, .6f), flicker);
                if (self.colored)
                    sprites[num + lgt].color = c.PackLeader ? headColor : Color.Lerp(headColor, new(1f, .007843137254902f, .3529411764705882f), flicker);
            }
        }
    }

    internal static void On_LongBodyScales_ApplyPalette(On.LizardCosmetics.LongBodyScales.orig_ApplyPalette orig, LongBodyScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        int lgt, num;
        Color c, c2;
        FSprite[] sprs;
        if (self is LongHeadScales)
        {
            if (self.lGraphics is LizardGraphics lg && lg is HunterSeekerGraphics or SilverLizardGraphics or MoleSalamanderGraphics or PolliwogGraphics or WaterSpitterGraphics or AlphaOrangeGraphics)
            {
                lgt = self.scalesPositions.Length;
                c = lg.HeadColor(1f);
                c2 = lg is HunterSeekerGraphics ? c : lg.effectColor;
                sprs = sLeaser.sprites;
                for (num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
                {
                    sprs[num].color = c;
                    if (self.colored)
                        sprs[num + lgt].color = c2;
                }
            }
        }
        else if (self is AxolotlGills && self.lGraphics is LizardGraphics lg)
        {
            if (lg is PolliwogGraphics && lg.lizard is Polliwog l && l.AI?.yellowAI is PolliwogCommunication co)
            {
                lgt = self.scalesPositions.Length;
                var flicker = l.Consious ? co.CurrentFlicker : 0f;
                sprs = sLeaser.sprites;
                c = lg.HeadColor(1f);
                c2 = lg.effectColor;
                for (num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
                {
                    sprs[num].color = Color.Lerp(c, Color.Lerp(c, c2, .6f), flicker);
                    if (self.colored)
                        sprs[num + lgt].color = co.PackLeader ? c : Color.Lerp(c, new(1f, .007843137254902f, .3529411764705882f), flicker);
                }
            }
            else if (self.lGraphics is MoleSalamanderGraphics or WaterSpitterGraphics or SilverLizardGraphics)
            {
                lgt = self.scalesPositions.Length;
                c = lg.HeadColor(1f);
                c2 = lg.effectColor;
                sprs = sLeaser.sprites;
                for (num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
                {
                    sprs[num].color = c;
                    if (self.colored)
                        sprs[num + lgt].color = c2;
                }
            }
        }
        else
        {
            if (self.lGraphics is HunterSeekerGraphics lg1)
            {
                lgt = self.scalesPositions.Length;
                c = lg1.BodyColor(1f);
                c2 = lg1.HeadColor(1f);
                sprs = sLeaser.sprites;
                for (num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
                {
                    sprs[num].color = c;
                    if (self.colored)
                        sprs[num + lgt].color = c2;
                }
            }
        }
    }

    internal static void On_BumpHawk_DrawSprites(On.LizardCosmetics.BumpHawk.orig_DrawSprites orig, BumpHawk self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            var sprites = sLeaser.sprites;
            int start = self.startSprite, numMax = start + self.numberOfSprites - 1;
            for (var num = numMax; num >= start; num--)
            {
                var spr = sprites[num];
                var num2 = Mathf.InverseLerp(start, numMax, num);
                if (self.coloredHawk)
                    spr.color = Color.Lerp(g.HeadColor(timeStacker), g.BodyColor(1f), num2);
                else
                    spr.color = g.DynamicBodyColor(1f);
            }
        }
    }

    internal static void On_BumpHawk_ApplyPalette(On.LizardCosmetics.BumpHawk.orig_ApplyPalette orig, BumpHawk self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            var sprites = sLeaser.sprites;
            int start = self.startSprite, numMax = start + self.numberOfSprites - 1;
            for (var num = numMax; num >= start; num--)
            {
                var spr = sprites[num];
                var num2 = Mathf.InverseLerp(start, numMax, num);
                if (self.coloredHawk)
                    spr.color = Color.Lerp(g.HeadColor(1f), g.BodyColor(1f), num2);
                else
                    spr.color = g.DynamicBodyColor(1f);
            }
        }
    }

    internal static void On_JumpRings_DrawSprites(On.LizardCosmetics.JumpRings.orig_DrawSprites orig, JumpRings self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            var sprs = sLeaser.sprites;
            var c = g.BodyColor(1f);
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                    sprs[self.RingSprite(i, j, 1)].color = c;
            }
        }
    }

    internal static void On_JumpRings_ApplyPalette(On.LizardCosmetics.JumpRings.orig_ApplyPalette orig, JumpRings self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            var sprs = sLeaser.sprites;
            var c = g.BodyColor(1f);
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                    sprs[self.RingSprite(i, j, 1)].color = c;
            }
        }
    }

    internal static void IL_Lizard_Act(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            s_MatchLdarg_0,
            s_MatchCallOrCallvirt_Creature_get_abstractCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_CyanLizard,
            s_MatchCall_Any,
            s_MatchBrfalse_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Isinst, il.Import(typeof(HunterSeeker)))
             .Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.Act!");
    }

    internal static void IL_Lizard_ActAnimation(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_Lizard_animation,
            s_MatchLdsfld_Lizard_Animation_Spit,
            s_MatchCall_Any,
            s_MatchBrfalse_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Lizard self) =>
             {
                 if (self is WaterSpitter l)
                 {
                     l.SpitWater();
                     return true;
                 }
                 return false;
             });
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.ActAnimation!");
    }

    internal static void On_Lizard_DamageAttackClosestChunk(On.Lizard.orig_DamageAttackClosestChunk orig, Lizard self, Creature target)
    {
        if (self.grabbedBy is List<Creature.Grasp> l && l.Count > 0 && l[0].grabber is FatFireFly c && c == target)
            return;
        orig(self, target);
    }

    internal static void IL_Lizard_EnterAnimation(ILContext il)
    {
        var c = new ILCursor(il);
        var label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchCallOrCallvirt_Creature_get_abstractCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_YellowLizard,
            s_MatchCall_Any,
            s_MatchBrfalse_Any))
        {
            label.Target = c.Next;
            c.Index -= 7;
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Isinst, il.Import(typeof(Polliwog)))
             .Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.EnterAnimation!");
    }

    internal static void On_Lizard_EnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard self, Lizard.Animation anim, bool forceAnimationChange)
    {
        if (self is AlphaOrange && (forceAnimationChange || (int)anim >= (int)self.animation) && self.animation != anim && anim == Lizard.Animation.PreyReSpotted && !self.safariControlled)
        {
            if (self.AI.yellowAI.pack?.PackLeader == self.abstractCreature)
                self.voice.MakeSound(LizardVoice.Emotion.SpottedPreyFirstTime);
            else
                self.voice.MakeSound(LizardVoice.Emotion.ReSpottedPrey, Random.Range(.1f, .25f));
        }
        orig(self, anim, forceAnimationChange);
    }

    internal static bool On_LizardGraphics_get_HeadLightsUpFromNoise(Func<LizardGraphics, bool> orig, LizardGraphics self)
    {
        if (self is MoleSalamanderGraphics s)
            return s.blackSalamander;
        return self is not NoodleEaterGraphics and not CommonEelGraphics && orig(self);
    }
        
    internal static bool On_Lizard_HitHeadShield(On.Lizard.orig_HitHeadShield orig, Lizard self, Vector2 direction) => self is not NoodleEater and not CommonEel && orig(self, direction);

    internal static void IL_Lizard_SwimBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            s_MatchLdcI4_1,
            s_MatchStloc_OutLoc1))
        {
            var l = il.Body.Variables[s_loc1];
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, l)
             .EmitDelegate((Lizard self, bool flag) => self is not WaterSpitter and not Polliwog and not MoleSalamander and not CommonEel && flag);
            c.Emit(OpCodes.Stloc, l);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.SwimBehavior! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchRet))
        {
            c.Prev.OpCode = OpCodes.Ldarg_0;
            c.EmitDelegate((Lizard self) =>
            {
                if (self is WaterSpitter)
                    self.salamanderLurk = false;
            });
            c.Emit(OpCodes.Ret);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.SwimBehavior! (part 2)");
    }

    internal static void On_LizardAI_BitCreature(On.LizardAI.orig_BitCreature orig, LizardAI self, BodyChunk chunk)
    {
        orig(self, chunk);
        if (chunk?.owner is Player p && p.objectInStomach is AbstractPhysicalObject obj && obj.type == AbstractObjectType.FumeFruit)
        {
            FumeFruit.Explode(p);
            obj.Destroy();
            p.objectInStomach = null;
        }
    }

    internal static bool On_LizardAI_ComfortableIdlePosition(On.LizardAI.orig_ComfortableIdlePosition orig, LizardAI self) => orig(self) || (self is PolliwogAI or WaterSpitterAI or MoleSalamanderAI or CommonEelAI && self.lizard is Lizard l && l.room.GetTile(l.firstChunk.pos).AnyWater);

    internal static bool On_LizardAI_FallRisk(On.LizardAI.orig_FallRisk orig, LizardAI self, IntVector2 tile) => (self is not CommonEelAI || !self.lizard.room.GetTile(self.lizard.room.aimap.getAItile(tile).fallRiskTile).AnyWater) && orig(self, tile);

    internal static void On_LizardAI_GiftRecieved(On.LizardAI.orig_GiftRecieved orig, LizardAI self, SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe)
    {
        if (self is not CommonEelAI)
            orig(self, giftOfferedToMe);
    }

    internal static float On_LizardAI_IdleSpotScore(On.LizardAI.orig_IdleSpotScore orig, LizardAI self, WorldCoordinate coord)
    {
        var res = orig(self, coord);
        if (coord.room != self.creature.pos.room || !coord.TileDefined)
            return res;
        if (self.lizard?.room.aimap.WorldCoordinateAccessibleToCreature(coord, self.creature.creatureTemplate) is null or false || !self.pathFinder.CoordinateReachableAndGetbackable(coord) || coord.CompareDisregardingNode(self.forbiddenIdleSpot))
            return res;
        if (self is PolliwogAI or WaterSpitterAI or MoleSalamanderAI && self.lizard is Lizard l)
        {
            if (!l.room.GetTile(coord).AnyWater)
                res += 20f;
            res += Mathf.Max(0f, coord.Tile.FloatDist(self.creature.pos.Tile) - 30f) * 1.5f;
            res += Mathf.Abs(coord.y - l.room.DefaultWaterLevel(coord.Tile)) * 10f;
            res += l.room.aimap.getTerrainProximity(coord) * 10f;
            if (self is MoleSalamanderAI or CommonEelAI)
            {
                if (l.room.aimap.getAItile(coord).narrowSpace)
                    res -= 10f;
                res += l.room.aimap.getAItile(coord.Tile).visibility * .1f;
            }
        }
        return res;
    }

    internal static float On_LizardAI_LikeOfPlayer(On.LizardAI.orig_LikeOfPlayer orig, LizardAI self, Tracker.CreatureRepresentation player) => self is CommonEelAI ? 0f : orig(self, player);

    internal static void On_LizardAI_ReactToNoise(On.LizardAI.orig_ReactToNoise orig, LizardAI self, NoiseTracker.TheorizedSource source, InGameNoise noise)
    {
        if (self is MoleSalamanderAI && source.creatureRep is not null)
        {
            self.lizard.bubble = Math.Max(self.lizard.bubble, 4);
            return;
        }
        orig(self, source, noise);
    }

    internal static bool On_LizardAI_UnpleasantFallRisk(On.LizardAI.orig_UnpleasantFallRisk orig, LizardAI self, IntVector2 tile) => (self is not CommonEelAI || !self.lizard.room.GetTile(self.lizard.room.aimap.getAItile(tile).fallRiskTile).AnyWater) && orig(self, tile);

    internal static void IL_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_Math_Max_int_int,
            s_MatchStfld_LizardAI_LizardTrackState_vultureMask))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((LizardAI self, RelationshipTracker.DynamicRelationship dRelation) =>
             {
                 if (self is CommonEelAI)
                     (dRelation.state as LizardAI.LizardTrackState)!.vultureMask = 0;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship! (part 1)");
        if (c.TryGotoNext(
            s_MatchLdarg_1,
            s_MatchLdfld_RelationshipTracker_DynamicRelationship_state,
            s_MatchIsinst_LizardAI_LizardTrackState,
            s_MatchLdfld_LizardAI_LizardTrackState_vultureMask,
            s_MatchLdcI4_0,
            s_MatchBle_OutLabel))
        {
            c.Next.OpCode = OpCodes.Ldarg_0;
            ++c.Index;
            c.Emit(OpCodes.Isinst, il.Import(typeof(MoleSalamanderAI)))
             .Emit(OpCodes.Brtrue, s_label)
             .Emit(OpCodes.Ldarg_1);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 2)");
    }

    internal static CreatureTemplate.Relationship On_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var res = orig(self, dRelation);
        var rel = dRelation.trackerRep.representedCreature.creatureTemplate;
        var tp = self.creature.creatureTemplate.type;
        if (tp == CreatureTemplate.Type.BlueLizard)
        {
            if (rel.type == CreatureTemplateType.SilverLizard || rel.type == CreatureTemplateType.HunterSeeker || rel.type == CreatureTemplateType.WaterSpitter)
                res = new(CreatureTemplate.Relationship.Type.Afraid, 1f);
        }
        else if (self is PolliwogAI)
        {
            if (rel.type == CreatureTemplateType.SilverLizard || rel.type == CreatureTemplateType.HunterSeeker || rel.type == CreatureTemplateType.WaterSpitter || (ModManager.DLCShared && tp == DLCSharedEnums.CreatureTemplateType.SpitLizard))
                res = new(CreatureTemplate.Relationship.Type.Afraid, 1f);
        }
        else if (tp == CreatureTemplateType.NoodleEater)
        {
            if (rel.type == CreatureTemplateType.SilverLizard || rel.type == CreatureTemplateType.HunterSeeker || rel.type == CreatureTemplateType.WaterSpitter || (ModManager.DLCShared && tp == DLCSharedEnums.CreatureTemplateType.SpitLizard))
                res = new(CreatureTemplate.Relationship.Type.Afraid, 1f);
            else if (rel.TopAncestor().type == CreatureTemplate.Type.Scavenger)
                res = new(CreatureTemplate.Relationship.Type.Ignores, 0f);
            else if (rel.type?.value == "DrainMite")
                res = new(CreatureTemplate.Relationship.Type.Eats, 1f);
            else if (rel.type?.value == "SnootShootNoot")
                res = new(CreatureTemplate.Relationship.Type.Afraid, .15f);
            else if ((rel.type == CreatureTemplate.Type.Slugcat && res.type == CreatureTemplate.Relationship.Type.Eats) || res.type == CreatureTemplate.Relationship.Type.Attacks)
                res.type = CreatureTemplate.Relationship.Type.Afraid;
        }
        else if ((self is HunterSeekerAI or WaterSpitterAI || tp == CreatureTemplateType.SilverLizard) && (rel.type == CreatureTemplate.Type.BlueLizard || rel.type == CreatureTemplateType.Polliwog || rel.type == CreatureTemplateType.NoodleEater))
            res = new(CreatureTemplate.Relationship.Type.Eats, 1f);
        else if (ModManager.DLCShared && tp == DLCSharedEnums.CreatureTemplateType.SpitLizard && (rel.type == CreatureTemplateType.Polliwog || rel.type == CreatureTemplateType.NoodleEater))
            res = new(CreatureTemplate.Relationship.Type.Eats, 1f);
        return res;
    }

    internal static bool On_LizardBreedParams_get_WallClimber(Func<LizardBreedParams, bool> orig, LizardBreedParams self) => self.template == CreatureTemplateType.HunterSeeker || self.template == CreatureTemplateType.NoodleEater || orig(self);

    internal static CreatureTemplate On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        CreatureTemplate temp;
        LizardBreedParams breedParams;
        if (type == CreatureTemplateType.SilverLizard)
        {
            temp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "SilverLizard";
            breedParams.template = type;
            breedParams.baseSpeed = 3.1f;
            breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[4] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[6] = new(.1f, 1f, 1f, 1f);
            breedParams.standardColor = new(.654f, .811f, .858f);
            breedParams.biteDelay = 3;
            breedParams.biteInFront = 20f;
            breedParams.biteRadBonus = 20f;
            breedParams.biteHomingSpeed = 4.5f;
            breedParams.biteChance = .9f;
            breedParams.attemptBiteRadius = 120f;
            breedParams.getFreeBiteChance = 1f;
            breedParams.biteDamage = 2f;
            breedParams.biteDamageChance = .4f;
            breedParams.toughness = 5f;
            breedParams.stunToughness = 4f;
            breedParams.regainFootingCounter = 1;
            breedParams.bodyMass = 4f;
            breedParams.bodySizeFac = 1.6f;
            breedParams.floorLeverage = 8f;
            breedParams.maxMusclePower = 12f;
            breedParams.wiggleSpeed = .4f;
            breedParams.wiggleDelay = 20;
            breedParams.bodyStiffnes = .4f;
            breedParams.swimSpeed = 1.1f;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
            breedParams.danger = .6f;
            breedParams.aggressionCurveExponent = .7f;
            breedParams.headShieldAngle = 160f;
            breedParams.canExitLounge = false;
            breedParams.canExitLoungeWarmUp = false;
            breedParams.findLoungeDirection = .5f;
            breedParams.loungeDistance = 200f;
            breedParams.preLoungeCrouch = 35;
            breedParams.preLoungeCrouchMovement = -.4f;
            breedParams.loungeSpeed = 2.7f;
            breedParams.loungeMaximumFrames = 30;
            breedParams.loungePropulsionFrames = 30;
            breedParams.loungeJumpyness = .6f;
            breedParams.loungeDelay = 60;
            breedParams.riskOfDoubleLoungeDelay = .1f;
            breedParams.postLoungeStun = 15;
            breedParams.loungeTendensy = 1f;
            temp.visualRadius = 2300f;
            temp.waterVision = .7f;
            temp.throughSurfaceVision = .95f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 4f / 9f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 9f);
            breedParams.biteDominance = 1f;
            breedParams.limbSize = 2.9f;
            breedParams.stepLength = 1f;
            breedParams.liftFeet = .3f;
            breedParams.feetDown = .5f;
            breedParams.noGripSpeed = .25f;
            breedParams.limbSpeed = 6f;
            breedParams.limbQuickness = .6f;
            breedParams.limbGripDelay = 1;
            breedParams.smoothenLegMovement = true;
            breedParams.legPairDisplacement = .3f;
            breedParams.walkBob = 2.7f;
            breedParams.tailSegments = 16;
            breedParams.tailStiffness = 300f;
            breedParams.tailStiffnessDecline = .25f;
            breedParams.tailLengthFactor = .8f;
            breedParams.tailColorationStart = .9f;
            breedParams.tailColorationExponent = 8f;
            breedParams.headSize = 1.3f;
            breedParams.neckStiffness = .37f;
            breedParams.jawOpenAngle = 150f;
            breedParams.jawOpenLowerJawFac = .7666667f;
            breedParams.jawOpenMoveJawsApart = 25f;
            breedParams.headGraphics = new int[5];
            breedParams.framesBetweenLookFocusChange = 20;
            breedParams.tamingDifficulty = 6f;
            temp.movementBasedVision = .3f;
            temp.waterPathingResistance = 3f;
            temp.dangerousToPlayer = breedParams.danger;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = greenTemplate;
            return temp;
        }
        if (type == CreatureTemplateType.NoodleEater)
        {
            temp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "NoodleEater";
            breedParams.template = type;
            breedParams.baseSpeed = 5.2f;
            breedParams.biteDamage = .5f;
            breedParams.biteDominance = 0f;
            breedParams.loungeTendensy = 0f;
            breedParams.toughness = .1f;
            breedParams.headShieldAngle = 0f;
            breedParams.tongueChance = .6f;
            breedParams.tamingDifficulty = 1f;
            breedParams.tailColorationStart = .4f;
            breedParams.headSize = .8f;
            breedParams.headGraphics = [2, 2, 2, 2, 2];
            breedParams.tongueAttackRange = 350f;
            breedParams.tongueWarmUp = 2;
            breedParams.tongueSegments = 7;
            breedParams.jawOpenAngle = 90f;
            breedParams.jawOpenLowerJawFac = .6666f;
            breedParams.jawOpenMoveJawsApart = 15f;
            breedParams.tailColorationExponent = 1.4f;
            breedParams.tailLengthFactor = 1.1f;
            breedParams.standardColor = NoodleEater.NEatColor;
            breedParams.limbSpeed = 9f;
            breedParams.limbSize *= .7f;
            breedParams.limbQuickness = .9f;
            breedParams.noGripSpeed = .6f;
            breedParams.toughness = 0.25f;
            breedParams.stunToughness = .4f;
            breedParams.tailSegments = 3;
            breedParams.walkBob = .2f;
            breedParams.stepLength = .2f;
            breedParams.limbThickness = 1.1f;
            breedParams.danger = 0f;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = blueTemplate;
            temp.abstractedLaziness = 10;
            temp.dangerousToPlayer = 0f;
            return temp;
        }
        if (type == CreatureTemplateType.Polliwog)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            breedParams.terrainSpeeds[4] = new(1.1f, 1f, 1f, 1f);
            breedParams.bodySizeFac = .7f;
            breedParams.headSize = 1f;
            breedParams.limbSize = .7f;
            breedParams.tongue = true;
            breedParams.toughness = .5f;
            temp.name = "Polliwog";
            temp.type = type;
            temp.baseDamageResistance = .5f;
            temp.meatPoints = 5;
            temp.canSwim = true;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander);
            temp.throwAction = "Call";
            return temp;
        }
        if (type == CreatureTemplateType.WaterSpitter)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            temp.name = "WaterSpitter";
            temp.type = type;
            temp.canSwim = true;
            temp.requireAImap = true;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.DropToFloor] = new(40f, PathCost.Legality.Allowed);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.LizardTurn] = new(60f, PathCost.Legality.Allowed);
            temp.waterPathingResistance = .5f;
            temp.movementBasedVision = .6f;
            breedParams.biteChance = .4f;
            breedParams.biteDelay = 15;
            breedParams.biteInFront = 40f;
            breedParams.biteHomingSpeed = .7f;
            breedParams.attemptBiteRadius = 100f;
            breedParams.getFreeBiteChance = .65f;
            breedParams.biteDamage = 1.6f;
            breedParams.toughness = 2.5f;
            breedParams.stunToughness = 1.5f;
            breedParams.regainFootingCounter = 10;
            breedParams.baseSpeed = 4.5f;
            breedParams.bodySizeFac = 1.2f;
            breedParams.floorLeverage = 7f;
            breedParams.maxMusclePower = 16f;
            temp.dangerousToPlayer = breedParams.danger = .55f;
            breedParams.aggressionCurveExponent = .95f;
            breedParams.wiggleSpeed = .5f;
            breedParams.wiggleDelay = 25;
            breedParams.bodyStiffnes = .5f;
            breedParams.swimSpeed = .25f;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 0;
            breedParams.biteDominance = .8f;
            breedParams.headShieldAngle = 100f;
            breedParams.loungeTendensy = 0f;
            temp.visualRadius = 1200f;
            temp.waterVision = 1f;
            temp.throughSurfaceVision = 1f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 5f / 18f);
            breedParams.limbSize = 1.4f;
            breedParams.stepLength = .9f;
            breedParams.liftFeet = .5f;
            breedParams.feetDown = 1f;
            breedParams.noGripSpeed = .05f;
            breedParams.limbSpeed = 3f;
            breedParams.limbQuickness = .3f;
            breedParams.limbGripDelay = 1;
            breedParams.smoothenLegMovement = false;
            breedParams.legPairDisplacement = 1f;
            breedParams.standardColor = Color.white;
            breedParams.walkBob = 4f;
            breedParams.tailSegments = 8;
            breedParams.tailStiffness = 300f;
            breedParams.tailStiffnessDecline = .6f;
            breedParams.tailLengthFactor = 1.3f;
            breedParams.tailColorationStart = .05f;
            breedParams.tailColorationExponent = 4f;
            breedParams.headSize = 1.1f;
            breedParams.neckStiffness = 1f;
            breedParams.jawOpenAngle = 40f;
            breedParams.jawOpenLowerJawFac = .5f;
            breedParams.jawOpenMoveJawsApart = 14f;
            breedParams.framesBetweenLookFocusChange = 180;
            breedParams.tamingDifficulty = 3.8f;
            breedParams.tongue = false;
            breedParams.headGraphics = [1, 1, 1, 1, 2];
            temp.meatPoints = 7;
            temp.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
            temp.wormGrassImmune = true;
            temp.bodySize = 3f;
            temp.baseDamageResistance = 4f;
            temp.baseStunResistance = 2.5f;
            temp.damageRestistances[2, 0] = 2.5f;
            temp.damageRestistances[2, 1] = 3f;
            temp.throwAction = "Spit";
            temp.jumpAction = "N/A";
            return temp;
        }
        if (type == CreatureTemplateType.HunterSeeker)
        {
            temp = orig(CreatureTemplate.Type.WhiteLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            temp.dangerousToPlayer = breedParams.danger = .65f;
            temp.type = type;
            temp.name = "HunterSeeker";
            temp.throwAction = "Camouflage/Launch";
            temp.requireAImap = true;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.WhiteLizard);
            return temp;
        }
        if (type == CreatureTemplateType.MoleSalamander)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            breedParams.headSize = 1.06f;
            breedParams.biteInFront = 26f;
            breedParams.biteDamage = 1f;
            breedParams.biteDamageChance = 5f / 14f;
            breedParams.stunToughness = 1f;
            breedParams.regainFootingCounter = 8;
            breedParams.baseSpeed = 4f;
            breedParams.bodySizeFac = .76f;
            breedParams.bodyMass = 1.5f;
            breedParams.floorLeverage = .35f;
            breedParams.maxMusclePower = 4.8f;
            breedParams.danger = .45f;
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.biteDominance = .5f;
            breedParams.limbSize = .7f;
            breedParams.stepLength = .6f;
            breedParams.liftFeet = .25f;
            breedParams.limbQuickness = .6f;
            breedParams.standardColor = new(.1f, .1f, .1f);
            breedParams.walkBob = 3f;
            breedParams.tailSegments = 12;
            breedParams.tailStiffness = 50f;
            breedParams.tailStiffnessDecline = .6f;
            breedParams.tailLengthFactor = 2f;
            breedParams.tailColorationStart = .5f;
            breedParams.jawOpenMoveJawsApart = 14f;
            breedParams.framesBetweenLookFocusChange = 100;
            breedParams.tamingDifficulty = 4f;
            breedParams.jawOpenAngle = 40f;
            breedParams.jawOpenLowerJawFac = .3f;
            temp.dangerousToPlayer = breedParams.danger;
            temp.visualRadius = .001f;
            temp.waterVision = 40000f;
            temp.throughSurfaceVision = 0f;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander);
            temp.requireAImap = true;
            temp.name = "Mole Salamander";
            temp.type = CreatureTemplateType.MoleSalamander;
            temp.BlizzardWanderer = true;
            temp.canSwim = true;
            temp.BlizzardAdapted = true;
            temp.throughSurfaceVision = 0f;
            return temp;
        }
        if (type == CreatureTemplateType.CommonEel)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.biteDelay = 2;
            breedParams.biteInFront = 20f;
            breedParams.biteRadBonus = 20f;
            breedParams.biteHomingSpeed = 3f;
            breedParams.biteChance = 1f;
            breedParams.attemptBiteRadius = 100f;
            breedParams.getFreeBiteChance = 0f;
            breedParams.biteDamage = 2.5f;
            breedParams.biteDamageChance = 1f;
            breedParams.toughness = 3f;
            breedParams.stunToughness = 3f;
            breedParams.baseSpeed = 3.75f;
            breedParams.bodyMass = 2f;
            breedParams.bodySizeFac = 1.5f;
            breedParams.maxMusclePower = 10f;
            breedParams.wiggleSpeed = .2f;
            breedParams.wiggleDelay = 30;
            breedParams.bodyStiffnes = .2f;
            breedParams.swimSpeed = 8f;
            breedParams.danger = .9f;
            breedParams.aggressionCurveExponent = .7f;
            breedParams.loungeTendensy = 0f;
            breedParams.perfectVisionAngle = .6f;
            breedParams.periferalVisionAngle = .9f;
            breedParams.biteDominance = 1f;
            breedParams.limbSize = 0f;
            breedParams.limbThickness = 0f;
            breedParams.standardColor = CommonEel.EelCol;
            breedParams.walkBob = 1.5f;
            breedParams.tailSegments = 24;
            breedParams.tailStiffness = 250f;
            breedParams.tailStiffnessDecline = .5f;
            breedParams.tailLengthFactor = 1.5f;
            breedParams.tailColorationStart = 0f;
            breedParams.tailColorationExponent = 0f;
            breedParams.headSize = 2f;
            breedParams.neckStiffness = 1f;
            breedParams.jawOpenAngle = 90f;
            breedParams.jawOpenLowerJawFac = .33333f;
            breedParams.jawOpenMoveJawsApart = 2f;
            breedParams.headGraphics = [2, 2, 2, 2, 2];
            breedParams.framesBetweenLookFocusChange = 50;
            breedParams.tamingDifficulty = float.MaxValue;
            breedParams.template = type;
            breedParams.tongue = false;
            temp.dangerousToPlayer = breedParams.danger;
            temp.waterPathingResistance = .35f;
            temp.jumpAction = "N/A";
            temp.baseDamageResistance = 3.1f;
            temp.baseStunResistance = 3.4f;
            temp.visualRadius = 4000f;
            temp.waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
            temp.waterVision = 1f;
            temp.throughSurfaceVision = 0f;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigEel);
            temp.requireAImap = true;
            temp.canSwim = true;
            temp.name = "Common Eel";
            temp.type = type;
            temp.bodySize = 4f;
            temp.meatPoints = 8;
            temp.BlizzardWanderer = true;
            temp.BlizzardAdapted = true;
            temp.pathingPreferencesTiles[(int)AItile.Accessibility.Floor] = new(10f, PathCost.Legality.Unwanted);
            temp.pathingPreferencesTiles[(int)AItile.Accessibility.Corridor] = new(10f, PathCost.Legality.Allowed);
            temp.pathingPreferencesTiles[(int)AItile.Accessibility.Climb] = new(10000f, PathCost.Legality.IllegalTile);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.DropToClimb] = new(10000f, PathCost.Legality.IllegalConnection);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.BetweenRooms] = new(5f, PathCost.Legality.Allowed);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.Slope] = new(10f, PathCost.Legality.Unwanted);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.CeilingSlope] = new(10f, PathCost.Legality.Unwanted);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.DropToWater] = new(1f, PathCost.Legality.Allowed);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.DropToFloor] = new(10f, PathCost.Legality.Unwanted);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.ReachOverGap] = new(10f, PathCost.Legality.Unwanted);
            return temp;
        }
        if (type == CreatureTemplateType.AlphaOrange)
        {
            breedParams = new LizardBreedParams(type)
            {
                terrainSpeeds = new LizardBreedParams.SpeedMultiplier[Enum.GetNames(typeof(AItile.Accessibility)).Length],
                bodyRadFac = 1f,
                pullDownFac = 1f,
                bodyLengthFac = 1f,
                biteDelay = 12,
                biteInFront = 15f,
                biteHomingSpeed = 1.2f,
                biteChance = 1f / 6f,
                attemptBiteRadius = 40f,
                getFreeBiteChance = .65f,
                biteDamage = 1f,
                biteDamageChance = .4f,
                toughness = .8f,
                stunToughness = 1f,
                regainFootingCounter = 3,
                baseSpeed = 4.84f,
                bodyMass = 2f,
                bodySizeFac = 1f,
                floorLeverage = .5f,
                maxMusclePower = 5f,
                danger = .7f,
                aggressionCurveExponent = .9f,
                wiggleSpeed = 1f,
                wiggleDelay = 15,
                bodyStiffnes = .2f,
                swimSpeed = .45f,
                idleCounterSubtractWhenCloseToIdlePos = 1,
                headShieldAngle = 100f,
                canExitLounge = true,
                canExitLoungeWarmUp = true,
                findLoungeDirection = 1f,
                loungeDistance = 130f,
                preLoungeCrouch = 35,
                preLoungeCrouchMovement = -.3f,
                loungeSpeed = 4.5f,
                loungeMaximumFrames = 20,
                loungePropulsionFrames = 10,
                loungeJumpyness = .9f,
                loungeDelay = 110,
                riskOfDoubleLoungeDelay = .8f,
                postLoungeStun = 10,
                loungeTendensy = .033f,
                perfectVisionAngle = Mathf.Lerp(1f, -1f, 1f / 12f),
                periferalVisionAngle = Mathf.Lerp(1f, -1f, 19f / 36f),
                biteDominance = .45f,
                limbSize = 1f,
                limbThickness = 1f,
                stepLength = .5f,
                liftFeet = .3f,
                feetDown = .5f,
                noGripSpeed = .1f,
                limbSpeed = 5f,
                limbQuickness = .5f,
                limbGripDelay = 1,
                smoothenLegMovement = true,
                legPairDisplacement = .2f,
                standardColor = new(1f, .6f, 0f),
                walkBob = 4f,
                tailSegments = 6,
                tailStiffness = 250f,
                tailStiffnessDecline = .2f,
                tailLengthFactor = 1.8f,
                tailColorationStart = .3f,
                tailColorationExponent = 2f,
                headSize = 1.2f,
                neckStiffness = .2f,
                jawOpenAngle = 110f,
                jawOpenLowerJawFac = .55f,
                jawOpenMoveJawsApart = 23f,
                headGraphics = new int[5],
                framesBetweenLookFocusChange = 80,
                tamingDifficulty = 3f
            };
            var speeds = breedParams.terrainSpeeds;
            for (var i = 0; i < speeds.Length; i++)
                speeds[i] = new(.1f, 1f, 1f, 1f);
            speeds[1] = new(1f, 1f, 1f, 1f);
            speeds[3] = new(1.2f, 1f, 1f, 1f);
            speeds[4] = new(.5f, 1f, .75f, 1f);
            speeds[5] = new(.8f, 1f, 1f, 1f);
            speeds[6] = new(.6f, 1f, 1f, 1f);
            temp = new CreatureTemplate(type, lizardAncestor,
            [
                new(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed),
                new(AItile.Accessibility.CurvedFloor, 1f, PathCost.Legality.Allowed),
                new(AItile.Accessibility.Corridor, 1.2f, PathCost.Legality.Allowed),
                new(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed),
                new(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed),
                new(AItile.Accessibility.Ceiling, 1.2f, PathCost.Legality.Allowed),
            ],
            [
                new(MovementConnection.MovementType.DropToFloor, 2f, PathCost.Legality.Allowed),
                new(MovementConnection.MovementType.DropToClimb, 2f, PathCost.Legality.Allowed),
                new(MovementConnection.MovementType.ShortCut, 5f, PathCost.Legality.Allowed),
                new(MovementConnection.MovementType.ReachOverGap, 1.1f, PathCost.Legality.Allowed),
                new(MovementConnection.MovementType.ReachUp, 1.1f, PathCost.Legality.Allowed),
                new(MovementConnection.MovementType.ReachDown, 1.1f, PathCost.Legality.Allowed),
                new(MovementConnection.MovementType.CeilingSlope, 2f, PathCost.Legality.Allowed),
            ],
            new(CreatureTemplate.Relationship.Type.Ignores, 0f))
            {
                name = "AlphaOrange",
                waterPathingResistance = 5f,
                visualRadius = 1000f,
                waterVision = .4f,
                throughSurfaceVision = .85f,
                breedParameters = breedParams,
                baseDamageResistance = breedParams.toughness * 2f,
                baseStunResistance = breedParams.toughness,
                meatPoints = 6,
                doPreBakedPathing = false,
                preBakedPathingAncestor = blueTemplate,
                virtualCreature = false,
                pickupAction = "Bite",
                jumpAction = "Call",
                throwAction = "Launch",
            };
            temp.damageRestistances[(int)Creature.DamageType.Bite, 0] = 2.5f;
            temp.damageRestistances[(int)Creature.DamageType.Bite, 1] = 3f;
            return temp;
        }
        return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }

    internal static void On_LizardBubble_ctor(On.LizardBubble.orig_ctor orig, LizardBubble self, LizardGraphics lizardGraphics, float intensity, float stickiness, float extraSpeed)
    {
        orig(self, lizardGraphics, intensity, stickiness, extraSpeed);
        if (lizardGraphics is NoodleEaterGraphics)
            self.lifeTime /= 2;
        else if (lizardGraphics is CommonEelGraphics)
            self.Destroy();
    }

    internal static void On_LizardBubble_DrawSprites(On.LizardBubble.orig_DrawSprites orig, LizardBubble self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.slatedForDeletetion && self.room == rCam.room && self.lizardGraphics is NoodleEaterGraphics gr)
        {
            var s0 = sLeaser.sprites[0];
            s0.color = gr.effectColor;
            if (gr.lizard?.dead is true)
                s0.isVisible = false;
        }
    }

    internal static Color On_LizardGraphics_get_effectColor(Func<LizardGraphics, Color> orig, LizardGraphics self)
    {
        var color = orig(self);
        if (self is WaterSpitterGraphics)
        {
            color = Color.Lerp(self.palette.waterSurfaceColor1, Color.white, .1f);
            if (self.lizard is WaterSpitter ws && ws.rotModule is LizardRotModule mod && ws.LizardState is LizardState st && st.rotType != LizardState.RotType.Slight)
                color = Color.Lerp(color, mod.RotEyeColor, st.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        }
        else if (self is MoleSalamanderGraphics && self.blackSalamander)
            color = self.palette.blackColor;
        return color;
    }

    internal static Color On_LizardGraphics_get_HeadColor1(Func<LizardGraphics, Color> orig, LizardGraphics self)
    {
        if (self is MoleSalamanderGraphics)
            return self.HeadLightsUpFromNoise ? Color.Lerp(self.palette.blackColor, new(.5f, .5f, .5f), self.blackLizardLightUpHead) : self.SalamanderColor;
        if (self is HunterSeekerGraphics)
            return Color.Lerp(Color.white, self.whiteCamoColor, self.whiteCamoColorAmount);
        return orig(self);
    }

    internal static Color On_LizardGraphics_get_HeadColor2(Func<LizardGraphics, Color> orig, LizardGraphics self)
    {
        if (self is HunterSeekerGraphics)
            return Color.Lerp(self.palette.blackColor, self.whiteCamoColor, self.whiteCamoColorAmount);
        if (self is MoleSalamanderGraphics)
            return self.HeadLightsUpFromNoise ? Color.Lerp(self.palette.blackColor, new(.5f, .5f, .5f), self.blackLizardLightUpHead) : self.SalamanderColor;
        return orig(self);
    }

    internal static int On_LizardGraphics_AddCosmetic(On.LizardGraphics.orig_AddCosmetic orig, LizardGraphics self, int spriteIndex, Template cosmetic)
    {
        if ((self is NoodleEaterGraphics && cosmetic is not LizardRotGraphics) || self is CommonEelGraphics)
            return spriteIndex;
        return orig(self, spriteIndex, cosmetic);
    }

    internal static bool On_LizardRotGraphics_get_ApplyPaletteInDraw(Func<LizardRotGraphics, bool> orig, LizardRotGraphics self) => orig(self) || self.lGraphics is HunterSeekerGraphics;

    internal static Color On_LizardGraphics_BodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
    {
        if (self is MoleSalamanderGraphics && !self.blackSalamander)
            return self.SalamanderColor;
        if (self is HunterSeekerGraphics)
            return self.DynamicBodyColor(f);
        return orig(self, f);
    }

    internal static Color On_LizardGraphics_DynamicBodyColor(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
    {
        if (self is NoodleEaterGraphics)
            return self.palette.blackColor;
        if (self is HunterSeekerGraphics)
            return Color.Lerp(Color.white, self.whiteCamoColor, self.whiteCamoColorAmount);
        if (self is MoleSalamanderGraphics && !self.blackSalamander)
            return self.SalamanderColor;
        if (self is CommonEelGraphics)
            return self.palette.blackColor;
        return orig(self, f);
    }

    internal static LizardGraphics.IndividualVariations On_LizardGraphics_GenerateIvars(On.LizardGraphics.orig_GenerateIvars orig, LizardGraphics self)
    {
        var res = orig(self);
        if (self is HunterSeekerGraphics)
            res.tailColor = Random.value > .5f ? Random.value : 0f;
        else if (self is MoleSalamanderGraphics)
            res.fatness = Custom.ClampedRandomVariation(.45f, .06f, .5f) * 2f;
        return res;
    }

    internal static Color On_LizardGraphics_HeadColor(On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
    {
        if (self is NoodleEaterGraphics or CommonEelGraphics)
            return self.palette.blackColor;
        var color = orig(self, timeStacker);
        if (self.lizard.AI?.yellowAI is PolliwogCommunication c && c.PackLeader)
        {
            var flicker = Mathf.Lerp(c.LastFlicker, c.CurrentFlicker, timeStacker);
            if (!self.lizard.Consious)
                flicker = 0f;
            color = Color.Lerp(color, new(1f, .007843137254902f, .3529411764705882f), flicker);
        }
        return color;
    }

    internal static void IL_LizardGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var label2 = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_LizardGraphics_lizard,
            s_MatchCallOrCallvirt_Creature_get_Template,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Salamander,
            s_MatchCall_Any,
            s_MatchBrfalse_Any)
        && label2 is not null)
        {
            label2.Target = c.Next;
            c.Index -= 6;
            c.EmitDelegate((LizardGraphics self) => self is PolliwogGraphics or MoleSalamanderGraphics or CommonEelGraphics);
            c.Emit(OpCodes.Brtrue, label2)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardGraphics.Update! (part 1)");
        c.Index = 0;
        var hsg = il.Import(typeof(HunterSeekerGraphics));
        for (var i = 0; i < 2; i++)
        {
            var label = il.DefineLabel();
            if (c.TryGotoNext(MoveType.After,
                s_MatchLdarg_0,
                s_MatchLdfld_LizardGraphics_lizard,
                s_MatchCallOrCallvirt_Creature_get_Template,
                s_MatchLdfld_CreatureTemplate_type,
                s_MatchLdsfld_CreatureTemplate_Type_WhiteLizard,
                s_MatchCall_Any,
                s_MatchBrfalse_Any))
            {
                label.Target = c.Next;
                c.Index -= 6;
                c.Emit(OpCodes.Isinst, hsg)
                 .Emit(OpCodes.Brtrue, label)
                 .Emit(OpCodes.Ldarg_0);
                if (i == 0)
                    c.Index += 12;
            }
            else
                LBMergedModsPlugin.s_logger.LogError($"Couldn't ILHook LizardGraphics.Update! (part {i + 2})");
        }
    }

    internal static void On_LizardGraphics_WhiteFlicker(On.LizardGraphics.orig_WhiteFlicker orig, LizardGraphics self, int fl)
    {
        if (self is NoodleEaterGraphics or CommonEelGraphics)
            self.whiteFlicker = 0;
        else
            orig(self, fl);
    }

    internal static bool On_LizardJumpModule_get_canChainJump(Func<LizardJumpModule, bool> orig, LizardJumpModule self) => (self.lizard is HunterSeeker l && l.grasps[0] is null) || orig(self);

    internal static void On_LizardLimb_ctor(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
    {
        orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        if (owner is LizardGraphics l)
        {
            if (l is NoodleEaterGraphics)
            {
                self.grabSound = SoundID.Lizard_BlueWhite_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_BlueWhite_Foot_Grab;
            }
            else if (l is SilverLizardGraphics)
            {
                self.grabSound = SoundID.Lizard_Green_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_Green_Foot_Release;
            }
        }
    }

    internal static PathCost On_LizardPather_HeuristicForCell(On.LizardPather.orig_HeuristicForCell orig, LizardPather self, PathFinder.PathingCell cell, PathCost costToGoal)
    {
        if (self.AI is PolliwogAI)
            return costToGoal;
        if (self.AI is WaterSpitterAI or MoleSalamanderAI or CommonEelAI && self.InThisRealizedRoom(cell.worldCoordinate) && self.creature.Room.realizedRoom.aimap.getAItile(cell.worldCoordinate).AnyWater)
            return new(cell.worldCoordinate.Tile.FloatDist(self.creaturePos.Tile), costToGoal.legality);
        return orig(self, cell, costToGoal);
    }

    internal static void On_LizardSpit_AddToContainer(On.LizardSpit.orig_AddToContainer orig, LizardSpit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (self is LizardWaterSpit)
            orig(self, sLeaser, rCam, rCam.ReturnFContainer("Background"));
        else
            orig(self, sLeaser, rCam, newContainer);
    }

    internal static void On_LizardSpit_ApplyPalette(On.LizardSpit.orig_ApplyPalette orig, LizardSpit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self is LizardWaterSpit)
        {
            var sprites = sLeaser.sprites;
            sprites[self.DotSprite].color = sprites[self.JaggedSprite].color = palette.waterColor1;
            var lg = self.slime.GetLength(0);
            for (var i = 0; i < lg; i++)
                sprites[self.SlimeSprite(i)].color = palette.waterColor1;
        }
        else
            orig(self, sLeaser, rCam, palette);
    }

    internal static void IL_LizardSpit_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var instr = ins[i];
            if (instr.MatchLdsfld<SoundID>("Red_Lizard_Spit_Hit_NPC") || instr.MatchLdsfld<SoundID>("Red_Lizard_Spit_Hit_Player") || instr.MatchLdsfld<SoundID>("Red_Lizard_Spit_Hit_Wall"))
            {
                c.Goto(instr, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((SoundID ID, LizardSpit self) => self is LizardWaterSpit ? SoundID.Swollen_Water_Nut_Terrain_Impact : ID);
            }
        }
    }

    internal static void IL_LizardSpitTracker_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchCallOrCallvirt_LizardAI_LizardSpitTracker_get_lizardAI,
            s_MatchCallOrCallvirt_LizardAI_get_lizard,
            s_MatchLdflda_Creature_lastInputWithDiagonals,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_Player_InputPackage_thrw))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, LizardAI.LizardSpitTracker self) => flag && self.AI is not WaterSpitterAI);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.LizardSpitTracker.Update!");
    }

    internal static void On_LizardTongue_ctor(On.LizardTongue.orig_ctor orig, LizardTongue self, Lizard lizard)
    {
        orig(self, lizard);
        if (lizard is NoodleEater)
        {
            self.range = 400f;
            self.elasticRange = 0f;
            self.lashOutSpeed = 30f;
            self.reelInSpeed = 0f;
            self.chunkDrag = 0f;
            self.terrainDrag = 0f;
            self.dragElasticity = 0f;
            self.emptyElasticity = .07f;
            self.involuntaryReleaseChance = .0333333341f;
            self.voluntaryReleaseChance = 1f;
            self.baseDragOnly = true;
            self.totR = self.range * 1.1f;
        }
        else if (lizard is Polliwog)
        {
            self.range = 140f;
            self.lashOutSpeed = 16f;
            self.reelInSpeed = .000625f;
            self.terrainDrag = self.chunkDrag = .01f;
            self.dragElasticity = .1f;
            self.emptyElasticity = .8f;
            self.involuntaryReleaseChance = 1f / 400f;
            self.voluntaryReleaseChance = .0125f;
            self.elasticRange = .55f;
            self.baseDragOnly = true;
            self.totR = self.range * 1.1f;
        }
        else if (lizard is HunterSeeker)
        {
            self.range = 140f;
            self.lashOutSpeed = 16f;
            self.reelInSpeed = .002f;
            self.chunkDrag = .01f;
            self.terrainDrag = .01f;
            self.dragElasticity = .1f;
            self.emptyElasticity = .8f;
            self.involuntaryReleaseChance = .005f;
            self.voluntaryReleaseChance = .02f;
            self.elasticRange = .55f;
            self.totR = self.range * 1.1f;
        }
        else if (lizard is MoleSalamander)
        {
            self.range = 140f;
            self.lashOutSpeed = 16f;
            self.reelInSpeed = .000625f;
            self.chunkDrag = .01f;
            self.terrainDrag = .01f;
            self.dragElasticity = .1f;
            self.emptyElasticity = .8f;
            self.involuntaryReleaseChance = .0025f;
            self.voluntaryReleaseChance = .0125f;
            self.elasticRange = .55f;
            self.totR = self.range * 1.1f;
        }
    }

    internal static void On_LizardVoice_ctor(On.LizardVoice.orig_ctor orig, LizardVoice self, Lizard lizard)
    {
        orig(self, lizard);
        if (lizard is NoodleEater)
            self.myPitch *= 1.5f;
    }

    internal static SoundID On_LizardVoice_GetMyVoiceTrigger(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
    {
        var res = orig(self);
        List<SoundID> list;
        SoundID soundID;
        if (self.lizard is Lizard l)
        {
            if (l is  WaterSpitter)
            {
                var array = new[]
                {
                    NewSoundID.M4R_WaterSpitter_Voice_A,
                    NewSoundID.M4R_WaterSpitter_Voice_B,
                    NewSoundID.M4R_WaterSpitter_Voice_C
                };
                list = [];
                for (var i = 0; i < array.Length; i++)
                {
                    soundID = array[i];
                    if (soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            else if (l is CommonEel)
            {
                var array = new[]
               {
                    NewSoundID.M4R_CommonEel_Voice,
                    NewSoundID.M4R_CommonEel_Hiss,
                    NewSoundID.M4R_CommonEel_BigHiss
                };
                list = [];
                for (var i = 0; i < array.Length; i++)
                {
                    soundID = array[i];
                    if (soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            else if (l is HunterSeeker or SilverLizard)
            {
                var array = new[]
                {
                    SoundID.Lizard_Voice_Pink_A,
                    SoundID.Lizard_Voice_Pink_B,
                    SoundID.Lizard_Voice_Pink_C,
                    SoundID.Lizard_Voice_Pink_D,
                    SoundID.Lizard_Voice_Pink_E
                };
                list = [];
                for (var i = 0; i < array.Length; i++)
                {
                    soundID = array[i];
                    if (soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            else if (l is NoodleEater)
            {
                soundID = SoundID.Lizard_Voice_Blue_A;
                if (soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                    res = soundID;
                else
                    res = SoundID.None;
            }
            else if (l is Polliwog)
            {
                var array = new[] { "A", "B" };
                list = [];
                for (var i = 0; i < array.Length; i++)
                {
                    soundID = new("Lizard_Voice_Salamander_" + array[i]);
                    if (soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            else if (l is MoleSalamander)
            {
                soundID = MMFEnums.MMFSoundID.Lizard_Voice_Black_A;
                if (soundID is not null && soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                    res = soundID;
                else
                    res = SoundID.None;
            }
            else if (l is AlphaOrange)
            {
                soundID = MMFEnums.MMFSoundID.Lizard_Voice_Yellow_A;
                if (soundID is not null && soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                    res = soundID;
                else
                    res = SoundID.None;
            }
        }
        return res;
    }

    internal static void On_LongBodyScales_DrawSprites(On.LizardCosmetics.LongBodyScales.orig_DrawSprites orig, LongBodyScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is HunterSeekerGraphics lg)
        {
            var lgt = self.scalesPositions.Length;
            Color c = lg.BodyColor(1f), c2 = lg.HeadColor(timeStacker);
            var sprs = sLeaser.sprites;
            for (var num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
            {
                sprs[num].color = c;
                if (self.colored)
                    sprs[num + lgt].color = c2;
            }
        }
    }

    internal static void On_LongHeadScales_ctor(On.LizardCosmetics.LongHeadScales.orig_ctor orig, LongHeadScales self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics is MoleSalamanderGraphics)
        {
            self.colored = !lGraphics.blackSalamander;
            self.numberOfSprites = !self.colored ? self.scalesPositions.Length : (self.scalesPositions.Length * 2);
        }
    }

    internal static void On_LongHeadScales_DrawSprites(On.LizardCosmetics.LongHeadScales.orig_DrawSprites orig, LongHeadScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.colored && self.lGraphics is HunterSeekerGraphics lg)
        {
            var lgt = self.scalesPositions.Length;
            var c = lg.HeadColor(timeStacker);
            var sprs = sLeaser.sprites;
            for (var num = self.startSprite + lgt - 1; num >= self.startSprite; num--)
                sprs[num + lgt].color = c;
        }
    }

    internal static void IL_LurkTracker_LurkPosScore(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_LizardAI_LurkTracker_lizard,
            s_MatchCallOrCallvirt_Creature_get_Template,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_WhiteLizard,
            s_MatchCall_Any,
            s_MatchBrfalse_Any))
        {
            label.Target = c.Next;
            c.Index -= 6;
            c.Emit<LizardAI.LurkTracker>(OpCodes.Ldfld, "lizard")
             .Emit(OpCodes.Isinst, il.Import(typeof(HunterSeeker)))
             .Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.LurkTracker.LurkPosScore! (part 1)");
    }

    internal static float On_LurkTracker_Utility(On.LizardAI.LurkTracker.orig_Utility orig, LizardAI.LurkTracker self)
    {
        if (self.lizard is Lizard l)
        {
            if (l is Polliwog or MoleSalamander or CommonEel)
            {
                if (self.LurkPosScore(self.lurkPosition) > 0f)
                    return l.room?.GetTile(self.lurkPosition).AnyWater is true ? .5f : .2f;
            }
            else if (l is HunterSeeker)
                return .5f;
        }
        return orig(self);
    }

    internal static void On_SpineSpikes_DrawSprites(On.LizardCosmetics.SpineSpikes.orig_DrawSprites orig, SpineSpikes self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            Color bc = g.BodyColor(1f), hc = g.HeadColor(timeStacker);
            var sprs = sLeaser.sprites;
            var start = self.startSprite;
            var bumps = self.bumps;
            for (var i = start; i < start + bumps; i++)
            {
                sprs[i].color = bc;
                if (self.colored == 1)
                    sprs[i + bumps].color = hc;
                else if (self.colored == 2)
                {
                    var f2 = Mathf.InverseLerp(start, start + bumps - 1, i);
                    sprs[i + bumps].color = Color.Lerp(hc, bc, Mathf.Pow(f2, .5f));
                }
            }
        }
    }

    internal static void On_SpineSpikes_ApplyPalette(On.LizardCosmetics.SpineSpikes.orig_ApplyPalette orig, SpineSpikes self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            Color bc = g.BodyColor(1f), hc = g.HeadColor(1f);
            var sprs = sLeaser.sprites;
            var start = self.startSprite;
            var bumps = self.bumps;
            for (var i = start; i < start + bumps; i++)
            {
                sprs[i].color = bc;
                if (self.colored == 1)
                    sprs[i + bumps].color = hc;
                else if (self.colored == 2)
                {
                    var f2 = Mathf.InverseLerp(start, start + bumps - 1, i);
                    sprs[i + bumps].color = Color.Lerp(hc, bc, Mathf.Pow(f2, .5f));
                }
            }
        }
    }

    internal static void On_TailFin_DrawSprites(On.LizardCosmetics.TailFin.orig_DrawSprites orig, TailFin self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.colored && self.lGraphics is HunterSeekerGraphics g)
        {
            var sprs = sLeaser.sprites;
            var bumps = self.bumps;
            var start = self.startSprite;
            for (var i = 0; i < 2; i++)
            {
                var num = i * bumps * 2;
                var col = g.HeadColor(timeStacker);
                for (var j = start; j < start + bumps; j++)
                    sprs[j + bumps + num].color = col;
            }
        }
    }

    internal static void On_TailFin_ApplyPalette(On.LizardCosmetics.TailFin.orig_ApplyPalette orig, TailFin self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.colored && self.lGraphics is HunterSeekerGraphics g)
        {
            var sprs = sLeaser.sprites;
            var bumps = self.bumps;
            var start = self.startSprite;
            for (var i = 0; i < 2; i++)
            {
                var num = i * bumps * 2;
                var col = g.HeadColor(1f);
                for (var j = start; j < start + bumps; j++)
                    sprs[j + bumps + num].color = col;
            }
        }
    }

    internal static void IL_TailGeckoScales_DrawSprites(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCall<Color>("Lerp"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldarg_3)
                 .EmitDelegate((Color color, TailGeckoScales self, float timeStacker) => self.lGraphics is HunterSeekerGraphics g ? Color.Lerp(color, g.HeadColor(timeStacker), g.whiteCamoColorAmount) : color);
            }
        }
    }

    internal static void On_Whiskers_ctor(On.LizardCosmetics.Whiskers.orig_ctor orig, Whiskers self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics is WaterSpitterGraphics)
            self.spritesOverlap = Template.SpritesOverlap.BehindHead;
    }

    internal static void On_Whiskers_ApplyPalette(On.LizardCosmetics.Whiskers.orig_ApplyPalette orig, Whiskers self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.lGraphics is MoleSalamanderGraphics or WaterSpitterGraphics)
        {
            var col = self.lGraphics.HeadColor(1f);
            for (var i = 0; i < self.amount; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    var verts = (sLeaser.sprites[self.startSprite + i * 2 + j] as TriangleMesh)!.verticeColors;
                    for (var k = 0; k < 4; k++)
                    {
                        for (var l = k * 4; l < k * 4 + (k == 3 ? 3 : 4); l++)
                            verts[l] = Color.Lerp(col, Color.white, (k - 1) / 2f * self.whiskerLightUp[i, j, 0]);
                    }
                }
            }
        }
    }

    internal static Vector2 On_Whiskers_AnchorPoint(On.LizardCosmetics.Whiskers.orig_AnchorPoint orig, Whiskers self, int side, int m, float timeStacker)
    {
        var vec = orig(self, side, m, timeStacker);
        if (self.lGraphics is LizardGraphics g && g.lizard is Lizard l)
        {
            if (g is WaterSpitterGraphics grw)
                vec = Vector2.Lerp(g.head.lastPos, g.head.pos, timeStacker) + Custom.DegToVec(grw.HeadRotation(l, timeStacker)) * 7f * g.iVars.headSize + self.whiskerDir(side, m, timeStacker);
            else if (g is MoleSalamanderGraphics grm)
                vec = Vector2.Lerp(g.head.lastPos, g.head.pos, timeStacker) + Custom.DegToVec(grm.HeadRotation(l, timeStacker)) * 10.9f * g.iVars.headSize + self.whiskerDir(side, m, timeStacker);
        }
        return vec;
    }

    internal static void On_WingScales_DrawSprites(On.LizardCosmetics.WingScales.orig_DrawSprites orig, WingScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            var c = g.BodyColor(1f);
            var sprs = sLeaser.sprites;
            var num = self.numberOfSprites;
            for (var i = 0; i < num; i++)
                sprs[self.startSprite + i].color = c;
        }
        else if (self.lGraphics is AlphaOrangeGraphics g2)
        {
            var headColor = g2.HeadColor(timeStacker);
            var scls = self.scales;
            var sprs = sLeaser.sprites;
            var l1 = scls.GetLength(1);
            var l0 = scls.GetLength(0);
            for (var i = 0; i < l1; i++)
            {
                for (var j = 0; j < l0; j++)
                    sprs[self.ScaleSprite(j, i)].color = headColor;
            }
        }
    }

    internal static void On_WingScales_ApplyPalette(On.LizardCosmetics.WingScales.orig_ApplyPalette orig, WingScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.lGraphics is HunterSeekerGraphics g)
        {
            var c = g.BodyColor(1f);
            var sprs = sLeaser.sprites;
            var num = self.numberOfSprites;
            for (var i = 0; i < num; i++)
                sprs[self.startSprite + i].color = c;
        }
        else if (self.lGraphics is AlphaOrangeGraphics g2)
        {
            var headColor = g2.HeadColor(1f);
            var scls = self.scales;
            var sprs = sLeaser.sprites;
            var l1 = scls.GetLength(1);
            var l0 = scls.GetLength(0);
            for (var i = 0; i < l1; i++)
            {
                for (var j = 0; j < l0; j++)
                    sprs[self.ScaleSprite(j, i)].color = headColor;
            }
        }
    }

    internal static YellowAI.YellowPack On_YellowAI_Pack(On.YellowAI.orig_Pack orig, YellowAI self, Creature liz)
    {
        if (liz?.abstractCreature?.abstractAI?.RealAI is not LizardAI ai || ai.yellowAI?.pack is null)
            return self.pack;
        return orig(self, liz);
    }

    internal static void IL_YellowAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_YellowAI_lizard,
            s_MatchLdflda_Creature_inputWithDiagonals,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_Player_InputPackage_jmp))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, YellowAI self) => self is PolliwogCommunication && self.lizard is Polliwog l ? l.inputWithDiagonals!.Value.thrw && self.pack?.members.Count > 1 : flag);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook YellowAI.Update! (part 1)");
        if (c.TryGotoNext(MoveType.After,
           s_MatchCall_Mathf_Max,
           s_MatchStfld_YellowAI_commFlicker))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((YellowAI self) =>
             {
                 if (self is PolliwogCommunication co && self.lizard is Polliwog l)
                 {
                     co.LastFlicker = co.CurrentFlicker;
                     co.CurrentFlicker = Mathf.Clamp(co.Increase ? co.CurrentFlicker + .25f : co.CurrentFlicker - .2f, -.5f, 1f);
                     if (co.CurrentFlicker >= 1f || !l.Consious || self.communicating <= 0)
                         co.Increase = false;
                     else if (self.communicating > 0 && co.CurrentFlicker <= -.5f && self.pack?.members.Count > 1)
                         co.Increase = true;
                 }
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook YellowAI.Update! (part 2)");
        c.Index = 0;
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdarg_0,
            s_MatchLdfld_YellowAI_lizard,
            s_MatchCallOrCallvirt_Any,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_CreatureTemplate_Relationship_type,
            s_MatchLdsfld_CreatureTemplate_Relationship_Type_Pack)
         && c.TryGotoNext(
            s_MatchBrfalse_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, s_loc1)
             .EmitDelegate((bool flag, YellowAI self, int i) => flag || (self is not PolliwogCommunication && self.lizard.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplateType.AlphaOrange));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook YellowAI.Update! (part 3)");
    }

    internal static void On_YellowPack_FindLeader(On.YellowAI.YellowPack.orig_FindLeader orig, YellowAI.YellowPack self)
    {
        orig(self);
        var mems = self.members;
        for (var i = 0; i < mems.Count; i++)
        {
            var mem = mems[i];
            if (mem?.lizard?.realizedCreature is Polliwog l && l.AI?.yellowAI is PolliwogCommunication c)
                c.PackLeader = mem.role == YellowAI.YellowPack.Role.Leader;
        }
    }

    internal static void On_YellowPack_RemoveLizard_AbstractCreature(On.YellowAI.YellowPack.orig_RemoveLizard_AbstractCreature orig, YellowAI.YellowPack self, AbstractCreature removeLizard)
    {
        var mems = self.members;
        for (var num = mems.Count - 1; num >= 0; num--)
        {
            if (mems[num]?.lizard == removeLizard && removeLizard?.realizedCreature is Polliwog l && l.AI?.yellowAI is PolliwogCommunication c && c.PackLeader)
                c.PackLeader = false;
        }
        orig(self, removeLizard);
    }

    internal static void On_YellowPack_RemoveLizard_int(On.YellowAI.YellowPack.orig_RemoveLizard_int orig, YellowAI.YellowPack self, int index)
    {
        if (self.members[index]?.lizard?.realizedCreature is Polliwog l && l.AI?.yellowAI is PolliwogCommunication c && c.PackLeader)
            c.PackLeader = false;
        orig(self, index);
    }

    public static bool Camouflaged(this Lizard self) => (!ModManager.DLCShared || self.Template.type != DLCSharedEnums.CreatureTemplateType.ZoopLizard) && self.graphicsModule is LizardGraphics gr && gr.Camouflaged >= .8f;
}