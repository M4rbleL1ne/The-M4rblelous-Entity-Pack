using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class HazerMomGraphics : GraphicsModule
{
    public const int MESH_SPRITE = 0, BODY_SPRITE = 1;
    public static HSLColor WhiteCol, RedCol;
    public ChunkDynamicSoundLoop? SoundLoop;
    public List<Vector2[][]> Tentacles;
    public BodyChunk? LookAtObj;
    public Vector2[] Scales;
    public float DeadColor, LastDeadColor, EyeOpen, LastEyeOpen, PupSize, LastPupSize, PupGetToSize;
    public Vector2 LookDir, LastLookDir, LookPos, SmallEyeMovements;
    public int Blink;
    public HSLColor SkinColor, SecondColor, EyeColor;
    public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData;
    public float WhiteAmount;

    public virtual HazerMom Squid => (owner as HazerMom)!;

    public virtual int ClosedEyeSprite => Tentacles.Count + Scales.Length + 2;

    public virtual int EyeSprite => Tentacles.Count + Scales.Length + 3;

    public virtual int PupilSprite => Tentacles.Count + Scales.Length + 4;

    public virtual int EyeDotSprite => Tentacles.Count + Scales.Length + 5;

    public virtual int EyeHighLightSprite => Tentacles.Count + Scales.Length + 6;

    public virtual int TotalSprites => Tentacles.Count + Scales.Length + 7;

    static HazerMomGraphics()
    {
        var col = Custom.RGB2HSL(new Color(.87f, .87f, .87f));
        WhiteCol = new(col.x, col.y, col.z);
        col = Custom.RGB2HSL(Color.red);
        RedCol = new(col.x, col.y, col.z);
    }

    public HazerMomGraphics(HazerMom ow) : base(ow, false)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        WhiteAmount = ow.abstractCreature.superSizeMe ? 0f : 1f;
        SkinColor = HSLColor.Lerp(new((Random.value < .5f ? .348f : .56f) + Mathf.Lerp(-.03f, .03f, Random.value), .6f + Random.value * .1f, .7f + Random.value * .1f), WhiteCol, .85f * WhiteAmount);
        SecondColor = HSLColor.Lerp(new(SkinColor.hue + Mathf.Lerp(-.1f, .1f, Random.value), Mathf.Lerp(SkinColor.saturation, 1f, Random.value), SkinColor.lightness - Random.value * .4f), RedCol, .9f * WhiteAmount);
        EyeColor = HSLColor.Lerp(new((SkinColor.hue + SecondColor.hue) * .5f + .5f, 1f, .4f + Random.value * .1f), RedCol, .9f * WhiteAmount);
        Tentacles = [];
        var num = Random.Range(4, 8);
        for (var i = 0; i < num; i++)
        {
            var f = Mathf.Lerp(34f / num * Mathf.Lerp(.7f, 1.2f, Random.value), 8f, .2f) * 1.5f;
            var tent = new Vector2[Math.Max(4, Mathf.RoundToInt(f))][];
            Tentacles.Add(tent);
            for (var j = 0; j < tent.Length; j++)
                tent[j] = [default, default, default, Custom.RNV() * Random.value];
        }
        var scales = Scales = new Vector2[Random.Range(12, 18)];
        for (int k = 0; k < scales.Length; k++)
            scales[k] = new(Random.value, Math.Max(Random.value * 1.4f, .3f));
        Random.state = state;
        DeadColor = ow.State.alive ? 0f : 1f;
        LastDeadColor = DeadColor;
        EyeOpen = 1f;
        LastEyeOpen = EyeOpen;
        Reset();
    }

    public virtual int ScaleSprite(int s) => 2 + s;

    public virtual int TentacleSprite(int t) => 2 + Scales.Length + t;

    public override void Reset()
    {
        base.Reset();
        var tentacles = Tentacles;
        for (var i = 0; i < tentacles.Count; i++)
        {
            var tenti = tentacles[i];
            for (var j = 0; j < tenti.Length; j++)
            {
                var tentij = tenti[j];
                tentij[0] = Squid.ChunkInOrder0.pos + new Vector2(0f, 5f * i);
                tentij[1] = tentij[0];
                tentij[2] *= 0f;
            }
        }
    }

    public virtual float ObjectInterestingScore(BodyChunk? randomChunk)
    {
        if (randomChunk is null)
            return 0f;
        return (1f + Vector2.Distance(randomChunk.lastLastPos, randomChunk.pos) * .4f) * (1f + randomChunk.owner.TotalMass) * .5f / Vector2.Distance(Squid.mainBodyChunk.pos, randomChunk.pos);
    }

    public override void Update()
    {
        base.Update();
        LastDeadColor = DeadColor;
        LastLookDir = LookDir;
        LastEyeOpen = EyeOpen;
        LastPupSize = PupSize;
        if (Squid.dead)
            DeadColor = Mathf.Min(1f, DeadColor + 1f / 154f);
        if (SoundLoop is null && Squid.Spraying)
            SoundLoop = new(Squid.bodyChunks[1])
            {
                sound = SoundID.Hazer_Squirt_Smoke_LOOP
            };
        else if (SoundLoop is ChunkDynamicSoundLoop snd)
        {
            snd.Volume = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Squid.InkLeft * .2f * Mathf.PI)), .2f);
            snd.Pitch = .6f + .4f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Squid.InkLeft * .2f * Mathf.PI)), .7f);
            snd.Update();
            if (!Squid.Spraying)
            {
                if (SoundLoop.emitter is SoundEmitter emit)
                    emit.slatedForDeletetion = true;
                SoundLoop = null;
            }
        }
        var objs = Squid.room.physicalObjects;
        if (Random.value < .05f && objs.Length > 0)
        {
            BodyChunk? chunk;
            var objsNum = objs[Random.Range(0, objs.Length)];
            if (objsNum.Count > 0)
            {
                var physicalObject = objsNum[Random.Range(0, objsNum.Count)];
                chunk = physicalObject.bodyChunks[Random.Range(0, physicalObject.bodyChunks.Length)];
                if (physicalObject != Squid && physicalObject.NoCamo() && physicalObject.abstractPhysicalObject.SameRippleLayer(Squid.abstractPhysicalObject) && ObjectInterestingScore(chunk) > ObjectInterestingScore(LookAtObj) && Custom.DistLess(Squid.mainBodyChunk.pos, chunk.pos, 400f))
                    LookAtObj = chunk;
            }
        }
        if (LookAtObj is BodyChunk ch)
        {
            if (ch.owner.room != Squid.room || ch.owner.slatedForDeletetion || !Custom.DistLess(ch.pos, Squid.mainBodyChunk.pos, 600f) || Random.value < .02f)
                LookAtObj = null;
            else
                LookPos = ch.pos;
        }
        else if (Random.value < 1f / 7f)
        {
            if (Random.value < .1f)
            {
                var pos = Squid.mainBodyChunk.pos + Custom.RNV() * (Random.value * 600f);
                if (!Squid.room.GetTile(pos).Solid)
                    LookPos = pos;
            }
            else
            {
                var pos2 = LookPos + Custom.RNV() * (Random.value * 100f);
                if (!Squid.room.GetTile(pos2).Solid)
                    LookPos = pos2;
            }
        }
        if (Squid.dead)
        {
            EyeOpen = Custom.LerpAndTick(EyeOpen, 1f, .08f, 1f / 30f);
            LookDir *= .9f;
        }
        else
        {
            SmallEyeMovements *= .94f;
            if (Random.value < 1f / 3f)
                SmallEyeMovements = Custom.RNV() * (Random.value * Mathf.Min(Vector2.Distance(Squid.mainBodyChunk.pos, LookPos) * .5f, 120f));
            if (LookAtObj is not null)
                LookDir = Vector2.Lerp(LookDir, Vector2.ClampMagnitude((LookPos + SmallEyeMovements - Squid.ChunkInOrder0.pos) / 60f, 1f), .3f);
            --Blink;
            if (Blink < -Random.Range(80, 1000) || (Random.value < .05f && Blink < 0 && Blink > -10))
                Blink = Random.Range(6, 27);
            EyeOpen = Custom.LerpAndTick(EyeOpen, (Blink < 0 && (Squid.MoveCounter < 0 || Squid.Swim > .5f) && !Squid.Spraying) ? 1f : 0f, .08f, .1f);
            PupSize = Custom.LerpAndTick(PupSize, PupGetToSize, .04f, 1f / 21f);
            if (Random.value < 1f / 41f)
                PupGetToSize = Random.value < .5f ? 1f : Random.value;
        }
        var degAng = Custom.AimFromOneVectorToAnother(Squid.ChunkInOrder1.pos, Squid.ChunkInOrder0.pos);
        var vector = Custom.DirVec(Squid.ChunkInOrder1.pos, Squid.ChunkInOrder0.pos);
        var tents = Tentacles;
        for (var i = 0; i < tents.Count; i++)
        {
            var tentsi = tents[i];
            var vector2 = TentacleDir(i, 1f, false);
            for (var j = 0; j < tentsi.Length; j++)
            {
                var tentsij = tentsi[j];
                var num2 = (float)j / (tentsi.Length - 1);
                tentsij[1] = tentsij[0];
                tentsij[0] += tentsij[2];
                tentsij[2] *= 1f - .5f * num2;
                tentsij[2] += (Vector2)Vector3.Slerp(vector, vector2, Mathf.Pow(num2, 1f - .7f * Squid.Swim)) * ((2.5f + Squid.Swim) * Mathf.Pow(1f - num2, 1.5f));
                if (j > 1 && Squid.room.GetTile(tentsij[0]).Solid)
                {
                    var cd = ScratchTerrainCollisionData.Set(tentsij[0], tentsij[1], tentsij[2], 1f, default, true);
                    cd = SharedPhysics.VerticalCollision(Squid.room, cd);
                    cd = SharedPhysics.HorizontalCollision(Squid.room, cd);
                    tentsij[0] = cd.pos;
                    tentsij[2] = cd.vel;
                }
                if (Squid.room.PointSubmerged(tentsij[0]))
                {
                    tentsij[2] *= .8f;
                    tentsij[2].y += .3f * num2;
                }
                else
                    tentsij[2].y -= .9f * Squid.room.gravity * num2;
                tentsij[2] += Custom.RotateAroundOrigo(tentsij[3], degAng) * (1f + num2);
                if (!Squid.dead)
                {
                    if (Random.value < .025f)
                        tentsij[3] = Custom.RNV() * Random.value;
                    else if (j > 0)
                        tentsij[3] = Vector2.Lerp(tentsij[3], tentsi[j - 1][3], .1f);
                }
                ConnectSegment(i, j);
            }
            for (var num3 = tentsi.Length - 1; num3 >= 0; num3--)
                ConnectSegment(i, num3);
        }
    }

    public virtual Vector2 TentacleDir(int t, float timeStacker, bool con)
    {
        float num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(Squid.ChunkInOrder1.lastPos, Squid.ChunkInOrder1.pos, timeStacker), Vector2.Lerp(Squid.ChunkInOrder0.lastPos, Squid.ChunkInOrder0.pos, timeStacker)),
            t2 = Mathf.InverseLerp(0f, Tentacles.Count - 1, t);
        if (Squid.Swim > 0f)
        {
            var t3 = Mathf.Lerp(0f, Mathf.Pow(.5f + .5f * Mathf.Sin(Squid.SwimCycle * Mathf.PI * 2f), .5f), Squid.Swim);
            return Custom.DegToVec(num + Mathf.Lerp(-1f, 1f, t2) * (con ? Mathf.Lerp(10f, 70f, t3) : Mathf.Lerp(20f, 160f, t3)));
        }
        return Custom.DegToVec(num + Mathf.Lerp(-1f, 1f, t2) * (con ? 30f : 80f));
    }

    public virtual Vector2 TentacleConPos(int t, float timeStacker)
    {
        var ch0 = Squid.ChunkInOrder0;
        var ch1 = Squid.ChunkInOrder1;
        return Vector2.Lerp(ch0.lastPos, ch0.pos, timeStacker) + Custom.DirVec(Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker), Vector2.Lerp(ch0.lastPos, ch0.pos, timeStacker)) * 4f + TentacleDir(t, timeStacker, true) * 4f;
    }

    public virtual void ConnectSegment(int c, int i)
    {
        var num = 2f + Mathf.Sin(Mathf.InverseLerp(0f, Tentacles.Count - 1, c) * Mathf.PI) * 3f;
        if (i == 0)
        {
            var tentsci = Tentacles[c][i];
            var vector = TentacleConPos(c, 1f);
            var vector2 = Custom.DirVec(tentsci[0], vector) * (num - Vector2.Distance(tentsci[0], vector));
            tentsci[0] -= vector2;
            tentsci[2] -= vector2;
        }
        else
        {
            var tentsci = Tentacles[c][i];
            var tentscim1 = Tentacles[c][i - 1];
            var vector3 = Custom.DirVec(tentsci[0], tentscim1[0]) * (num - Vector2.Distance(tentsci[0], tentscim1[0]));
            tentsci[0] -= vector3 * .6f;
            tentsci[2] -= vector3 * .6f;
            tentscim1[0] += vector3 * .4f;
            tentscim1[2] += vector3 * .4f;
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprs = sLeaser.sprites = new FSprite[TotalSprites];
        sprs[MESH_SPRITE] = TriangleMesh.MakeLongMesh(36, false, false);
        sprs[BODY_SPRITE] = new("Circle20")
        {
            scaleX = 2.25f * .75f,
            scaleY = 1.5f
        };
        sprs[ClosedEyeSprite] = new("pixel")
        {
            scaleX = 2.5f * 1.25f,
            scaleY = 22.5f * 1.25f
        };
        sprs[EyeSprite] = new("Circle20")
        {
            scaleY = 1.75f * .75f
        };
        sprs[PupilSprite] = new("Circle20");
        sprs[EyeDotSprite] = new("pixel") { scale = 2.5f };
        sprs[EyeHighLightSprite] = new("tinyStar") { scale = 2.5f };
        var tents = Tentacles;
        for (var i = 0; i < tents.Count; i++)
            sprs[TentacleSprite(i)] = TriangleMesh.MakeLongMesh(tents[i].Length, true, true);
        for (var j = 0; j < Scales.Length; j++)
        {
            sprs[ScaleSprite(j)] = new("pixel")
            {
                scaleX = 4f,
                scaleY = 6f,
                anchorY = 0f
            };
        }
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            sprs[i].RemoveFromContainer();
            newContainer.AddChild(sprs[i]);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        var vector = Vector2.Lerp(Squid.ChunkInOrder0.lastPos, Squid.ChunkInOrder0.pos, timeStacker);
        vector += Custom.DirVec(Vector2.Lerp(Squid.ChunkInOrder1.lastPos, Squid.ChunkInOrder1.pos, timeStacker), vector) * 5f;
        Vector2 a = vector,
            a2 = Vector2.Lerp(Squid.ChunkInOrder2.lastPos, Squid.ChunkInOrder2.pos, timeStacker),
            vector2 = Vector2.Lerp(Squid.ChunkInOrder1.lastPos, Squid.ChunkInOrder1.pos, timeStacker),
            vector3 = Custom.DirVec(a2, a);
        a2 += Custom.DirVec(vector2, a2) * 5f;
        var vector4 = Vector2.Lerp(LastLookDir, LookDir, timeStacker);
        var sprites = sLeaser.sprites;
        sprites[BODY_SPRITE].x = a.x - camPos.x;
        sprites[BODY_SPRITE].y = a.y - camPos.y;
        sprites[BODY_SPRITE].rotation = Custom.VecToDeg(Vector3.Slerp(Custom.DirVec(a2, a), vector3, .5f));
        var num = Mathf.Lerp(LastEyeOpen, EyeOpen, timeStacker);
        var vector5 = a + vector3 * 1.5f;
        sprites[EyeSprite].x = vector5.x - camPos.x;
        sprites[EyeSprite].y = vector5.y - camPos.y;
        sprites[ClosedEyeSprite].x = vector5.x - camPos.x;
        sprites[ClosedEyeSprite].y = vector5.y - camPos.y;
        if (num == 1f)
            sprites[EyeSprite].rotation = 0f;
        else
            sprites[EyeSprite].rotation = Custom.VecToDeg(vector3);
        sprites[ClosedEyeSprite].rotation = Custom.VecToDeg(vector3);
        sprites[EyeSprite].scaleX = Mathf.Lerp(1f, 7f, Mathf.Pow(num, .5f)) / 6f;
        sprites[EyeHighLightSprite].x = vector5.x - 2f * num - camPos.x;
        sprites[EyeHighLightSprite].y = vector5.y + 2f * num - camPos.y;
        sprites[EyeHighLightSprite].alpha = .5f * Mathf.InverseLerp(.5f, 1f, num) * (1f - DeadColor);
        var num2 = Mathf.Lerp(1.2f, 2.25f, Custom.SCurve(Mathf.Lerp(LastPupSize, PupSize, timeStacker), .75f)) * Mathf.Pow(num, .75f);
        vector5 += vector4 * ((3.5f - num2) * num);
        sprites[PupilSprite].x = vector5.x - camPos.x;
        sprites[PupilSprite].y = vector5.y - camPos.y;
        if (!Squid.dead && rCam.room.PointSubmerged(vector5 + new Vector2(0f, 5f)))
            sprites[PupilSprite].color = new(0f, .003921569f, 0f);
        else
            sprites[PupilSprite].color = Color.Lerp(EyeColor.rgb, new(.35f, .35f, .35f), DeadColor);
        sprites[PupilSprite].scale = num2 * 2f / 6f;
        sprites[EyeDotSprite].x = vector5.x - camPos.x;
        sprites[EyeDotSprite].y = vector5.y - camPos.y;
        var scales = Scales;
        for (var i = 0; i < scales.Length; i++)
        {
            var scale = scales[i];
            var num3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(a, vector2, scale.y), Vector2.Lerp(vector2, a2, scale.y));
            var vector6 = (scale.y < .5f) ? Vector2.Lerp(a, vector2, Mathf.InverseLerp(0f, .5f, scale.y)) : Vector2.Lerp(vector2, a2, Mathf.InverseLerp(.5f, 1f, scale.y));
            vector6 += Custom.RotateAroundOrigo(new Vector2((-2f + 4f * scale.x) * (2f + .4f * Squid.InkLeft), 0f), num3);
            var scaleSpr = sprites[ScaleSprite(i)];
            scaleSpr.x = vector6.x - camPos.x;
            scaleSpr.y = vector6.y - camPos.y;
            scaleSpr.rotation = num3;
            scaleSpr.isVisible = scale.y < 1f;
        }
        for (var j = 0; j < 36; j++)
        {
            float num4 = j / 35f,
                f = (j + 1) / 35f;
            Vector2 vector7 = Bez(a, a2, vector2, num4),
                b = Bez(a, a2, vector2, f),
                normalized = (vector - vector7).normalized,
                vector8 = Custom.PerpendicularVector(normalized);
            float num5 = Vector2.Distance(vector7, vector) * 1.1f,
                num6 = Vector2.Distance(vector7, b) * .9f,
                num7 = 7f * num4 + 3.5f * Mathf.Sin(num4 * Mathf.PI) * Mathf.Lerp(1f, 3f, Squid.InkLeft * .2f),
                num8 = num7;
            if (num4 <= .35f)
            {
                if (num4 <= 0f)
                    num7 *= .5f;
                else if (num4 <= .1f)
                    num7 *= .65f;
                else if (num4 <= .2f)
                    num7 *= .8f;
                else
                    num7 *= .95f;
            }
            else if (num4 >= .65f)
            {
                if (num4 >= 1f)
                {
                    num8 *= .5f;
                    num5 *= 1.5f;
                }
                else if (num4 >= .9f)
                {
                    num8 *= .65f;
                    num5 *= 1.35f;
                }
                else if (num4 >= .8f)
                {
                    num5 *= 1.2f;
                    num8 *= .8f;
                }
                else
                {
                    num5 *= 1.05f;
                    num8 *= .95f;
                }
            }
            var mesh = (sLeaser.sprites[MESH_SPRITE] as TriangleMesh)!;
            mesh.MoveVertice(j * 4, vector - vector8 * num7 - normalized * num5 - camPos);
            mesh.MoveVertice(j * 4 + 1, vector + vector8 * num7 - normalized * num5 - camPos);
            mesh.MoveVertice(j * 4 + 2, vector7 - vector8 * num8 + normalized * num6 - camPos);
            mesh.MoveVertice(j * 4 + 3, vector7 + vector8 * num8 + normalized * num6 - camPos);
            vector = vector7;
        }
        var tents = Tentacles;
        for (var l = 0; l < tents.Count; l++)
        {
            var tentsl = tents[l];
            var tentSpr = (sprites[TentacleSprite(l)] as TriangleMesh)!;
            tentSpr.isVisible = true;
            vector = TentacleConPos(l, timeStacker);
            var num9 = 8f;
            for (var m = 0; m < tentsl.Length; m++)
            {
                var tentslm = tentsl[m];
                Vector2 vector9 = Vector2.Lerp(tentslm[1], tentslm[0], timeStacker),
                    normalized2 = (vector9 - vector).normalized,
                    vector10 = Custom.PerpendicularVector(normalized2);
                float num10 = Vector2.Distance(vector9, vector),
                    num11 = 2.5f + 3f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, 5f, m) * Mathf.PI)), 2f);
                if (m == 0)
                {
                    tentSpr.MoveVertice(m * 4, vector - vector10 * ((num9 + num11) * .5f) - camPos);
                    tentSpr.MoveVertice(m * 4 + 1, vector + vector10 * ((num9 + num11) * .5f) - camPos);
                }
                else
                {
                    tentSpr.MoveVertice(m * 4, vector - vector10 * ((num9 + num11) * .5f) + normalized2 * num10 - camPos);
                    tentSpr.MoveVertice(m * 4 + 1, vector + vector10 * ((num9 + num11) * .5f) + normalized2 * num10 - camPos);
                }
                tentSpr.MoveVertice(m * 4 + 2, vector9 - vector10 * num11 - normalized2 * num10 - camPos);
                if (m < tentsl.Length - 1)
                    tentSpr.MoveVertice(m * 4 + 3, vector9 + vector10 * num11 - normalized2 * num10 - camPos);
                vector = vector9;
                num9 = num11;
            }
        }
        if (DeadColor != LastDeadColor)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
    }

    public static Vector2 Bez(Vector2 A, Vector2 B, Vector2 C, float f)
    {
        if (f < .5f)
            return Custom.Bezier(A, (A + C) / 2f, C, C + Custom.DirVec(B, A) * Vector2.Distance(A, C) / 4f, f);
        return Custom.Bezier(C, C + Custom.DirVec(A, B) * Vector2.Distance(C, B) / 2f, B, (B + C) / 2f, f);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        var sprites = sLeaser.sprites;
        var blk = Color.Lerp(palette.blackColor, new(.87f, .87f, .87f), .35f * WhiteAmount);
        var a = Color.Lerp(Custom.HSL2RGB(SkinColor.hue, SkinColor.saturation * (1f - .4f * DeadColor), SkinColor.lightness * (1f - .3f * DeadColor)), palette.fogColor, .2f + .1f * DeadColor);
        a = Color.Lerp(a, blk, .2f * DeadColor);
        var a2 = Color.Lerp(Custom.HSL2RGB(SecondColor.hue, SecondColor.saturation * (1f - .2f * DeadColor), SecondColor.lightness * (1f - .5f * DeadColor)), blk, .05f + .4f * Mathf.Max(DeadColor * .5f, 0f));
        sprites[BODY_SPRITE].color = sprites[MESH_SPRITE].color = a;
        var b = Color.Lerp(a2, blk, .1f + .3f * Mathf.Max(DeadColor, 0f));
        for (var i = 0; i < Tentacles.Count; i++)
        {
            var vertCols = (sprites[TentacleSprite(i)] as TriangleMesh)!.verticeColors;
            for (var j = 0; j < vertCols.Length; j++)
                vertCols[j] = Color.Lerp(a, b, Mathf.InverseLerp(1f, vertCols.Length - 1, j));
        }
        sprites[ClosedEyeSprite].color = Color.Lerp(Color.Lerp(a, a2, .8f), blk, .3f);
        a = Color.Lerp(Color.Lerp(a, blk, .7f - .5f * DeadColor), a2, .6f);
        for (var k = 0; k < Scales.Length; k++)
            sprites[ScaleSprite(k)].color = a;
        sprites[EyeSprite].color = blk;
        sprites[EyeDotSprite].color = blk;
    }
}