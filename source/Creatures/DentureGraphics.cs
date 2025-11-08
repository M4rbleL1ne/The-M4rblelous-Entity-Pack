using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class DentureGraphics : GraphicsModule, IMuddableGraphics
{
    public const int BODY_SPRITE = 0, BODY_SPRITE2 = 1, BODY_SPRITE3 = 2, JAW1_SPRITE = 3, JAW2_SPRITE = 4;
    public Color BlackColor;
    public bool MoveToFront, AlbinoForm;

    public virtual Denture Creature => (owner as Denture)!;

    public DentureGraphics(Denture ow) : base(ow, false) => AlbinoForm = Creature.abstractCreature.Albino();

    public override void Reset() { }

    public override void PushOutOf(Vector2 pos, float rad) { }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("Futile_White")
            {
                alpha = .25f,
                shader = Custom.rainWorld.Shaders["JaggedCircle"]
            },
            new("Futile_White")
            {
                alpha = .25f,
                shader = Custom.rainWorld.Shaders["JaggedCircle"]
            },
            new("Futile_White")
            {
                alpha = .25f,
                shader = Custom.rainWorld.Shaders["JaggedCircle"]
            },
            new("DentureJawPart")
            {
                anchorX = 23f / 66f,
                anchorY = 13f / 201f,
                scale = 0f
            },
            new("DentureJawPart")
            {
                anchorX = 23f / 66f,
                anchorY = 13f / 201f,
                scale = 0f
            }
        ];
        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprs = sLeaser.sprites;
        FSprite body = sprs[BODY_SPRITE],
            body2 = sprs[BODY_SPRITE2],
            body3 = sprs[BODY_SPRITE3],
            jaw1 = sprs[JAW1_SPRITE],
            jaw2 = sprs[JAW2_SPRITE];
        if (Creature is Denture ow)
        {
            var pos = Vector2.Lerp(ow.firstChunk.lastPos, ow.firstChunk.pos, timeStacker);
            if (ow.abstractCreature.superSizeMe)
                pos -= ow.OutDir * 5f;
            var tweakedPos = pos - camPos;
            jaw1.SetPosition(tweakedPos);
            jaw2.SetPosition(tweakedPos);
            body.SetPosition(tweakedPos - ow.OutDir * 10f);
            var perp = new Vector2(-ow.OutDir.y, ow.OutDir.x) * 3f;
            body2.SetPosition(tweakedPos - ow.OutDir * 1.8f + perp);
            body3.SetPosition(tweakedPos - ow.OutDir * 1.8f - perp);
            var open = Mathf.Lerp(ow.LastJawOpen, ow.JawOpen, timeStacker);
            var angleDir = ow.GraphicsAngleDir();
            jaw1.rotation = Mathf.Lerp(0f, -80f, open) + angleDir;
            jaw2.rotation = Mathf.Lerp(0f, 80f, open) + angleDir;
            var sucked = Mathf.Lerp(ow.LastSuckedIntoShortcut, ow.SuckedIntoShortcut, timeStacker) * .75f;
            var baseScale = ow.JawRad / 150f;
            jaw1.scaleY = jaw2.scaleY = baseScale * (1f - sucked) * (1f + (1f - open) * .1f);
            jaw1.scaleX = baseScale * (1f - sucked);
            jaw2.scaleX = -baseScale * (1f - sucked);
            body.scale = baseScale * (1f - sucked) * 2.1f;
            body2.scale = body3.scale = baseScale * (1f - sucked) * 1.9f * (1f + (1f - open) * 3f);
            if (MoveToFront)
            {
                jaw1.MoveToFront();
                jaw2.MoveToFront();
                MoveToFront = false;
            }
            var rm = rCam.room;
            Color clr;
            if (rm is not null && AlbinoForm)
                clr = MaxBlack(Color.Lerp(BlackColor, new(.87f, .87f, .87f), .35f - rm.Darkness(pos) * (1f - rm.LightSourceExposure(pos)) * .15f));
            else
                clr = BlackColor;
            for (var i = 0; i < sprs.Length; i++)
            {
                var spr = sprs[i];
                spr.isVisible = spr.scaleY >= .08f;
                spr.color = clr;
            }
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => BlackColor = palette.blackColor;

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite is JAW1_SPRITE or JAW2_SPRITE;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) { }
}