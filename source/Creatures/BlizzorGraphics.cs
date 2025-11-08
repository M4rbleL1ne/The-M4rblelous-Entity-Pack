using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class BlizzorGraphics : MirosBirdGraphics, IMuddableGraphics
{
    public BlizzorGraphics(Blizzor ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        eyeCol = Custom.HSL2RGB(Mathf.Lerp(.65f, .69f, Random.value), .9f, .5f);
        eyeSize *= 1.2f;
        if (eyeSize < 1f)
            eyeSize = 1f;
        tighSize *= 1.2f;
        plumageLength *= 1.1f;
        plumageDensity *= 2f;
        plumageWidth *= 1.2f;
        neckFatness *= 2f;
        beakFatness *= 1.1f;
        Random.state = state;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprites = sLeaser.sprites;
        sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("BlizzorHead");
        sprites[NeckSprite].element = Futile.atlasManager.GetElementWithName("BlizzorNeck");
        sprites[BodySprite].element = Futile.atlasManager.GetElementWithName("BlizzorBody");
        var lgs = legs;
        for (var i = 0; i < lgs.Length; i++)
            sprites[lgs[i].firstSprite].element = Futile.atlasManager.GetElementWithName("BlizzorTigh");
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        var sprites = sLeaser.sprites;
        var clr = Color.Lerp(Color.white, palette.fogColor, Mathf.Lerp(palette.fogAmount, 0f, .75f) + .1f);
        for (var i = 0; i < sprites.Length; i++)
        {
            if (i != EyeTrailSprite && i != EyeSprite && (i < FirstBeakSprite || i > LastBeakSprite) && (i < FirstLegSprite || i > LastLegSprite))
                sprites[i].color = clr;
        }
        var lgs = legs;
        for (var i = 0; i < lgs.Length; i++)
            sprites[lgs[i].firstSprite].color = clr;
        sprites[EyeSprite].color = EyeColor;
    }


    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite != EyeSprite && sprite != EyeTrailSprite;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) { }
}