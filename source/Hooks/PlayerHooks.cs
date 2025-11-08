global using static LBMergedMods.Hooks.PlayerHooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class PlayerHooks
{
    internal static void On_GourmandCombos_InitCraftingLibrary(On.MoreSlugcats.GourmandCombos.orig_InitCraftingLibrary orig)
    {
        orig();
        InitGourmandCombos();
    }

    internal static void On_Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (!PlayerData.TryGetValue(abstractCreature, out _))
            PlayerData.Add(abstractCreature, new() { OriginalBounce = self.bounce });
    }

    internal static bool On_Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        if (testObj is BouncingMelon or Physalis or SmallPuffBall or FumeFruit && (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
            return true;
        if (testObj is Durian)
            return false;
        return orig(self, testObj);
    }

    internal static void On_Player_EatMeatUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MSC)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MSC))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, Player self, int graspIndex) => flag && self.grasps[graspIndex].grabbed is not SurfaceSwimmer and not HazerMom);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.EatMeatUpdate!");
    }

    internal static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is ThornyStrawberry or BlobPiece or BouncingMelon or LittleBalloon or TintedBeetle or Physalis or LimeMushroom or GummyAnther or MarineEye or SmallPuffBall or DendriticNeuron or MiniFruit or XyloWorm or FumeFruit or Durian or DarkGrub)
            return Player.ObjectGrabability.OneHand;
        if (obj is RubberBlossom or MiniFruitSpawner)
            return Player.ObjectGrabability.CantGrab;
        if (obj is StarLemon)
            return Player.ObjectGrabability.TwoHands;
        return orig(self, obj);
    }

    internal static void On_Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
    {
        orig(self, grasp);
        if (grasp.grabber is ThornBug or DivingBeetle or Caterpillar)
        {
            self.dangerGraspTime = 0;
            self.dangerGrasp = grasp;
        }
    }

    internal static void IL_Player_GrabUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchIsinst_IPlayerEdible))
            c.EmitDelegate((IPlayerEdible obj) => obj is BouncingMelon ? null : obj);
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdcI4_M1,
            s_MatchBeq_Any)
         && c.TryGotoNext(
            s_MatchCallOrCallvirt_Creature_get_grasps,
            s_MatchLdloc_InLoc1,
            s_MatchLdelemRef,
            s_MatchLdfld_Creature_Grasp_grabbed,
            s_MatchIsinst_KarmaFlower,
            s_MatchBrtrue_OutLabel))
        {
            var loc1 = il.Body.Variables[s_loc1];
            c.Emit(OpCodes.Ldloc, loc1)
             .EmitDelegate((Player self, int num6) => self.grasps[num6]?.grabbed is MarineEye or LimeMushroom);
            c.Emit(OpCodes.Brtrue, s_label)
             .Emit(OpCodes.Ldarg_0);
            if (c.TryGotoNext(MoveType.After,
                s_MatchCallOrCallvirt_Player_get_CanPutSpearToBack))
            {
                c.Emit(OpCodes.Ldloc, loc1)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, int num6, Player self) => flag || (num6 >= 0 && self.grasps[num6]?.grabbed is BouncingMelon));
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 3)");
            if (c.TryGotoNext(
                s_MatchCallOrCallvirt_Creature_get_grasps,
                s_MatchLdloc_InLoc1,
                s_MatchLdelemRef,
                s_MatchLdfld_Creature_Grasp_grabbed,
                s_MatchIsinst_KarmaFlower,
                s_MatchBrtrue_OutLabel))
            {
                c.Emit(OpCodes.Ldloc, loc1)
                 .EmitDelegate((Player self, int num6) => self.grasps[num6]?.grabbed is MarineEye or LimeMushroom);
                c.Emit(OpCodes.Brtrue, s_label)
                 .Emit(OpCodes.Ldarg_0);
                if (c.TryGotoNext(MoveType.After,
                    s_MatchLdfld_Player_objectInStomach))
                {
                    c.Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((AbstractPhysicalObject objectInStomach, Player self) =>
                     {
                         var grs = self.grasps;
                         for (var i = 0; i < grs.Length; i++)
                         {
                             if (grs[i]?.grabbed is BouncingMelon mel)
                                 return null;
                         }
                         return objectInStomach;
                     });
                    if (c.TryGotoNext(MoveType.After,
                        s_MatchLdfld_Player_objectInStomach)
                     && c.TryGotoNext(MoveType.After,
                        s_MatchLdfld_Player_objectInStomach))
                    {
                        c.Emit(OpCodes.Ldarg_0)
                         .EmitDelegate((AbstractPhysicalObject objectInStomach, Player self) =>
                         {
                             var grs = self.grasps;
                             for (var i = 0; i < grs.Length; i++)
                             {
                                 if (grs[i]?.grabbed is BouncingMelon mel)
                                     return null;
                             }
                             return objectInStomach;
                         });
                    }
                    else
                        LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 6)");
                }
                else
                    LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 5)");
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 4)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 2)");
    }

    internal static bool On_Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        var grs = self.grasps;
        for (var i = 0; i < grs.Length; i++)
        {
            if (grs[i]?.grabbed is ThornyStrawberry st && !st.SpikesRemoved())
                return false;
        }
        return orig(self);
    }

    internal static bool On_Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    {
        if (grabCheck is TintedBeetle or SurfaceSwimmer or BouncingBall)
            return true;
        return orig(self, grabCheck);
    }

    internal static void On_Player_MaulingUpdate(On.Player.orig_MaulingUpdate orig, Player self, int graspIndex)
    {
        if (self.grasps[graspIndex] is Creature.Grasp gr && gr.grabbed is ScavengerSentinel c && self.maulTimer > 15)
        {
            var bs = c.bodyChunks;
            bs[0].mass = .5f;
            bs[1].mass = .3f;
            bs[2].mass = .05f;
        }
        orig(self, graspIndex);
    }

    internal static void On_Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        if (grasp >= 0 && self.grasps[grasp]?.grabbed is BouncingMelon mel)
        {
            if (ModManager.MMF && self.room.game.session is StoryGameSession sess)
                sess.RemovePersistentTracker(mel.abstractPhysicalObject);
            self.mainBodyChunk.vel.y += 2f;
            self.room.PlaySound(SoundID.Slugcat_Swallow_Item, self.mainBodyChunk);
            self.room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, self.mainBodyChunk);
            self.ObjectEaten(mel);
            self.ReleaseGrasp(grasp);
            mel.Destroy();
            if (PlayerData.TryGetValue(self.abstractCreature, out var props))
                props.BounceEffectDuration = 5000;
        }
        else
            orig(self, grasp);
    }

    internal static void IL_Player_TerrainImpact(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_60))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float val, Player self) => PlayerData.TryGetValue(self.abstractCreature, out var props) ? val + props.BounceEffectDuration / 10f : val);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.TerrainImpact!");
    }

    internal static void On_Player_Stun(On.Player.orig_Stun orig, Player self, int st)
    {
        if (st >= 40 && self.objectInStomach is AbstractPhysicalObject obj && obj.type == AbstractObjectType.FumeFruit)
        {
            FumeFruit.Explode(self);
            obj.Destroy();
            self.objectInStomach = null;
        }
        orig(self, st);
    }

    internal static void On_Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp]?.grabbed is ThornyStrawberry st && st.SpikesRemoved())
        {
            self.AerobicIncrease(.75f);
            self.TossObject(grasp, eu);
            self.dontGrabStuff = self.isNPC ? 45 : 15;
            (self.graphicsModule as PlayerGraphics)?.LookAtObject(st);
            st.Forbid();
            self.ReleaseGrasp(grasp);
        }
        else
            orig(self, grasp, eu);
    }

    internal static void On_Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp]?.grabbed is ThornyStrawberry st)
        {
            st.SetRandomSpin();
            st.rotationSpeed = 0f;
        }
        orig(self, grasp, eu);
    }

    internal static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        var flag = PlayerData.TryGetValue(self.abstractCreature, out var props);
        var chunks = self.bodyChunks;
        if (flag)
        {
            var effect = props.BounceEffectDuration / 5000f;
            if (self.bodyMode != Player.BodyModeIndex.Crawl
                && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                && self.animation != Player.AnimationIndex.CrawlTurn
                && self.animation != Player.AnimationIndex.DownOnFours
                && self.animation != Player.AnimationIndex.LedgeCrawl
                && self.animation != Player.AnimationIndex.LedgeGrab
                && self.animation != Player.AnimationIndex.CorridorTurn
                && self.animation != Player.AnimationIndex.BellySlide)
            {
                if (effect > 0f)
                {
                    for (var i = 0; i < chunks.Length; i++)
                    {
                        var chunk = chunks[i];
                        var contact = chunk.ContactPoint;
                        if (contact.y != 0 && Math.Abs(chunk.vel.y) < 1f)
                            chunk.vel.y = 0f;
                    }
                }
                self.bounce = props.OriginalBounce * (1f + 12f * effect);
            }
            else
                self.bounce = props.OriginalBounce;
            if (!self.inShortcut)
            {
                if (props.BounceEffectDuration > 0)
                    --props.BounceEffectDuration;
                if (props.BlueFaceDuration > 0)
                    --props.BlueFaceDuration;
                if (props.GrubVisionDuration > 0)
                    --props.GrubVisionDuration;
            }
        }
        orig(self, eu);
        if (flag && props.BounceEffectDuration > 0)
        {
            var velFac = .9f + .1f * (1f - props.BounceEffectDuration / 5000f);
            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var contact = chunk.ContactPoint;
                if (contact != default)
                    chunk.vel *= velFac;
            }
        }
    }

    internal static void On_PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!rCam.room.game.DEBUGMODE && self.player is Player p && PlayerData.TryGetValue(p.abstractCreature, out var props))
        {
            var num = .5f + .5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * Mathf.PI * 2f);
            Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker),
                vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
            if (p.aerobicLevel > .5f)
                vector += Custom.DirVec(vector2, vector) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(.5f, 1f, p.aerobicLevel) * .5f;
            var num2 = Mathf.InverseLerp(.3f, .5f, Math.Abs(Custom.DirVec(vector2, vector).y));
            var ef = props.BounceEffectDuration / 6500f;
            //inverted for pups somehow, bug in msc code?
            FSprite spr;
            if (ModManager.MSC && p.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
            {
                spr = sLeaser.sprites[1];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-.05f, -.15f, self.malnourished), .05f, num) * num2, .15f, p.sleepCurlUp) * .5f * ef;
                spr = sLeaser.sprites[0];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - (p.sleepCurlUp * .1f + .025f * num - .025f * self.malnourished) * ef;
            }
            else
            {
                spr = sLeaser.sprites[0];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-.05f, -.15f, self.malnourished), .05f, num) * num2, .15f, p.sleepCurlUp) * .5f * ef;
                spr = sLeaser.sprites[1];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - (p.sleepCurlUp * .1f + .025f * num - .025f * self.malnourished) * ef;
            }
            spr = sLeaser.sprites[9];
            spr.color = Color.Lerp(spr.color, MarineCol, props.BlueFaceDuration / 5000f);
        }
    }

    internal static void IL_PlayerGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_swallowAndRegurgitateCounter)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_swallowAndRegurgitateCounter)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_swallowAndRegurgitateCounter))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int swallowAndRegurgitateCounter, PlayerGraphics self) =>
             {
                 var grs = self.player.grasps;
                 for (var i = 0; i < grs.Length; i++)
                 {
                     if (grs[i]?.grabbed is BouncingMelon mel)
                         return 0;
                 }
                 return swallowAndRegurgitateCounter;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook PlayerGraphics.Update!");
    }

    internal static void On_PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (!self.player.isNPC && PlayerData.TryGetValue(self.player.abstractCreature, out var dt))
        {
            var pos = self.player.mainBodyChunk.pos;
            if (dt.GrubVision is DarkGrubVision vision)
            {
                vision.SetPos = pos;
                vision.SetRad = Math.Min(dt.GrubVisionDuration * 6f, 14400f);
                if (vision.slatedForDeletetion || self.player.room.Darkness(pos) == 0f || vision.room != self.player.room)
                {
                    if (!vision.slatedForDeletetion)
                        vision.Destroy();
                    dt.GrubVision = null;
                }
            }
            else if (self.player.room.Darkness(pos) > 0f && dt.GrubVisionDuration > 0)
                self.player.room.AddObject(dt.GrubVision = new(pos, Math.Min(dt.GrubVisionDuration * 6f, 14400f)));
        }
    }

    internal static void On_PlayerSessionRecord_AddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenObject)
    {
        orig(self, eatenObject);
        if (eatenObject is BlobPiece or DarkGrub)
        {
            self.vegetarian = false;
            self.carnivorous = true;
        }
    }

    internal static void IL_SlugcatHand_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_objectInStomach))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractPhysicalObject objectInStomach, SlugcatHand self) =>
             {
                 var grs = (self.owner.owner as Player)!.grasps;
                 for (var i = 0; i < grs.Length; i++)
                 {
                     if (grs[i]?.grabbed is BouncingMelon mel)
                         return null;
                 }
                 return objectInStomach;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SlugcatHand.Update!");
    }

    internal static int On_SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
    {
        var res = orig(slugcatIndex, eatenobject);
        if (eatenobject is Fly b && b.IsSeed() && slugcatIndex != SlugcatStats.Name.Red && (!ModManager.MSC || (slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Spear && slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Saint && slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
            res += 2;
        else if (eatenobject is BouncingMelon or GummyAnther or DendriticNeuron or MiniFruit or DarkGrub && slugcatIndex != SlugcatStats.Name.Red && (!ModManager.MSC || (slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Spear && slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
            res += 2;
        return res;
    }

    internal static void InitGourmandCombos()
    {
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.SlimeMold);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractObjectType.ThornyStrawberry);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.FlyLure, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.Fly);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, DLCSharedEnums.AbstractObjectType.LillyPuck);
        SetCombo(AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.ThornyStrawberry, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.JellyFish);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.JellyFish);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.DataPearl, AbstractObjectType.BlobPiece);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractObjectType.BlobPiece);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.Lantern, DLCSharedEnums.AbstractObjectType.GlowWeed);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(AbstractObjectType.BlobPiece, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.Mushroom, AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb,  CreatureTemplate.Type.Snail);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.SporePlant,  CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, DLCSharedEnums.AbstractObjectType.LillyPuck);
        SetCombo(AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.BlobPiece, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.BlobPiece, AbstractObjectType.BlobPiece);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.DataPearl, AbstractObjectType.LimeMushroom);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractObjectType.LimeMushroom);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.SlimeMold,  CreatureTemplate.Type.SmallNeedleWorm);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.FlyLure,  CreatureTemplate.Type.Fly);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.SporePlant, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.BubbleGrass,  CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.SingularityBomb,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.LimeMushroom, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.Seed,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.LillyPuck, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GlowWeed, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.DandelionPeach, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.SlimeMold);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.MarineEye, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.MarineEye, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.FlyLure, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.Fly);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, DLCSharedEnums.AbstractObjectType.LillyPuck);
        SetCombo(AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.MarineEye, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.MarineEye, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MarineEye, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.SporeProjectile);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.WaterNut, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.BlobPiece, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.JellyFish, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.SporePlant, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.SporeProjectile, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.SporeProjectile, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.SporeProjectile, DLCSharedEnums.AbstractObjectType.Seed,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.SporeProjectile, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, DLCSharedEnums.AbstractObjectType.GlowWeed, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.SporeProjectile, DLCSharedEnums.AbstractObjectType.DandelionPeach, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.SlimeMold);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.Physalis, AbstractObjectType.Physalis);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.Physalis, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.FlyLure, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.Fly);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, DLCSharedEnums.AbstractObjectType.LillyPuck);
        SetCombo(AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.Physalis, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.Physalis, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.Physalis, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.SlimeMold);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.GummyAnther, AbstractObjectType.GummyAnther);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.GummyAnther, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.FlyLure, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.Fly);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, DLCSharedEnums.AbstractObjectType.LillyPuck);
        SetCombo(AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.GummyAnther, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.GummyAnther, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.GummyAnther, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.LittleBalloon);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.WaterNut, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SporePlant, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.LittleBalloon, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.LittleBalloon, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.LittleBalloon, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.LittleBalloon, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.LittleBalloon, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.LittleBalloon, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.LittleBalloon, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.LittleBalloon, AbstractObjectType.MarineEye);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.SporeProjectile, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.MiniBlueFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.WaterNut, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SporePlant, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.NeedleEgg,  CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.MiniBlueFruit, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.MiniBlueFruit, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.MiniBlueFruit, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.MiniBlueFruit, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.MiniBlueFruit, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.MiniBlueFruit, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractObjectType.MarineEye);
        SetCombo(AbstractObjectType.MiniBlueFruit, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.SmallCentipede, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplate.Type.VultureGrub, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplate.Type.SmallNeedleWorm, AbstractObjectType.MiniBlueFruit, AbstractObjectType.MarineEye);
        SetCombo(CreatureTemplate.Type.Hazer, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.Rock, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.VultureMask, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.SporePlant,  CreatureTemplate.Type.TubeWorm);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, AbstractPhysicalObject.AbstractObjectType.JellyFish);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(CreatureTemplateType.MiniScutigera, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.VultureGrub, CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, CreatureTemplate.Type.SmallCentipede);
        SetCombo(CreatureTemplateType.MiniScutigera, CreatureTemplateType.MiniScutigera);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.MiniScutigera, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.Rock, AbstractPhysicalObject.AbstractObjectType.Lantern);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractPhysicalObject.AbstractObjectType.Lantern);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.VultureMask, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.MiniBlueFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.JellyFish, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.VultureMask);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.Mushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.LimeMushroom, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, DLCSharedEnums.AbstractObjectType.Seed);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, DLCSharedEnums.AbstractObjectType.GlowWeed);
        SetCombo(CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(CreatureTemplateType.XyloWorm, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(CreatureTemplateType.XyloWorm, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, DLCSharedEnums.AbstractObjectType.Seed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplate.Type.Fly, CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, CreatureTemplate.Type.VultureGrub);
        SetCombo(CreatureTemplateType.XyloWorm, CreatureTemplateType.XyloWorm);
        SetCombo(CreatureTemplateType.XyloWorm, CreatureTemplate.Type.SmallCentipede, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, CreatureTemplate.Type.SmallNeedleWorm, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(CreatureTemplateType.XyloWorm, CreatureTemplate.Type.Hazer, AbstractPhysicalObject.AbstractObjectType.DangleFruit);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.FumeFruit);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.LittleBalloon, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.MiniBlueFruit, AbstractObjectType.SporeProjectile);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.SporeProjectile, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.Physalis, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.FumeFruit, AbstractObjectType.ThornyStrawberry, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplateType.MiniScutigera, DLCSharedEnums.AbstractObjectType.SingularityBomb);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplateType.XyloWorm, CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplate.Type.Hazer, CreatureTemplate.Type.Snail);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplate.Type.SmallNeedleWorm, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplate.Type.VultureGrub, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplate.Type.SmallCentipede, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.FumeFruit, CreatureTemplate.Type.Fly, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.FumeFruit, DLCSharedEnums.AbstractObjectType.DandelionPeach, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, DLCSharedEnums.AbstractObjectType.LillyPuck, DLCSharedEnums.AbstractObjectType.GlowWeed);
        SetCombo(AbstractObjectType.FumeFruit, DLCSharedEnums.AbstractObjectType.GlowWeed, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, DLCSharedEnums.AbstractObjectType.Seed, CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.JellyFish, DLCSharedEnums.AbstractObjectType.GlowWeed);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.Lantern);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractObjectType.LittleBalloon);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.FumeFruit, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, CreatureTemplate.Type.Hazer);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractObjectType.LittleBalloon);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.Mushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.Lantern, DLCSharedEnums.AbstractObjectType.GlowWeed);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.Rock, AbstractPhysicalObject.AbstractObjectType.WaterNut);
        SetCombo(AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractPhysicalObject.AbstractObjectType.Lantern);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.Rock, AbstractObjectType.ThornyStrawberry);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.FlareBomb, AbstractPhysicalObject.AbstractObjectType.Lantern);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.VultureMask, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.PuffBall, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.Lantern, AbstractPhysicalObject.AbstractObjectType.FlareBomb);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.Mushroom, AbstractObjectType.LimeMushroom);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.FlyLure, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.SporePlant, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, AbstractPhysicalObject.AbstractObjectType.FlyLure);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, CreatureTemplate.Type.Fly);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
        SetCombo(AbstractObjectType.Durian, DLCSharedEnums.AbstractObjectType.SingularityBomb, MoreSlugcatsEnums.AbstractObjectType.FireEgg);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, AbstractPhysicalObject.AbstractObjectType.DataPearl);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
        SetCombo(AbstractObjectType.Durian, MoreSlugcatsEnums.AbstractObjectType.FireEgg, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.DangleFruit, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.WaterNut, AbstractObjectType.ThornyStrawberry);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.SlimeMold, AbstractPhysicalObject.AbstractObjectType.Lantern);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.JellyFish, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Durian, AbstractPhysicalObject.AbstractObjectType.EggBugEgg, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Durian, DLCSharedEnums.AbstractObjectType.Seed, CreatureTemplate.Type.SmallCentipede);
        SetCombo(AbstractObjectType.Durian, DLCSharedEnums.AbstractObjectType.GooieDuck, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, DLCSharedEnums.AbstractObjectType.LillyPuck, AbstractObjectType.LimeMushroom);
        SetCombo(AbstractObjectType.Durian, DLCSharedEnums.AbstractObjectType.GlowWeed, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Durian, DLCSharedEnums.AbstractObjectType.DandelionPeach, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Durian, CreatureTemplate.Type.Fly, AbstractPhysicalObject.AbstractObjectType.FlyLure);
        SetCombo(AbstractObjectType.Durian, CreatureTemplate.Type.SmallCentipede, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.Durian, CreatureTemplateType.MiniScutigera, AbstractPhysicalObject.AbstractObjectType.SporePlant);
        SetCombo(AbstractObjectType.Durian, CreatureTemplate.Type.VultureGrub, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.Durian, CreatureTemplateType.XyloWorm, AbstractPhysicalObject.AbstractObjectType.Mushroom);
        SetCombo(AbstractObjectType.Durian, CreatureTemplate.Type.SmallNeedleWorm, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, CreatureTemplate.Type.Hazer, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.BlobPiece, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.GummyAnther, AbstractPhysicalObject.AbstractObjectType.FlyLure);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.LimeMushroom, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.LittleBalloon, AbstractObjectType.FumeFruit);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.MarineEye, AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.MiniBlueFruit, AbstractObjectType.SporeProjectile);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.Physalis, AbstractObjectType.ThornyStrawberry);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.SporeProjectile, AbstractObjectType.LimeMushroom);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.ThornyStrawberry, DLCSharedEnums.AbstractObjectType.GooieDuck);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.FumeFruit, AbstractPhysicalObject.AbstractObjectType.PuffBall);
        SetCombo(AbstractObjectType.Durian, AbstractObjectType.Durian);
    }

    internal static void ResizeGourmandCombos()
    {
        _ = GourmandCombos.craftingGrid_CritterObjects;
        var cnt = GourmandCombos.objectsLibrary.Count;
        GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.Physalis] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.FumeFruit] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.Durian] = cnt;
        ++cnt;
        var arrayOrig = GourmandCombos.craftingGrid_ObjectsOnly;
        var arrayNew = new GourmandCombos.CraftDat[cnt, cnt];
        int l0 = arrayOrig.GetLength(0), l1 = arrayOrig.GetLength(1), i, j;
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_ObjectsOnly = arrayNew;
        var cnt2 = GourmandCombos.critsLibrary.Count;
        GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera] = cnt2;
        ++cnt2;
        GourmandCombos.critsLibrary[CreatureTemplateType.XyloWorm] = cnt2;
        ++cnt2;
        arrayOrig = GourmandCombos.craftingGrid_CrittersOnly;
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt2];
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CrittersOnly = arrayNew;
        arrayOrig = GourmandCombos.craftingGrid_CritterObjects;
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt];
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CritterObjects = arrayNew;
    }

    public static void SetCombo(CreatureTemplate.Type a, CreatureTemplate.Type b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.critsLibrary[b], 2, null, result);

    public static void SetCombo(CreatureTemplate.Type a, CreatureTemplate.Type b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.critsLibrary[b], 2, result, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, CreatureTemplate.Type b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[b], GourmandCombos.objectsLibrary[a], 1, result, null);

    public static void SetCombo(CreatureTemplate.Type a, AbstractPhysicalObject.AbstractObjectType b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.objectsLibrary[b], 1, result, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, CreatureTemplate.Type b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[b], GourmandCombos.objectsLibrary[a], 1, null, result);

    public static void SetCombo(CreatureTemplate.Type a, AbstractPhysicalObject.AbstractObjectType b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.objectsLibrary[b], 1, null, result);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, AbstractPhysicalObject.AbstractObjectType b, CreatureTemplate.Type result) => GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[a], GourmandCombos.objectsLibrary[b], 0, null, result);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, AbstractPhysicalObject.AbstractObjectType b, AbstractPhysicalObject.AbstractObjectType result) => GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[a], GourmandCombos.objectsLibrary[b], 0, result, null);

    public static void SetCombo(CreatureTemplate.Type a, CreatureTemplate.Type b) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.critsLibrary[b], 2, null, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, CreatureTemplate.Type b) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[b], GourmandCombos.objectsLibrary[a], 1, null, null);

    public static void SetCombo(CreatureTemplate.Type a, AbstractPhysicalObject.AbstractObjectType b) => GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[a], GourmandCombos.objectsLibrary[b], 1, null, null);

    public static void SetCombo(AbstractPhysicalObject.AbstractObjectType a, AbstractPhysicalObject.AbstractObjectType b) => GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[a], GourmandCombos.objectsLibrary[b], 0, null, null);
}