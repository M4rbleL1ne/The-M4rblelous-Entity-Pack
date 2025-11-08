using System;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class BouncingBallGraphics : SnailGraphics, IMuddableGraphics
{
    public int[] EffectColorRND;

    public BouncingBallGraphics(BouncingBall ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        EffectColorRND = [Random.Range(0, 2), Random.Range(0, 2)];
        Random.state = state;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        base.AddToContainer(sLeaser, rCam, newContainer);
        var sprs = sLeaser.sprites;
        if (sprs.Length >= 10)
        {
            var spr = sprs[8];
            spr.RemoveFromContainer();
            rCam.ReturnFContainer("Midground").AddChild(spr);
            spr = sprs[9];
            spr.RemoveFromContainer();
            rCam.ReturnFContainer("Shadows").AddChild(spr);
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (snail is BouncingBall s)
        {
            var sprs = sLeaser.sprites;
            sprs[6].color = palette.blackColor;
            ref Color sc1 = ref s.shellColor[1], sc0 = ref s.shellColor[0];
            sc0 = palette.texture.GetPixel(30, 5 - EffectColorRND[0] * 2);
            sc1 = palette.texture.GetPixel(30, 5 - EffectColorRND[1] * 2);
            sprs[7].color = sc1;
            sprs[8].color = sc1;
            for (var j = 0; j < 2; j++)
                sprs[4 + j].color = sc0;
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprs = sLeaser.sprites;
        if (snail is not BouncingBall s || s.dead)
        {
            for (var i = 0; i < sprs.Length; i++)
                sprs[i].isVisible = false;
            sLeaser.CleanSpritesAndRemove();
            return;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (s.room is Room rm)
        {
            FSprite spr8 = sprs[8], spr7 = sprs[7], spr9 = sprs[9];
            var chunk = s.bodyChunks[0];
            spr8.x = spr7.x;
            spr8.y = spr7.y;
            spr8.rotation = spr7.rotation;
            spr8.scale = spr7.scale / 2f;
            spr9.x = Mathf.Lerp(chunk.lastPos.x, chunk.pos.x, timeStacker) - camPos.x;
            spr9.y = Mathf.Lerp(chunk.lastPos.y, chunk.pos.y, timeStacker) - camPos.y;
            spr9.scaleY = rm.lightAngle.magnitude * .25f * shadowExtensionFac;
            var a = Mathf.Lerp(s.LastAlpha, s.Alpha, timeStacker);
            spr7.alpha = a;
            spr8.alpha = a;
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        if (snail is BouncingBall s)
        {
            ref var sprites = ref sLeaser.sprites;
            if (sprites.Length < 10)
                Array.Resize(ref sprites, 10);
            sprites[6].element = Futile.atlasManager.GetElementWithName("BoBShellA");
            sprites[6].color = rCam.currentPalette.blackColor;
            sprites[7].element = Futile.atlasManager.GetElementWithName("BoBShellB");
            sprites[7].alpha = .75f;
            sprites[8] = new("BoBShellC2") { color = s.shellColor[1] };
            sprites[9] = new("Circle20")
            {
                scaleX = s.firstChunk.rad / 10f,
                anchorY = 0f,
                rotation = s.room is not Room rm ? 0f : Custom.AimFromOneVectorToAnother(new(rm.lightAngle.x, -rm.lightAngle.y), default),
                color = new(.003921569f, 0f, 0f)
            };
            for (var j = 0; j < 2; j++)
                sprites[4 + j].scale = 2f;
            AddToContainer(sLeaser, rCam, null);
        }
    }

    public virtual new bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite is 0 or 1 or 2 or 3 or 6;

    public virtual new void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) => mudOverlay.layeringMap[mudOverlay.GetMudSprite(6)] = sLeaser.sprites[8];
}