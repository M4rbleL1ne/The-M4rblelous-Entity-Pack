using RWCustom;
using UnityEngine;
using System;

namespace LBMergedMods.Creatures;

public class XyloWormGraphics : GraphicsModule, IMuddableGraphics
{
    public float DeadColor, LastDeadColor;

    public XyloWorm Worm => (owner as XyloWorm)!;

    public XyloWormGraphics(XyloWorm ow) : base(ow, false)
    {
        DeadColor = ow.State.alive ? 0f : 1f;
        LastDeadColor = DeadColor;
    }

    public override void Update()
    {
        base.Update();
        LastDeadColor = DeadColor;
        if (Worm.dead)
            DeadColor = Math.Min(1f, DeadColor + 1f / 154f);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var big = Worm?.Big is true;
        sLeaser.sprites = [TriangleMesh.MakeLongMesh(6, false, false), new(big ? "BigXyloWormTeeth" : "XyloWormTeeth") { scale = big ? 1.1f : .9f }];
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        sLeaser.RemoveAllSpritesFromContainer();
        newContainer ??= rCam.ReturnFContainer("Items");
        var spr = sLeaser.sprites[0];
        spr.RemoveFromContainer();
        newContainer.AddChild(spr);
        spr = sLeaser.sprites[1];
        spr.RemoveFromContainer();
        newContainer.AddChild(spr);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        var w = Worm;
        var ch0 = w.ChunkInOrder(0);
        var ch1 = w.ChunkInOrder(1);
        var ch2 = w.ChunkInOrder(2);
        Vector2 vector2 = Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker),
            vector = Vector2.Lerp(ch0.lastPos, ch0.pos, timeStacker);
        vector += Custom.DirVec(vector2, vector) * 5f;
        var a = Vector2.Lerp(vector, vector2, w.Swallowed * .9f);
        var a2 = Vector2.Lerp(Vector2.Lerp(ch2.lastPos, ch2.pos, timeStacker), vector2, w.Swallowed * .9f);
        a2 += Custom.DirVec(vector2, a2) * 5f;
        a2 += Custom.DirVec(vector2, a2) * 5f;
        var s1 = sLeaser.sprites[1];
        s1.SetPosition(a - camPos);
        s1.rotation = Custom.VecToDeg(HeadDir(timeStacker));
        var mesh = (sLeaser.sprites[0] as TriangleMesh)!;
        for (var j = 0; j < 6; j++)
        {
            float num3 = j / 5f,
                f = (j + 1) / 5f;
            Vector2 vector5 = Bez(a, a2, vector2, num3),
                b = Bez(a, a2, vector2, f);
            if (j > w.BitesLeft * 3)
            {
                vector5 = a;
                b = a;
            }
            var fac = w.Big ? 2f : 1.2f;
            Vector2 normalized = (vector - vector5).normalized,
                vector6 = Custom.PerpendicularVector(normalized);
            float num4 = Vector2.Distance(vector5, vector) * .2f * fac,
                num5 = Vector2.Distance(vector5, b) * .2f * fac,
                num6 = (2f + Mathf.Sin(num3 * Mathf.PI)) * fac,
                num7 = num6;
            if (num3 == 0f)
                num6 *= .5f;
            else if (num3 == 1f)
            {
                num7 *= .5f;
                num4 *= .5f;
                num5 *= .5f;
            }
            mesh.MoveVertice(j * 4, vector - vector6 * num6 - normalized * num4 - camPos);
            mesh.MoveVertice(j * 4 + 1, vector + vector6 * num6 - normalized * num4 - camPos);
            mesh.MoveVertice(j * 4 + 2, vector5 - vector6 * num7 + normalized * num5 - camPos);
            mesh.MoveVertice(j * 4 + 3, vector5 + vector6 * num7 + normalized * num5 - camPos);
            vector = vector5;
        }
        if (DeadColor != LastDeadColor)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
    }

    public static Vector2 Bez(Vector2 a, Vector2 b, Vector2 c, float f)
    {
        if (f < .5f)
            return Custom.Bezier(a, (a + c) / 2f, c, c + Custom.DirVec(b, a) * Vector2.Distance(a, c) / 4f, f);
        return Custom.Bezier(c, c + Custom.DirVec(a, b) * Vector2.Distance(c, b) / 2f, b, (b + c) / 2f, f);
    }

    public virtual Vector2 HeadDir(float timeStacker)
    {
        var worm = Worm;
        BodyChunk ch0 = worm.ChunkInOrder(0),
            ch1 = worm.ChunkInOrder(1),
            ch2 = worm.ChunkInOrder(2);
        var vector = Vector2.Lerp(ch0.lastPos, ch0.pos, timeStacker);
        vector += Custom.DirVec(Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker), vector) * 5f;
        Vector2 vector2 = Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker),
            vector3 = Vector2.Lerp(ch2.lastPos, ch2.pos, timeStacker);
        vector3 += Custom.DirVec(vector2, vector3) * 5f;
        return Custom.DirVec(vector, Bez(vector, vector3, vector2, .1f)).normalized;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = Worm?.Rotten is true ? palette.blackColor : Color.Lerp(Color.Lerp(palette.fogColor, new(1f, 1f, .65f), Mathf.Lerp(.4f, .3f, DeadColor)), palette.blackColor, Mathf.Lerp(Mathf.Pow(palette.darkness, 2f), 1f, .5f * DeadColor));
        sLeaser.sprites[1].color = palette.blackColor;
    }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => true;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) { }
}