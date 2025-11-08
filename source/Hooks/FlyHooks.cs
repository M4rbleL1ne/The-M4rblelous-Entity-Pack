global using static LBMergedMods.Hooks.FlyHooks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;

namespace LBMergedMods.Hooks;

public static class FlyHooks
{
    public static Dictionary<string, bool> SeedRooms = [];

    internal static void On_Fly_Act(On.Fly.orig_Act orig, Fly self, bool eu)
    {
        if (self.IsSeed())
        {
            if (self.movMode == Fly.MovementMode.Hang)
            {
                self.ReleaseGrasp(0);
                self.movMode = Fly.MovementMode.BatFlight;
            }
            else if (self.movMode == Fly.MovementMode.SwarmFlight)
                self.movMode = Fly.MovementMode.BatFlight;
        }
        orig(self, eu);
    }

    internal static void On_Fly_BatFlight(On.Fly.orig_BatFlight orig, Fly self, bool panic)
    {
        orig(self, panic);
        if (self.IsSeed())
            self.mainBodyChunk.vel *= 1.04f;
    }

    internal static void On_Fly_NewRoom(On.Fly.orig_NewRoom orig, Fly self, Room room)
    {
        orig(self, room);
        if (Seed.TryGetValue(self.abstractCreature, out var prop) && !prop.Born)
        {
            prop.Born = true;
            var flag = false;
            if (room.game?.GetStorySession?.saveStateNumber?.value == "LBHardhatCat")
            {
                var state = Random.state;
                Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                flag = Random.value <= .45f;
                Random.state = state;
            }
            if (room.roomSettings?.GetEffectAmount(RoomEffectType.SeedBats) > 0 || flag)
                prop.IsSeed = true;
        }
    }

    internal static void On_FlyAI_Update(On.FlyAI.orig_Update orig, FlyAI self)
    {
        if (self.fly?.IsSeed() is true && (self.behavior == FlyAI.Behavior.Swarm || self.behavior == FlyAI.Behavior.Chain))
            self.behavior = FlyAI.Behavior.Idle;
        orig(self);
    }

    internal static void On_FlyGraphics_DrawSprites(On.FlyGraphics.orig_DrawSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.culled && self.fly?.IsSeed(out var prop) is true)
        {
            if (sLeaser.sprites.Length < 6)
                AddSeedSprites(sLeaser, rCam, prop);
            FSprite s3 = sLeaser.sprites[3], s0 = sLeaser.sprites[0], e1 = sLeaser.sprites[prop.Ext1], e2 = sLeaser.sprites[prop.Ext2], s1 = sLeaser.sprites[1], s2 = sLeaser.sprites[2];
            s0.rotation += 180f;
            s3.CopyNoColor(s0);
            e1.CopyNoColor(s1);
            e2.CopyNoColor(s2);
            e1.color = e2.color = s3.color;
            s0.color = s2.color = s1.color = rCam.currentPalette.blackColor;
        }
    }

    internal static void On_FlyGraphics_InitiateSprites(On.FlyGraphics.orig_InitiateSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.fly?.IsSeed(out var prop) is true)
            AddSeedSprites(sLeaser, rCam, prop);
    }

    internal static bool On_FlyGraphics_MuddableSprite(On.FlyGraphics.orig_MuddableSprite orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, int sprite) => self.fly?.IsSeed() is true || orig(self, sLeaser, sprite);

    public static bool IsSeed(this AbstractCreature self) => Seed.TryGetValue(self, out var prop) && prop.IsSeed;

    public static bool IsSeed(this Fly self) => Seed.TryGetValue(self.abstractCreature, out var prop) && prop.IsSeed;

    public static bool IsSeed(this Fly self, out FlyProperties prop) => Seed.TryGetValue(self.abstractCreature, out prop) && prop.IsSeed;

    internal static void CopyNoColor(this FSprite self, FSprite other)
    {
        self.x = other.x;
        self.y = other.y;
        self.isVisible = other.isVisible;
        self.scaleX = other.scaleX;
        self.scaleY = other.scaleY;
        self.rotation = other.rotation;
        self.alpha = other.alpha;
        self.shader = other.shader;
    }

    internal static void AddSeedSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FlyProperties prop)
    {
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 2);
        var f1 = Futile.atlasManager.GetElementWithName("SeedWing1");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < 3; i++)
        {
            var s = sprs[i];
            s.element = f1;
            s.anchorY = 0;
        }
        var s3 = sprs[3];
        s3.element = Futile.atlasManager.GetElementWithName("SeedWing2");
        s3.anchorY = 0;
        for (var i = 0; i < sprs.Length; i++)
        {
            if (sprs[i] is null)
            {
                prop.Ext1 = i;
                break;
            }
        }
        prop.Ext2 = prop.Ext1 + 1;
        var cont = rCam.ReturnFContainer("Midground");
        cont.AddChild(sprs[prop.Ext1] = new("SeedWing2") { anchorY = 0 });
        cont.AddChild(sprs[prop.Ext2] = new("SeedWing2") { anchorY = 0 });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool SeedBatRooms(this AbstractRoom self)
    {
        var nm = self.FileName;
        if (SeedRooms.TryGetValue(nm, out var flag))
            return flag;
        var res = File.Exists(AssetManager.ResolveFilePath("levels/" + nm + "_seedbats.txt")) ||
            (self.world?.name is string s && File.Exists(AssetManager.ResolveFilePath("world/" + s.ToLower() + "-rooms/" + nm + "_seedbats.txt")));
        SeedRooms.Add(nm, res);
        return res;
    }
}