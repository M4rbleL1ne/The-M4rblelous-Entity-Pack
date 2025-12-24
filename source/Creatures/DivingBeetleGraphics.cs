using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class DivingBeetleGraphics : GraphicsModule, IMuddableGraphics
{
    public const int HEAD_SPRITE = 8, MESH_SPRITE = 9, SHINE_MESH_SPRITE = 10, TOTAL_SPRITES = 27;
    public const float LEG_LENGTH = 5f;
    public static Color BugCol = new(.5f, .5f, .25f);
    public DivingBeetle Bug;
    public Vector2[][] DrawPositions, LegsTravelDirs;
    public float Flip, LastFlip, Darkness, LastDarkness, LegsThickness, BodyThickness, AntennaeLength, Hue, ColoredAntennae;
    public Color ShineColor, CurrentSkinColor;
    public Limb[][] Legs;
    public Vector2 BreathDir;
    public GenericBodyPart[][] Knees;
    public int LegsDangleCounter;
    public float[][] MandibleMovements;
    public GenericBodyPart[] Mandibles, Antennae;
    public GenericBodyPart TailEnd;

    public DivingBeetleGraphics(DivingBeetle ow) : base(ow, false)
    {
        Bug = ow;
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var chs = Bug.bodyChunks;
        var dp = DrawPositions = new Vector2[chs.Length][];
        TailEnd = new(this, 2.5f, .5f, .99f, chs[2]);
        for (var i = 0; i < dp.Length; i++)
            dp[i] = new Vector2[2];
        LastDarkness = -1f;
        Mandibles =
        [
            new(this, .25f, .5f, .9f, chs[0]),
            new(this, .25f, .5f, .9f, chs[0])
        ];
        Antennae =
        [
            new(this, 1f, .5f, .9f, chs[0]),
            new(this, 1f, .5f, .9f, chs[0])
        ];
        var legs = Legs = new Limb[2][];
        Knees = new GenericBodyPart[2][];
        LegsTravelDirs = new Vector2[2][];
        MandibleMovements = new float[2][];
        for (var l = 0; l < legs.Length; l++)
        {
            var legsl = legs[l] = new Limb[2];
            var kne = Knees[l] = new GenericBodyPart[2];
            LegsTravelDirs[l] = new Vector2[2];
            MandibleMovements[l] = new float[2];
            for (var m = 0; m < legsl.Length; m++)
            {
                legsl[m] = new(this, chs[0], l * 4 + m, .05f, .7f, .99f, 22f, .95f);
                kne[m] = new(this, .5f, .5f, .99f, chs[0]);
            }
        }
        bodyParts = new BodyPart[13];
        bodyParts[0] = TailEnd;
        bodyParts[1] = Mandibles[0];
        bodyParts[2] = Mandibles[1];
        bodyParts[3] = Antennae[0];
        bodyParts[4] = Antennae[1];
        var num = 5;
        for (var n = 0; n < legs.Length; n++)
        {
            var legsn = legs[n];
            for (var num2 = 0; num2 < legsn.Length; num2++)
            {
                bodyParts[num] = legsn[num2];
                ++num;
                bodyParts[num] = Knees[n][num2];
                ++num;
            }
        }
        BodyThickness = Mathf.Lerp(1.2f, 1.4f, Random.value) * 1.5f;
        LegsThickness = Mathf.Lerp(1.1f, 1.4f, Random.value) * .25f;
        AntennaeLength = Mathf.Lerp(.6f, 1f, Random.value) * .4f;
        ColoredAntennae = Random.value < .5f ? Random.value : 0f;
        Hue = 115f / 180f + (1f - Custom.ClampedRandomVariation(.5f, .5f, .3f) * 2f) * .18f;
        Reset();
        Random.state = state;
    }

    public virtual int LegSprite(int side, int leg) => side * 2 + leg;

    public virtual int MandibleSprite(int side, int part) => 4 + side * 2 + part;

    public virtual int SegmentSprite(int s) => 11 + s;

    public virtual int WingSprite(int side) => 21 + side;

    public virtual int AntennaSprite(int side) => 23 + side;

    public override void Reset()
    {
        base.Reset();
        var drawPos = DrawPositions;
        for (var i = 0; i < drawPos.Length; i++)
        {
            var dpi = drawPos[i];
            var pos = Bug.bodyChunks[i].pos;
            dpi[1] = pos;
            dpi[0] = pos;
        }
    }

    public override void Update()
    {
        base.Update();
        var chs = Bug.bodyChunks;
        var fch = chs[0];
        var drp = DrawPositions;
        for (var i = 0; i < drp.Length; i++)
        {
            var dr = drp[i];
            dr[1] = dr[0];
            dr[0] = chs[i].pos;
        }
        if (!Bug.Consious || !Bug.Footing || Bug.Swimming)
            LegsDangleCounter = 30;
        else if (LegsDangleCounter > 0)
        {
            --LegsDangleCounter;
            if (Bug.Footing)
            {
                for (var j = 0; j < 2; j++)
                {
                    if (Bug.room.aimap.TileAccessibleToCreature(chs[j].pos, Bug.Template))
                        LegsDangleCounter = 0;
                }
            }
        }
        LastFlip = Flip;
        TailEnd.Update();
        TailEnd.ConnectToPoint(chs[2].pos + Custom.DirVec(chs[1].pos, chs[2].pos) * 12f + Custom.PerpendicularVector(chs[1].pos, chs[2].pos) * (Flip * 10f), 12f, false, .2f, chs[1].vel, .5f, .1f);
        TailEnd.vel.y -= .4f;
        TailEnd.vel += Custom.DirVec(chs[1].pos, chs[2].pos) * .8f;
        if (!Bug.dead)
        {
            TailEnd.vel += BreathDir * .7f;
            BreathDir = Vector2.ClampMagnitude(BreathDir + Custom.RNV() * (Random.value * .01f), 1f);
        }
        var num = Custom.AimFromOneVectorToAnother(chs[1].pos, fch.pos);
        var vector2 = Custom.DirVec(chs[1].pos, fch.pos);
        var bodyChunk = Bug.grasps[0]?.grabbedChunk;
        var mands = Mandibles;
        for (var m = 0; m < mands.Length; m++)
        {
            var mandMvt = MandibleMovements[m];
            var mand = mands[m];
            mand.Update();
            mand.ConnectToPoint(fch.pos + vector2 * (12f + 4f * mandMvt[0]), 12f + 4f * mandMvt[0], false, 0f, fch.vel, .1f, 0f);
            if (Bug.Consious)
            {
                mandMvt[0] = Custom.LerpAndTick(mandMvt[0], mandMvt[1], 0f, 1f / 20f);
                if (Random.value < 1f / 20f)
                    mandMvt[1] = Mathf.Clamp(mandMvt[1] + Mathf.Lerp(-1f, 1f, Random.value) * .75f, -1f, 1f);
            }
            mand.vel += (vector2 + Custom.PerpendicularVector(vector2) * Mathf.Lerp(m == 0 ? -1f : 1f, Flip * 10f, Math.Abs(Flip) * .9f)).normalized;
            if (bodyChunk is not null)
            {
                mand.pos = Vector2.Lerp(mand.pos, bodyChunk.pos, .5f);
                mand.vel *= .7f;
            }
            var ant = Antennae[m];
            ant.Update();
            var vector4 = (Custom.DirVec(chs[1].pos, fch.pos) + Custom.PerpendicularVector(chs[1].pos, fch.pos) * (Flip * .5f) + Custom.PerpendicularVector(chs[1].pos, fch.pos) * ((m == 0 ? -1f : 1f) * (1f - Math.Abs(Flip)) * .35f)).normalized;
            ant.ConnectToPoint(fch.pos, 50f * AntennaeLength, false, 0f, fch.vel, .05f, 0f);
            ant.vel += vector4 * Custom.LerpMap(Vector2.Distance(ant.pos, fch.pos + vector4 * (50f * AntennaeLength)), 10f, 150f, 0f, 14f, .7f);
            if (Bug.Consious)
                ant.vel += Custom.RNV() * Random.value;
        }
        var num3 = 0f;
        var num4 = 0;
        var legs = Legs;
        for (var n = 0; n < legs.Length; n++)
        {
            var legsn = legs[n];
            for (var num5 = 0; num5 < legsn.Length; num5++)
            {
                var leg = legsn[num5];
                num3 += Custom.DistanceToLine(leg.pos, chs[1].pos, fch.pos);
                if (leg.OverLappingHuntPos)
                    num4++;
            }
        }
        if (!float.IsNaN(num3))
            Flip = Custom.LerpAndTick(Flip, Mathf.Clamp(num3 / 40f, -1f, 1f), .07f, .1f);
        var num6 = 0f;
        if (Bug.Consious)
        {
            if (Bug.room.GetTile(fch.pos + Custom.PerpendicularVector(fch.pos, chs[1].pos) * 20f).Solid)
                num6 += 1f;
            if (Bug.room.GetTile(fch.pos - Custom.PerpendicularVector(fch.pos, chs[1].pos) * 20f).Solid)
                num6 -= 1f;
        }
        if (num6 != 0f)
            Flip = Custom.LerpAndTick(Flip, num6, .07f, .05f);
        var num7 = 0;
        for (var num9 = 0; num9 < legs.Length; num9++)
        {
            var legsn = legs[num9];
            for (var num8 = 0; num8 < legsn.Length; num8++)
            {
                var legc = legsn[num8];
                var knee = Knees[num9][num8];
                float t = Mathf.InverseLerp(0f, legsn.Length - 1, num8), num10 = .5f + .5f * Mathf.Sin((Bug.RunCycle + num7 * .25f) * Mathf.PI);
                LegsTravelDirs[num9][num8] = Vector2.Lerp(LegsTravelDirs[num9][num8], Bug.TravelDir, Mathf.Pow(Random.value, 1f - .9f * num10));
                legc.Update();
                if (legc.mode == Limb.Mode.HuntRelativePosition || LegsDangleCounter > 0)
                    legc.mode = Limb.Mode.Dangle;
                Vector2 vector5 = Custom.DegToVec(num + Mathf.Lerp(40f, 160f, t) * (num6 != 0f ? 0f - num6 : num9 == 0 ? 1f : -1f)),
                    vector6 = fch.pos + (Vector2)Vector3.Slerp(LegsTravelDirs[num9][num8], vector5, .1f) * (LEG_LENGTH * .85f * Mathf.Pow(num10, .5f));
                legc.ConnectToPoint(vector6, LEG_LENGTH, false, 0f, fch.vel, .1f, 0f);
                legc.ConnectToPoint(fch.pos, LEG_LENGTH, false, 0f, fch.vel, .1f, 0f);
                knee.Update();
                knee.vel += Custom.DirVec(vector6, knee.pos) * ((LEG_LENGTH * .55f - Vector2.Distance(knee.pos, vector6)) * .15f * Bug.BurstSpeed);
                knee.pos += Custom.DirVec(vector6, knee.pos) * ((LEG_LENGTH * .55f - Vector2.Distance(knee.pos, vector6)) * .6f);
                knee.vel += Custom.DirVec(legc.pos, knee.pos) * ((LEG_LENGTH * .55f - Vector2.Distance(knee.pos, legc.pos)) * .15f * Bug.BurstSpeed);
                knee.pos += Custom.DirVec(legc.pos, knee.pos) * ((LEG_LENGTH * .55f - Vector2.Distance(knee.pos, legc.pos)) * .6f);
                if (Custom.DistLess(knee.pos, fch.pos, 15f))
                {
                    knee.vel += Custom.DirVec(fch.pos, knee.pos) * ((15f - Vector2.Distance(knee.pos, fch.pos)) * Bug.BurstSpeed * .25f);
                    knee.pos += Custom.DirVec(fch.pos, knee.pos) * (15f - Vector2.Distance(knee.pos, fch.pos));
                }
                knee.vel = Vector2.Lerp(knee.vel, fch.vel, .8f) * (Bug.BurstSpeed * .25f);
                knee.vel += Custom.PerpendicularVector(chs[1].pos, fch.pos) * (Mathf.Lerp(num9 == 0 ? -1f : 1f, Mathf.Sign(Flip), Math.Abs(Flip)) * 2.25f * Bug.BurstSpeed);
                if (!Custom.DistLess(knee.pos, vector6, 200f))
                    knee.pos = vector6 + Custom.RNV() * (Random.value * .1f);
                if (LegsDangleCounter > 0 || num10 < .1f)
                {
                    var vector7 = vector6 + LegsTravelDirs[num9][num8] * (LEG_LENGTH * .5f);
                    legc.vel = Vector2.Lerp(legc.vel, vector7 - legc.pos, Bug.Swimming ? .25f : .05f) * (Bug.BurstSpeed * .25f);
                    legc.vel.y -= .4f * Bug.BurstSpeed * .25f;
                }
                else
                {
                    var vector8 = vector6 + vector5 * LEG_LENGTH;
                    for (var num11 = 0; num11 < legs.Length; num11++)
                    {
                        var legsm = legs[num11];
                        for (var num12 = 0; num12 < legsm.Length; num12++)
                        {
                            var leg = legsm[num12];
                            if (num11 != num9 && num12 != num8 && Custom.DistLess(vector8, leg.absoluteHuntPos, LEG_LENGTH * .1f))
                                vector8 = leg.absoluteHuntPos + Custom.DirVec(leg.absoluteHuntPos, vector8) * (LEG_LENGTH * .1f);
                        }
                    }
                    var num13 = 1.2f;
                    if (!legc.reachedSnapPosition)
                        legc.FindGrip(Bug.room, vector6, vector6, LEG_LENGTH * num13, vector8, -2, -2, true);
                    else if (!Custom.DistLess(vector6, legc.absoluteHuntPos, LEG_LENGTH * num13 * Mathf.Pow(1f - num10, .2f)))
                        legc.mode = Limb.Mode.Dangle;
                }
                num7++;
            }
        }
        if (Bug.VoiceSound?.slatedForDeletetion is false)
        {
            var ants = Antennae;
            for (var num14 = 0; num14 < ants.Length; num14++)
                ants[num14].pos += Custom.RNV() * (Random.value * 4f);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprites = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        sprites[MESH_SPRITE] = TriangleMesh.MakeLongMesh(12, false, false);
        sprites[SHINE_MESH_SPRITE] = TriangleMesh.MakeLongMesh(11, false, true, "wwdvb_lh");
        sprites[HEAD_SPRITE] = new("Circle20")
        {
            scaleX = .7f,
            scaleY = .8f
        };
        for (var i = 0; i < 10; i++)
            sprites[SegmentSprite(i)] = new("pixel") { anchorY = 0f };
        var legs = Legs;
        for (var j = 0; j < legs.Length; j++)
        {
            var lg = legs[j].Length;
            for (var k = 0; k < lg; k++)
                sprites[LegSprite(j, k)] = TriangleMesh.MakeLongMesh(12, false, false, "wwdvb_fur");
        }
        for (var l = 0; l < 2; l++)
        {
            sprites[MandibleSprite(l, 0)] = new("CentipedeLegA");
            sprites[MandibleSprite(l, 1)] = new("CentipedeLegB");
            sprites[WingSprite(l)] = new("wwdvb_shell");
            sprites[AntennaSprite(l)] = TriangleMesh.MakeLongMesh(8, false, ColoredAntennae > 0f);
        }
        sprites[25] = new("wwdvb_wingw") { anchorX = 0f, anchorY = .5f, shader = Custom.rainWorld.Shaders["DivingBeetleFin"] };
        sprites[26] = new("wwdvb_wingw2") { anchorX = 1f, anchorY = .5f, shader = Custom.rainWorld.Shaders["DivingBeetleFin2"] };
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        var sprites = sLeaser.sprites;
        var s0 = sprites[0];
        sprites[25].MoveBehindOtherNode(s0);
        sprites[26].MoveBehindOtherNode(s0);
        RefreshColor(sLeaser);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (culled)
            return;
        var sp = Mathf.Clamp01(Mathf.Lerp(Bug.LastBurstSpeed, Bug.BurstSpeed, timeStacker * (1f + Bug.ChunkVelResult.magnitude * .003333f)) * Bug.ChunkVelResult.magnitude * .03333f);
        float num = Mathf.Lerp(LastFlip, Flip, timeStacker);
        var bPos = Vector2.Lerp(Bug.firstChunk.lastPos, Bug.firstChunk.pos, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(bPos);
        if (Darkness > .5f)
            Darkness = Mathf.Lerp(Darkness, .5f, rCam.room.LightSourceExposure(bPos));
        if (LastDarkness != Darkness)
        {
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            RefreshColor(sLeaser);
        }
        Vector2 vector = Vector2.Lerp(DrawPositions[0][1], DrawPositions[0][0], timeStacker), vector2 = Vector2.Lerp(DrawPositions[1][1], DrawPositions[1][0], timeStacker),
            vector3 = Vector2.Lerp(DrawPositions[2][1], DrawPositions[2][0], timeStacker), vector4 = Custom.DirVec(vector2, vector),
            vector5 = Custom.PerpendicularVector(vector4), vector6 = Vector2.Lerp(TailEnd.lastPos, TailEnd.pos, timeStacker), normalized = rCam.room.lightAngle.normalized;
        normalized.y *= -1f;
        var sprites = sLeaser.sprites;
        sprites[HEAD_SPRITE].SetPosition(vector - camPos);
        sprites[HEAD_SPRITE].rotation = Custom.VecToDeg(vector4);
        Vector2 vector7 = vector + vector4, vector8 = vector;
        float num3 = 0f, num4 = 0f;
        var mesh = (sprites[MESH_SPRITE] as TriangleMesh)!;
        var sMesh = (sprites[SHINE_MESH_SPRITE] as TriangleMesh)!;
        for (var i = 0; i < 12; i++)
        {
            var num5 = Mathf.InverseLerp(0f, 11f, i);
            var vector9 = Custom.Bezier(vector + vector4 * 3f, vector2, vector6, vector3, num5);
            float num6 = Mathf.Lerp(6f, 2f, num5) + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num5, 1.7f) * Mathf.PI)), .75f) * Mathf.Lerp(7f, 5f, Math.Abs(num)) * BodyThickness,
                a = Mathf.Lerp(.5f + Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, 10f, i) * Mathf.PI)), .25f) * 2f, num6 * .5f, num5 * .5f);
            a = Mathf.Lerp(a, Mathf.Max(a, num6 * .5f), Math.Abs(Vector2.Dot((vector9 - vector7).normalized, normalized))) * 1.4f;
            Vector2 vector10 = vector9 - normalized * (num6 - a), vector11 = Custom.PerpendicularVector(vector9, vector7);
            mesh.MoveVertice(i * 4, (vector7 + vector9) * .5f - vector11 * ((num6 + num3) * .5f) - camPos);
            mesh.MoveVertice(i * 4 + 1, (vector7 + vector9) * .5f + vector11 * ((num6 + num3) * .5f) - camPos);
            mesh.MoveVertice(i * 4 + 2, vector9 - vector11 * num6 - camPos);
            mesh.MoveVertice(i * 4 + 3, vector9 + vector11 * num6 - camPos);
            if (i < 11)
            {
                sMesh.MoveVertice(i * 4, (vector8 + vector10) * .5f - vector11 * ((a + num4) * .25f) - camPos);
                sMesh.MoveVertice(i * 4 + 1, (vector8 + vector10) * .5f + vector11 * ((a + num4) * .25f) - camPos);
                sMesh.MoveVertice(i * 4 + 2, vector10 - vector11 * a - camPos);
                sMesh.MoveVertice(i * 4 + 3, vector10 + vector11 * a - camPos);
                var verts = sMesh.verticeColors;
                if (Bug.Submersion >= 1f)
                {
                    var clr = new Color(0f, .003921569f, 0f);
                    for (var j = 0; j < verts.Length; j++)
                        verts[j] = clr;
                }
                else
                {
                    for (var j = 0; j < verts.Length; j++)
                        verts[j] = Color.Lerp(CurrentSkinColor, ShineColor, .25f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0f, verts.Length - 1, j), 4f) * Mathf.PI)), .5f));
                }
                if (i > 1)
                {
                    Vector2 vector12 = vector9 - vector11 * num6,
                        vector13 = vector9 + vector11 * num6;
                    var spr = sprites[SegmentSprite(i - 1)];
                    spr.SetPosition(vector12 - camPos);
                    spr.rotation = Custom.AimFromOneVectorToAnother(vector12, vector13);
                    spr.scaleY = Vector2.Distance(vector12, vector13);
                    spr.scaleX = 2f;
                }
            }
            vector7 = vector9;
            num3 = num6;
            vector8 = vector10;
            num4 = a;
        }
        var legs = Legs;
        for (var j = 0; j < legs.Length; j++)
        {
            var legsj = legs[j];
            for (var k = 0; k < legsj.Length; k++)
            {
                float t2 = Mathf.InverseLerp(0f, legs[legs.Length - j - 1].Length - 1, k), num7 = 5f;
                var vector14 = Vector2.Lerp(vector, vector2, .3f);
                vector14 += vector5 * ((j == 0 ? 1f : -1f) * 3f * (1f - Math.Abs(num)));
                vector14 += vector4 * Mathf.Lerp(5f, -11f, t2);
                Vector2 vector15 = Vector2.Lerp(legs[legs.Length - j - 1][k].lastPos, legs[legs.Length - j - 1][k].pos, timeStacker), vector16 = Vector2.Lerp(Knees[legs.Length - j - 1][k].lastPos, Knees[legs.Length - j - 1][k].pos, timeStacker),
                    vector17 = Vector2.Lerp(vector14, vector16, .5f), vector18 = Vector2.Lerp(vector16, vector15, .5f), vector19 = Vector2.Lerp(vector17, vector18, .5f);
                vector17 = vector19 + Custom.DirVec(vector19, vector17) * (num7 * .5f);
                vector18 = vector19 + Custom.DirVec(vector19, vector18) * (num7 * .5f);
                vector7 = vector14;
                num3 = 2f;
                var lMesh = (sprites[LegSprite(j, k)] as TriangleMesh)!;
                for (var l = 0; l < 12; l++)
                {
                    var num8 = Mathf.InverseLerp(0f, 11f, l);
                    var vector20 = num8 >= .5f ? Custom.Bezier((vector18 + vector17) * .5f, vector18 + Custom.DirVec(vector17, vector18) * 7f, vector15, vector15 + Custom.DirVec(vector15, vector14) * 14f, Mathf.InverseLerp(.5f, 1f, num8)) : Custom.Bezier(vector14, vector14 + Custom.DirVec(vector14, vector15) * 10f, (vector18 + vector17) * .5f, vector17 + Custom.DirVec(vector18, vector17) * 7f, Mathf.InverseLerp(0f, .5f, num8));
                    var num9 = (Mathf.Lerp(4f, .5f, Mathf.Pow(num8, .25f)) + Mathf.Sin(Mathf.Pow(num8, 2.5f) * Mathf.PI) * 1.5f) * LegsThickness;
                    var vector21 = Custom.PerpendicularVector(vector20, vector7);
                    lMesh.MoveVertice(l * 4, (vector7 + vector20) * .5f - vector21 * ((num9 + num3) * .5f) - camPos);
                    lMesh.MoveVertice(l * 4 + 1, (vector7 + vector20) * .5f + vector21 * ((num9 + num3) * .5f) - camPos);
                    lMesh.MoveVertice(l * 4 + 2, vector20 - vector21 * num9 - camPos);
                    lMesh.MoveVertice(l * 4 + 3, vector20 + vector21 * num9 - camPos);
                    vector7 = vector20;
                    num3 = num9;
                }
            }
        }
        for (var m = 0; m < 2; m++)
        {
            var num11 = Mathf.Lerp(m == 0 ? 1f : -1f, num, Mathf.Pow(Math.Abs(num), 2f));
            var mand = Mandibles[m];
            Vector2 vector23 = vector + vector4 * 4f + vector5 * (num11 * -3f), vector24 = Vector2.Lerp(mand.lastPos, mand.pos, timeStacker), vector25 = Custom.InverseKinematic(vector23, vector24, 16f, 18f, num11);
            var s = sprites[MandibleSprite(m, 0)];
            s.SetPosition(vector23 - camPos);
            s.anchorY = 0f;
            s.rotation = Custom.AimFromOneVectorToAnother(vector23, vector25);
            s.scaleY = Vector2.Distance(vector23, vector25) / s.element.sourcePixelSize.y;
            s.scaleX = -Math.Sign(num11);
            var s2 = sprites[MandibleSprite(m, 1)];
            s2.SetPosition(vector25 - camPos);
            s2.anchorY = 0f;
            s2.rotation = Custom.AimFromOneVectorToAnother(vector25, vector24);
            s2.scaleY = Vector2.Distance(vector25, vector24) / s.element.sourcePixelSize.y;
            s2.scaleX = Math.Sign(num11) * -.6f;
            var wing = sprites[WingSprite(m)];
            var vector26 = Custom.DegToVec(90f * num + (m == 0 ? -1f : 1f) * 34f);
            var tst = sprites[25 + m];
            if (vector26.y < 0f)
                tst.isVisible = wing.isVisible = false;
            else
            {
                tst.alpha = sp;
                tst.isVisible = wing.isVisible = true;
                var vector27 = Vector2.Lerp(vector, vector2, .2f) - vector5 * (6f * vector26.x);
                tst.SetPosition(vector27 - camPos);
                wing.SetPosition(vector27 - camPos);
                tst.rotation = wing.rotation = Custom.VecToDeg(vector4);
                wing.scaleX = vector26.y * .4f;
                tst.scaleX = vector26.y * .5f;
                tst.scaleY = 1.1f - sp * .5f;
                tst.anchorY = .5f + sp * .5f;
                wing.scaleY = .9f;
                var val = Math.Abs(Vector2.Dot(vector4, Custom.DegToVec(Custom.VecToDeg(normalized) - 90f + (90f * num + (m == 0 ? -1f : 1f) * 24f) * .4f)));
                if (rCam.room.PointSubmerged(vector27))
                    wing.color = new Color(0f, .003921569f, 0f);
                else
                    wing.color = Color.Lerp(CurrentSkinColor, ShineColor, .15f + Custom.LerpMap(val, .55f, 1f, 0f, .25f, 3f));
            }
            vector23 = vector;
            var ant = Antennae[m];
            vector25 = Vector2.Lerp(ant.lastPos, ant.pos, timeStacker);
            var normalized2 = (Custom.DirVec(vector2, vector) + Custom.PerpendicularVector(vector2, vector) * (num * .5f) + Custom.PerpendicularVector(vector2, vector) * ((m == 0 ? -1f : 1f) * (1f - Math.Abs(num)) * .35f)).normalized;
            vector7 = vector;
            num3 = 3f;
            var anMesh = (sprites[AntennaSprite(m)] as TriangleMesh)!;
            for (var num13 = 0; num13 < 8; num13++)
            {
                num11 = Mathf.InverseLerp(0f, 7f, num13);
                Vector2 vector30 = Custom.Bezier(vector23, vector23 + normalized2 * (30f * AntennaeLength), vector25, vector25, num11), vector31 = Custom.PerpendicularVector(vector30, vector7);
                var num14 = Mathf.Lerp(1f, .5f, num11);
                anMesh.MoveVertice(num13 * 4, (vector7 + vector30) * .5f - vector31 * ((num3 + num14) * .5f) - camPos);
                anMesh.MoveVertice(num13 * 4 + 1, (vector7 + vector30) * .5f + vector31 * ((num3 + num14) * .5f) - camPos);
                anMesh.MoveVertice(num13 * 4 + 2, vector30 - vector31 * num14 - camPos);
                anMesh.MoveVertice(num13 * 4 + 3, vector30 + vector31 * num14 - camPos);
                vector7 = vector30;
                num3 = num14;
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        CurrentSkinColor = Color.Lerp(palette.blackColor, BugCol, .1f - Darkness * .08f);
        ShineColor = Color.Lerp(Color.Lerp(Custom.HSL2RGB(Hue, .55f, .65f), palette.fogColor, .25f + .75f * Mathf.InverseLerp(.5f, 1f, Darkness)), BugCol, .4f);
        RefreshColor(sLeaser);
    }

    public virtual void RefreshColor(RoomCamera.SpriteLeaser sLeaser)
    {
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
        {
            if (i != WingSprite(0) && i != WingSprite(1))
                sprites[i].color = CurrentSkinColor;
        }
        var sMesh = (sprites[SHINE_MESH_SPRITE] as TriangleMesh)!;
        for (var i = 0; i < 11; i++)
        {
            var verts = sMesh.verticeColors;
            if (Bug.Submersion >= 1f)
            {
                var clr = new Color(0f, .003921569f, 0f);
                for (var j = 0; j < verts.Length; j++)
                    verts[j] = clr;
            }
            else
            {
                for (var j = 0; j < verts.Length; j++)
                    verts[j] = Color.Lerp(CurrentSkinColor, ShineColor, .25f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0f, verts.Length - 1, j), 4f) * Mathf.PI)), .5f));
            }
        }
        if (ColoredAntennae <= 0f)
            return;
        for (var k = 0; k < 2; k++)
        {
            var verts2 = (sprites[AntennaSprite(k)] as TriangleMesh)!.verticeColors;
            for (var l = 0; l < verts2.Length; l++)
                verts2[l] = Color.Lerp(CurrentSkinColor, ShineColor, Mathf.InverseLerp(verts2.Length / 2, verts2.Length - 1, l) * ColoredAntennae);
        }
    }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite != SHINE_MESH_SPRITE;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) => mudOverlay.layeringMap[mudOverlay.GetMudSprite(MESH_SPRITE)] = sLeaser.sprites[SHINE_MESH_SPRITE];
}