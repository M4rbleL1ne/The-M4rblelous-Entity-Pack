using System;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class M4RTailFlyGraphics : HoverflyGraphics, HasDanglers
{
    public const int NEW_TOTAL_SPRITES = TOTAL_SPRITES + 1;
    public Dangler Tail;
    public Dangler.DanglerProps TailProps = new();

    public virtual float TailPosXBonus => CurrentHead switch
    {
        HeadState.FlyFastLeft => 4f,
        HeadState.FlyLeft or HeadState.LookLeft => 2f,
        HeadState.FlyFastRight => -4f,
        HeadState.FlyRight or HeadState.LookRight => -2f,
        _ => 0f
    };

    public override string HeadSprite => CurrentHead switch
    {
        HeadState.FlyFastLeft or HeadState.FlyFastRight => "M4RNFHead3",
        HeadState.FlyLeft or HeadState.FlyRight or HeadState.LookLeft or HeadState.LookRight => "M4RNFHead2",
        _ => "M4RNFHead1",
    };

    public override string HeadHighlightSprite => CurrentHead switch
    {
        HeadState.FlyFastLeft or HeadState.FlyFastRight => "M4RNFEyes3",
        HeadState.FlyLeft or HeadState.FlyRight or HeadState.LookLeft or HeadState.LookRight => "M4RNFEyes2",
        _ => "M4RNFEyes1"
    };

    public M4RTailFlyGraphics(M4RTailFly ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        Tail = new(this, 0, ow.TailLength, 5f, 5f, true);
        Tail.Reset();
        var segs = Tail.segments;
        var tailThickness = Mathf.Lerp(2.6f, 3f, Random.value);
        for (var j = 0; j < segs.Length; j++)
        {
            var t = j / (float)(segs.Length - 1);
            var seg = segs[j];
            seg.rad = Mathf.Lerp(Mathf.Lerp(1f, .5f, Mathf.Pow(t, .7f)), .5f + Mathf.Sin(Mathf.Pow(t, 2.5f) * Mathf.PI) * .5f, t) * tailThickness;
            seg.conRad = Mathf.Lerp(7.5f * .5f, 1.25f * .5f, t) * tailThickness;
        }
        Random.state = state;
    }

    public override void Reset()
    {
        base.Reset();
        Tail?.Reset();
    }

    public virtual Dangler.DanglerProps Props(int index) => TailProps;

    public virtual Vector2 DanglerConnection(int index, float timeStacker)
    {
        var fc = Fly.firstChunk;
        return Vector2.Lerp(fc.lastPos, fc.pos, timeStacker) + new Vector2(TailPosXBonus, 0f);
    }

    public override void Update()
    {
        base.Update();
        GlideCounter = 0;
        if (!culled)
            Tail.Update();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        Array.Resize(ref sLeaser.sprites, NEW_TOTAL_SPRITES);
        var sprs = sLeaser.sprites;
        var spr = sprs[TOTAL_SPRITES] = TriangleMesh.MakeLongMesh(Tail.segments.Length, false, true); // not using Tail.InitSprite because I need custom color
        rCam.ReturnFContainer("Midground").AddChild(spr);
        spr.MoveBehindOtherNode(sprs[BODY_SPRITE]);
        for (var i = HIGHLIGHT_SPRITE + 1; i < EYE_A_DARK; i++)
        {
            spr = sprs[i];
            spr.element = Futile.atlasManager.GetElementWithName(spr.element.name.Replace("Hoverfly", "M4RNF"));
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        base.AddToContainer(sLeaser, rCam, newContainer);
        var sprs = sLeaser.sprites;
        sprs[HIGHLIGHT_SPRITE].MoveInFrontOfOtherNode(sprs[EYE_A_DARK - 1]);
        if (sprs.Length > TOTAL_SPRITES)
        {
            var spr = sprs[TOTAL_SPRITES];
            rCam.ReturnFContainer("Midground").AddChild(spr);
            spr.MoveBehindOtherNode(sprs[BODY_SPRITE]);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled)
        {
            var sprs = sLeaser.sprites;
            Tail.DrawSprite(TOTAL_SPRITES, sLeaser, rCam, timeStacker, camPos);
            for (var i = EYE_A_DARK; i <= EYE_B_SPRITE; i++)
                sprs[i].isVisible = false;
            var flag = CurrentWing == WingState.Swarm;
            for (var i = 0; i < 2; i++)
                sprs[4 + i].SetElementByName(flag ? "M4RNFSmallWing2" : "M4RNFSmallWing1");
            sprs[HIGHLIGHT_SPRITE].isVisible = BlinkCounter >= 0;
        }
    }

    public override void WingMovementUpdate()
    {
        var fly = Fly;
        var fc = fly.firstChunk;
        if (fly.Consious)
        {
            if (fc.submersion > 0f)
                CurrentWing = WingState.Water;
            else if (!fly.Flying || fly.room?.aimap is AImap m && m.getAItile(fc.pos).narrowSpace || fly.inShortcut)
                CurrentWing = WingState.Crawl;
            else
                CurrentWing = WingState.Swarm;
        }
        else
            CurrentWing = WingState.Dead;
        switch (CurrentWing)
        {
            case WingState.Water:
                LastWingProgress = WingProgress;
                if (WingProgress <= 0f)
                    PushWingUp = true;
                else if (WingProgress >= 1f)
                    PushWingUp = false;
                WingProgress += (PushWingUp ? .125f : -.085f) / 3f;
                break;
            case WingState.Swarm:
                LastWingProgress = WingProgress;
                if (WingProgress <= .1f)
                    PushWingUp = true;
                else if (WingProgress >= .9f)
                    PushWingUp = false;
                WingProgress += PushWingUp ? 1f : -.5f;
                break;
            case WingState.Crawl:
            case WingState.Dead:
                var y = Custom.RotateAroundOrigo(fc.pos - fc.lastPos, Custom.AimFromOneVectorToAnother(fc.lastPos, fc.pos)).y;
                LastWingProgress = WingProgress;
                WingProgress = Mathf.Clamp(Mathf.Lerp(WingProgress, DeathWingPosition - y * .1f, .3f), 0f, 1f);
                break;
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        var col = Color.Lerp(IVars.Color, palette.blackColor, .5f * palette.darkness);
        var sprs = sLeaser.sprites;
        sprs[HIGHLIGHT_SPRITE].color = col;
        var tail = (sprs[TOTAL_SPRITES] as TriangleMesh)!;
        tail.color = palette.blackColor;
        var verts = tail.verticeColors;
        var lgt = verts.Length - 1f;
        for (var i = 0; i < verts.Length; i++)
            verts[i] = Color.Lerp(palette.blackColor, col, i / lgt);
    }
}