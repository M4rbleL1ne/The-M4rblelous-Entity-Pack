using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class FatFireFlyGraphics : VultureGraphics
{
    public class FireSprite(Vector2 pos, bool altForm) : UpdatableAndDeletable, IDrawable
    {
        public Vector2 Pos = pos, LastPos = pos, Vel = Custom.RNV() * (1.5f * Random.value);
        public float LifeTime = Mathf.Lerp(10f, 40f, Random.value), Life = 1f, LastLife;
        public bool AltForm = altForm;

        public override void Update(bool eu)
        {
            base.Update(eu);
            Vel *= .8f;
            Vel.y += .4f;
            Vel += Custom.RNV() * (Random.value * .5f);
            LastLife = Life;
            Life -= 1f / LifeTime;
            if (Life < 0f)
                Destroy();
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            var sprite = sLeaser.sprites[0];
            sprite.RemoveFromContainer();
            rCam.ReturnFContainer("Water").AddChild(sprite);
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [new("Futile_White") { shader = Custom.rainWorld.Shaders["LBFFFLight"] }];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var sprites = sLeaser.sprites;
            var s0 = sprites[0];
            var life = Mathf.Lerp(LastLife, Life, timeStacker);
            s0.SetPosition(Vector2.Lerp(LastPos, Pos, timeStacker) - camPos);
            s0.scale = life * 2.5f;
            s0.alpha = life * .125f * (.9f + .1f * rCam.currentPalette.darkness);
            s0.color = Custom.HSL2RGB(AltForm ? Mathf.Lerp(194f / 360f, 250f / 360f, life) : Mathf.Lerp(.01f, .07f, life), 1f, .5f);
            if (!sLeaser.deleteMeNextFrame && (slatedForDeletetion || room != rCam.room))
                sLeaser.CleanSpritesAndRemove();
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
    }

    public bool AltForm;

    public FatFireFlyGraphics(FatFireFly ow) : base(ow)
    {
        albino = ow.abstractCreature.Albino();
        feathersPerWing /= 2;
        AltForm = ow.abstractCreature.superSizeMe;
        var vgs = wings;
        var w2 = wings = new VultureFeather[2, feathersPerWing];
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < feathersPerWing; j++)
                w2[i, j] = vgs[i, j * 2];
        }
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        if (AltForm)
        {
            ColorB = new(Mathf.Lerp(.5278f, .5972f, Random.value), Mathf.Lerp(.65f, .7f, 1f - Random.value * Random.value), Mathf.Lerp(.35f, .4f, Random.value * Random.value));
            ColorA = new(ColorB.hue + Mathf.Lerp(-.03f, .03f, Random.value), Mathf.Lerp(.7f, .8f, Random.value), Mathf.Lerp(.4f, .5f, Random.value));
        }
        else
        {
            ColorB = new(Mathf.Lerp(0f, .1f, Random.value), Mathf.Lerp(.95f, 1f, 1f - Random.value * Random.value), Mathf.Lerp(.45f, .5f, Random.value * Random.value));
            ColorA = new(ColorB.hue + Mathf.Lerp(-.03f, .03f, Random.value), Mathf.Lerp(.9f, .95f, Random.value), Mathf.Lerp(.5f, .55f, Random.value));
        }
        Random.state = state;
    }

    public override void Update()
    {
        base.Update();
        if (owner is Vulture v && v.room is Room rm && rm.BeingViewed && !v.dead)
        {
            var ws = wings;
            var l0 = ws.GetLength(0);
            var l1 = ws.GetLength(1);
            for (var i = 0; i < l0; i++)
            {
                for (var j = 0; j < l1; j++)
                {
                    var wi = ws[i, j];
                    wi.pos = Vector2.Lerp(wi.pos, wi.ConnectedPos, .75f);
                    if (Random.value > .35f && wi.wing is VultureTentacle t && t.mode == VultureTentacle.Mode.Fly && IsPositionInsideBoundries(wi.pos, rm) && !rm.PointSubmerged(wi.pos))
                    {
                        rm.AddObject(new FireSprite(wi.pos, AltForm));
                        if (Random.value <= .05f)
                            rm.PlaySound(SoundID.Firecracker_Burn, wi.pos, .25f, 1.5f, t.owner?.abstractPhysicalObject);
                    }
                }
            }
        }
    }

    public static bool IsPositionInsideBoundries(Vector2 pos, Room rm) => pos.x >= -50f && pos.x <= rm.PixelWidth + 50f && pos.y >= -50f && pos.y <= rm.PixelHeight + 50f;

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprites = sLeaser.sprites;
        FSprite fs0 = sprites[FrontShieldSprite(0)], bs0 = sprites[BackShieldSprite(0)], bs1 = sprites[BackShieldSprite(1)], body = sprites[BodySprite];
        bs1.anchorX = bs0.anchorX = fs0.anchorX = body.anchorX;
        bs1.anchorY = bs0.anchorY = fs0.anchorY = body.anchorY;
        body.element = Futile.atlasManager.GetElementWithName("FFFBody");
        fs0.element = Futile.atlasManager.GetElementWithName("FFFBodyColor");
        bs0.element = Futile.atlasManager.GetElementWithName("Futile_White");
        bs0.shader = Custom.rainWorld.Shaders["HeatDistortion"];
        bs1.element = Futile.atlasManager.GetElementWithName("Futile_White");
        bs1.shader = Custom.rainWorld.Shaders["HeatDistortion"];
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        var sprites = sLeaser.sprites;
        for (var j = 0; j < 2; j++)
        {
            for (var m = 0; m < feathersPerWing; m++)
                sprites[FeatherColorSprite(j, m)].isVisible = false;
        }
        FSprite fs0 = sprites[FrontShieldSprite(0)],
            bs0 = sprites[BackShieldSprite(0)],
            bs1 = sprites[BackShieldSprite(1)],
            body = sprites[BodySprite],
            eye = sprites[EyesSprite];
        var altForm = AltForm;
        var albino = this.albino;
        if (!spritesInShadowMode)
        {
            fs0.isVisible = true;
            eye.color = Color.Lerp(altForm ? (albino ? new(0f, .15f, .75f) : new(.92f, .92f, .95f)) : new(.75f, .15f, 0f), eye.color, altForm && !albino ? .25f : .5f);
            fs0.color = Color.Lerp(ColorB.rgb, Color.Lerp(altForm ? Color.blue : Color.red, albino ? Color.black : Color.white, .5f), .25f);
        }
        else
            fs0.isVisible = false;
        eye.element = Futile.atlasManager.GetElementWithName("FFFEyes" + headGraphic);
        bs1.x = bs0.x = fs0.x = body.x;
        bs1.y = bs0.y = fs0.y = body.y;
        bs1.rotation = bs0.rotation = fs0.rotation = body.rotation;
        fs0.scale = body.scale;
        bs1.scale = bs0.scale = 25f;
        bs1.color = bs0.color = Color.white;
        if (vulture is Vulture v && v.dead)
        {
            bs0.alpha = Math.Max(bs0.alpha - .1f, 0f);
            bs1.alpha = Math.Max(bs1.alpha - .1f, 0f);
        }
        else if (spritesInShadowMode)
            bs1.alpha = bs0.alpha = 0f;
        else
        {
            bs0.alpha = Math.Min(bs0.alpha + .1f, 1f);
            bs1.alpha = Math.Min(bs1.alpha + .1f, 1f);
        }
        sprites[FrontShieldSprite(1)].isVisible = false;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        base.AddToContainer(sLeaser, rCam, newContainer);
        newContainer = rCam.ReturnFContainer("GrabShaders");
        var bs = sLeaser.sprites[BackShieldSprite(0)];
        bs.RemoveFromContainer();
        newContainer.AddChild(bs);
        bs = sLeaser.sprites[BackShieldSprite(1)];
        bs.RemoveFromContainer();
        newContainer.AddChild(bs);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var albino = this.albino = vulture.abstractCreature.Albino();
        var altForm = AltForm = vulture.abstractCreature.superSizeMe;
        base.ApplyPalette(sLeaser, rCam, palette);
        var sprites = sLeaser.sprites;
        FSprite fs0 = sprites[FrontShieldSprite(0)],
            bs0 = sprites[BackShieldSprite(0)],
            bs1 = sprites[BackShieldSprite(1)],
            eye = sprites[EyesSprite];
        bs1.color = bs0.color = Color.white;
        if (!spritesInShadowMode)
        {
            eye.color = Color.Lerp(altForm ? (albino ? new(0f, .15f, .75f) : new(.92f, .92f, .95f)) : new(.75f, .15f, 0f), eye.color, altForm && !albino ? .25f : .5f);
            fs0.color = Color.Lerp(ColorB.rgb, Color.Lerp(altForm ? Color.blue : Color.red, albino ? Color.black : Color.white, .5f), .25f);
        }
    }
}