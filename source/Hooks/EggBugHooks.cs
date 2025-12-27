global using static LBMergedMods.Hooks.EggBugHooks;
using UnityEngine;
using RWCustom;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class EggBugHooks
{
    internal static void IL_EggBug_Act(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_0_3))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float num, EggBug self) => self is SurfaceSwimmer ? 10f : num);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook EggBug.Act! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_SoundID_Egg_Bug_Scurry))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((SoundID snd, EggBug self) => self is SurfaceSwimmer && Random.value < .075f ? NewSoundID.M4R_SurfaceSwimmer_Chip : snd);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook EggBug.Act! (part 2)");
    }

    internal static void IL_EggBug_TryJump(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_SoundID_Egg_Bug_Scurry))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((SoundID snd, EggBug self) => self is SurfaceSwimmer ? NewSoundID.M4R_SurfaceSwimmer_Chip : snd);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook EggBug.TryJump!");
    }

    internal static void On_EggBug_DropEggs(On.EggBug.orig_DropEggs orig, EggBug self)
    {
        if (self is SurfaceSwimmer)
            return;
        if (self is M4RMamaBug)
        {
            self.dropEggs = false;
            if (self.graphicsModule is M4RMamaBugGraphics grs)
            {
                for (var l = 0; l < 4; l++)
                {
                    for (var m = 0; m < 5; m++)
                    {
                        var eggGr = grs.eggs[l, m];
                        if (Random.value >= .5f)
                        {
                            var abstractEgg = new EggBugEgg.AbstractBugEgg(self.room.world, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), self.hue);
                            self.room.abstractRoom.AddEntity(abstractEgg);
                            abstractEgg.RealizeInRoom();
                            if (abstractEgg.realizedObject is EggBugEgg egg)
                            {
                                egg.firstChunk.HardSetPosition(grs.EggAttachPos(l, m, 1f));
                                egg.firstChunk.vel = eggGr.vel + Custom.DegToVec(180f * Mathf.Pow(Random.value, 3f) * (Random.value < .5f ? -1f : 1f)) * (16f * Mathf.Pow(Random.value, .3f));
                                egg.setRotation = Custom.RNV();
                                egg.swell = 0f;
                                egg.rotVel = Mathf.Lerp(-20f, 20f, Random.value);
                                egg.liquid = 1f;
                            }
                        }
                        else
                            self.room.AddObject(new EggBugEgg.LiquidDrip(eggGr.pos, eggGr.vel * 3f, Color.Lerp(grs.eggColors[1], grs.blackColor, .4f)));
                    }
                }
            }
            else
            {
                var b1 = self.bodyChunks[1];
                for (var n = 0; n < 20; n++)
                {
                    if (Random.value >= .5f)
                    {
                        var newPos = b1.pos + Custom.RNV() * (4f * Random.value);
                        var abstractEgg = new EggBugEgg.AbstractBugEgg(self.room.world, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), self.hue);
                        self.room.abstractRoom.AddEntity(abstractEgg);
                        abstractEgg.RealizeInRoom();
                        if (abstractEgg.realizedObject is EggBugEgg egg)
                        {
                            egg.firstChunk.HardSetPosition(newPos);
                            egg.firstChunk.vel = b1.vel + Custom.DegToVec(180f * Mathf.Pow(Random.value, 3f) * (Random.value < .5f ? -1f : 1f)) * (16f * Mathf.Pow(Random.value, .3f));
                            egg.setRotation = Custom.RNV();
                            egg.swell = 0f;
                            egg.rotVel = Mathf.Lerp(-20f, 20f, Random.value);
                        }
                    }
                }
            }
            self.room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, self.mainBodyChunk, false, 1.05f, .95f);
            return;
        }
        orig(self);
    }

    internal static void On_EggBug_MoveTowards(On.EggBug.orig_MoveTowards orig, EggBug self, Vector2 moveTo)
    {
        if (self is SurfaceSwimmer && !self.safariControlled && self.room is Room rm && !CanMove(moveTo, rm))
        {
            self.mainBodyChunk.vel.y -= 5f;
            return;
        }
        orig(self, moveTo);
    }

    internal static void On_EggBug_Run(On.EggBug.orig_Run orig, EggBug self, MovementConnection followingConnection)
    {
        if (self is SurfaceSwimmer && self.safariControlled)
        {
            AItile tl1 = self.room.aimap.getAItile(followingConnection.startCoord), tl2 = self.room.aimap.getAItile(followingConnection.destinationCoord);
            if (tl1.acc is AItile.Accessibility.Air or AItile.Accessibility.Wall && !tl1.AnyWater && tl2.acc is AItile.Accessibility.Air or AItile.Accessibility.Wall && !tl2.AnyWater)
                return;
        }
        orig(self, followingConnection);
    }

    internal static float On_EggBugAI_IdleScore(On.EggBugAI.orig_IdleScore orig, EggBugAI self, WorldCoordinate coord) => self is SurfaceSwimmerAI && self.bug?.room?.aimap?.getAItile(coord)?.acc >= AItile.Accessibility.Climb ? float.MaxValue : orig(self, coord);

    internal static bool On_EggBugAI_UnpleasantFallRisk(On.EggBugAI.orig_UnpleasantFallRisk orig, EggBugAI self, IntVector2 tile)
    {
        if (self is SurfaceSwimmerAI && self.bug.room.GetTile(self.bug.room.aimap.getAItile(tile).fallRiskTile).AnyWater)
            return false;
        return orig(self, tile);
    }

    internal static CreatureTemplate.Relationship On_EggBugAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.EggBugAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, EggBugAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c && c.abstractPhysicalObject.SameRippleLayer(self.creature))
        {
            var grs = c.grasps;
            if (grs is not null)
            {
                for (var i = 0; i < grs.Length; i++)
                {
                    if (grs[i]?.grabbed is LimeMushroom)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Afraid;
                        result.intensity = 1f;
                        break;
                    }
                }
            }
        }
        return result;
    }

    internal static bool On_EggBugGraphics_get_ShowEggs(Func<EggBugGraphics, bool> orig, EggBugGraphics self) => self is not SurfaceSwimmerGraphics && orig(self);

    internal static void IL_EggBugGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var ctr = 0;
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Room.Tile>("get_Solid"))
            {
                ++ctr;
                c.Goto(i, MoveType.After);
                if (ctr == 1)
                {
                    c.Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool solid, EggBugGraphics self) =>
                     {
                         var mbcpos = self.bug.mainBodyChunk.pos;
                         if (self is SurfaceSwimmerGraphics && self.bug.room.PointSubmerged(self.bug.room.MiddleOfTile(Room.StaticGetTilePosition(mbcpos + Custom.PerpendicularVector(mbcpos, self.bug.bodyChunks[1].pos) * 20f))))
                             return true;
                         return solid;
                     });
                }
                else if (ctr == 2)
                {
                    c.Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool solid, EggBugGraphics self) =>
                     {
                         var mbcpos = self.bug.mainBodyChunk.pos;
                         if (self is SurfaceSwimmerGraphics && self.bug.room.PointSubmerged(self.bug.room.MiddleOfTile(Room.StaticGetTilePosition(mbcpos - Custom.PerpendicularVector(mbcpos, self.bug.bodyChunks[1].pos) * 20f))))
                             return true;
                         return solid;
                     });
                    break;
                }
            }
        }
    }

    internal static Vector2 On_EggBugGraphics_EggAttachPos(On.EggBugGraphics.orig_EggAttachPos orig, EggBugGraphics self, int s, int egg, float timeStacker)
    {
        if (self is M4RMamaBugGraphics m)
            return m.EggAttachPos(s, egg, timeStacker);
        return orig(self, s, egg, timeStacker);
    }

    internal static int On_EggBugGraphics_FrontEggSprite(On.EggBugGraphics.orig_FrontEggSprite orig, EggBugGraphics self, int s, int e, int part)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.FrontEggSprite(s, e, part);
        return orig(self, s, e, part);
    }

    internal static int On_EggBugGraphics_LegSprite(On.EggBugGraphics.orig_LegSprite orig, EggBugGraphics self, int leg, int side, int part)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.LegSprite(leg, side, part);
        return orig(self, leg, side, part);
    }

    internal static int On_EggBugGraphics_EyeSprite(On.EggBugGraphics.orig_EyeSprite orig, EggBugGraphics self, int eye)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.EyeSprite(eye);
        return orig(self, eye);
    }

    internal static int On_EggBugGraphics_AntennaSprite(On.EggBugGraphics.orig_AntennaSprite orig, EggBugGraphics self, int side)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.AntennaSprite(side);
        return orig(self, side);
    }

    internal static int On_EggBugGraphics_BackEggSprite(On.EggBugGraphics.orig_BackEggSprite orig, EggBugGraphics self, int s, int e, int part)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.BackEggSprite(s, e, part);
        return orig(self, s, e, part);
    }

    internal static int On_EggBugGraphics_get_TotalSprites(Func<EggBugGraphics, int> orig, EggBugGraphics self)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.TOTAL_SPRITES;
        return orig(self);
    }

    internal static int On_EggBugGraphics_get_MeshSprite(Func<EggBugGraphics, int> orig, EggBugGraphics self)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.MESH_SPRITE;
        return orig(self);
    }

    internal static int On_EggBugGraphics_get_HeadSprite(Func<EggBugGraphics, int> orig, EggBugGraphics self)
    {
        if (self is M4RMamaBugGraphics)
            return M4RMamaBugGraphics.HEAD_SPRITE;
        return orig(self);
    }

    internal static bool CanMove(Vector2 moveTo, Room rm)
    {
        Room.Tile tl = rm.GetTile(moveTo),
            tl2 = rm.GetTile(moveTo with { y = moveTo.y + 20f }),
            tl3 = rm.GetTile(moveTo with { y = moveTo.y - 20f }),
            tl4 = rm.GetTile(moveTo with { x = moveTo.x - 20f }),
            tl5 = rm.GetTile(moveTo with { x = moveTo.x + 20f }),
            tl6 = rm.GetTile(new Vector2(moveTo.x - 20f, moveTo.y - 20f)),
            tl7 = rm.GetTile(new Vector2(moveTo.x + 20f, moveTo.y - 20f)),
            tl8 = rm.GetTile(new Vector2(moveTo.x + 20f, moveTo.y + 20f)),
            tl9 = rm.GetTile(new Vector2(moveTo.x - 20f, moveTo.y + 20f));
        if (tl.InvalidTile() && tl2.InvalidTile() && tl3.InvalidTile() && tl4.InvalidTile() && tl5.InvalidTile() && tl6.InvalidTile() && tl7.InvalidTile() && tl8.InvalidTile() && tl9.InvalidTile())
            return false;
        return true;
    }

    internal static bool InvalidTile(this Room.Tile tl) => tl.Terrain == Room.Tile.TerrainType.Air && !tl.AnyBeam && !tl.AnyWater;
}